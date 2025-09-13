using System.IO;
using Windows.Win32.System.LibraryLoader;
using static Windows.Win32.PInvoke;
namespace Igneous.Modding;

/// <summary>
/// Provides properties to validate modification libraries.
/// </summary>

public sealed class ModificationLibrary
{
    /// <summary>
    /// The filename of the modification library.
    /// </summary>

    public readonly string Filename;

    /// <summary>
    /// Check if the modification library exists.
    /// </summary>

    public readonly bool Exists;

    /// <summary>
    /// Check if the modification library is valid.
    /// </summary>

    public readonly bool Valid;

    /// <summary>
    /// Create an instance of a modification library.
    /// </summary>

    /// <param name="path">
    /// The path of the modification library.
    /// </param>

    public ModificationLibrary(string path)
    {
        Filename = Path.GetFullPath(path);
        Exists = File.Exists(Filename);
        Valid = Exists && FreeLibrary(LoadLibraryEx(Filename, LOAD_LIBRARY_FLAGS.DONT_RESOLVE_DLL_REFERENCES));
    }

    /// <summary>
    /// Create an instance of a modification library.
    /// </summary>

    /// <param name="path">
    /// The path of the modification library.
    /// </param>

    public static implicit operator ModificationLibrary(string path) => new(path);
}