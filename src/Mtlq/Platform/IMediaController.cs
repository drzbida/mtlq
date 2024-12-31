using System.Threading.Tasks;
using Mtlq.Models;

namespace Mtlq.Platform;

public interface IMediaController
{
    Task<MediaSession[]> GetActiveSessionsAsync();

    Task<MediaSession?> TogglePlaySession(string source);
}
