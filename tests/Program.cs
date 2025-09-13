using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Igneous;

static class Program
{
    [STAThread]
    static void Main()
    {
        Method();
    }

    static void Method()
    {
        Console.WriteLine($"Thread ID: {Environment.CurrentManagedThreadId}");
        Minecraft.Preview.Launch();
    }
}