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
using System.Text;

namespace CalculadoraDiasUteis.Core;

/// <summary>
///     Códigos dos calendários do usuário, tirados do nome: "Fórum da Lapa" vira "forum-da-lapa".
/// </summary>
public static class CalendarCodes
{
    private const int MaxLength = 40;

    /// <summary>
    ///     Um código novo para o nome, diferente dos que já existem.
    /// </summary>
    /// <param name="name">O nome do calendário.</param>
    /// <param name="inUse">Diz se um código já está em uso, sem distinguir maiúsculas.</param>
    public static string New(string name, Func<string, bool> inUse)
    {
        var code = FromName(name);
        if (!inUse(code))
        {
            return code;
        }

        for (var i = 2;; i++)
        {
            var candidate = $"{code}-{i.ToString(CultureInfo.InvariantCulture)}";
            if (!inUse(candidate))
            {
                return candidate;
            }
        }
    }

    private static string FromName(string name)
    {
        // sem acentos, em minúsculas, e um hífen no lugar de cada sequência de outros caracteres
        var builder = new StringBuilder();
        foreach (var c in name.Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsAsciiLetterOrDigit(c))
            {
                builder.Append(char.ToLowerInvariant(c));
            }
            else if (builder.Length > 0 && builder[^1] != '-')
            {
                builder.Append('-');
            }
        }

        var code = builder.ToString().Trim('-');
        if (code.Length > MaxLength)
        {
            code = code[..MaxLength].TrimEnd('-');
        }

        return code.Length == 0 ? "calendario" : code;
    }
}
