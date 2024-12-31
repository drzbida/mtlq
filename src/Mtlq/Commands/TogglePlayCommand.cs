using System;
using System.CommandLine;
using System.Threading.Tasks;
using Mtlq.Models;
using Mtlq.Platform;

namespace Mtlq.Commands;

public partial class TogglePlayCommand : BaseCommand<MediaSession>
{
    private readonly IMediaController _controller;

    public TogglePlayCommand(IMediaController controller)
        : base("toggle", "Toggle media on / off", MediaJsonContext.Default.MediaSession)
    {
        _controller = controller;
        var queryArg = new Argument<string>("session", "Toggle media on / off for the specified source");
        AddArgument(queryArg);
        this.SetHandler((string session) =>
                WrapExecuteAsync(() => ExecuteAsync(session)),
                queryArg
            );
    }

    protected async Task<MediaSession> ExecuteAsync(string source)
    {
        var session = await _controller.TogglePlaySession(source);
        return session == null ? throw new InvalidOperationException("No media session found") : session.Value;
    }
}
