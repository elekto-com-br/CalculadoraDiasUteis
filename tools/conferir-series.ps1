<#
.SYNOPSIS
    Confere um calendário dos vizinhos contra a série diária do banco central do país.

.DESCRIPTION
    O banco central só publica a série em dia útil bancário: um dia de semana sem dado é um dia em que os bancos não
    funcionaram. O script baixa a série do período, compara com o arquivo do calendário e lista as diferenças nos dois
    sentidos. Serve na revisão de dezembro para achar o fechamento que nenhuma regra prevê (greve, asueto, feriado
    extraordinário) e o feriado do arquivo que não aconteceu.

    - ar-BA: API de estatísticas do BCRA, v4.0. Dia fechado é o que não tem a BADLAR de bancos privados (variável 7)
      nem a taxa de depósitos a 30 dias (variável 12).
    - uy-MVD: cotações do BCU, dólar interbancário "DLS. USA BILLETE" (moeda 2225).

    O py-ASU não tem série diária conferível: o BCP não publica uma em formato aberto. A revisão dele é pelos decretos.

    As diferenças não são erros por si: as já conhecidas estão explicadas nos documentos de docs/calendarios. O BCU,
    por exemplo, publica a cotação de fechamento de 31 de dezembro sem operar.

.PARAMETER Calendar
    O código do calendário: ar-BA ou uy-MVD.

.PARAMETER From
    O primeiro dia conferido. O padrão é 1º de janeiro do ano corrente.

.PARAMETER To
    O último dia conferido. O padrão é ontem; a conferência para no último dia que a série já publicou.

.EXAMPLE
    .\tools\conferir-series.ps1 ar-BA

.EXAMPLE
    .\tools\conferir-series.ps1 uy-MVD -From 2000-01-01 -To 2026-10-23
#>
<#
    Calculadora de Dias Úteis - Calcula Prazos e Datas Finais considerando os feriados
    Copyright (C) 2021-2026 by Elekto Produtos Financeiros Ltda.

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.

    Entre em contato com a Elekto em https://elekto.com.br/
#>
param(
    [Parameter(Mandatory, Position = 0)][ValidateSet('ar-BA', 'uy-MVD')][string]$Calendar,
    [datetime]$From = [datetime]::new((Get-Date).Year, 1, 1),
    [datetime]$To = (Get-Date).Date.AddDays(-1)
)

$ErrorActionPreference = 'Stop'

$From = $From.Date
$To = $To.Date
if ($To -lt $From) { throw 'O último dia vem antes do primeiro.' }

function Test-Weekday([datetime]$Date) {
    $Date.DayOfWeek -ne [DayOfWeek]::Saturday -and $Date.DayOfWeek -ne [DayOfWeek]::Sunday
}

# Os feriados do arquivo do calendário, com a descrição
function Read-Calendar {
    $folder = Join-Path $PSScriptRoot '..\src\Elekto.Calendars\Calendars'
    $file = @(Get-ChildItem -LiteralPath $folder -Filter "*.calendar.$Calendar.txt")
    if ($file.Count -ne 1) { throw "Não achei o arquivo do calendário $Calendar em $folder." }
    $holidays = @{}
    foreach ($line in (Get-Content -LiteralPath $file[0].FullName -Encoding utf8 | Select-Object -Skip 1)) {
        if ($line -match '^(\d{8})(?:;(.*))?$') {
            $holidays[[datetime]::ParseExact($Matches[1], 'yyyyMMdd', $null)] = $Matches[2]
        }
    }
    $holidays
}

# Os dias com dado na série do BCRA, ano a ano (a API devolve no máximo 3.000 linhas por consulta)
function Get-BcraDates([int]$Variable) {
    $dates = [Collections.Generic.HashSet[datetime]]::new()
    for ($year = $From.Year; $year -le $To.Year; $year++) {
        $uri = "https://api.bcra.gob.ar/estadisticas/v4.0/monetarias/$Variable" +
               "?desde=$year-01-01&hasta=$year-12-31&limit=3000"
        $response = Invoke-RestMethod -Uri $uri -TimeoutSec 60 -MaximumRetryCount 3 -RetryIntervalSec 2
        foreach ($item in $response.results.detalle) {
            [void]$dates.Add([datetime]::ParseExact($item.fecha, 'yyyy-MM-dd', $null))
        }
    }
    , $dates
}

function Get-ArgentinaOpenDays {
    $badlar = Get-BcraDates 7
    $deposits = Get-BcraDates 12
    $badlar.UnionWith($deposits)
    , $badlar
}

# Os dias com cotação no BCU, ano a ano
function Get-UruguayOpenDays {
    $dates = [Collections.Generic.HashSet[datetime]]::new()
    $uri = 'https://www.bcu.gub.uy/_layouts/15/BCU.Cotizaciones/handler/CotizacionesHandler.ashx?op=getcotizaciones'
    for ($year = $From.Year; $year -le $To.Year; $year++) {
        $body = @{
            KeyValuePairs = @{
                Monedas = @(@{ Val = '2225'; Text = 'DLS. USA BILLETE' })
                FechaDesde = "01/01/$year"
                FechaHasta = "31/12/$year"
                Grupo = '2'
            }
        } | ConvertTo-Json -Depth 5
        $response = Invoke-RestMethod -Uri $uri -Method Post -Body $body -ContentType 'application/json; charset=utf-8' `
            -UserAgent 'Mozilla/5.0' -TimeoutSec 60 -MaximumRetryCount 3 -RetryIntervalSec 2
        if ($response.cotizacionesoutlist.RespuestaStatus.status -ne 1) {
            throw "O BCU recusou a consulta de $year`: $($response.cotizacionesoutlist.RespuestaStatus.mensaje)"
        }
        foreach ($quote in $response.cotizacionesoutlist.Cotizaciones) {
            # "/Date(ms)/" é a meia-noite de Montevidéu em UTC, que cai no mesmo dia; o PowerShell 7 já o converte
            $date = $quote.Fecha
            if ($date -isnot [datetime]) {
                $date = [DateTimeOffset]::FromUnixTimeMilliseconds([long]($date -replace '[^\d-]', '')).UtcDateTime
            } elseif ($date.Kind -eq [DateTimeKind]::Local) {
                $date = $date.ToUniversalTime()
            }
            [void]$dates.Add($date.Date)
        }
    }
    , $dates
}

$holidays = Read-Calendar
$open = if ($Calendar -eq 'ar-BA') { Get-ArgentinaOpenDays } else { Get-UruguayOpenDays }

$published = @($open | Where-Object { (Test-Weekday $_) -and $_ -ge $From -and $_ -le $To })
if (-not $published.Count) { throw 'A série não tem dado algum no período.' }
$last = ($published | Measure-Object -Maximum).Maximum
if ($last -lt $To) { $To = $last }

$closedNotInFile = [Collections.Generic.List[string]]::new()
$inFileButOpen = [Collections.Generic.List[string]]::new()
for ($day = $From; $day -le $To; $day = $day.AddDays(1)) {
    if (-not (Test-Weekday $day)) { continue }
    $isOpen = $open.Contains($day)
    $isHoliday = $holidays.ContainsKey($day)
    $label = $day.ToString('yyyy-MM-dd ddd', [Globalization.CultureInfo]::GetCultureInfo('pt-BR'))
    if (-not $isOpen -and -not $isHoliday) { $closedNotInFile.Add($label) }
    if ($isOpen -and $isHoliday) { $inFileButOpen.Add("$label  $($holidays[$day])") }
}

Write-Host "$Calendar, de $($From.ToString('yyyy-MM-dd')) a $($To.ToString('yyyy-MM-dd'))"
Write-Host ''
Write-Host "Sem dado na série e fora do calendário: $($closedNotInFile.Count)"
$closedNotInFile | ForEach-Object { Write-Host "  $_" }
Write-Host ''
Write-Host "No calendário e com dado na série: $($inFileButOpen.Count)"
$inFileButOpen | ForEach-Object { Write-Host "  $_" }
