using System.Diagnostics;
using System.IO;
using System.Linq;
using Windows.ApplicationModel;
using Windows.Management.Deployment;

namespace Igneous.Management;

public sealed partial class Manifest
{
    static readonly PackageManager _packageManager = new();

    readonly string _packageFamilyName;

    Manifest(string packageFamilyName) => _packageFamilyName = packageFamilyName;

    public string Version
    {
        get
        {
            var result = "0.0.0";
            var packages = _packageManager.FindPackagesForUser(string.Empty, _packageFamilyName);

            Package package = packages.FirstOrDefault();
            if (package is not null)
            {
                var path = package.InstalledLocation.Path;
                path = Path.Combine(path, "Minecraft.Windows.exe");

                result = FileVersionInfo.GetVersionInfo(path).FileVersion;

                if (result is not null)
                    result = result.Substring(0, result.LastIndexOf('.'));
                else
                {
                    var version = package.Id.Version;
                    result = $"{version.Major}.{version.Minor}.{version.Build}";
                }
            }

            return result;
        }
    }
}

partial class Manifest
{
    public static readonly Manifest Release = new(Minecraft.UWP.PackageFamilyName);

    public static readonly Manifest Preview = new(Minecraft.WindowsBeta.PackageFamilyName);
}