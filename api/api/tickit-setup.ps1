# Manage-Application.ps1

param (
    [Parameter(Mandatory = $true)]
    [ValidateSet("set-creds", "run")]
    [string]$Action
)

function Load-DotEnv {
    param (
        [string]$Path
    )

    if (Test-Path $Path) {
        Get-Content $Path | ForEach-Object {
            if ($_ -match '^\s*([^#][^=]*)\s*=\s*(.*)\s*$') {
                $name = $matches[1].Trim()
                $value = $matches[2].Trim()
                [System.Environment]::SetEnvironmentVariable($name, $value, [System.EnvironmentVariableTarget]::Process)
            }
        }
    } else {
        Write-Error ".env file not found at path: $Path"
        exit 1
    }
}

$envFilePath = ".env"
Load-DotEnv -Path $envFilePath

switch ($Action) {
    "set-creds" {
        if (-not (Test-Path -Path ".\\Properties\\secrets.json")) {
            dotnet user-secrets init
        }

        dotnet user-secrets set "GitHub:ClientId" $env:GITHUB_CLIENT_ID
        dotnet user-secrets set "GitHub:ClientSecret" $env:GITHUB_CLIENT_SECRET
        dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=$env:DBSERVER;Database=$env:DBNAME;User Id=$env:DBUSERID;Password=$env:DBPASSWORD;TrustServerCertificate=True"
    }
    "run" {
        dotnet clean
        dotnet build
        dotnet run
    }
    default {
        Write-Error "Invalid action. Use 'set-creds' or 'run'."
        exit 1
    }
}
