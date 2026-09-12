param([string]$BaseUrl = 'https://yumdash.ansenherrick.com')

$ErrorActionPreference = 'Stop'
if (-not $env:YUMDASH_SMOKE_EMAIL -or -not $env:YUMDASH_SMOKE_PASSWORD) {
    throw 'Set YUMDASH_SMOKE_EMAIL and YUMDASH_SMOKE_PASSWORD to an existing admin account.'
}

$session = [Microsoft.PowerShell.Commands.WebRequestSession]::new()
$loginUrl = "$BaseUrl/Account/Login?ReturnUrl=%2FAdmin"
$login = Invoke-WebRequest $loginUrl -WebSession $session -TimeoutSec 30
$tokenMatch = [regex]::Match($login.Content, 'name="__RequestVerificationToken"[^>]*value="([^"]+)"')
if (-not $tokenMatch.Success) { throw 'Login form did not contain an antiforgery token.' }

$response = Invoke-WebRequest $loginUrl -Method Post -WebSession $session -TimeoutSec 30 -SkipHttpErrorCheck -Body @{
    Email = $env:YUMDASH_SMOKE_EMAIL
    Password = $env:YUMDASH_SMOKE_PASSWORD
    RememberMe = 'false'
    __RequestVerificationToken = [System.Net.WebUtility]::HtmlDecode($tokenMatch.Groups[1].Value)
}
if ($response.StatusCode -ne 200 -or $response.BaseResponse.RequestMessage.RequestUri.AbsolutePath -notmatch '^/Admin/?$') {
    throw "Admin login/dashboard failed: HTTP $($response.StatusCode), path $($response.BaseResponse.RequestMessage.RequestUri.AbsolutePath)."
}
Write-Output 'PASS: admin login and dashboard render successfully.'

foreach ($exportPath in @('/Admin/Reservations/ExportWeekly', '/Admin/Reservations/ExportWeekly?startDate=2026-09-11')) {
    $export = Invoke-WebRequest "$BaseUrl$exportPath" -WebSession $session -TimeoutSec 30 -SkipHttpErrorCheck
    if ($export.StatusCode -ne 200 -or [string]$export.Headers['Content-Type'] -notlike 'text/csv*') {
        throw "Weekly export failed: HTTP $($export.StatusCode)."
    }
    Write-Output "PASS: $exportPath returns CSV."
}
