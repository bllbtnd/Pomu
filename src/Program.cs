using AppKit;

namespace Pomu;

static class Program
{
    static void Main(string[] args)
    {
        NSApplication.Init();
        var app = NSApplication.SharedApplication;
        app.Delegate = new AppDelegate();
        app.Run();
    }
}
