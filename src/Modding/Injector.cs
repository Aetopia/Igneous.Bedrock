using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Igneous.Core;
using Igneous.Windows;
using Windows.Win32.Foundation;
using Windows.Win32.Security;
using Windows.Win32.Security.Authorization;
using static Windows.Win32.PInvoke;

namespace Igneous.Modding;

/// <summary>
/// Provides services for injection modifications into Minecraft: Bedrock Edition.
/// </summary>

public unsafe sealed partial class Injector
{
    readonly Game _game;

    readonly bool uwp;

    Injector(Game game)
    {
        _game = game;
        uwp = game is UWPGame;
    }
}

unsafe partial class Injector
{
    static readonly ACL* _acl;

    static Injector()
    {
        fixed (char* name = "ALL APPLICATION PACKAGES")
        {
            TRUSTEE_W trustee = new()
            {
                ptstrName = name,
                TrusteeForm = TRUSTEE_FORM.TRUSTEE_IS_NAME,
                TrusteeType = TRUSTEE_TYPE.TRUSTEE_IS_WELL_KNOWN_GROUP
            };

            EXPLICIT_ACCESS_W access = new()
            {
                Trustee = trustee,
                grfAccessMode = ACCESS_MODE.SET_ACCESS,
                grfInheritance = ACE_FLAGS.SUB_CONTAINERS_AND_OBJECTS_INHERIT,
                grfAccessPermissions = (uint)GENERIC_ACCESS_RIGHTS.GENERIC_ALL
            };

            ACL* acl = null;
            SetEntriesInAcl(1, &access, null, &acl);

            _acl = acl;
        }
    }
}

unsafe partial class Injector
{
    /// <summary>
    /// Launches &amp; injects modifications libraries into Minecraft: Bedrock Edition.
    /// </summary>

    /// <param name="libraries">
    /// List of modifications libraries to inject.
    /// </param>

    /// <returns>
    /// The process identifier of the game.
    /// </returns>

    /// <exception cref="FileNotFoundException">
    /// Thrown if any specified modification library doesn't exist.
    /// </exception>

    /// <exception cref="BadImageFormatException">
    /// Thrown if any specified modification library is invalid.
    /// </exception>

    public uint? Launch(params IReadOnlyCollection<ModificationLibrary> libraries)
    {
        if (_game.Launch() is not uint processId)
            return null;

        using ProcessHandle process = new(processId);
        using InjectionSession session = new(process);

        foreach (var library in libraries)
        {
            if (!library.Exists)
                throw new FileNotFoundException(null, library.Filename);

            if (!library.Valid)
                throw new BadImageFormatException(null, library.Filename);

            fixed (char* filename = library.Filename)
            {
                if (uwp)
                {
                    const SE_OBJECT_TYPE type = SE_OBJECT_TYPE.SE_FILE_OBJECT;
                    const OBJECT_SECURITY_INFORMATION information = OBJECT_SECURITY_INFORMATION.DACL_SECURITY_INFORMATION;
                    SetNamedSecurityInfo(filename, type, information, PSID.Null, PSID.Null, _acl, null);
                }
                session.AddLibrary(filename);
            }
        }

        session.InjectLibraries();
        return processId;
    }
}

partial class Injector
{
    /// <summary>
    /// Provides injection services for Minecraft.
    /// </summary>

    public static readonly Injector Release = new(Minecraft.Release);

    /// <summary>
    /// Provides injection services for Minecraft Preview.
    /// </summary>

    public static readonly Injector Preview = new(Minecraft.Preview);
}