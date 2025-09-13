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
       // var tasks = new Task[2];
        //  tasks[0] = Task.Run(Method);
        //tasks[1] = Task.Run(Method);
        //  Task.WaitAll(tasks);
    }

    static void Method()
    {
        Console.WriteLine($"Thread ID: {Environment.CurrentManagedThreadId}");
        Minecraft.Preview.Launch();
    }
}