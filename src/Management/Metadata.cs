using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Management.Deployment;

namespace Igneous.Management;

public sealed partial class Metadata
{
    static readonly PackageManager _packageManager = new();

    readonly string _packageFamilyName;

    Metadata(string packageFamilyName) => _packageFamilyName = packageFamilyName;

    public string Version
    {
        get
        {
            var package = _packageManager.FindPackagesForUser(string.Empty, _packageFamilyName).First();
            var path = Path.Combine(package.InstalledLocation.Path, "Minecraft.Windows.exe");
            var information = FileVersionInfo.GetVersionInfo(path);

            if (information.FileVersion is not null)
            {
                var length = information.FileVersion.LastIndexOf('.');
                return information.FileVersion.Substring(0, length);
            }

            var version = package.Id.Version;
            return $"{version.Major}.{version.Minor}.{version.Build}";
        }
    }

    public bool? GDK
    {
        get
        {
            var package = _packageManager.FindPackagesForUser(string.Empty, _packageFamilyName).First();

            if (package.SignatureKind is not PackageSignatureKind.Store)
                return null;
                
            var path = Path.Combine(package.InstalledLocation.Path, "MicrosoftGame.config");
            return File.Exists(path);
        }
    }
}

partial class Metadata
{
    public static readonly Metadata Release = new(Minecraft.UWP.PackageFamilyName);

    public static readonly Metadata Preview = new(Minecraft.WindowsBeta.PackageFamilyName);
}