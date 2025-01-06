#if LINUX
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tmds.DBus;
using Mtlq.Models;

namespace Mtlq.Platform;

[DBusInterface("org.mpris.MediaPlayer2.Player")]
public interface IMediaPlayer2Player : IDBusObject
{
    Task PlayPauseAsync();
    Task NextAsync();
    Task PreviousAsync();
    Task<IDictionary<string, object>> GetAllAsync();
}

[DBusInterface("org.freedesktop.DBus")]
public interface IDBus : IDBusObject
{
    Task<string[]> ListNamesAsync();
}

public class LinuxController : IMediaController
{
    private const string MPRIS_PREFIX = "org.mpris.MediaPlayer2.";
    private readonly Connection _connection;

    public LinuxController()
    {
        _connection = new Connection(Address.Session);
        _connection.ConnectAsync().GetAwaiter().GetResult();
    }

    private async Task<IEnumerable<string>> GetMediaPlayerNamesAsync()
    {
        var dbus = _connection.CreateProxy<IDBus>("org.freedesktop.DBus", "/org/freedesktop/DBus");
        var names = await dbus.ListNamesAsync();
        return names.Where(name => name.StartsWith(MPRIS_PREFIX));
    }

    private async Task<MediaSession> GetSessionInfoAsync(string playerName)
    {
        var player = _connection.CreateProxy<IMediaPlayer2Player>(
            playerName,
            "/org/mpris/MediaPlayer2"
        );

        var metadata = await player.GetAllAsync();
        string playbackStatus = null;
        if (metadata.TryGetValue("PlaybackStatus", out var status))
        {
            playbackStatus = status as string;
        }

        return new MediaSession
        {
            Source = playerName.Replace(MPRIS_PREFIX, string.Empty),
            Title = GetMetadataValue(metadata, "Metadata", "xesam:title"),
            Artist = GetMetadataValue(metadata, "Metadata", "xesam:artist"),
            CurrentTime = "",
            TotalTime = "",
            Status = MapPlaybackStatus(playbackStatus),
        };
    }

    private static string GetMetadataValue(
        IDictionary<string, object> properties,
        string propName,
        string key = ""
    )
    {
        if (properties.TryGetValue(propName, out var value))
        {
            if (!string.IsNullOrEmpty(key) && value is IDictionary<string, object> metadata)
            {
                if (metadata.TryGetValue(key, out var metadataValue))
                {
                    if (metadataValue is string[] array && array.Length > 0)
                        return array[0];
                    return metadataValue?.ToString() ?? string.Empty;
                }
                return string.Empty;
            }
            return value?.ToString() ?? string.Empty;
        }
        return string.Empty;
    }

    private static PlaybackStatus MapPlaybackStatus(string status) =>
        (status ?? string.Empty).ToLower() switch
        {
            "playing" => PlaybackStatus.Playing,
            "paused" => PlaybackStatus.Paused,
            "stopped" => PlaybackStatus.Stopped,
            _ => PlaybackStatus.Unknown,
        };

    private async Task<(string playerName, IMediaPlayer2Player player)> GetPlayerAsync(
        string source
    )
    {
        var players = await GetMediaPlayerNamesAsync();
        string playerName;

        if (string.IsNullOrEmpty(source))
        {
            playerName = players.FirstOrDefault() ?? string.Empty;
        }
        else
        {
            playerName =
                players.FirstOrDefault(p =>
                    p.Replace(MPRIS_PREFIX, string.Empty)
                        .Equals(source, StringComparison.OrdinalIgnoreCase)
                ) ?? string.Empty;
        }

        return (
            playerName,
            _connection.CreateProxy<IMediaPlayer2Player>(playerName, "/org/mpris/MediaPlayer2")
        );
    }

    public async Task<MediaSession[]> GetActiveSessionsAsync()
    {
        var players = await GetMediaPlayerNamesAsync();
        var tasks = players.Select(p => GetSessionInfoAsync(p));
        var sessions = await Task.WhenAll(tasks);
        return sessions;
    }

    private async Task<MediaSession?> WaitForStateChangeAsync(
        string source,
        Func<IMediaPlayer2Player, Task> action
    )
    {
        var (playerName, player) = await GetPlayerAsync(source);
        if (player == null)
            return null;

        var initialState = await GetSessionInfoAsync(playerName);
        await action(player);

        int[] delays = { 15, 30, 55 };

        foreach (var delay in delays)
        {
            await Task.Delay(delay);
            var currentState = await GetSessionInfoAsync(playerName);

            if (
                !string.Equals(currentState.Title, initialState.Title)
                || !string.Equals(currentState.Artist, initialState.Artist)
                || currentState.Status != initialState.Status
            )
            {
                return currentState;
            }
        }

        return await GetSessionInfoAsync(playerName);
    }

    public async Task<MediaSession?> TogglePlaySession(string source) =>
        await WaitForStateChangeAsync(source, player => player.PlayPauseAsync());

    public async Task<MediaSession?> NextSession(string source) =>
        await WaitForStateChangeAsync(source, player => player.NextAsync());

    public async Task<MediaSession?> PreviousSession(string source) =>
        await WaitForStateChangeAsync(source, player => player.PreviousAsync());
}
#endif
