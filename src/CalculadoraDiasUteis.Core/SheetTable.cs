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
using Elekto.Calendars;

namespace CalculadoraDiasUteis.Core;

/// <summary>
///     O tipo dos valores de uma coluna, para cada formato gravar do jeito certo: data como data, número como número.
/// </summary>
public enum ColumnKind
{
    /// <summary>Datas, guardadas como <see cref="DateTime" />.</summary>
    Date,

    /// <summary>Texto.</summary>
    Text,

    /// <summary>Números inteiros.</summary>
    Number
}

/// <summary>
///     Uma coluna de tabela, com o cabeçalho já no idioma do usuário.
/// </summary>
public sealed record TableColumn(string Header, ColumnKind Kind);

/// <summary>
///     Uma tabela pronta para copiar ou gravar em planilha: um nome, as colunas e as linhas.
/// </summary>
/// <param name="Name">O nome, usado na aba da planilha.</param>
/// <param name="Columns">As colunas.</param>
/// <param name="Rows">As linhas; cada valor é <see cref="DateTime" />, <see cref="string" />, <see cref="int" /> ou nulo.</param>
public sealed record SheetTable(string Name, IReadOnlyList<TableColumn> Columns, IReadOnlyList<object?[]> Rows);

/// <summary>
///     Os textos das tabelas, que vêm dos recursos do aplicativo.
/// </summary>
public sealed record SheetTexts(
    string PeriodSheet,
    string HolidaysSheet,
    string Date,
    string WeekDay,
    string IsWorkday,
    string WorkdayCount,
    string Holiday,
    string Description);

/// <summary>
///     As tabelas que a calculadora exporta: as datas de um período e os feriados de um calendário.
/// </summary>
public static class SheetTables
{
    /// <summary>
    ///     Todas as datas entre a inicial e a final, inclusive, com o dia da semana, a marca de dia útil (1 ou 0, como
    ///     na 1.x, para somar na planilha), os dias úteis contados desde a inicial e o feriado, se houver.
    /// </summary>
    public static SheetTable Period(CatalogEntry entry, DateTime start, DateTime end, SheetTexts texts,
        CultureInfo culture)
    {
        var calendar = entry.Calendar ?? throw new CatalogException(entry.Error ?? CatalogError.NoWorkDays, entry.ErrorDetail);
        start = start.Date;
        end = end.Date;
        if (start > end)
        {
            (start, end) = (end, start);
        }

        if (start < calendar.MinDate || end > calendar.MaxDate)
        {
            throw new CatalogException(CatalogError.DateOutsideRange, (start < calendar.MinDate ? start : end)
                .ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        }

        var rows = new List<object?[]>();
        var workdays = 0;
        for (var date = start; date <= end; date = date.AddDays(1))
        {
            var isWorkday = calendar.IsWorkday(date);
            if (isWorkday)
            {
                workdays++;
            }

            // feriado sem descrição leva o nome da coluna, para não ficar igual a um dia comum
            var holiday = entry.HolidayAt(date) is { Origin: not HolidayOrigin.Removed } h ? h.Description ?? texts.Holiday : null;
            rows.Add([date, date.ToString("dddd", culture), isWorkday ? 1 : 0, workdays, holiday]);
        }

        return new SheetTable(texts.PeriodSheet,
        [
            new TableColumn(texts.Date, ColumnKind.Date),
            new TableColumn(texts.WeekDay, ColumnKind.Text),
            new TableColumn(texts.IsWorkday, ColumnKind.Number),
            new TableColumn(texts.WorkdayCount, ColumnKind.Number),
            new TableColumn(texts.Holiday, ColumnKind.Text)
        ], rows);
    }

    /// <summary>
    ///     Os feriados em vigor de um calendário, de um ano ou de todos, com o dia da semana e a descrição.
    /// </summary>
    public static SheetTable Holidays(CatalogEntry entry, int? year, SheetTexts texts, CultureInfo culture)
    {
        var rows = entry.Holidays
                        .Where(h => h.Origin != HolidayOrigin.Removed && (year == null || h.Date.Year == year))
                        .Select(h => new object?[] { h.Date, h.Date.ToString("dddd", culture), h.Description })
                        .ToList();

        return new SheetTable(texts.HolidaysSheet,
        [
            new TableColumn(texts.Date, ColumnKind.Date),
            new TableColumn(texts.WeekDay, ColumnKind.Text),
            new TableColumn(texts.Description, ColumnKind.Text)
        ], rows);
    }
}
