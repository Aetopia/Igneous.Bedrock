namespace Igneous;

using Core;

public sealed partial class Minecraft
{
    internal static class UWP
    {
        internal const string ApplicationUserModelId = $"{PackageFamilyName}!App";
        internal const string PackageFamilyName = "Microsoft.MinecraftUWP_8wekyb3d8bbwe";
    }

    internal static class WindowsBeta
    {
        internal const string ApplicationUserModelId = $"{PackageFamilyName}!Game";
        internal const string PackageFamilyName = "Microsoft.MinecraftWindowsBeta_8wekyb3d8bbwe";
    }

    public static readonly IGame Release = new UWPGame(UWP.PackageFamilyName, UWP.ApplicationUserModelId);

    public static readonly IGame Preview = new GDKGame(WindowsBeta.PackageFamilyName, WindowsBeta.ApplicationUserModelId);
}