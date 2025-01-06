# mtlq

> 🎵 A cross-platform CLI providing JSON output for controlling media sessions

`mtlq` works with native platform APIs to control media playback (e.g. Spotify, YouTube in browsers).

- **Windows: System Media Transport Controls (SMTC)**  
  SMTC is the system Windows uses to handle media controls, like play, pause, and skip, through the volume overlay and media keys.

- **Linux: D-Bus + MPRIS2**  
  D-Bus is a system that lets applications communicate with each other, and MPRIS2 is a standard built on top of it to control media players and access their metadata.


## ✨ Installation

### Windows
```powershell
# Get latest version and install directory
$version = (Invoke-RestMethod "https://api.github.com/repos/drzbida/mtlq/releases/latest").tag_name.Substring(1)
$installDir = "$env:LOCALAPPDATA\Programs\mtlq"
New-Item -ItemType Directory -Force -Path $installDir

# Download and extract
Invoke-WebRequest -Uri "https://github.com/drzbida/mtlq/releases/download/v${version}/mtlq_${version}_win-x64.zip" -OutFile "mtlq.zip"
Expand-Archive mtlq.zip -DestinationPath $installDir -Force

# Add to PATH (current session)
$env:Path += ";$installDir"

# Add to PATH (permanent)
[Environment]::SetEnvironmentVariable(
    "Path",
    [Environment]::GetEnvironmentVariable("Path", "User") + ";$installDir",
    "User"
)
```

### Linux
```bash
MTLQ_VERSION=$(curl -s "https://api.github.com/repos/drzbida/mtlq/releases/latest" | grep -Po '"tag_name": *"v\K[^"]*')
curl -Lo mtlq.tar.gz "https://github.com/drzbida/mtlq/releases/download/v${MTLQ_VERSION}/mtlq_${MTLQ_VERSION}_linux-x64.tar.gz"
tar xf mtlq.tar.gz -C ~/.local/bin
chmod +x ~/.local/bin/mtlq
```

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

