#if LINUX
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tmds.DBus;
using Mtlq.Models;

namespace Mtlq.Platform;

public struct PropertyChanges
{
    public IDictionary<string, object> Changed { get; set; }
    public string[] Invalidated { get; set; }
}

[DBusInterface("org.mpris.MediaPlayer2.Player")]
public interface IMediaPlayer2Player : IDBusObject
{
    Task PlayPauseAsync();
    Task NextAsync();
    Task PreviousAsync();
    Task<IDictionary<string, object>> GetAllAsync();

    Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
}

[DBusInterface("org.freedesktop.DBus")]
public interface IDBus : IDBusObject
{
    Task<string[]> ListNamesAsync();
}

public class LinuxController : IMediaController, IDisposable
{
    private const string MPRIS_PREFIX = "org.mpris.MediaPlayer2.";
    private readonly Lazy<Task<Connection>> _lazyConnection;
    private readonly Dictionary<string, TaskCompletionSource<MediaSession>> _stateChangeSources;
    private readonly Dictionary<string, IDisposable> _propertyWatchers;

    public LinuxController()
    {
        _lazyConnection = new Lazy<Task<Connection>>(async () =>
        {
            var connection = new Connection(Address.Session);
            await connection.ConnectAsync();
            return connection;
        });
        _stateChangeSources = [];
        _propertyWatchers = [];
    }

    private async Task<Connection> GetConnectionAsync()
    {
        try
        {
            return await _lazyConnection.Value;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to initialize DBus connection", ex);
        }
    }

    private async Task<IEnumerable<string>> GetMediaPlayerNamesAsync()
    {
        var connection = await GetConnectionAsync();
        var dbus = connection.CreateProxy<IDBus>("org.freedesktop.DBus", "/org/freedesktop/DBus");
        var names = await dbus.ListNamesAsync();
        return names.Where(name => name.StartsWith(MPRIS_PREFIX));
    }

    private static string GetMetadataValue(IDictionary<string, object> properties, string key)
    {
        if (
            !properties.TryGetValue("Metadata", out var value)
            || value is not IDictionary<string, object> metadata
            || !metadata.TryGetValue(key, out var metadataValue)
        )
        {
            return string.Empty;
        }

        return metadataValue switch
        {
            string[] array when array.Length > 0 => array[0],
            var val => val?.ToString() ?? string.Empty,
        };
    }

    private static PlaybackStatus MapPlaybackStatus(string status) =>
        (status?.ToLower()) switch
        {
            "playing" => PlaybackStatus.Playing,
            "paused" => PlaybackStatus.Paused,
            "stopped" => PlaybackStatus.Stopped,
            _ => PlaybackStatus.Unknown,
        };

    private async Task<MediaSession> GetSessionInfoAsync(string playerName)
    {
        var connection = await GetConnectionAsync();
        var player = connection.CreateProxy<IMediaPlayer2Player>(
            playerName,
            "/org/mpris/MediaPlayer2"
        );

        var metadata = await player.GetAllAsync();
        metadata.TryGetValue("PlaybackStatus", out var status);
        var playbackStatus = status as string;
        return new MediaSession
        {
            Source = playerName[MPRIS_PREFIX.Length..],
            Title = GetMetadataValue(metadata, "xesam:title"),
            Artist = GetMetadataValue(metadata, "xesam:artist"),
            CurrentTime = string.Empty,
            TotalTime = string.Empty,
            Status = MapPlaybackStatus(playbackStatus),
        };
    }

    private async Task<(string playerName, IMediaPlayer2Player player)> GetPlayerAsync(
        string source
    )
    {
        var players = await GetMediaPlayerNamesAsync();
        var playerName = string.IsNullOrEmpty(source)
            ? players.FirstOrDefault()
            : players.FirstOrDefault(p =>
                p[MPRIS_PREFIX.Length..].Equals(source, StringComparison.OrdinalIgnoreCase)
            );

        if (playerName == null)
        {
            throw new InvalidOperationException(
                $"No media player found{(string.IsNullOrEmpty(source) ? "" : $" for source: {source}")}"
            );
        }

        var connection = await GetConnectionAsync();
        return (
            playerName,
            connection.CreateProxy<IMediaPlayer2Player>(playerName, "/org/mpris/MediaPlayer2")
        );
    }

    public async Task<MediaSession[]> GetActiveSessionsAsync()
    {
        var players = await GetMediaPlayerNamesAsync();
        return await Task.WhenAll(players.Select(GetSessionInfoAsync));
    }

    private async Task<MediaSession?> WaitForStateChangeAsync(
        string source,
        Func<IMediaPlayer2Player, Task> action
    )
    {
        var (playerName, player) = await GetPlayerAsync(source);
        var tcs = new TaskCompletionSource<MediaSession>();

        _stateChangeSources[playerName] = tcs;

        if (_propertyWatchers.TryGetValue(playerName, out var existingWatcher))
        {
            existingWatcher.Dispose();
        }

        _propertyWatchers[playerName] = await player.WatchPropertiesAsync(async changes =>
        {
            if (_stateChangeSources.TryGetValue(playerName, out var source))
            {
                var newState = await GetSessionInfoAsync(playerName);
                source.TrySetResult(newState);
                _stateChangeSources.Remove(playerName);
            }
        });

        await action(player);

        try
        {
            return await tcs.Task;
        }
        finally
        {
            if (_propertyWatchers.TryGetValue(playerName, out var watcher))
            {
                watcher.Dispose();
                _propertyWatchers.Remove(playerName);
            }
            _stateChangeSources.Remove(playerName);
        }
    }

    public Task<MediaSession?> TogglePlaySession(string source) =>
        WaitForStateChangeAsync(source, player => player.PlayPauseAsync());

    public Task<MediaSession?> NextSession(string source) =>
        WaitForStateChangeAsync(source, player => player.NextAsync());

    public Task<MediaSession?> PreviousSession(string source) =>
        WaitForStateChangeAsync(source, player => player.PreviousAsync());

    public async void Dispose()
    {
        foreach (var watcher in _propertyWatchers.Values)
        {
            watcher.Dispose();
        }
        _propertyWatchers.Clear();

        if (_lazyConnection.IsValueCreated)
        {
            var connection = await _lazyConnection.Value;
            connection.Dispose();
        }
    }
}
#endif
