using System.IO;
using Windows.Win32.System.LibraryLoader;
using static Windows.Win32.PInvoke;
namespace Igneous.Modding;

public sealed class ModificationLibrary
{
    public readonly string Filename;

    public readonly bool Exists;

    public readonly bool Valid;

    public ModificationLibrary(string path)
    {
        Filename = Path.GetFullPath(path);
        Exists = File.Exists(Filename);
        Valid = Exists && FreeLibrary(LoadLibraryEx(Filename, LOAD_LIBRARY_FLAGS.DONT_RESOLVE_DLL_REFERENCES));
    }

    public static implicit operator ModificationLibrary(string @this) => new(@this);
}