using System;
using System.CommandLine;
using System.Threading.Tasks;
using Mtlq.Models;
using Mtlq.Platform;

namespace Mtlq.Commands;

public partial class ToggleCommand : BaseCommand<MediaSession>
{
    private readonly IMediaController _controller;

    public ToggleCommand(IMediaController controller)
        : base("toggle", "Toggle media on / off", MediaJsonContext.Default.MediaSession)
    {
        _controller = controller;
        var queryArg = new Option<string>(
            "session",
            "Toggle media on / off for the specified source"
        );
        AddOption(queryArg);
        this.SetHandler(
            (string session) => WrapExecuteAsync(() => ExecuteAsync(session)),
            queryArg
        );
    }

    protected async Task<MediaSession> ExecuteAsync(string source)
    {
        var session = await _controller.TogglePlaySession(source);
        return session == null
            ? throw new InvalidOperationException("No media session found")
            : session.Value;
    }
}
