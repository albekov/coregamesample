function Exec
{
    [CmdletBinding()]
    param(
        [Parameter(Position=0,Mandatory=1)][scriptblock]$cmd,
        [Parameter(Position=1,Mandatory=0)][string]$errorMessage = ($msgs.error_bad_command -f $cmd)
    )
    & $cmd
    if ($lastexitcode -ne 0) {
        throw ("Exec: " + $errorMessage)
    }
}

if(Test-Path .\release) { Remove-Item .\release -Force -Recurse }

exec { & dotnet restore .\src\Game }
exec { & dotnet build .\src\Game }

$release = Join-Path $pwd release
exec { & dotnet publish .\src\Game -c Release -o $release }
