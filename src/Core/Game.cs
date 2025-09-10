using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;
using static Windows.Win32.PInvoke;

namespace Igneous.Core;

public unsafe abstract class Game
{
    internal Game(string packageFamilyName, string applicationUserModelId)
    {
        _packageFamilyName = packageFamilyName;
        _applicationUserModelId = applicationUserModelId;
    }

    static readonly IApplicationActivationManager _applicationActivationManager;

    static Game()
    {
        ApplicationActivationManager applicationActivationManager = new();
        _applicationActivationManager = (IApplicationActivationManager)applicationActivationManager;
    }

    protected readonly string _packageFamilyName, _applicationUserModelId;

    protected uint Activate()
    {
        fixed (char* applicationUserModelId = _applicationUserModelId)
        {
            _applicationActivationManager.ActivateApplication(applicationUserModelId, null, ACTIVATEOPTIONS.AO_NOERRORUI, out uint processId);
            return processId;
        }
    }

    public bool Installed
    {
        get
        {
            uint count = 0, length = 0;
            var error = GetPackagesByPackageFamily(_packageFamilyName, ref count, null, ref length, null);
            return error is WIN32_ERROR.ERROR_INSUFFICIENT_BUFFER && count > 0;
        }
    }

    public abstract uint? Launch();

    public abstract void Terminate();

    public abstract bool Running { get; }
}