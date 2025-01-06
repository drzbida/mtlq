using System.Threading.Tasks;
using Mtlq.Models;

namespace Mtlq.Platform;

public interface IMediaController
{
    Task<MediaSession[]> GetActiveSessionsAsync();

    Task<MediaSession?> ToggleSessionAsync(string source);

    Task<MediaSession?> NextSessionAsync(string source);

    Task<MediaSession?> PreviousSessionAsync(string source);
}
