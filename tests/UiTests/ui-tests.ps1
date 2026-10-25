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
<#
.SYNOPSIS
    Testes de interface da calculadora, com o WinApp CLI (winapp ui), de preferência no Windows Sandbox.

.DESCRIPTION
    Compila e instala o aplicativo no alvo escolhido com os dados apagados, e então o usa como uma pessoa usaria:
    escolhe datas nos calendários, digita prazos, troca opções e calendários, abre a página "Sobre" e volta,
    fecha e reabre o aplicativo. Os resultados esperados vêm de Expected.cs, que faz as mesmas contas com a
    biblioteca Elekto.Calendars para as datas que o teste escolhe.

    Requer o WinApp CLI 0.7 ou posterior, o SDK do .NET 10 e, no alvo sandbox, o Windows Sandbox habilitado
    e a sessão do Windows desbloqueada.

.EXAMPLE
    .\tests\UiTests\ui-tests.ps1 -Target sandbox
#>
param(
    [ValidateSet('sandbox', 'local')][string]$Target = 'sandbox',
    [string]$ArtifactDirectory = (Join-Path ([IO.Path]::GetTempPath()) "CalculadoraDiasUteis-ui-tests\$(Get-Date -Format 'yyyyMMdd-HHmmss')"),
    [ValidateNotNullOrEmpty()][string]$WorkflowId = [guid]::NewGuid().ToString('N')
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $false

$ProjectDirectory = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..\src\CalculadoraDiasUteis'))
$ExpectedScript = Join-Path $PSScriptRoot 'Expected.cs'
$ScopeArgs = @(if ($Target -eq 'sandbox') { '--on'; 'sandbox' })
$previousWorkflowId = $env:WINAPP_UI_WORKFLOW_ID
$env:WINAPP_UI_WORKFLOW_ID = $WorkflowId
$results = [Collections.Generic.List[object]]::new()
$screenshots = [Collections.Generic.List[string]]::new()
$uiStarted = $false
$AppPid = 0
$hwnd = ''

#region Ferramentas

function Invoke-WinAppChecked {
    param([string[]]$Arguments)
    $global:LASTEXITCODE = $null
    $output = @(& winapp @Arguments 2>&1 | ForEach-Object { "$_" })
    $code = $global:LASTEXITCODE
    if ($null -eq $code -or $code -ne 0) {
        throw "winapp $($Arguments -join ' ') falhou (código $code). $($output -join "`n")"
    }
    $output
}

function Invoke-Ui {
    param([string[]]$Arguments, [string]$Window)
    $selector = if ($Window) { @('-w', $Window) } else { @('-w', $script:hwnd) }
    Invoke-WinAppChecked (@('ui') + $Arguments + $ScopeArgs + $selector)
}

# Extrai o JSON da saída do winapp, que pode trazer avisos antes dele
function ConvertFrom-WinAppJson {
    param([string[]]$Lines)
    $text = $Lines -join "`n"
    $start = $text.IndexOfAny([char[]]'[{')
    if ($start -lt 0) { throw "Sem JSON na saída: $text" }
    $text.Substring($start) | ConvertFrom-Json
}

function Test-UI {
    param([string]$Name, [scriptblock]$Script)
    try {
        & $Script | Out-Null
        $results.Add([ordered]@{ name = $Name; status = 'PASS' })
        Write-Host "PASS  $Name"
    } catch {
        # Teclado e foco de verdade exigem a sessão do Windows desbloqueada; o resto funciona sem ela
        if ("$_" -match 'No interactive desktop is available') {
            $results.Add([ordered]@{ name = $Name; status = 'SKIP'; detail = 'Exige a sessão do Windows desbloqueada.' })
            Write-Host "SKIP  $Name (exige a sessão do Windows desbloqueada)" -ForegroundColor Yellow
            return
        }
        $results.Add([ordered]@{ name = $Name; status = 'FAIL'; detail = "$_" })
        Write-Host "FAIL  $Name`n      $_" -ForegroundColor Red
    }
}

function Save-Screenshot {
    param([string]$Name)
    $path = Join-Path $ArtifactDirectory $Name
    if (Test-Path -LiteralPath $path) { throw "Escolha outro arquivo para a captura: $path" }
    Invoke-Ui @('screenshot', '--output', $path) | Out-Null
    if (-not (Test-Path -LiteralPath $path -PathType Leaf) -or (Get-Item -LiteralPath $path).Length -eq 0) {
        throw "A captura não chegou ao computador: $path"
    }
    $screenshots.Add($path)
}

# O texto mostrado por um elemento, sem as marcas de direção (U+200E) que o WinUI põe nas datas
function Get-Text {
    param([string]$Id, [string]$Window)
    $value = ConvertFrom-WinAppJson (Invoke-Ui @('get-value', $Id, '--json') -Window $Window)
    ([string]$value.text) -replace "`u{200E}", ''
}

function Assert-Text {
    param([string]$Id, [string]$Expected, [int]$TimeoutMs = 3000)
    $deadline = [DateTime]::UtcNow.AddMilliseconds($TimeoutMs)
    do {
        $actual = Get-Text $Id
        if ($actual -eq $Expected) { return }
        Start-Sleep -Milliseconds 150
    } while ([DateTime]::UtcNow -lt $deadline)
    throw "$Id mostra '$actual'; o esperado era '$Expected'."
}

function Assert-Property {
    param([string]$Id, [string]$Property, [string]$Expected)
    Invoke-Ui @('wait-for', $Id, '-p', $Property, '--value', $Expected, '-t', '3000') | Out-Null
}

function Assert-Selected {
    param([string]$Id)
    Assert-Property $Id 'IsSelected' 'True'
}

# As janelas auxiliares (calendário aberto, lista do ComboBox) aparecem como PopupHost da janela principal
function Get-Popup {
    $deadline = [DateTime]::UtcNow.AddSeconds(3)
    do {
        $windows = @(ConvertFrom-WinAppJson (Invoke-WinAppChecked (@('ui', 'list-windows', '-a', "$script:AppPid", '--json') + $ScopeArgs)))
        $popup = $windows | Where-Object { $_.title -eq 'PopupHost' -and [string]$_.ownerHwnd -eq $script:hwnd } |
            Select-Object -First 1
        if ($popup) { return [string]$popup.hwnd }
        Start-Sleep -Milliseconds 150
    } while ([DateTime]::UtcNow -lt $deadline)
    throw 'A janela auxiliar (PopupHost) não abriu.'
}

# Busca elementos pelo texto; o winapp sai com código 1 quando não acha nada, o que aqui não é erro
function Search-Ui {
    param([string]$Text, [string]$Window)
    $selector = if ($Window) { @('-w', $Window) } else { @('-a', "$script:AppPid") }
    $output = @(& winapp (@('ui', 'search', $Text, '--json') + $ScopeArgs + $selector) 2>&1 | ForEach-Object { "$_" })
    @((ConvertFrom-WinAppJson $output).matches)
}

# Volta à página principal se um teste da página Sobre parou no meio
function Restore-MainPage {
    if (-not @(Search-Ui 'CalendarComboBox').Count) {
        Invoke-Ui @('invoke', 'PART_BackButton') | Out-Null
        Invoke-Ui @('wait-for', 'CalendarComboBox', '-t', '3000') | Out-Null
    }
}

function Find-InPopup {
    param([string]$Popup, [string]$Text, [string]$Type)
    $match = @(Search-Ui $Text $Popup | Where-Object { $_.name -eq $Text -and (-not $Type -or $_.type -eq $Type) }) |
        Select-Object -First 1
    if (-not $match) { throw "'$Text' não foi encontrado na janela auxiliar." }
    $match.selector
}

# Abre o calendário de um seletor de data e escolhe o dia 15 do mês mostrado, que é o mês da data atual do
# seletor; o dia 15 é o único que aparece uma só vez na grade de qualquer mês
function Select-Day15 {
    param([string]$Picker)
    Invoke-Ui @('invoke', $Picker) | Out-Null
    $popup = Get-Popup
    Invoke-Ui @('invoke', (Find-InPopup $popup '15' 'DataItem')) -Window $popup | Out-Null
}

function Set-Term {
    param([int]$Days)
    # Pelo próprio NumberBox (RangeValuePattern), que confirma o valor na hora; a caixa de texto dentro dele só
    # confirmaria ao perder o foco
    Invoke-Ui @('set-value', 'TermNumberBox', "$Days") | Out-Null
}

function Select-Calendar {
    param([string]$Description)
    Invoke-Ui @('invoke', 'CalendarComboBox') | Out-Null
    $popup = Get-Popup
    Invoke-Ui @('invoke', (Find-InPopup $popup $Description)) -Window $popup | Out-Null
}

function Start-App {
    param([switch]$Clean)
    $arguments = @('run', $ProjectDirectory) + $ScopeArgs + @('--detach', '--json')
    if ($Clean) { $arguments += '--clean' } else { $arguments += '--no-build' }
    $launch = ConvertFrom-WinAppJson (Invoke-WinAppChecked $arguments)
    if ($launch.Error -or -not $launch.ProcessId -or [int]$launch.ProcessId -le 0) {
        throw "O aplicativo não informou o ProcessId. $($launch.Error)"
    }
    $script:AppPid = [int]$launch.ProcessId
    if ($Target -eq 'sandbox' -and
        ($launch.ProcessScope -ne 'sandbox' -or $launch.UiTargetArgs -ne "--on sandbox -a $script:AppPid")) {
        throw 'A resposta do lançamento não traz o par ProcessScope / UiTargetArgs esperado do Sandbox.'
    }

    $deadline = [DateTime]::UtcNow.AddSeconds(15)
    do {
        $windows = @(ConvertFrom-WinAppJson (Invoke-WinAppChecked (@('ui', 'list-windows', '-a', "$script:AppPid", '--json') + $ScopeArgs)))
        $main = $windows | Where-Object { $_.hwnd -and $_.title -ne 'PopupHost' -and [string]$_.ownerHwnd -eq '0' } |
            Select-Object -First 1
        if ($main) { break }
        Start-Sleep -Milliseconds 300
    } while ([DateTime]::UtcNow -lt $deadline)
    if (-not $main) { throw 'A janela do aplicativo não apareceu.' }
    $script:hwnd = [string]$main.hwnd
    Invoke-Ui @('wait-for', 'CalendarComboBox', '-t', '5000') | Out-Null
}

function Stop-App {
    $command = @('target', 'exec', 'sandbox', '--', 'taskkill', '/PID', "$script:AppPid", '/F')
    if ($Target -eq 'sandbox') { Invoke-WinAppChecked $command | Out-Null }
    else { Stop-Process -Id $script:AppPid -Force }
}

#endregion

try {
    $ArtifactDirectory = [IO.Path]::GetFullPath($ArtifactDirectory)
    New-Item -ItemType Directory -Force -Path $ArtifactDirectory | Out-Null

    # Uma instância deixada aberta por outra execução impediria reinstalar o pacote
    if ($Target -eq 'sandbox') {
        try { Invoke-WinAppChecked @('target', 'exec', 'sandbox', '--', 'taskkill', '/IM', 'CalculadoraDiasUteis.exe', '/F') | Out-Null }
        catch { Write-Verbose 'Nenhuma instância aberta.' }
    } else {
        Get-Process CalculadoraDiasUteis -ErrorAction SilentlyContinue | Stop-Process -Force
    }

    # Primeira execução, com os dados do aplicativo apagados: valem os padrões da 1.2
    Start-App -Clean
    $uiStarted = $true

    $invariant = [Globalization.CultureInfo]::InvariantCulture
    $today = [DateTime]::ParseExact((Get-Text 'PeriodStartPicker'), 'dd/MM/yyyy', $invariant)
    $expectedJson = & dotnet run $ExpectedScript -- $today.ToString('yyyy-MM-dd', $invariant)
    if ($LASTEXITCODE -ne 0) { throw "Expected.cs falhou: $expectedJson" }
    $e = ($expectedJson | Select-Object -Last 1) | ConvertFrom-Json

    Test-UI 'Captura inicial' { Save-Screenshot '01-inicial.png' }

    Test-UI 'Padrões: br-BC, financeiro, dias úteis, sem ajuste, 252 dias' {
        Assert-Text 'CalendarComboBox' 'Brasil - Banco Central'
        Assert-Selected 'FinancialRadioButton'
        Assert-Selected 'WorkDaysRadioButton'
        Assert-Selected 'AdjustNoneRadioButton'
        Assert-Property 'AdjustNoneRadioButton' 'IsEnabled' 'False'
        Assert-Text 'InputBox' '252'
    }

    Test-UI 'Padrões: de hoje a um ano depois, e 252 dias úteis a partir de hoje' {
        Assert-Text 'PeriodEndPicker' $today.AddYears(1).ToString('dd/MM/yyyy', $invariant)
        Assert-Text 'TermStartPicker' $today.ToString('dd/MM/yyyy', $invariant)
        Assert-Text 'WorkDaysText' $e.todayWork
        Assert-Text 'ActualDaysText' $e.todayActual
        Assert-Text 'FinalDateText' $e.todayFinal
    }

    Test-UI 'Escolher as datas do período no calendário' {
        Select-Day15 'PeriodStartPicker'
        Assert-Text 'PeriodStartPicker' $e.d0
        Select-Day15 'PeriodEndPicker'
        Assert-Text 'PeriodEndPicker' $e.d1
    }

    Test-UI 'Dias entre as datas, financeiro' {
        Assert-Text 'WorkDaysText' $e.financialWork
        Assert-Text 'ActualDaysText' $e.financialActual
    }

    Test-UI 'Dias entre as datas, dias cheios' {
        Invoke-Ui @('invoke', 'FullDaysRadioButton') | Out-Null
        Assert-Text 'WorkDaysText' $e.fullWork
        Assert-Text 'ActualDaysText' $e.fullActual
    }

    Test-UI 'Data final de 30 dias úteis' {
        Select-Day15 'TermStartPicker'
        Assert-Text 'TermStartPicker' $e.d0
        Set-Term 30
        Assert-Text 'FinalDateText' $e.work30
    }

    Test-UI 'Prazo negativo conta para trás' {
        Set-Term -10
        Assert-Text 'FinalDateText' $e.workMinus10
    }

    Test-UI 'Dias corridos habilitam o ajuste; sem ajuste, avisa o dia não útil' {
        Invoke-Ui @('invoke', 'ActualDaysRadioButton') | Out-Null
        Assert-Property 'AdjustFollowingRadioButton' 'IsEnabled' 'True'
        Set-Term $e.weekendTerm
        Assert-Text 'FinalDateText' $e.actualNone
        Assert-Property 'FinalDateText' 'Name' $e.actualNoneDescription
    }

    Test-UI 'Ajuste para o dia útil seguinte' {
        Invoke-Ui @('invoke', 'AdjustFollowingRadioButton') | Out-Null
        Assert-Text 'FinalDateText' $e.actualFollowing
    }

    Test-UI 'Ajuste para o dia útil seguinte modificado' {
        Invoke-Ui @('invoke', 'AdjustModifiedFollowingRadioButton') | Out-Null
        Assert-Text 'FinalDateText' $e.actualModified
        Save-Screenshot '02-dias-corridos.png'
    }

    Test-UI 'Prazo além do calendário mostra o aviso, sem data' {
        Invoke-Ui @('invoke', 'WorkDaysRadioButton') | Out-Null
        Set-Term 100000
        Invoke-Ui @('wait-for', 'TermErrorInfoBar', '-t', '3000') | Out-Null
        Invoke-Ui @('wait-for', 'FinalDateText', '--gone', '-t', '3000') | Out-Null
        Save-Screenshot '03-erro.png'
        Invoke-Ui @('invoke', 'ActualDaysRadioButton') | Out-Null
        Set-Term $e.weekendTerm
        Assert-Text 'FinalDateText' $e.actualModified
        Invoke-Ui @('wait-for', 'TermErrorInfoBar', '--gone', '-t', '3000') | Out-Null
    }

    Test-UI 'Trocar o calendário refaz as duas contas' {
        Select-Calendar 'Estados Unidos - NYSE (Nova Iorque)'
        Assert-Text 'CalendarComboBox' 'Estados Unidos - NYSE (Nova Iorque)'
        Assert-Text 'WorkDaysText' $e.nyseFullWork
        Assert-Text 'ActualDaysText' $e.nyseFullActual
        Assert-Text 'FinalDateText' $e.nyseActualModified
    }

    Test-UI 'Página Sobre pelo botão, volta pelo botão da barra de título' {
        try {
            Invoke-Ui @('invoke', 'AboutButton') | Out-Null
            Assert-Text 'VersionText' 'Versão 2.0.0'
            # Nenhuma dica solta com a tecla dos atalhos de voltar (ela aparecia logo depois de navegar)
            Start-Sleep -Seconds 1
            if (@(Search-Ui 'Esc' | Where-Object { $_.name -eq 'Esc' }).Count) { throw 'Apareceu a dica "Esc".' }
            Save-Screenshot '04-sobre.png'
            Invoke-Ui @('invoke', 'PART_BackButton') | Out-Null
            Invoke-Ui @('wait-for', 'CalendarComboBox', '-t', '3000') | Out-Null
        } finally {
            Restore-MainPage
        }
    }

    # O Sandbox não tem programa associado a .txt; a tradução, em HTML, abre no Edge, que existe nele
    Test-UI 'A tradução da licença, que vem no pacote, abre no navegador' {
        if ($Target -ne 'sandbox') { throw 'Este teste fecha o navegador ao final, por isso só roda no Sandbox.' }
        try {
            Invoke-Ui @('invoke', 'AboutButton') | Out-Null
            Invoke-Ui @('invoke', 'LicenseTranslationButton') | Out-Null
            $deadline = [DateTime]::UtcNow.AddSeconds(20)
            do {
                $windows = @(ConvertFrom-WinAppJson (Invoke-WinAppChecked (@('ui', 'list-windows', '--json') + $ScopeArgs)))
                $browser = $windows | Where-Object { $_.processName -eq 'msedge' } | Select-Object -First 1
                if ($browser) { break }
                Start-Sleep -Milliseconds 500
            } while ([DateTime]::UtcNow -lt $deadline)
            if (-not $browser) { throw 'O navegador não abriu.' }
            Invoke-Ui @('wait-for', 'OpenErrorInfoBar', '--gone', '-t', '1000') | Out-Null
        } finally {
            try { Invoke-WinAppChecked @('target', 'exec', 'sandbox', '--', 'taskkill', '/IM', 'msedge.exe', '/F') | Out-Null }
            catch { Write-Verbose 'O navegador não estava aberto.' }
            Restore-MainPage
        }
    }

    Test-UI 'Página Sobre pelo F1, volta pelo Esc' {
        try {
            Invoke-Ui @('send-keys', 'f1', '--target', 'CalendarComboBox', '--via', 'send-input') | Out-Null
            Invoke-Ui @('wait-for', 'VersionText', '-t', '3000') | Out-Null
            Invoke-Ui @('send-keys', 'escape', '--target', 'LicenseButton', '--via', 'send-input') | Out-Null
            Invoke-Ui @('wait-for', 'CalendarComboBox', '-t', '3000') | Out-Null
        } finally {
            Restore-MainPage
        }
    }

    Test-UI 'Ao voltar da página Sobre, nada se perdeu' {
        Assert-Text 'CalendarComboBox' 'Estados Unidos - NYSE (Nova Iorque)'
        Assert-Text 'PeriodStartPicker' $e.d0
        Assert-Text 'WorkDaysText' $e.nyseFullWork
        Assert-Text 'FinalDateText' $e.nyseActualModified
    }

    Test-UI 'Todos os controles da página têm AutomationId' {
        $inspection = ConvertFrom-WinAppJson (Invoke-Ui @('inspect', '--interactive', '--json'))
        function Get-Elements($nodes) {
            foreach ($node in $nodes) {
                if ($null -ne $node) { $node; Get-Elements $node.children }
            }
        }
        $allElements = @(Get-Elements @($inspection.windows | ForEach-Object { $_.elements }))
        if (-not $allElements.Count) { throw 'A inspeção não trouxe elemento algum.' }
        $appElements = @($allElements | Where-Object {
            $_.type -match 'Button|Edit|ComboBox|RadioButton|Hyperlink|Spinner' -and
            $_.name -notmatch '^(Minimize|Maximize|Close|System)$'
        })
        if ($appElements.Count -lt 10) { throw "Só $($appElements.Count) controles foram encontrados." }
        $missingId = @($appElements | Where-Object { -not $_.automationId })
        if ($missingId.Count) {
            throw "Sem AutomationId: $(($missingId | ForEach-Object { "$($_.type) '$($_.name)'" }) -join ', ')"
        }
    }

    # Segunda execução, sem apagar os dados: as escolhas voltam como foram deixadas
    Test-UI 'As escolhas são lembradas ao reabrir' {
        Stop-App
        Start-App
        Assert-Text 'CalendarComboBox' 'Estados Unidos - NYSE (Nova Iorque)'
        Assert-Selected 'FullDaysRadioButton'
        Assert-Selected 'ActualDaysRadioButton'
        Assert-Selected 'AdjustModifiedFollowingRadioButton'
        Assert-Text 'InputBox' "$($e.weekendTerm)"
        Save-Screenshot '05-reaberto.png'
    }
} catch {
    $results.Add([ordered]@{ name = 'Preparação ou execução dos testes'; status = 'FAIL'; detail = "$_" })
    Write-Host "FAIL  Preparação ou execução dos testes`n      $_" -ForegroundColor Red
} finally {
    try {
        if ($uiStarted) { Invoke-WinAppChecked (@('ui', 'yield') + $ScopeArgs) | Out-Null }
    } catch {
        $results.Add([ordered]@{ name = 'Liberar a área de trabalho'; status = 'FAIL'; detail = "$_" })
    } finally {
        $env:WINAPP_UI_WORKFLOW_ID = $previousWorkflowId
    }
}

$failed = @($results | Where-Object { $_.status -eq 'FAIL' }).Count
$skipped = @($results | Where-Object { $_.status -eq 'SKIP' }).Count
$report = [ordered]@{
    target = $Target; appPid = $AppPid; workflowId = $WorkflowId
    passed = $results.Count - $failed - $skipped; failed = $failed; skipped = $skipped
    results = @($results.ToArray()); screenshots = @($screenshots.ToArray())
}
$reportPath = Join-Path $ArtifactDirectory 'test-results.json'
$report | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $reportPath -Encoding utf8
Write-Host "Passaram: $($report.passed) | Falharam: $failed | Pulados: $skipped | Resultados: $reportPath"
if ($failed) { exit 1 }
exit 0
