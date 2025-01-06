#if WINDOWS
using System;
using System.Linq;
using System.Threading.Tasks;
using Mtlq.Models;
using Windows.Foundation;
using Windows.Media.Control;

namespace Mtlq.Platform;

public class WindowsController : IMediaController
{
    private class SessionInfo()
    {
        public GlobalSystemMediaTransportControlsSession Source { get; }
        public GlobalSystemMediaTransportControlsSessionPlaybackInfo PlaybackInfo { get; set; }
        public GlobalSystemMediaTransportControlsSessionMediaProperties MediaProperties { get; set; }
        public GlobalSystemMediaTransportControlsSessionTimelineProperties TimelineProperties { get; set; }

        public SessionInfo(
            GlobalSystemMediaTransportControlsSession source,
            GlobalSystemMediaTransportControlsSessionPlaybackInfo playbackInfo,
            GlobalSystemMediaTransportControlsSessionMediaProperties mediaProperties,
            GlobalSystemMediaTransportControlsSessionTimelineProperties timelineProperties
        )
            : this()
        {
            Source = source;
            PlaybackInfo = playbackInfo;
            MediaProperties = mediaProperties;
            TimelineProperties = timelineProperties;
        }

        public MediaSession ToMediaSession()
        {
            return new MediaSession
            {
                Source = Source.SourceAppUserModelId ?? string.Empty,
                Title = MediaProperties?.Title ?? string.Empty,
                Artist = MediaProperties?.Artist ?? string.Empty,
                CurrentTime = TimelineProperties.Position.ToString(),
                TotalTime = TimelineProperties.EndTime.ToString(),
                Status = MapPlaybackStatus(PlaybackInfo?.PlaybackStatus),
            };
        }

        private static PlaybackStatus MapPlaybackStatus(
            GlobalSystemMediaTransportControlsSessionPlaybackStatus? status
        ) =>
            status switch
            {
                GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing =>
                    PlaybackStatus.Playing,
                GlobalSystemMediaTransportControlsSessionPlaybackStatus.Paused =>
                    PlaybackStatus.Paused,
                GlobalSystemMediaTransportControlsSessionPlaybackStatus.Stopped =>
                    PlaybackStatus.Stopped,
                _ => PlaybackStatus.Unknown,
            };
    }

    private GlobalSystemMediaTransportControlsSessionManager _sessionManager;

    private async Task<GlobalSystemMediaTransportControlsSessionManager> GetSessionManagerAsync()
    {
        return _sessionManager ??=
            await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
    }

    private static async Task<SessionInfo> GetSessionInfoAsync(
        GlobalSystemMediaTransportControlsSession session
    )
    {
        var playbackInfo = session.GetPlaybackInfo();
        var mediaProps = await session.TryGetMediaPropertiesAsync();
        var timelineProperties = session.GetTimelineProperties();
        return new SessionInfo(session, playbackInfo, mediaProps, timelineProperties);
    }

    private async Task<SessionInfo> GetSession(string source)
    {
        var sessions = await GetSessionsInfoAsync();

        if (source is null)
        {
            return sessions.FirstOrDefault();
        }

        return sessions.FirstOrDefault(s => s.Source.SourceAppUserModelId == source);
    }

    private async Task<SessionInfo[]> GetSessionsInfoAsync()
    {
        var manager = await GetSessionManagerAsync();
        var sessions = manager.GetSessions();
        int count = sessions.Count;
        var tasks = new Task<SessionInfo>[count];
        for (int i = 0; i < count; i++)
        {
            var session = sessions[i];
            tasks[i] = GetSessionInfoAsync(session);
        }
        var result = await Task.WhenAll(tasks);
        return [.. result.OrderByDescending(s => s.TimelineProperties.LastUpdatedTime)];
    }

    private async Task<MediaSession?> SkipTrack(
        string source,
        Func<GlobalSystemMediaTransportControlsSession, IAsyncOperation<bool>> skipAction,
        bool handleResetToStart = false
    )
    {
        var session = await GetSession(source);
        if (session is null)
        {
            return null;
        }

        var tcs = new TaskCompletionSource<MediaSession>();
        var originalTitle = session.MediaProperties?.Title;
        var wasReset = false;

        // NOTE: This logic is necessary to handle players where the track is reset to the start
        // when skipping the track while it's not near the start.
        // This is the case for Spotify, for example.
        // Aditionally, the speed of which the events are fired is vastly different between players.
        // For example, Spotify fires the events almost instantly, while Firefox on a YouTube playlist
        // takes a few seconds to fire the events.

        void OnMediaPropertiesChanged(
            GlobalSystemMediaTransportControlsSession sender,
            MediaPropertiesChangedEventArgs args
        )
        {
            GetSessionInfoAsync(session.Source)
                .ContinueWith(t =>
                {
                    if (t.Result.MediaProperties?.Title != originalTitle)
                    {
                        tcs.TrySetResult(t.Result.ToMediaSession());
                    }
                });
        }

        void OnTimelineChanged(
            GlobalSystemMediaTransportControlsSession sender,
            TimelinePropertiesChangedEventArgs args
        )
        {
            if (!handleResetToStart)
                return;

            var timeline = sender.GetTimelineProperties();
            if (!wasReset && timeline.Position.TotalSeconds < 1)
            {
                wasReset = true;
                var secondTcs = new TaskCompletionSource<MediaSession>();

                async Task DoSecondSkip()
                {
                    await skipAction(session.Source);
                    tcs.TrySetResult(await secondTcs.Task);
                }

                _ = DoSecondSkip();
            }
        }

        try
        {
            session.Source.MediaPropertiesChanged += OnMediaPropertiesChanged;
            session.Source.TimelinePropertiesChanged += OnTimelineChanged;
            await skipAction(session.Source);
            return await tcs.Task;
        }
        finally
        {
            session.Source.MediaPropertiesChanged -= OnMediaPropertiesChanged;
            session.Source.TimelinePropertiesChanged -= OnTimelineChanged;
        }
    }

    public async Task<MediaSession[]> GetActiveSessionsAsync()
    {
        var sessions = await GetSessionsInfoAsync();
        return [.. sessions.Select(s => s.ToMediaSession())];
    }

    public async Task<MediaSession?> ToggleSessionAsync(string source)
    {
        var session = await GetSession(source);
        if (session is null)
        {
            return null;
        }

        await session.Source.TryTogglePlayPauseAsync();
        session.PlaybackInfo = session.Source.GetPlaybackInfo();
        return session.ToMediaSession();
    }

    public Task<MediaSession?> NextSessionAsync(string source) =>
        SkipTrack(source, session => session.TrySkipNextAsync());

    public Task<MediaSession?> PreviousSessionAsync(string source) =>
        SkipTrack(source, session => session.TrySkipPreviousAsync(), handleResetToStart: true);
}
#endif
