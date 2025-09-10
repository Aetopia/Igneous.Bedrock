namespace Igneous.Core;

public interface IGame
{
    public uint? Launch();
    public void Terminate();
    public bool Installed { get; }
    public bool Running { get; }
}