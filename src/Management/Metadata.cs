using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Management.Deployment;

namespace Igneous.Management;

/// <summary>
/// Provides services for querying metadata for Minecraft: Bedrock Edition.
/// </summary>

public sealed partial class Metadata
{
    static readonly PackageManager _packageManager = new();

    readonly string _packageFamilyName;

    Metadata(string packageFamilyName) => _packageFamilyName = packageFamilyName;

    /// <summary>
    /// Queries the installed version of Minecraft: Bedrock Edition.
    /// </summary>

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

    /// <summary>
    /// Queries if Minecraft: Bedrock Edition is using the Game Development Kit.
    /// </summary>

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
    /// <summary>
    /// Provides metadata for Minecraft.
    /// </summary>

    public static readonly Metadata Release = new(Minecraft.UWP.PackageFamilyName);

    /// <summary>
    /// Provides metadata for Minecraft Preview.
    /// </summary>

    public static readonly Metadata Preview = new(Minecraft.WindowsBeta.PackageFamilyName);
}