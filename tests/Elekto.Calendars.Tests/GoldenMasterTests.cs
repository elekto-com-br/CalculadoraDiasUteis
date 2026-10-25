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
using System.IO.Compression;
using System.Text;

namespace Elekto.Calendars.Tests;

/// <summary>
///     Refaz na biblioteca as chamadas gravadas a partir do executável da versão 1.2 e exige o mesmo resultado.
/// </summary>
/// <remarks>
///     O arquivo de referência e a cópia congelada dos calendários estão em GoldenMaster; o README de lá explica a origem.
/// </remarks>
[TestClass]
public sealed class GoldenMasterTests
{
    private static readonly string Folder = Path.Combine(AppContext.BaseDirectory, "GoldenMaster");

    private static readonly Lazy<IReadOnlyList<Row>> Rows = new(LoadRows);

    private static readonly Lazy<TextsInFolderCalendarInfoProvider> Provider =
        new(() => new TextsInFolderCalendarInfoProvider(Path.Combine(Folder, "calendars-1.2")));

    private static readonly Lazy<CalendarServer> Server = new(() => new CalendarServer(Provider.Value));

    [TestMethod]
    [DataRow("br-BC")]
    [DataRow("br-SP")]
    [DataRow("us-NYC")]
    [DataRow("gb-LND")]
    [DataRow("br-BC+us-NYC")]
    public void ReproduzOsResultadosDaVersao12(string calendarName)
    {
        var calendar = calendarName == "br-BC+us-NYC"
            ? Server.Value.GetCalendar("br-BC").UnionWith(Server.Value.GetCalendar("us-NYC"))
            : Server.Value.GetCalendar(calendarName);

        var rows = Rows.Value.Where(r => r.Calendar == calendarName).ToList();
        var failures = rows.Select(r => (Row: r, Expected: ExpectedFor(calendar, r), Actual: Run(calendar, r)))
                           .Where(x => x.Actual != x.Expected)
                           .ToList();

        Assert.IsTrue(rows.Count > 5000, $"Só {rows.Count} casos para {calendarName}; o arquivo de referência está incompleto?");
        Assert.AreEqual(0, failures.Count,
            $"{failures.Count} de {rows.Count} casos diferem da 1.2:{Environment.NewLine}" +
            string.Join(Environment.NewLine, failures.Take(20).Select(f => $"{f.Row}: esperado {Cut(f.Expected)}, obtido {Cut(f.Actual)}")));
    }

    [TestMethod]
    public void ProvedorListaOsCalendariosComoNaVersao12()
    {
        Assert.AreEqual(Expected("EnumCalendarNames"), string.Join(" ", Provider.Value.EnumCalendarNames()));
        Assert.AreEqual(Expected("DefaultCalendarName"), Provider.Value.DefaultCalendarName);
    }

    /// <summary>
    ///     O resultado da 1.2, menos onde a 2.0 o corrigiu de propósito.
    /// </summary>
    /// <remarks>
    ///     Na 1.2, um prazo negativo de mais de 5 dias úteis contado de um dia não útil depois do último dia útil do
    ///     calendário lançava exceção, enquanto um de até 5 devolvia a data. Desde a fase 2 da 2.0, o resultado é sempre o
    ///     da contagem um a um (ver docs/ROADMAP.md, fase 0).
    /// </remarks>
    private static string ExpectedFor(ICalendar c, Row r)
    {
        var workDays = r.Operation switch
        {
            "AddWorkDays" => r.Int(2),
            "AddDays" when r.Enum<PeriodType>(3) == PeriodType.WorkDays => r.Int(2),
            _ => 0
        };

        var date = workDays < -5 ? r.Date(1) : default;
        if (workDays >= -5 || r.Expected != "!ArgumentOutOfRangeException" || date <= c.MaxWorkDate || date > c.MaxDate)
        {
            return r.Expected;
        }

        for (var i = 0; i > workDays; i--)
        {
            date = c.GetPrevWorkday(date);
        }

        return Format(date);
    }

    private static string Run(ICalendar c, Row r)
    {
        try
        {
            return r.Operation switch
            {
                "Name" => c.Name,
                "Description" => c.Description,
                "MinDate" => Format(c.MinDate),
                "MaxDate" => Format(c.MaxDate),
                "MinWorkDate" => Format(c.MinWorkDate),
                "MaxWorkDate" => Format(c.MaxWorkDate),
                "Holidays" => Format(c.Holidays),
                "NonWorkWeekDays" => string.Join(" ", c.NonWorkWeekDays),
                "IsWorkday" => Format(c.IsWorkday(r.Date(1))),
                "IsProperHoliday" => Format(c.IsProperHoliday(r.Date(1))),
                "GetNextWorkday" => Format(c.GetNextWorkday(r.Date(1))),
                "GetNextOrSameWorkday" => Format(c.GetNextOrSameWorkday(r.Date(1))),
                "GetPrevWorkday" => Format(c.GetPrevWorkday(r.Date(1))),
                "GetPrevOrSameWorkday" => Format(c.GetPrevOrSameWorkday(r.Date(1))),
                "AdjustFinalDate" => Format(c.AdjustFinalDate(r.Date(1), r.Enum<FinalDateAdjust>(2))),
                "AddWorkDays" => Format(c.AddWorkDays(r.Date(1), r.Int(2), r.Enum<FinalDateAdjust>(3))),
                "AddActualDays" => Format(c.AddActualDays(r.Date(1), r.Int(2), r.Enum<FinalDateAdjust>(3))),
                "AddDays" => Format(c.AddDays(r.Date(1), r.Int(2), r.Enum<PeriodType>(3))),
                "GetDeltaDays" => Format(c.GetDeltaDays(r.Date(1), r.Date(2), r.Enum<PeriodType>(3))),
                "GetDeltaWorkDays" => Format(c.GetDeltaWorkDays(r.Date(1), r.Date(2), r.Enum<DeltaTerminalDayAdjust>(3))),
                "GetDeltaActualDays" => Format(c.GetDeltaActualDays(r.Date(1), r.Date(2), r.Enum<DeltaTerminalDayAdjust>(3))),
                "GetWorkDates" => Format(c.GetWorkDates(r.Date(1), r.Date(2), r.Enum<DeltaTerminalDayAdjust>(3))),
                "GetNonWorkDates" => Format(c.GetNonWorkDates(r.Date(1), r.Date(2))),
                "GetActualMonthHead" => Format(c.GetActualMonthHead(r.Date(1), r.Int(2))),
                "GetActualMonthTail" => Format(c.GetActualMonthTail(r.Date(1), r.Int(2))),
                "GetWorkingMonthHead" => Format(c.GetWorkingMonthHead(r.Date(1), r.Int(2))),
                "GetWorkingMonthTail" => Format(c.GetWorkingMonthTail(r.Date(1), r.Int(2))),
                "GetNthWorkingDay" => Format(c.GetNthWorkingDay(r.Date(1), r.Int(2), r.Int(3))),
                "GetNthToLastWorkingDay" => Format(c.GetNthToLastWorkingDay(r.Date(1), r.Int(2), r.Int(3))),
                _ => throw new UnknownOperationException(r.Operation)
            };
        }
        catch (Exception e) when (e is not UnknownOperationException)
        {
            return "!" + e.GetType().Name;
        }
    }

    private static string Format(DateTime d) => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static string Format(int i) => i.ToString(CultureInfo.InvariantCulture);

    private static string Format(bool b) => b ? "true" : "false";

    private static string Format(IEnumerable<DateTime> dates) =>
        string.Join(" ", dates.Select(d => d.ToString("yyyyMMdd", CultureInfo.InvariantCulture)));

    private static string Cut(string s) => s.Length <= 80 ? s : s[..80] + "...";

    private static string Expected(string providerOperation) =>
        Rows.Value.Single(r => r.Calendar == "provider" && r.Operation == providerOperation).Expected;

    private static IReadOnlyList<Row> LoadRows()
    {
        using var file = File.OpenRead(Path.Combine(Folder, "golden-1.2.csv.gz"));
        using var gzip = new GZipStream(file, CompressionMode.Decompress);
        using var reader = new StreamReader(gzip, Encoding.UTF8);

        var rows = new List<Row>();
        while (reader.ReadLine() is { } line)
        {
            if (line.StartsWith('#'))
            {
                continue;
            }

            var fields = line.Split(';');
            rows.Add(new Row(fields[0], fields[1], fields[2..5], fields[5]));
        }

        return rows;
    }

    private sealed record Row(string Calendar, string Operation, string[] Args, string Expected)
    {
        public DateTime Date(int position) =>
            DateTime.ParseExact(Args[position - 1], "yyyy-MM-dd", CultureInfo.InvariantCulture);

        public int Int(int position) => int.Parse(Args[position - 1], CultureInfo.InvariantCulture);

        public T Enum<T>(int position) where T : struct, System.Enum => System.Enum.Parse<T>(Args[position - 1]);

        public override string ToString() => $"{Calendar};{Operation};{string.Join(";", Args)}";
    }

    private sealed class UnknownOperationException(string operation)
        : Exception($"Operação desconhecida no arquivo de referência: {operation}");
}
