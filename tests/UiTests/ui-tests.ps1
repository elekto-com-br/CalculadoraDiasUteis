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
    fecha e reabre o aplicativo, cria e edita calendários, exporta e importa. Os resultados esperados vêm de Expected.cs, que faz as mesmas contas com a
    biblioteca Elekto.Calendars para as datas que o teste escolhe.

    Requer o WinApp CLI 0.7 ou posterior, o SDK do .NET 10 e, no alvo sandbox, o Windows Sandbox habilitado
    e a sessão do Windows desbloqueada.

.PARAMETER AxeCli
    O zip do Axe.Windows CLI (AxeWindowsCLI-<versão>.zip, de https://github.com/microsoft/axe-windows/releases), o
    verificador automático do Accessibility Insights. Com ele, as telas principais e uma caixa de diálogo são
    verificadas no Sandbox, e qualquer erro de acessibilidade falha o teste; sem ele, esses testes são pulados.

.PARAMETER Package
    Um pacote MSIX já gerado (winapp package), para testar o que vai para a Store em vez do projeto compilado em
    Debug. Só no Sandbox, junto com -Certificate, o certificado .pfx que assinou o pacote, de senha "password".

.EXAMPLE
    .\tests\UiTests\ui-tests.ps1 -Target sandbox

.EXAMPLE
    .\tests\UiTests\ui-tests.ps1 -Package .\CalculadoraDiasUteis.msix -Certificate .\CalculadoraDiasUteis_cert.pfx
#>
param(
    [ValidateSet('sandbox', 'local')][string]$Target = 'sandbox',
    [string]$ArtifactDirectory = (Join-Path ([IO.Path]::GetTempPath()) "CalculadoraDiasUteis-ui-tests\$(Get-Date -Format 'yyyyMMdd-HHmmss')"),
    [ValidateNotNullOrEmpty()][string]$WorkflowId = [guid]::NewGuid().ToString('N'),
    [string]$Package,
    [string]$Certificate,
    [string]$AxeCli
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $false

$ProjectDirectory = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..\src\CalculadoraDiasUteis'))
$ExpectedScript = Join-Path $PSScriptRoot 'Expected.cs'
$ExportFileName = 'Calendários.json'
$PackageName = 'Elekto.CalculadoraDiasUteis'
$LegacyFileName = '0050.calendar.sao-paulo.txt'
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
        # Um teste que depende de um parâmetro não informado é pulado
        if ("$_" -like 'PULAR:*') {
            $reason = "$_".Substring(6).Trim()
            $results.Add([ordered]@{ name = $Name; status = 'SKIP'; detail = $reason })
            Write-Host "SKIP  $Name ($reason)" -ForegroundColor Yellow
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

# As janelas auxiliares abertas agora; a dica de um botão com foco também é uma delas
function Get-Popups {
    $windows = @(ConvertFrom-WinAppJson (Invoke-WinAppChecked (@('ui', 'list-windows', '-a', "$script:AppPid", '--json') + $ScopeArgs)))
    @($windows | Where-Object { $_.title -eq 'PopupHost' -and [string]$_.ownerHwnd -eq $script:hwnd } |
        ForEach-Object { [string]$_.hwnd })
}

# Abre o calendário de um seletor de data e escolhe um dia do mês mostrado; só os dias de 15 a 22 aparecem uma vez
# só na grade de qualquer mês
function Select-Day {
    param([string]$Picker, [string]$Day)
    Invoke-Ui @('invoke', $Picker) | Out-Null
    $deadline = [DateTime]::UtcNow.AddSeconds(3)
    do {
        foreach ($popup in Get-Popups) {
            $match = @(Search-Ui $Day $popup | Where-Object { $_.name -eq $Day -and $_.type -eq 'DataItem' }) |
                Select-Object -First 1
            if ($match) {
                Invoke-Ui @('invoke', $match.selector) -Window $popup | Out-Null
                return
            }
        }
        Start-Sleep -Milliseconds 150
    } while ([DateTime]::UtcNow -lt $deadline)
    throw "O dia $Day não apareceu no calendário de $Picker."
}

# O seletor de um feriado da lista pelo começo do que o Narrador lê: a data por extenso, a descrição e a origem
function Find-Holiday {
    param([string]$Text, [switch]$Anywhere)
    $deadline = [DateTime]::UtcNow.AddSeconds(3)
    do {
        $match = @(Search-Ui $Text | Where-Object {
            $_.type -eq 'ListItem' -and ($Anywhere -or $_.name.StartsWith($Text)) }) | Select-Object -First 1
        if ($match) { return $match.selector }
        Start-Sleep -Milliseconds 150
    } while ([DateTime]::UtcNow -lt $deadline)
    throw "O feriado '$Text' não está na lista."
}

# A mensagem da última operação, na barra de aviso da página de calendários
function Assert-Status {
    param([string]$Text, [int]$TimeoutSeconds = 3)
    $deadline = [DateTime]::UtcNow.AddSeconds($TimeoutSeconds)
    do {
        if (@(Search-Ui $Text | Where-Object { $_.name -eq $Text }).Count) { return }
        Start-Sleep -Milliseconds 150
    } while ([DateTime]::UtcNow -lt $deadline)
    throw "A mensagem '$Text' não apareceu."
}

function Select-EditCalendar {
    param([string]$Name)
    Invoke-Ui @('invoke', 'EditCalendarComboBox') | Out-Null
    $popup = Get-Popup
    Invoke-Ui @('invoke', (Find-InPopup $popup $Name)) -Window $popup | Out-Null
    Assert-Text 'EditCalendarComboBox' $Name
}

# Fecha uma caixa de diálogo que um teste deixou aberta e volta à página de calendários
function Restore-CalendarsPage {
    if (@(Search-Ui 'CloseButton' | Where-Object { $_.selector -eq 'CloseButton' -and $_.type -eq 'Button' }).Count) {
        try { Invoke-Ui @('invoke', 'CloseButton') | Out-Null } catch { Write-Verbose 'A caixa já tinha fechado.' }
    }
    if (-not @(Search-Ui 'EditCalendarComboBox').Count) {
        Restore-MainPage
        Invoke-Ui @('invoke', 'CalendarsButton') | Out-Null
        Invoke-Ui @('wait-for', 'EditCalendarComboBox', '-t', '5000') | Out-Null
    }
}

# A caixa "Abrir" ou "Salvar como" do Windows: a do Windows App SDK é uma janela do aplicativo, a clássica vem do
# PickerHost, um processo do Windows; as duas pertencem à janela do aplicativo
function Get-FileDialog {
    $deadline = [DateTime]::UtcNow.AddSeconds(10)
    do {
        $windows = @(ConvertFrom-WinAppJson (Invoke-WinAppChecked (@('ui', 'list-windows', '--json') + $ScopeArgs)))
        $dialog = $windows | Where-Object {
            $_.className -eq '#32770' -and ([string]$_.ownerHwnd -eq $script:hwnd -or $_.processName -eq 'PickerHost') } |
            Select-Object -First 1
        if ($dialog) { return [string]$dialog.hwnd }
        Start-Sleep -Milliseconds 300
    } while ([DateTime]::UtcNow -lt $deadline)
    throw 'A caixa de arquivo do Windows não abriu.'
}

function Wait-FileDialogClosed {
    param([string]$Window)
    $deadline = [DateTime]::UtcNow.AddSeconds(5)
    do {
        $windows = @(ConvertFrom-WinAppJson (Invoke-WinAppChecked (@('ui', 'list-windows', '--json') + $ScopeArgs)))
        if (-not @($windows | Where-Object { [string]$_.hwnd -eq $Window }).Count) { return }
        # A confirmação de substituir um arquivo que já existe
        $confirm = $windows | Where-Object { [string]$_.ownerHwnd -eq $Window } | Select-Object -First 1
        if ($confirm) { Invoke-Ui @('invoke', 'CommandButton_6') -Window ([string]$confirm.hwnd) | Out-Null }
        Start-Sleep -Milliseconds 300
    } while ([DateTime]::UtcNow -lt $deadline)
    throw 'A caixa de arquivo do Windows não fechou.'
}

# Salva com o nome sugerido, na pasta Documentos: no Sandbox, o nome escrito por automação não chega à caixa
function Save-WithSuggestedName {
    $window = Get-FileDialog
    Invoke-Ui @('invoke', '1') -Window $window | Out-Null
    Wait-FileDialogClosed $window
}

# Abre um arquivo da pasta Documentos, acionando-o na lista da caixa "Abrir", como num clique duplo
function Open-FromDocuments {
    param([string]$FileName)
    $window = Get-FileDialog
    $item = @(Search-Ui $FileName $window | Where-Object { $_.name -eq $FileName -and $_.type -eq 'ListItem' }) |
        Select-Object -First 1
    if (-not $item) { throw "O arquivo $FileName não aparece na caixa Abrir." }
    Invoke-Ui @('invoke', $item.selector) -Window $window | Out-Null
    Wait-FileDialogClosed $window
}

# A pasta Documentos onde o aplicativo está
function Get-TargetDocuments {
    (Invoke-Target @('powershell', '-NoProfile', '-Command', "[Environment]::GetFolderPath('MyDocuments')") |
        Select-Object -Last 1).Trim()
}

# Confere que um arquivo existe onde o aplicativo está e não está vazio
function Assert-TargetFile {
    param([string]$Path)
    Invoke-Target @('powershell', '-NoProfile', '-Command',
        "if (-not (Test-Path -LiteralPath '$Path') -or (Get-Item -LiteralPath '$Path').Length -eq 0) { exit 1 }") | Out-Null
}

# Apaga um arquivo exportado. Com a sessão do Windows bloqueada, a automação não põe o foco no nome do arquivo, e o
# Salvar da caixa seguinte usaria o último arquivo salvo, que fica marcado na lista; para uma pessoa, o foco começa no nome
function Remove-TargetFile {
    param([string]$Path)
    Invoke-Target @('powershell', '-NoProfile', '-Command',
        "Remove-Item -LiteralPath '$Path' -ErrorAction Ignore; exit 0") | Out-Null
}

# As linhas não vazias do texto na área de transferência de onde o aplicativo está, e se há também a versão em HTML
function Get-TargetClipboard {
    $lines = @(Invoke-Target @('powershell', '-NoProfile', '-Command',
        "(Get-Clipboard | Where-Object { `$_ }).Count; [bool](Get-Clipboard -TextFormatType Html | Select-String 'mso-number-format')"))
    [pscustomobject]@{ Lines = [int]$lines[-2]; HasHtml = $lines[-1].Trim() -eq 'True' }
}

# Verifica a janela do aplicativo com o Axe.Windows, as regras automáticas do Accessibility Insights
function Assert-Axe {
    param([string]$ScanId)
    if (-not $AxeCli) { throw 'PULAR: sem -AxeCli' }
    if ($Target -ne 'sandbox') { throw 'PULAR: o Axe.Windows roda no Sandbox' }
    if (-not $script:AxeExe) {
        $push = ConvertFrom-WinAppJson (Invoke-WinAppChecked @('target', 'push', 'sandbox', [IO.Path]::GetFullPath($AxeCli), 'axe\AxeWindowsCLI.zip', '--json'))
        $folder = Join-Path (Split-Path $push.targetPath) 'cli'
        Invoke-Target @('powershell', '-NoProfile', '-Command',
            "Expand-Archive -Path '$($push.targetPath)' -DestinationPath '$folder' -Force") | Out-Null
        $script:AxeExe = Join-Path $folder 'AxeWindowsCLI.exe'
    }
    Start-Sleep -Milliseconds 500
    $output = @(Invoke-Target @('powershell', '-NoProfile', '-Command',
        "& '$script:AxeExe' --processId $script:AppPid --outputDirectory 'C:\WinApp\work\axe\$WorkflowId' --scanId '$ScanId'; exit 0"))
    $line = $output | Where-Object { $_ -match '(\d+) errors? (was|were) found' } | Select-Object -Last 1
    if (-not $line -or $line -notmatch '(\d+) errors? (was|were) found') {
        throw "O Axe.Windows não deu resultado: $($output -join ' ')"
    }
    if ($Matches[1] -ne '0') {
        throw "$line O relatório (.a11ytest) ficou no Sandbox, em C:\WinApp\work\axe\$WorkflowId, e abre no Accessibility Insights."
    }
}

# Roda um comando onde o aplicativo está
function Invoke-Target {
    param([string[]]$Command)
    if ($Target -eq 'sandbox') { Invoke-WinAppChecked (@('target', 'exec', 'sandbox', '--') + $Command) }
    else { & $Command[0] $Command[1..($Command.Count - 1)] }
}

# Os controles interativos da página mostrada têm AutomationId, para os testes e as ferramentas de acessibilidade
function Assert-AutomationIds {
    param([int]$Minimum)
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
    if ($appElements.Count -lt $Minimum) { throw "Só $($appElements.Count) controles foram encontrados." }
    $missingId = @($appElements | Where-Object { -not $_.automationId })
    if ($missingId.Count) {
        throw "Sem AutomationId: $(($missingId | ForEach-Object { "$($_.type) '$($_.name)'" }) -join ', ')"
    }
}

# Instala no Sandbox o pacote a testar, no lugar do registrado pelo winapp, confiando no certificado que o assinou
function Install-TestPackage {
    $msix = ConvertFrom-WinAppJson (Invoke-WinAppChecked @('target', 'push', 'sandbox', $Package, 'ui-tests\pacote.msix', '--json'))
    $pfx = ConvertFrom-WinAppJson (Invoke-WinAppChecked @('target', 'push', 'sandbox', $Certificate, 'ui-tests\pacote.pfx', '--json'))
    Invoke-Target @('powershell', '-NoProfile', '-Command', (
        "Get-AppxPackage -Name '$PackageName' | Remove-AppxPackage; " +
        "Import-PfxCertificate -FilePath '$($pfx.targetPath)' -CertStoreLocation Cert:\LocalMachine\TrustedPeople " +
        "-Password (ConvertTo-SecureString 'password' -AsPlainText -Force) | Out-Null; " +
        "Add-AppxPackage -Path '$($msix.targetPath)'")) | Out-Null
}

# Abre o pacote instalado, como pelo menu Iniciar; com -Clean, apaga antes os dados dele
function Start-InstalledApp {
    param([switch]$Clean)
    if ($Clean) { Invoke-Target @('powershell', '-NoProfile', '-Command', "Get-AppxPackage -Name '$PackageName' | Reset-AppxPackage") | Out-Null }
    # O comando no alvo encerra o que ele mesmo abriu ao terminar; a pausa dá tempo ao Explorer de pedir a abertura
    $launch = "Start-Process explorer.exe -ArgumentList ('shell:AppsFolder\' + (Get-AppxPackage -Name '$PackageName').PackageFamilyName + '!App'); Start-Sleep -Seconds 3"

    # Logo depois de instalado, o pacote demora a aparecer para o Explorer: o pedido de abertura se repete
    $deadline = [DateTime]::UtcNow.AddSeconds(45)
    $nextLaunch = [DateTime]::MinValue
    do {
        if ([DateTime]::UtcNow -ge $nextLaunch) {
            Invoke-Target @('powershell', '-NoProfile', '-Command', $launch) | Out-Null
            $nextLaunch = [DateTime]::UtcNow.AddSeconds(8)
        }
        $windows = @(ConvertFrom-WinAppJson (Invoke-WinAppChecked (@('ui', 'list-windows', '--json') + $ScopeArgs)))
        $main = $windows | Where-Object { $_.processName -eq 'CalculadoraDiasUteis' -and $_.className -eq 'WinUIDesktopWin32WindowClass' } |
            Select-Object -First 1
        if ($main) { break }
        Start-Sleep -Milliseconds 500
    } while ([DateTime]::UtcNow -lt $deadline)
    if (-not $main) {
        throw "A janela do aplicativo instalado não apareceu. Janelas: $(($windows | ForEach-Object { "$($_.processName)/$($_.className)" }) -join ', ')"
    }
    $script:AppPid = [int]$main.processId
    $script:hwnd = [string]$main.hwnd
    # Se um pedido repetido abriu uma segunda janela, ela sai
    Invoke-Target @('powershell', '-NoProfile', '-Command',
        "Get-Process CalculadoraDiasUteis | Where-Object Id -ne $script:AppPid | Stop-Process -Force; exit 0") | Out-Null
    Invoke-Ui @('wait-for', 'CalendarComboBox', '-t', '5000') | Out-Null
}

function Start-App {
    param([switch]$Clean)
    if ($Package) {
        Start-InstalledApp -Clean:$Clean
        return
    }

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

# Muda o tamanho da janela como quem arrasta a borda; o script roda no alvo, onde está a janela
function Set-WindowSize {
    param([int]$Width, [int]$Height)
    $arguments = @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', $script:ResizeScript,
        '-Hwnd', $script:hwnd, '-Width', "$Width", '-Height', "$Height")
    if ($Target -eq 'sandbox') { Invoke-WinAppChecked (@('target', 'exec', 'sandbox', '--', 'powershell') + $arguments) | Out-Null }
    else { & powershell @arguments }
}

function Get-WindowSize {
    $windows = @(ConvertFrom-WinAppJson (Invoke-WinAppChecked (@('ui', 'list-windows', '-a', "$script:AppPid", '--json') + $ScopeArgs)))
    $main = $windows | Where-Object { [string]$_.hwnd -eq $script:hwnd } | Select-Object -First 1
    [pscustomobject]@{ Width = [int]$main.width; Height = [int]$main.height }
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

    # O script que muda o tamanho da janela roda onde ela está
    $script:ResizeScript = Join-Path $PSScriptRoot 'Set-WindowSize.ps1'
    if ($Target -eq 'sandbox') {
        $push = ConvertFrom-WinAppJson (Invoke-WinAppChecked @('target', 'push', 'sandbox', $script:ResizeScript, 'ui-tests\Set-WindowSize.ps1', '--json'))
        $script:ResizeScript = $push.targetPath
    }

    # Na pasta Documentos do Sandbox, um calendário no formato da 1.x para importar, e nenhum arquivo exportado
    # por outra execução; fora do Sandbox, os testes de exportar e importar não rodam, para não mexer nos seus arquivos
    if ($Target -eq 'sandbox') {
        $legacy = Join-Path $ArtifactDirectory $LegacyFileName
        $year = (Get-Date).Year
        Set-Content -LiteralPath $legacy -Encoding utf8BOM -Value @(
            'Cidade de São Paulo', "${year}0125;Aniversário de São Paulo", "${year}0709;Revolução Constitucionalista",
            "${year}1120;Dia da Consciência Negra")
        $push = ConvertFrom-WinAppJson (Invoke-WinAppChecked @('target', 'push', 'sandbox', $legacy, "ui-tests\$LegacyFileName", '--json'))
        Invoke-Target @('powershell', '-NoProfile', '-Command', (
            "`$documents = [Environment]::GetFolderPath('MyDocuments'); " +
            "Remove-Item -LiteralPath (Join-Path `$documents '$ExportFileName') -ErrorAction Ignore; " +
            "Remove-Item -Path (Join-Path `$documents '*.xlsx') -ErrorAction Ignore; " +
            "Copy-Item -LiteralPath '$($push.targetPath)' -Destination `$documents -Force; exit 0")) | Out-Null
    }

    if ($Package) {
        if ($Target -ne 'sandbox' -or -not $Certificate) { throw 'O teste de um pacote só roda no Sandbox, com -Certificate.' }
        $Package = [IO.Path]::GetFullPath($Package)
        $Certificate = [IO.Path]::GetFullPath($Certificate)
        Install-TestPackage
    } elseif ($Target -eq 'sandbox') {
        # Um pacote instalado por um teste de -Package impediria o winapp de registrar o projeto
        Invoke-Target @('powershell', '-NoProfile', '-Command',
            "Get-AppxPackage -Name '$PackageName' | Where-Object { -not `$_.IsDevelopmentMode } | Remove-AppxPackage; exit 0") | Out-Null
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

    Test-UI 'Axe.Windows na página principal' { Assert-Axe 'principal' }

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
            foreach ($link in 'ElektoLink', 'SourceCodeLink', 'ElektoPrivacyLink', 'GitHubPrivacyLink') {
                Invoke-Ui @('wait-for', $link, '-t', '2000') | Out-Null
            }
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

    # Com uma dica própria, o WinUI não acrescenta a tecla do atalho; o "(F1)" está no texto da dica
    Test-UI 'A dica do botão Sobre mostra a tecla F1' {
        Invoke-Ui @('hover', 'AboutButton') | Out-Null
        $deadline = [DateTime]::UtcNow.AddSeconds(3)
        do {
            $tips = @(Search-Ui 'F1' | Where-Object { $_.type -eq 'ToolTip' })
            if ($tips.Count) { return }
            Start-Sleep -Milliseconds 200
        } while ([DateTime]::UtcNow -lt $deadline)
        throw 'A dica do botão Sobre não mostra F1.'
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

    Test-UI 'Axe.Windows na página Sobre' {
        try {
            if ($AxeCli) {
                Invoke-Ui @('invoke', 'AboutButton') | Out-Null
                Invoke-Ui @('wait-for', 'VersionText', '-t', '3000') | Out-Null
            }
            Assert-Axe 'sobre'
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

    Test-UI 'A janela não fica menor que o mínimo' {
        try {
            Set-WindowSize 200 200
            $size = Get-WindowSize
            if ($size.Width -lt 500 -or $size.Height -lt 400) { throw "A janela ficou com $($size.Width) x $($size.Height)." }
            Save-Screenshot '06-minima.png'
        } finally {
            Set-WindowSize 960 600
        }
    }

    Test-UI 'Todos os controles da página têm AutomationId' {
        Assert-AutomationIds 10
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

    #region Fase 2: calendários editáveis

    Test-UI 'Editar calendários abre no calendário escolhido, com os feriados do ano' {
        Select-Calendar 'Brasil - Banco Central'
        Invoke-Ui @('invoke', 'CalendarsButton') | Out-Null
        Invoke-Ui @('wait-for', 'EditCalendarComboBox', '-t', '5000') | Out-Null
        Assert-Text 'EditCalendarComboBox' 'Brasil - Banco Central'
        Assert-Text 'CalendarKindText' 'Calendário da Elekto, sem alterações suas'
        Assert-Text 'HolidaysSummaryText' $e.holidaysThisYear
        Find-Holiday "$($e.christmasLong), $($e.christmasDescription), Da Elekto" | Out-Null
        Save-Screenshot '07-calendarios.png'
    }

    Test-UI 'Axe.Windows na página de calendários e na caixa Novo' {
        Assert-Axe 'calendarios'
        try {
            Invoke-Ui @('invoke', 'NewCalendarButton') | Out-Null
            Invoke-Ui @('wait-for', 'CalendarNameTextBox', '-t', '3000') | Out-Null
            Assert-Axe 'novo-calendario'
        } finally {
            Restore-CalendarsPage
        }
    }

    Test-UI 'Criar um calendário derivado do br-BC' {
        Invoke-Ui @('invoke', 'DeriveCalendarButton') | Out-Null
        Invoke-Ui @('wait-for', 'CalendarNameTextBox', '-t', '3000') | Out-Null
        Invoke-Ui @('set-value', 'CalendarNameTextBox', 'Fórum da Lapa') | Out-Null
        Invoke-Ui @('invoke', 'PrimaryButton') | Out-Null
        Invoke-Ui @('wait-for', 'CalendarNameTextBox', '--gone', '-t', '3000') | Out-Null
        Assert-Text 'EditCalendarComboBox' 'Fórum da Lapa'
        Assert-Text 'CalendarKindText' 'Derivado de Brasil - Banco Central, sem alterações suas'
        Assert-Status 'Calendário “Fórum da Lapa” criado.'
    }

    Test-UI 'Incluir um feriado local' {
        Invoke-Ui @('invoke', 'AddHolidayButton') | Out-Null
        Invoke-Ui @('wait-for', 'HolidayDescriptionTextBox', '-t', '3000') | Out-Null
        Select-Day 'HolidayDatePicker' $e.localDay
        Invoke-Ui @('set-value', 'HolidayDescriptionTextBox', 'Feriado do fórum') | Out-Null
        Save-Screenshot '08-incluir-feriado.png'
        Invoke-Ui @('invoke', 'PrimaryButton') | Out-Null
        Invoke-Ui @('wait-for', 'HolidayDescriptionTextBox', '--gone', '-t', '3000') | Out-Null
        Find-Holiday "$($e.localLong), Feriado do fórum, Incluído por você" | Out-Null
        Assert-Text 'CalendarKindText' 'Derivado de Brasil - Banco Central, com 1 alteração sua'
    }

    Test-UI 'Remover e restaurar um feriado herdado' {
        Invoke-Ui @('invoke', (Find-Holiday "$($e.christmasLong), $($e.christmasDescription), Herdado")) | Out-Null
        Invoke-Ui @('invoke', 'RemoveHolidayButton') | Out-Null
        Find-Holiday "$($e.christmasLong), $($e.christmasDescription), Removido por você" | Out-Null
        Assert-Text 'CalendarKindText' 'Derivado de Brasil - Banco Central, com 2 alterações suas'
        Save-Screenshot '09-removido.png'
        Invoke-Ui @('invoke', 'RestoreHolidayButton') | Out-Null
        Find-Holiday "$($e.christmasLong), $($e.christmasDescription), Herdado" | Out-Null
        Assert-Text 'CalendarKindText' 'Derivado de Brasil - Banco Central, com 1 alteração sua'
    }

    Test-UI 'Trocar a descrição do feriado local' {
        Invoke-Ui @('invoke', (Find-Holiday "$($e.localLong), Feriado do fórum")) | Out-Null
        Invoke-Ui @('invoke', 'EditHolidayButton') | Out-Null
        Invoke-Ui @('wait-for', 'HolidayDescriptionTextBox', '-t', '3000') | Out-Null
        Invoke-Ui @('set-value', 'HolidayDescriptionTextBox', 'Aniversário do fórum') | Out-Null
        Invoke-Ui @('invoke', 'PrimaryButton') | Out-Null
        Invoke-Ui @('wait-for', 'HolidayDescriptionTextBox', '--gone', '-t', '3000') | Out-Null
        Find-Holiday "$($e.localLong), Aniversário do fórum, Incluído por você" | Out-Null
    }

    Test-UI 'Nome repetido é recusado sem fechar a caixa' {
        Invoke-Ui @('invoke', 'NewCalendarButton') | Out-Null
        Invoke-Ui @('wait-for', 'CalendarNameTextBox', '-t', '3000') | Out-Null
        Invoke-Ui @('set-value', 'CalendarNameTextBox', 'brasil - banco central') | Out-Null
        Invoke-Ui @('invoke', 'PrimaryButton') | Out-Null
        Invoke-Ui @('wait-for', 'CalendarDialogErrorInfoBar', '-t', '3000') | Out-Null
        if (-not @(Search-Ui 'Já existe um calendário chamado “brasil - banco central”.').Count) {
            throw 'A mensagem de nome repetido não apareceu.'
        }
        Save-Screenshot '10-nome-repetido.png'
        Invoke-Ui @('invoke', 'CloseButton') | Out-Null
        Invoke-Ui @('wait-for', 'CalendarNameTextBox', '--gone', '-t', '3000') | Out-Null
    }

    Test-UI 'Renomear o calendário' {
        Invoke-Ui @('invoke', 'RenameCalendarButton') | Out-Null
        Invoke-Ui @('wait-for', 'CalendarNameTextBox', '-t', '3000') | Out-Null
        Invoke-Ui @('set-value', 'CalendarNameTextBox', 'Fórum Regional da Lapa') | Out-Null
        Invoke-Ui @('invoke', 'PrimaryButton') | Out-Null
        Invoke-Ui @('wait-for', 'CalendarNameTextBox', '--gone', '-t', '3000') | Out-Null
        Assert-Text 'EditCalendarComboBox' 'Fórum Regional da Lapa'
    }

    Test-UI 'O feriado local vale nas contas da página principal' {
        Invoke-Ui @('invoke', 'PART_BackButton') | Out-Null
        Invoke-Ui @('wait-for', 'CalendarComboBox', '-t', '3000') | Out-Null
        Invoke-Ui @('invoke', 'FinancialRadioButton') | Out-Null
        Select-Day15 'PeriodStartPicker'
        Assert-Text 'WorkDaysText' $e.brBcFinancialFromD0
        Select-Calendar 'Fórum Regional da Lapa'
        Assert-Text 'WorkDaysText' $e.lapaFinancialFromD0
        Save-Screenshot '11-principal-derivado.png'
    }

    # Fase 3: as datas do período e os feriados vão para a planilha
    Test-UI 'Copiar as datas do período, em texto e em HTML' {
        if ($Target -ne 'sandbox') { throw 'Este teste troca a área de transferência, por isso só roda no Sandbox.' }
        Invoke-Ui @('invoke', 'CopyPeriodButton') | Out-Null
        Assert-Status "$($e.periodDays) datas copiadas, de $($e.d0) a $($e.periodEnd)."
        $clipboard = Get-TargetClipboard
        if ($clipboard.Lines -ne [int]$e.periodDays + 1) { throw "A área de transferência tem $($clipboard.Lines) linhas." }
        if (-not $clipboard.HasHtml) { throw 'A área de transferência não tem a versão em HTML.' }
    }

    Test-UI 'Exportar as datas do período numa planilha' {
        if ($Target -ne 'sandbox') { throw 'Este teste grava na pasta Documentos, por isso só roda no Sandbox.' }
        Invoke-Ui @('invoke', 'ExportPeriodButton') | Out-Null
        Save-WithSuggestedName
        $path = Join-Path (Get-TargetDocuments) "Fórum Regional da Lapa de $($e.periodFile).xlsx"
        try {
            Assert-Status "Planilha gravada em $path." -TimeoutSeconds 10
            Assert-TargetFile $path
            Save-Screenshot '15-datas-exportadas.png'
        } finally {
            Remove-TargetFile $path
        }
    }

    Test-UI 'Exceção num calendário da Elekto, desfeita depois' {
        try {
            Select-Calendar 'Brasil - Banco Central'
            Invoke-Ui @('invoke', 'CalendarsButton') | Out-Null
            Invoke-Ui @('wait-for', 'EditCalendarComboBox', '-t', '5000') | Out-Null
            Invoke-Ui @('invoke', (Find-Holiday "$($e.christmasLong), $($e.christmasDescription), Da Elekto")) | Out-Null
            Invoke-Ui @('invoke', 'RemoveHolidayButton') | Out-Null
            Assert-Text 'CalendarKindText' 'Calendário da Elekto, com 1 alteração sua'
            Invoke-Ui @('invoke', 'RestoreCalendarButton') | Out-Null
            Invoke-Ui @('wait-for', 'PrimaryButton', '-t', '3000') | Out-Null
            Invoke-Ui @('invoke', 'PrimaryButton') | Out-Null
            Assert-Text 'CalendarKindText' 'Calendário da Elekto, sem alterações suas'
        } finally {
            Restore-CalendarsPage
        }
    }

    Test-UI 'Copiar os feriados do ano e exportar os do calendário numa planilha' {
        if ($Target -ne 'sandbox') { throw 'Este teste troca a área de transferência e grava em Documentos, por isso só roda no Sandbox.' }
        Invoke-Ui @('invoke', 'CopyHolidaysButton') | Out-Null
        Assert-Status "$($e.holidaysThisYearCount) feriados de $($e.year) copiados."
        $clipboard = Get-TargetClipboard
        if ($clipboard.Lines -ne [int]$e.holidaysThisYearCount + 1) { throw "A área de transferência tem $($clipboard.Lines) linhas." }
        Invoke-Ui @('invoke', 'ExportHolidaysButton') | Out-Null
        Save-WithSuggestedName
        $path = Join-Path (Get-TargetDocuments) 'Feriados de Brasil - Banco Central.xlsx'
        try {
            Assert-Status "Planilha gravada em $path." -TimeoutSeconds 10
            Assert-TargetFile $path
        } finally {
            Remove-TargetFile $path
        }
    }

    Test-UI 'Exportar tudo para um arquivo' {
        if ($Target -ne 'sandbox') { throw 'Este teste grava na pasta Documentos, por isso só roda no Sandbox.' }
        Invoke-Ui @('invoke', 'ExportAllButton') | Out-Null
        Save-WithSuggestedName
        $documents = (Invoke-Target @('powershell', '-NoProfile', '-Command', "[Environment]::GetFolderPath('MyDocuments')") |
            Select-Object -Last 1).Trim()
        Assert-Status "Exportado para $(Join-Path $documents $ExportFileName)." -TimeoutSeconds 10
    }

    Test-UI 'Excluir o calendário pede confirmação' {
        Select-EditCalendar 'Fórum Regional da Lapa'
        Invoke-Ui @('invoke', 'DeleteCalendarButton') | Out-Null
        Invoke-Ui @('wait-for', 'PrimaryButton', '-t', '3000') | Out-Null
        Save-Screenshot '12-excluir.png'
        Invoke-Ui @('invoke', 'PrimaryButton') | Out-Null
        Assert-Status 'Calendário “Fórum Regional da Lapa” excluído.'
        Assert-Text 'EditCalendarComboBox' 'Brasil - Banco Central'
    }

    Test-UI 'Importar o arquivo exportado traz o calendário de volta, com a herança' {
        if ($Target -ne 'sandbox') { throw 'Este teste lê da pasta Documentos, por isso só roda no Sandbox.' }
        Invoke-Ui @('invoke', 'ImportButton') | Out-Null
        Open-FromDocuments $ExportFileName
        Invoke-Ui @('wait-for', 'ImportDialog', '-t', '5000') | Out-Null
        Save-Screenshot '13-importar.png'
        Invoke-Ui @('invoke', 'PrimaryButton') | Out-Null
        Assert-Status '1 calendário importado.'
        Assert-Text 'EditCalendarComboBox' 'Fórum Regional da Lapa'
        Assert-Text 'CalendarKindText' 'Derivado de Brasil - Banco Central, com 1 alteração sua'
        Find-Holiday "$($e.localLong), Aniversário do fórum, Incluído por você" | Out-Null
    }

    Test-UI 'Importar um arquivo de calendário da versão 1' {
        if ($Target -ne 'sandbox') { throw 'Este teste lê da pasta Documentos, por isso só roda no Sandbox.' }
        Invoke-Ui @('invoke', 'ImportButton') | Out-Null
        Open-FromDocuments $LegacyFileName
        Invoke-Ui @('wait-for', 'ImportDialog', '-t', '5000') | Out-Null
        Invoke-Ui @('invoke', 'PrimaryButton') | Out-Null
        Assert-Status '1 calendário importado.'
        Assert-Text 'EditCalendarComboBox' 'Cidade de São Paulo'
        Assert-Text 'CalendarKindText' 'Calendário próprio, sem base, com 3 feriados'
    }

    # Os atalhos valem só com o foco na lista, e o Esc numa caixa de diálogo fecha a caixa sem sair da página
    Test-UI 'Delete e F2 na lista de feriados, Esc na caixa de diálogo' {
        try {
            $july = Find-Holiday 'Revolução Constitucionalista' -Anywhere
            Invoke-Ui @('invoke', $july) | Out-Null
            Invoke-Ui @('send-keys', 'delete', '--target', $july, '--via', 'send-input') | Out-Null
            Assert-Text 'CalendarKindText' 'Calendário próprio, sem base, com 2 feriados'
            $january = Find-Holiday 'Aniversário de São Paulo' -Anywhere
            Invoke-Ui @('invoke', $january) | Out-Null
            Invoke-Ui @('send-keys', 'f2', '--target', $january, '--via', 'send-input') | Out-Null
            Invoke-Ui @('wait-for', 'HolidayDescriptionTextBox', '-t', '3000') | Out-Null
            Invoke-Ui @('send-keys', 'escape', '--target', 'HolidayDescriptionTextBox', '--via', 'send-input') | Out-Null
            Invoke-Ui @('wait-for', 'HolidayDescriptionTextBox', '--gone', '-t', '3000') | Out-Null
            Invoke-Ui @('wait-for', 'EditCalendarComboBox', '-t', '2000') | Out-Null
        } finally {
            Restore-CalendarsPage
        }
    }

    Test-UI 'Todos os controles da página de calendários têm AutomationId' {
        Assert-AutomationIds 8
    }

    Test-UI 'A página de calendários na janela mínima' {
        try {
            Set-WindowSize 500 400
            Save-Screenshot '14-calendarios-minima.png'
        } finally {
            Set-WindowSize 960 600
        }
    }

    # Terceira execução: os calendários do usuário ficam guardados no banco local
    Test-UI 'Os calendários continuam ao reabrir' {
        Stop-App
        Start-App
        Select-Calendar 'Fórum Regional da Lapa'
        Assert-Text 'WorkDaysText' $e.lapaFinancialToday
        Invoke-Ui @('invoke', 'CalendarsButton') | Out-Null
        Invoke-Ui @('wait-for', 'EditCalendarComboBox', '-t', '5000') | Out-Null
        Assert-Text 'EditCalendarComboBox' 'Fórum Regional da Lapa'
        Find-Holiday "$($e.localLong), Aniversário do fórum, Incluído por você" | Out-Null
    }

    #endregion
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
