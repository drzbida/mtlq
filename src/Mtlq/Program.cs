using System;
using System.CommandLine;
using System.Threading.Tasks;
using Mtlq.Commands;
using Mtlq.Platform;

namespace Mtlq;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var controller = CreatePlatformController();
        var rootCommand = new RootCommand("Cross-platform media session CLI")
        {
            new NowCommand(controller),
            new ToggleCommand(controller),
            new NextCommand(controller),
            new PreviousCommand(controller),
        };
        return await rootCommand.InvokeAsync(args);
    }

    private static IMediaController CreatePlatformController()
    {
#if WINDOWS
        return new WindowsController();
#endif
#if LINUX
        return new LinuxController();
#endif
        throw new PlatformNotSupportedException("Unsupported operating system");
    }
}
