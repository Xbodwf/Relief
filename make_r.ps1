# �л����ű�����Ŀ¼
Set-Location $PSScriptRoot

# ����
cls

# ��ȡ�����ļ�����
$version = (Get-Content "VERSION.txt" -Raw).Trim()
$adofaipath = (Get-Content "ADOFAIPath.txt" -Raw).Trim()
$modname = (Get-Content "MODNAME.txt" -Raw).Trim()
$dllpath = (Get-Content "DLL.txt" -Raw).Trim()

# ������ʱĿ¼�ṹ
New-Item -ItemType Directory -Path "tmp" -Force | Out-Null
Set-Location "tmp"
New-Item -ItemType Directory -Path $dllpath -Force | Out-Null

# ���ƻ����ļ�
Copy-Item "..\Info.json" -Destination $dllpath -Force

# ����Mod�ļ�
$sourceDir = "..\ScriptExecuter\$modname\bin\Debug"
$destDir = $dllpath

# ���ԴĿ¼�Ƿ����
if (-not (Test-Path $sourceDir)) {
    Write-Host "sourceDir is not defined"
    exit
}

# ����ԴĿ¼�µ������ļ���Ŀ��Ŀ¼
Get-ChildItem -Path $sourceDir -File | ForEach-Object {
    Copy-Item $_.FullName -Destination $destDir -Force
    Write-Host "[Copied] $($_.Name)"
}

# ����Info.json�еİ汾�滻
Set-Location $dllpath
$content = Get-Content "Info.json" -Raw
$updatedContent = $content -replace '\$VERSION', $version
Set-Content "InfoChanged.json" -Value $updatedContent -Force

# �滻ԭʼInfo.json
Remove-Item "Info.json" -Force
Rename-Item "InfoChanged.json" -NewName "Info.json" -Force

# ����tmpĿ¼
Set-Location ".."

# ����ZIPѹ����
$zipName = "$dllpath-$version.zip"
Compress-Archive -Path $dllpath -DestinationPath $zipName -Force

# ���Ƶ�ADOfAI��ModsĿ¼
$targetPath = "$adofaipath\Mods\$dllpath\"
New-Item -ItemType Directory -Path $targetPath -Force | Out-Null
Copy-Item -Path "$dllpath\*" -Destination $targetPath -Recurse -Force -ErrorAction SilentlyContinue

# �ƶ�ZIP�����ϲ�Ŀ¼
Move-Item -Path $zipName -Destination ".." -Force

# ���ؽű���Ŀ¼��������ʱ�ļ�
Set-Location ".."
Remove-Item -Path "tmp" -Recurse -Force -ErrorAction SilentlyContinue

# ��ͣ�ȴ��û�ȷ��
Read-Host -Prompt "������ɣ���������˳�"

