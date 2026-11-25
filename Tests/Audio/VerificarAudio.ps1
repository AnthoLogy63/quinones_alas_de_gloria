param([string]$UnityEditorPath)

$ErrorActionPreference = 'Stop'
$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$scratch = Join-Path $repoRoot 'Temp\AudioValidation'

if (-not $UnityEditorPath) {
    $versionText = Get-Content -LiteralPath (Join-Path $repoRoot 'ProjectSettings\ProjectVersion.txt') -Raw
    $declaredVersion = [regex]::Match($versionText, '(?m)^m_EditorVersion: (.+)').Groups[1].Value.Trim()
    $UnityEditorPath = "C:\Program Files\Unity\Hub\Editor\$declaredVersion\Editor\Unity.exe"
}
if (-not (Test-Path -LiteralPath $UnityEditorPath -PathType Leaf)) {
    throw 'No se encontró ese Unity. Indica el ejecutable instalado con -UnityEditorPath.'
}
$validationVersion = Split-Path (Split-Path (Split-Path $UnityEditorPath -Parent) -Parent) -Leaf
New-Item -ItemType Directory -Force -Path (Join-Path $scratch 'Assets\Editor'), (Join-Path $scratch 'Packages'), (Join-Path $scratch 'ProjectSettings') | Out-Null
"m_EditorVersion: $validationVersion" | Set-Content -LiteralPath (Join-Path $scratch 'ProjectSettings\ProjectVersion.txt')
'{"dependencies":{"com.unity.ugui":"2.0.0","com.unity.modules.audio":"1.0.0","com.unity.modules.physics":"1.0.0","com.unity.modules.imgui":"1.0.0","com.unity.modules.ui":"1.0.0","com.unity.modules.animation":"1.0.0"}}' | Set-Content -LiteralPath (Join-Path $scratch 'Packages\manifest.json')

function Copy-ToValidation([string]$Source) {
    $relative = [IO.Path]::GetRelativePath($repoRoot, $Source)
    $destination = Join-Path $scratch $relative
    New-Item -ItemType Directory -Force -Path (Split-Path $destination -Parent) | Out-Null
    Copy-Item -LiteralPath $Source -Destination $destination -Force
    if (Test-Path -LiteralPath "$Source.meta") {
        Copy-Item -LiteralPath "$Source.meta" -Destination "$destination.meta" -Force
    }
}

Get-ChildItem -LiteralPath (Join-Path $repoRoot 'Assets') -Recurse -File -Filter '*.cs' | ForEach-Object { Copy-ToValidation $_.FullName }
$catalogPath = Join-Path $repoRoot 'Assets\03_Compartidos\Audio\CatalogoAudio.json'
$catalog = Get-Content -LiteralPath $catalogPath -Raw | ConvertFrom-Json
foreach ($entry in $catalog.archivos) { Copy-ToValidation (Join-Path $repoRoot $entry.ruta) }
Copy-ToValidation $catalogPath
Copy-ToValidation (Join-Path $repoRoot 'Assets\01_Interfaces\MenuPrincipal\Prefabs\AudioMenu.prefab')
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'Editor\ComprobarAudio.cs') -Destination (Join-Path $scratch 'Assets\Editor\ComprobarAudio.cs') -Force

$log = Join-Path $scratch 'validation.log'
$unityArgs = @('-batchmode', '-nographics', '-projectPath', ('"' + $scratch + '"'), '-executeMethod', 'ComprobarAudio.Run', '-logFile', ('"' + $log + '"'))
$audioValidationProcess = Start-Process -FilePath $UnityEditorPath -ArgumentList $unityArgs -WindowStyle Hidden -PassThru
$audioValidationProcess.WaitForExit()
if ($audioValidationProcess.ExitCode -ne 0 -or -not (Select-String -LiteralPath $log -Pattern 'AUDIO_VALIDATION_PASS' -Quiet)) {
    throw "Falló la validación. Consulta $log"
}
Write-Output "AUDIO_VALIDATION_PASS — Unity $validationVersion; 35 clips y controladores. Registro: $log"
