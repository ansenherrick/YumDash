param(
    [string]$BaseUrl = 'https://yumdash.ansenherrick.com',
    [string]$Docker = 'docker'
)

$ErrorActionPreference = 'Stop'
$testEmail = 'reservation-smoke-' + [guid]::NewGuid().ToString('N') + '@example.invalid'
$session = [Microsoft.PowerShell.Commands.WebRequestSession]::new()
$formUrl = "$BaseUrl/Reservations/Create"

function Invoke-TestSql([string]$Sql) {
    $result = $Sql | & $Docker exec -i personal-desktop-services-postgres-1 psql -U appuser -d yumdash -v ON_ERROR_STOP=1 -v "test_email=$testEmail" -t -A
    if ($LASTEXITCODE -ne 0) { throw 'Reservation test database operation failed.' }
    return $result
}

function Submit-Reservation([hashtable]$Changes = @{}, [switch]$WithoutToken) {
    $page = Invoke-WebRequest $formUrl -WebSession $session -TimeoutSec 30
    $token = [regex]::Match($page.Content, 'name="__RequestVerificationToken"[^>]*value="([^"]+)"').Groups[1].Value
    if (-not $token) { throw 'Reservation form has no antiforgery token.' }
    $body = @{
        GuestName = 'Disposable reservation smoke test'
        Email = $testEmail
        Phone = '202-555-0100'
        ReservationDate = '2026-09-20T19:00'
        PartySize = '2'
        EstimatedSpendPerGuest = '35'
        Notes = ''
        __RequestVerificationToken = [System.Net.WebUtility]::HtmlDecode($token)
    }
    foreach ($key in $Changes.Keys) { $body[$key] = $Changes[$key] }
    if ($WithoutToken) { $body.Remove('__RequestVerificationToken') }
    return Invoke-WebRequest $formUrl -Method Post -WebSession $session -Body $body -TimeoutSec 30 -SkipHttpErrorCheck
}

try {
    $invalid = Submit-Reservation @{ PartySize = '0'; ReservationDate = '' }
    if ($invalid.StatusCode -ne 200 -or $invalid.Content -notmatch 'Choose a reservation date and time\.' -or $invalid.Content -notmatch 'Party Size must be between 1 and 24') {
        throw 'Invalid reservation fields did not show useful validation errors.'
    }
    $count = Invoke-TestSql 'SELECT count(*) FROM "Reservations" WHERE "Email" = :''test_email'';'
    if ([int]$count -ne 0) { throw 'Invalid reservation was saved.' }
    Write-Output 'PASS: invalid fields show errors without creating a reservation.'

    $noToken = Submit-Reservation -WithoutToken
    if ($noToken.StatusCode -ne 400) { throw 'Reservation submission bypassed antiforgery protection.' }
    Write-Output 'PASS: antiforgery protection remains enabled.'

    foreach ($notes in @('', 'Window seat requested')) {
        # Submitted server-managed values must be ignored by the form model.
        $created = Submit-Reservation @{ Notes = $notes; Status = 'Confirmed'; CreatedAt = '2000-01-01'; Id = '-99' }
        if ($created.StatusCode -ne 200 -or $created.Content -notmatch 'Reservation request sent\.') {
            throw "Valid reservation did not show success: HTTP $($created.StatusCode)."
        }
    }
    $json = Invoke-TestSql @'
SELECT json_build_object(
    'count', count(*),
    'pending', bool_and("Status" = 1),
    'generatedIds', bool_and("Id" > 0),
    'recent', bool_and("CreatedAt" > now() - interval '5 minutes'),
    'blankNotes', count(*) FILTER (WHERE "Notes" = ''),
    'filledNotes', count(*) FILTER (WHERE "Notes" = 'Window seat requested'),
    'validDate', bool_and("ReservationDate" > '2026-09-20'::timestamptz AND "ReservationDate" < '2026-09-22'::timestamptz)
) FROM "Reservations" WHERE "Email" = :'test_email';
'@
    $stored = $json | ConvertFrom-Json
    if ($stored.count -ne 2 -or -not $stored.pending -or -not $stored.generatedIds -or -not $stored.recent -or $stored.blankNotes -ne 1 -or $stored.filledNotes -ne 1 -or -not $stored.validDate) {
        throw 'Saved reservations did not match the submitted values and server-managed defaults.'
    }
    Write-Output 'PASS: reservations with and without Notes persist with valid dates and Pending status.'

    if ($env:YUMDASH_SMOKE_EMAIL -and $env:YUMDASH_SMOKE_PASSWORD) {
        $loginUrl = "$BaseUrl/Account/Login?ReturnUrl=%2FAdmin%2FReservations"
        $login = Invoke-WebRequest $loginUrl -WebSession $session -TimeoutSec 30
        $token = [regex]::Match($login.Content, 'name="__RequestVerificationToken"[^>]*value="([^"]+)"').Groups[1].Value
        $admin = Invoke-WebRequest $loginUrl -Method Post -WebSession $session -TimeoutSec 30 -SkipHttpErrorCheck -Body @{
            Email = $env:YUMDASH_SMOKE_EMAIL
            Password = $env:YUMDASH_SMOKE_PASSWORD
            __RequestVerificationToken = [System.Net.WebUtility]::HtmlDecode($token)
        }
        if ($admin.StatusCode -ne 200 -or $admin.Content -notmatch [regex]::Escape($testEmail)) {
            throw 'Created reservations did not appear in the admin reservation list.'
        }
        Write-Output 'PASS: created reservations appear in the authenticated admin list.'
    }
} finally {
    # Delete only the uniquely named reservations created by this test run.
    $null = Invoke-TestSql 'DELETE FROM "Reservations" WHERE "Email" = :''test_email'';'
}
