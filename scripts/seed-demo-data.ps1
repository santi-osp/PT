param(
    [string]$ApiBase = "http://localhost:5124/api"
)

$ErrorActionPreference = "Stop"
$api = $ApiBase.TrimEnd('/')

function Get-NitDv([string]$Number) {
    $weights = @(41, 37, 29, 23, 19, 17, 13, 7, 3)
    $sum = 0
    for ($index = 0; $index -lt 9; $index++) {
        $sum += ([int]::Parse($Number[$index])) * $weights[$index]
    }
    $remainder = $sum % 11
    if ($remainder -in @(0, 1)) { return "$remainder" }
    return "$(11 - $remainder)"
}

$neighborhoods = Invoke-RestMethod "$api/catalogs/neighborhoods"
$centers = Invoke-RestMethod "$api/catalogs/centers"
$records = @(
    @{ treatment="Sr"; businessName="Café Horizonte"; legalName="Juan Pérez Gómez"; documentType="CC"; document="710000101"; mobile="3007100101"; email=$null; stratum=3; neighborhood="Aranjuez"; center="MDE-C013"; blocked=$false },
    @{ treatment="Empresa"; businessName="Mercado Aranjuez S.A.S."; legalName="Mercado Aranjuez S.A.S."; documentType="NIT"; document="830910001"; mobile="3007100102"; email="contacto@mercadoaranjuez.example"; stratum=3; neighborhood="Aranjuez"; center="MDE-C013"; blocked=$false },
    @{ treatment="Sra"; businessName="Taller La 52"; legalName="Ana María Pérez-Gómez"; documentType="CC"; document="710000103"; mobile="3007100103"; email=$null; stratum=2; neighborhood="Aranjuez"; center="MDE-C001"; blocked=$true },
    @{ treatment="Sra"; businessName="Estudio Abril"; legalName="María José De la Hoz Pérez"; documentType="CC"; document="710000104"; mobile="3007100104"; email="abril@example.test"; stratum=5; neighborhood="Laureles"; center="MDE-C006"; blocked=$false },
    @{ treatment="Sr"; businessName="Bici Laureles"; legalName="José Luis O'Connor Pérez"; documentType="CE"; document="720000105"; mobile="3007100105"; email=$null; stratum=4; neighborhood="Laureles"; center="MDE-C006"; blocked=$false },
    @{ treatment="Empresa"; businessName="J&M Comercial S.A.S."; legalName="J&M Comercial S.A.S."; documentType="NIT"; document="830910006"; mobile="3007100106"; email="ventas@jmcomercial.example"; stratum=6; neighborhood="El Poblado"; center="MDE-C007"; blocked=$false },
    @{ treatment="Sra"; businessName="Panadería Rosales"; legalName="Laura Gómez Ríos"; documentType="CC"; document="710000107"; mobile="3007100107"; email=$null; stratum=4; neighborhood="Belén"; center="MDE-C008"; blocked=$false },
    @{ treatment="Sr"; businessName="Maderas Robledo"; legalName="Carlos Mejía López"; documentType="CC"; document="710000108"; mobile="3007100108"; email=$null; stratum=2; neighborhood="Robledo"; center="MDE-C011"; blocked=$false },
    @{ treatment="Sra"; businessName="Librería Oriente"; legalName="Sofía Torres Ruiz"; documentType="CC"; document="710000109"; mobile="3007100109"; email=$null; stratum=3; neighborhood="Buenos Aires"; center="MDE-C014"; blocked=$false },
    @{ treatment="Empresa"; businessName="Tienda 24/7 S.A.S."; legalName="Tienda 24/7 S.A.S."; documentType="NIT"; document="830910010"; mobile="3007100110"; email="datos@tienda247.example"; stratum=5; neighborhood="El Poblado"; center="MDE-C007"; blocked=$true },
    @{ treatment="Sr"; businessName="Servicios Belén"; legalName="Miguel Ángel Cano Díaz"; documentType="CC"; document="710000111"; mobile="3007100111"; email=$null; stratum=3; neighborhood="Belén"; center="MDE-C008"; blocked=$true }
)

$created = 0
$retired = 0
$existing = 0
foreach ($record in $records) {
    $matches = Invoke-RestMethod "$api/residential-customers?search=$($record.document)&status=All"
    $customer = $matches | Where-Object documentNumber -eq $record.document | Select-Object -First 1
    if (-not $customer) {
        $neighborhood = $neighborhoods | Where-Object name -eq $record.neighborhood | Select-Object -First 1
        $center = $centers | Where-Object code -eq $record.center | Select-Object -First 1
        if (-not $neighborhood -or -not $center) { throw "Catálogo faltante para $($record.document)." }
        $body = @{
            treatment = $record.treatment
            businessName = $record.businessName
            extendedLegalName = $record.legalName
            phone = $null
            phoneExtension = $null
            mobilePhone = $record.mobile
            email = $record.email
            documentType = $record.documentType
            documentNumber = $record.document
            verificationDigit = if ($record.documentType -eq "NIT") { Get-NitDv $record.document } else { $null }
            stratum = $record.stratum
            centerId = $center.id
            address = @{
                neighborhoodId = $neighborhood.id
                isRural = $true
                ruralAddress = "Dirección ficticia demo $($record.neighborhood)"
                mainRoadType = $null; mainRoadNumber = $null; mainRoadLetter = $null; mainRoadCardinality = $null
                secondaryRoadNumber1 = $null; secondaryRoadLetter = $null; secondaryRoadCardinality1 = $null
                secondaryRoadNumber2 = $null; secondaryRoadCardinality2 = $null
            }
        } | ConvertTo-Json -Depth 5
        $customer = Invoke-RestMethod "$api/residential-customers" -Method Post -ContentType "application/json" -Body $body
        $created++
    } else { $existing++ }

    if ($record.blocked -and -not $customer.isBlocked) {
        $customer = Invoke-RestMethod "$api/residential-customers/$($customer.id)/retire" -Method Patch -ContentType "application/json" -Body "{}"
        $retired++
    }
}

Write-Output "Demo seed complete: created=$created existing=$existing retired=$retired total=$($records.Count)"
