$app_name = 'mtlq'
$github_repo = "drzbida/mtlq"
$InformationPreference = "Continue"

function Install-Binary {
    Write-Information "Checking latest mtlq release..."
    $wc = New-Object Net.WebClient
    $wc.Headers.Add("User-Agent", "mtlq-installer")
    $latest = $wc.DownloadString("https://api.github.com/repos/$github_repo/releases/latest") | 
        ConvertFrom-Json
    $version = $latest.tag_name -replace '^v',''
    Write-Information "Found version $version"
    
    $download_url = "https://github.com/$github_repo/releases/download/v$version/mtlq_${version}_win-x64.zip"
    Write-Information "Release URL: $download_url"
    
    $tmp = New-Item -ItemType Directory -Path (Join-Path ([System.IO.Path]::GetTempPath()) ([System.Guid]::NewGuid()))
    $zip_path = Join-Path $tmp "mtlq.zip"

    Write-Information "Downloading mtlq v$version for Windows x64..."
    $wc.downloadFile($download_url, $zip_path)
    Write-Information "Download complete, extracting..."
    Expand-Archive -Path $zip_path -DestinationPath $tmp
    
    $dest_dir = if ($env:XDG_BIN_HOME) {
        $env:XDG_BIN_HOME
    } elseif ($env:XDG_DATA_HOME) {
        Join-Path $env:XDG_DATA_HOME "../bin"
    } else {
        Join-Path $HOME ".local/bin"
    }

    $dest_dir = New-Item -Force -ItemType Directory -Path $dest_dir
    Write-Information "Installing mtlq to: $dest_dir"

    Copy-Item "$tmp\mtlq.exe" -Destination "$dest_dir" -Force
    Write-Information "Installed mtlq.exe v$version"

    Remove-Item $tmp -Recurse -Force

    $RegistryPath = "HKCU:\Environment"
    $OldPath = (Get-Item -Path $RegistryPath).GetValue("Path", "", "DoNotExpandEnvironmentNames")
    
    if (";$OldPath;" -notlike "*;$dest_dir;*") {
        $NewPath = "$dest_dir;$OldPath"
        New-ItemProperty -Path $RegistryPath -Name "Path" -Value $NewPath -PropertyType String -Force | Out-Null
        Write-Information "Added installation directory to PATH"
        Write-Information ""
        Write-Information "To start using mtlq immediately, run:"
        Write-Information "    $dest_dir\mtlq.exe"
        Write-Information ""
        Write-Information "Or restart your terminal to have 'mtlq' available in PATH"
    } else {
        Write-Information "Installation directory already in PATH"
    }
    Write-Information ""
    Write-Information "Installation complete! Try running: mtlq --help"
}

try {
    Install-Binary
} catch {
    Write-Information "Installation failed: $_"
    exit 1
}
