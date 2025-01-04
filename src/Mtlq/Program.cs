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
        var rootCommand = new RootCommand("Cross-platform media session CLI");
        rootCommand.AddCommand(new NowCommand(controller));
        rootCommand.AddCommand(new ToggleCommand(controller));
        return await rootCommand.InvokeAsync(args);
    }

    private static IMediaController CreatePlatformController() =>
        OperatingSystem.IsWindows()
            ? new WindowsController()
            : throw new PlatformNotSupportedException("Unsupported operating system");
}
