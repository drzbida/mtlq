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
        var distinctOption = new Option<bool>("--distinct", "Only return distinct media");
        AddOption(distinctOption);
        this.SetHandler(
            (bool distinct) => WrapExecuteAsync(() => ExecuteAsync(distinct)),
            distinctOption
        );
    }

    protected async Task<MediaSession[]> ExecuteAsync(bool distinct)
    {
        var sessions = await _controller.GetActiveSessionsAsync();
        var playing = sessions.Where(s => s.Status == PlaybackStatus.Playing);
        if (distinct)
        {
            playing = playing.DistinctBy(s => (s.Title, s.Artist));
        }
        return [.. playing];
    }
}
