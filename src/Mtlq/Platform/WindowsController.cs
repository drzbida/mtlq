using System;
using System.Threading.Tasks;
using Windows.Media.Control;
using Mtlq.Models;

namespace Mtlq.Platform;

public class WindowsController : IMediaController
{
    public async Task<MediaSession[]> GetActiveSessionsAsync()
    {
        var sessionManager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
        var sessions = sessionManager.GetSessions();
        int count = sessions.Count;
        var tasks = new Task<MediaSession>[count];
        for (int i = 0; i < count; i++)
        {
            var session = sessions[i];
            tasks[i] = GetSessionInfoAsync(session);
        }
        return await Task.WhenAll(tasks);
    }

    public async Task<MediaSession?> TogglePlaySession(string source)
    {
        var sessionManager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
        var sessions = sessionManager.GetSessions();
        foreach (var session in sessions)
        {
            if (session.SourceAppUserModelId == source)
            {
                await session.TryTogglePlayPauseAsync();
                return await GetSessionInfoAsync(session);
            }
        }
        return null;
    }

    private static async Task<MediaSession> GetSessionInfoAsync(
        GlobalSystemMediaTransportControlsSession session)
    {
        var playbackInfo = session.GetPlaybackInfo();
        var mediaProps = await session.TryGetMediaPropertiesAsync();
        var timelineProperties = session.GetTimelineProperties();
        return new MediaSession
        {
            Source = session.SourceAppUserModelId ?? string.Empty,
            Title = mediaProps?.Title ?? string.Empty,
            Artist = mediaProps?.Artist ?? string.Empty,
            CurrentTime = timelineProperties.Position.ToString(),
            TotalTime = timelineProperties.EndTime.ToString(),
            Status = MapPlaybackStatus(playbackInfo?.PlaybackStatus)
        };
    }
    private static PlaybackStatus MapPlaybackStatus(GlobalSystemMediaTransportControlsSessionPlaybackStatus? status) =>
        status switch
        {
            GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing => PlaybackStatus.Playing,
            GlobalSystemMediaTransportControlsSessionPlaybackStatus.Paused => PlaybackStatus.Paused,
            GlobalSystemMediaTransportControlsSessionPlaybackStatus.Stopped => PlaybackStatus.Stopped,
            _ => PlaybackStatus.Unknown
        };
}
