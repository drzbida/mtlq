using System;
using System.CommandLine;
using System.Threading.Tasks;
using Mtlq.Models;
using Mtlq.Platform;

namespace Mtlq.Commands;

public partial class NextCommand : BaseCommand<MediaSession>
{
    private readonly IMediaController _controller;

    public NextCommand(IMediaController controller)
        : base("next", "Go to next track", MediaJsonContext.Default.MediaSession)
    {
        _controller = controller;
        var queryArg = new Option<string>("session", "Go to next track for the specified source");
        AddOption(queryArg);
        this.SetHandler(
            (string session) => WrapExecuteAsync(() => ExecuteAsync(session)),
            queryArg
        );
    }

    protected async Task<MediaSession> ExecuteAsync(string source)
    {
        var session = await _controller.NextSession(source);
        return session == null
            ? throw new InvalidOperationException("No media session found")
            : session.Value;
    }
}
