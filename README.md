# mtlq

> 🎵 A cross-platform CLI providing JSON output for controlling media sessions

`mtlq` works with native platform APIs to control media playback (e.g. Spotify, YouTube in browsers).

- **Windows: System Media Transport Controls (SMTC)**  
  SMTC is the system Windows uses to handle media controls, like play, pause, and skip, through the volume overlay and media keys.

- **Linux: D-Bus + MPRIS2**  
  D-Bus is a system that lets applications communicate with each other, and MPRIS2 is a standard built on top of it to control media players and access their metadata.

## 🚀 Usage

```bash
$ mtlq now | jq
[
  {
    "source": "spotify",
    "title": "squabble up",
    "artist": "Kendrick Lamar",
    "currentTime": "00:00:53.813000",
    "totalTime": "00:02:37.992000",
    "status": 1
  }
]
$ mtlq next session spotify | jq
{
  "source": "spotify",
  "title": "Not Like Us",
  "artist": "Kendrick Lamar",
  "currentTime": "00:00:00.136000",
  "totalTime": "00:04:34.192000",
  "status": 1
}
$ mtlq next session spotify | jq
{
  "source": "spotify",
  "title": "All The Stars (with SZA)",
  "artist": "Kendrick Lamar",
  "currentTime": "00:00:00.096000",
  "totalTime": "00:03:52.186000",
  "status": 1
}
$ mtlq previous session spotify | jq
{
  "source": "spotify",
  "title": "Not Like Us",
  "artist": "Kendrick Lamar",
  "currentTime": "00:00:00.095000",
  "totalTime": "00:04:34.192000",
  "status": 1
}
```

## ✨ Installation

### Windows
#### Install
```powershell
powershell -ExecutionPolicy ByPass -c "irm https://raw.githubusercontent.com/drzbida/mtlq/main/scripts/install.ps1 | iex"
```
#### Uninstall
```powershell
powershell -ExecutionPolicy ByPass -c "irm https://raw.githubusercontent.com/drzbida/mtlq/main/scripts/uninstall.ps1 | iex"
```

### Linux
#### Install
```bash
curl -sSL https://raw.githubusercontent.com/drzbida/mtlq/main/scripts/install.sh | bash
```
#### Uninstall
```bash
curl -sSL https://raw.githubusercontent.com/drzbida/mtlq/main/scripts/uninstall.sh | bash
```

Alternatively, you can download the files manually from the Releases page.

## ⚡ Compatibility

Works with modern applications that integrate with the operating system's media controls, for example:
- Web browsers (e.g. Chrome, Firefox, Edge)
- Music players (e.g. Spotify)

⚠️ Output quality depends on the application's implementation. For example, YouTube + Firefox on Linux does not expose total track duration. In cases like this, the field will be an empty string.

### Windows
Requires Windows 10 version 1809 or later.

### Linux
- ⚠️ Snap packages may not work due to their sandboxed environment limiting D-Bus access
- ⚠️ Flatpak apps might require additional permissions (unverified)

## 📸 Integration Examples

Wezterm integration and right status display

![image](https://github.com/user-attachments/assets/275bdc78-b836-4040-8537-26f92e01a669)

