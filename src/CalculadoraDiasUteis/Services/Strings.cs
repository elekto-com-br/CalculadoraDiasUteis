/*
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
 */
using System.Globalization;
using Microsoft.Windows.ApplicationModel.Resources;

namespace CalculadoraDiasUteis.Services;

/// <summary>
///     Os textos usados pelo código, tirados de <c>Strings\pt-BR\Resources.resw</c> (os da interface vêm por <c>x:Uid</c>).
/// </summary>
public static class Strings
{
    private static readonly ResourceLoader Loader = new();

    /// <summary>
    ///     O texto com a chave dada.
    /// </summary>
    public static string Get(string key)
    {
        return Loader.GetString(key);
    }

    /// <summary>
    ///     O texto com a chave dada, usado como formato para <paramref name="args" /> na cultura do Windows.
    /// </summary>
    public static string Format(string key, params object[] args)
    {
        return string.Format(CultureInfo.CurrentCulture, Get(key), args);
    }
}
