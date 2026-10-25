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
    Muda o tamanho de uma janela, em pixels físicos, sem movê-la. Usado pelos testes de interface, no alvo onde
    a janela está; o Windows aplica os limites de tamanho da própria janela, como faria ao arrastar a borda.
#>
param(
    [Parameter(Mandatory)][long]$Hwnd,
    [Parameter(Mandatory)][int]$Width,
    [Parameter(Mandatory)][int]$Height
)

$ErrorActionPreference = 'Stop'

Add-Type -Namespace Win32 -Name User32 -MemberDefinition @'
[DllImport("user32.dll", SetLastError = true)]
public static extern bool SetWindowPos(IntPtr hWnd, IntPtr insertAfter, int x, int y, int cx, int cy, uint flags);
'@

# SWP_NOMOVE | SWP_NOZORDER | SWP_NOACTIVATE
if (-not [Win32.User32]::SetWindowPos([IntPtr]$Hwnd, [IntPtr]::Zero, 0, 0, $Width, $Height, 0x0016)) {
    throw "SetWindowPos falhou (erro $([Runtime.InteropServices.Marshal]::GetLastWin32Error()))."
}
