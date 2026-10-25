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

namespace CalculadoraDiasUteis.Core.Tests;

/// <summary>
///     Calendários pequenos, no lugar dos do pacote, e um banco temporário para cada teste.
/// </summary>
internal static class TestData
{
    public static readonly DateTime Carnaval = new(2026, 2, 16);
    public static readonly DateTime Tiradentes = new(2026, 4, 21);
    public static readonly DateTime ConscienciaNegra = new(2026, 11, 20);
    public static readonly DateTime Thanksgiving = new(2026, 11, 26);

    public static CalendarInfo BrBc(params (DateTime Date, string Description)[] extra)
    {
        (DateTime, string)[] holidays =
        [
            (new DateTime(2026, 1, 1), "Confraternização Universal"),
            (Carnaval, "Carnaval"),
            (new DateTime(2026, 2, 17), "Carnaval"),
            (Tiradentes, "Tiradentes"),
            (ConscienciaNegra, "Dia Nacional de Zumbi e da Consciência Negra"),
            (new DateTime(2026, 12, 25), "Natal"),
            (new DateTime(2027, 1, 1), "Confraternização Universal"),
            .. extra
        ];
        return CalendarInfo.New("br-BC", holidays.Select(h => h.Item1), "Brasil - Banco Central",
            holidayDescriptions: holidays.Select(h => new KeyValuePair<DateTime, string>(h.Item1, h.Item2)));
    }

    public static CalendarInfo UsNyc()
    {
        (DateTime, string)[] holidays =
        [
            (new DateTime(2026, 1, 1), "New Year's Day"),
            (new DateTime(2026, 1, 19), "Martin Luther King Jr. Day"),
            (new DateTime(2026, 7, 3), "Independence Day (observed)"),
            (Thanksgiving, "Thanksgiving Day"),
            (new DateTime(2026, 12, 25), "Christmas Day")
        ];
        return CalendarInfo.New("us-NYC", holidays.Select(h => h.Item1), "Estados Unidos - Nova Iorque",
            holidayDescriptions: holidays.Select(h => new KeyValuePair<DateTime, string>(h.Item1, h.Item2)));
    }

    public static CalendarCatalog Catalog(CalendarStore store, params CalendarInfo[] originals) =>
        new(originals.Length > 0 ? originals : [BrBc(), UsNyc()], store);
}

/// <summary>
///     Um arquivo de banco numa pasta temporária, apagado no fim do teste.
/// </summary>
internal sealed class TempStore : IDisposable
{
    private readonly DirectoryInfo _folder = Directory.CreateTempSubdirectory("calendarios-");

    public string Path => System.IO.Path.Combine(_folder.FullName, "calendarios.db");

    public CalendarStore Open() => new(Path);

    public void Dispose() => _folder.Delete(true);
}
