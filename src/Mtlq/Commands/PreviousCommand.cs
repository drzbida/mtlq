using System;
using System.CommandLine;
using System.Threading.Tasks;
using Mtlq.Models;
using Mtlq.Platform;

namespace Mtlq.Commands;

public partial class PreviousCommand : BaseCommand<MediaSession>
{
    private readonly IMediaController _controller;

    public PreviousCommand(IMediaController controller)
        : base("previous", "Go to previous track", MediaJsonContext.Default.MediaSession)
    {
        _controller = controller;
        var queryArg = new Option<string>(
            "session",
            "Go to previous track for the specified source"
        );
        AddOption(queryArg);
        this.SetHandler(
            (string session) => WrapExecuteAsync(() => ExecuteAsync(session)),
            queryArg
        );
    }

    protected async Task<MediaSession> ExecuteAsync(string source)
    {
        var session = await _controller.PreviousSessionAsync(source);
        return session == null
            ? throw new InvalidOperationException("No media session found")
            : session.Value;
    }
}
