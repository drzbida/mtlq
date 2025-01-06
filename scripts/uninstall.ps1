$InformationPreference = "Continue"

function Uninstall-Mtlq {
    $install_dir = if ($env:XDG_BIN_HOME) {
        $env:XDG_BIN_HOME
    } elseif ($env:XDG_DATA_HOME) {
        Join-Path $env:XDG_DATA_HOME "../bin"
    } else {
        Join-Path $HOME ".local/bin"
    }

    $exe_path = Join-Path $install_dir "mtlq.exe"
    
    if (Test-Path $exe_path) {
        Write-Information "Removing mtlq from $install_dir"
        Remove-Item $exe_path -Force
        Write-Information "Removed mtlq.exe"

        $RegistryPath = "HKCU:\Environment"
        $OldPath = (Get-Item -Path $RegistryPath).GetValue("Path", "", "DoNotExpandEnvironmentNames")
        
        if (";$OldPath;" -like "*;$install_dir;*") {
            $NewPath = ($OldPath.Split(';') | Where-Object { $_ -ne $install_dir }) -join ';'
            New-ItemProperty -Path $RegistryPath -Name "Path" -Value $NewPath -PropertyType String -Force | Out-Null
            Write-Information "Removed installation directory from PATH"
        }
        
        Write-Information ""
        Write-Information "Uninstallation complete!"
    } else {
        Write-Information "mtlq is not installed in $install_dir"
    }
}

try {
    Uninstall-Mtlq
} catch {
    Write-Information "Uninstallation failed: $_"
    exit 1
}
