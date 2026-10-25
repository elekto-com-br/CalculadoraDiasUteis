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
using Elekto.Calendars;

namespace CalculadoraDiasUteis.Services;

/// <summary>
///     Os calendários enviados com o aplicativo, lidos da pasta <c>Calendars</c> do pacote.
/// </summary>
public static class CalendarCatalog
{
    /// <summary>
    ///     Carrega os calendários na ordem dos arquivos (o primeiro é o padrão).
    /// </summary>
    /// <exception cref="InvalidOperationException">A pasta não existe ou não tem calendário algum.</exception>
    public static IReadOnlyList<ICalendar> Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Calendars");
        if (!Directory.Exists(path))
        {
            throw new InvalidOperationException(Strings.Format("ErrorCalendarsFolderMissing", path));
        }

        var provider = new TextsInFolderCalendarInfoProvider(path);
        var server = new CalendarServer(provider);
        var calendars = provider.EnumCalendarNames().Select(server.GetCalendar).ToList();
        if (calendars.Count == 0)
        {
            throw new InvalidOperationException(Strings.Format("ErrorNoCalendars", path));
        }

        return calendars;
    }
}
