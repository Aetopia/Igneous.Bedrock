namespace Igneous;

using Core;

/// <summary>
/// Provides access to Minecraft: Bedrock Edition.
/// </summary>

public static class Minecraft
{
    internal static class UWP
    {
        internal const string ApplicationUserModelId = $"{PackageFamilyName}!App";
       
        internal const string PackageFamilyName = "Microsoft.MinecraftUWP_8wekyb3d8bbwe";

        internal const string Path = null;
    }

    internal static class WindowsBeta
    {
        internal const string ApplicationUserModelId = $"{PackageFamilyName}!Game";

        internal const string PackageFamilyName = "Microsoft.MinecraftWindowsBeta_8wekyb3d8bbwe";

        internal const string Path = "Minecraft Bedrock Preview";
    }

    /// <summary>
    /// Provides access to Minecraft.
    /// </summary>

    public static readonly Game Release = new UWPGame(UWP.PackageFamilyName, UWP.ApplicationUserModelId);

    /// <summary>
    /// Provides access to Minecraft Preview.
    /// </summary>

    public static readonly Game Preview = new GDKGame(WindowsBeta.PackageFamilyName, WindowsBeta.ApplicationUserModelId, WindowsBeta.Path);
}