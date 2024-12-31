using System.CommandLine;
using System.Linq;
using System.Threading.Tasks;
using Mtlq.Models;
using Mtlq.Platform;

namespace Mtlq.Commands;

public partial class NowCommand : BaseCommand<MediaSession[]>
{
    private readonly IMediaController _controller;

    public NowCommand(IMediaController controller)
        : base("now", "Get the currently playing media", MediaJsonContext.Default.MediaSessionArray)
    {
        _controller = controller;
        this.SetHandler(() => WrapExecuteAsync(ExecuteAsync));
    }

    protected override async Task<MediaSession[]> ExecuteAsync()
    {
        var sessions = await _controller.GetActiveSessionsAsync();
        return [.. sessions
            .Where(s => s.Status == PlaybackStatus.Playing)
            .DistinctBy(s => (s.Title, s.Artist))];
    }
}
