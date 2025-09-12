namespace Igneous;

using Core;

public static class Minecraft
{
    static class UWP
    {
        internal const string ApplicationUserModelId = $"{PackageFamilyName}!App";
        internal const string PackageFamilyName = "Microsoft.MinecraftUWP_8wekyb3d8bbwe";
        internal const string Path = null;
    }

    static class WindowsBeta
    {
        internal const string ApplicationUserModelId = $"{PackageFamilyName}!Game";
        internal const string PackageFamilyName = "Microsoft.MinecraftWindowsBeta_8wekyb3d8bbwe";
        internal const string Path = "Minecraft Bedrock Preview";
    }

    public static readonly Game Release = new UWPGame(UWP.PackageFamilyName, UWP.ApplicationUserModelId);

    public static readonly Game Preview = new GDKGame(WindowsBeta.PackageFamilyName, WindowsBeta.ApplicationUserModelId, WindowsBeta.Path);
}