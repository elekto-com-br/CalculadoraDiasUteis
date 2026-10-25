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

namespace Elekto.Calendars.Tests;

/// <summary>
///     Confere os calendários enviados com a aplicação contra as regras de feriado de cada praça, de 2000 em diante:
///     as datas e a descrição de cada uma.
/// </summary>
/// <remarks>
///     Só dias de semana entram na comparação das datas: feriado em fim de semana não muda nenhuma conta. Quando uma lei ou
///     uma bolsa criar, mudar ou extinguir um feriado, atualize a regra aqui e o arquivo em src/Elekto.Calendars/Calendars,
///     com a descrição. As descrições ficam no idioma da praça.
/// </remarks>
[TestClass]
public sealed class CalendarRulesTests
{
    private const int FirstYear = 2000;

    /// <summary>
    ///     Fechamentos da B3 que nenhuma regra explica e que constam do calendário desde antes da 1.2.
    /// </summary>
    private static readonly Holiday[] B3OneOff =
    [
        new(new DateTime(2011, 12, 30), "Sem pregão no fim de ano"), // véspera de um 31/12 em sábado
        new(new DateTime(2014, 6, 12), "Abertura da Copa do Mundo") // em São Paulo
    ];

    private static readonly Holiday[] NyseOneOff =
    [
        new(new DateTime(2001, 9, 11), "September 11 attacks"),
        new(new DateTime(2001, 9, 12), "September 11 attacks"),
        new(new DateTime(2001, 9, 13), "September 11 attacks"),
        new(new DateTime(2001, 9, 14), "September 11 attacks"),
        new(new DateTime(2004, 6, 11), "National Day of Mourning for Ronald Reagan"),
        new(new DateTime(2007, 1, 2), "National Day of Mourning for Gerald Ford"),
        new(new DateTime(2012, 10, 29), "Hurricane Sandy"),
        new(new DateTime(2012, 10, 30), "Hurricane Sandy"),
        new(new DateTime(2018, 12, 5), "National Day of Mourning for George H. W. Bush"),
        new(new DateTime(2025, 1, 9), "National Day of Mourning for Jimmy Carter")
    ];

    /// <summary>
    ///     Feriados bancários regulares da Inglaterra que foram remanejados em um ano: a data regular e a efetiva.
    /// </summary>
    private static readonly (DateTime Regular, Holiday Moved)[] EnglandMoved =
    [
        (new DateTime(2002, 5, 27), new Holiday(new DateTime(2002, 6, 4), "Spring bank holiday")), // Golden Jubilee
        (new DateTime(2012, 5, 28), new Holiday(new DateTime(2012, 6, 4), "Spring bank holiday")), // Diamond Jubilee
        (new DateTime(2020, 5, 4), new Holiday(new DateTime(2020, 5, 8), "Early May bank holiday (VE Day)")),
        (new DateTime(2022, 5, 30), new Holiday(new DateTime(2022, 6, 2), "Spring bank holiday")) // Platinum Jubilee
    ];

    private static readonly Holiday[] EnglandOneOff =
    [
        new(new DateTime(2002, 6, 3), "Golden Jubilee bank holiday"),
        new(new DateTime(2011, 4, 29), "Royal wedding"),
        new(new DateTime(2012, 6, 5), "Diamond Jubilee bank holiday"),
        new(new DateTime(2022, 6, 3), "Platinum Jubilee bank holiday"),
        new(new DateTime(2022, 9, 19), "State Funeral of Queen Elizabeth II"),
        new(new DateTime(2023, 5, 8), "Coronation of King Charles III")
    ];

    private static readonly Lazy<TextsInFolderCalendarInfoProvider> Provider =
        new(() => new TextsInFolderCalendarInfoProvider(Path.Combine(AppContext.BaseDirectory, "Calendars")));

    private static readonly Lazy<CalendarServer> Server = new(() => new CalendarServer(Provider.Value));

    [TestMethod]
    [DataRow("br-BC")]
    [DataRow("br-SP")]
    [DataRow("us-NYC")]
    [DataRow("gb-LND")]
    public void CalendarioSegueAsRegrasDaPraca(string name)
    {
        var calendar = Server.Value.GetCalendar(name);
        var (first, last) = Years(calendar);

        var expected = Expected(name, first, last).Select(h => h.Date).Where(IsWeekday).ToHashSet();
        var actual = calendar.Holidays.Where(d => d.Year >= first && d.Year <= last && IsWeekday(d)).ToHashSet();

        var missing = expected.Except(actual).Order().ToList();
        var extra = actual.Except(expected).Order().ToList();
        Assert.IsTrue(missing.Count == 0 && extra.Count == 0,
            $"{name} ({first} a {last}) não bate com as regras.{Environment.NewLine}" +
            $"Faltam: {Join(missing)}{Environment.NewLine}Sobram: {Join(extra)}");
    }

    [TestMethod]
    [DataRow("br-BC")]
    [DataRow("br-SP")]
    [DataRow("us-NYC")]
    [DataRow("gb-LND")]
    public void FeriadoTemADescricaoDaRegra(string name)
    {
        var calendar = Server.Value.GetCalendar(name);
        var descriptions = Provider.Value.GetCalendarInfo(name).HolidayDescriptions;
        var (first, last) = Years(calendar);

        // uma data com duas regras, como a Sexta-feira Santa num 21 de abril, leva as duas descrições
        var expected = Expected(name, first, last)
                       .GroupBy(h => h.Date)
                       .ToDictionary(g => g.Key, g => string.Join(" e ", g.Select(h => h.Name).Distinct()));
        var wrong = calendar.Holidays
                            .Where(d => d.Year >= first && d.Year <= last)
                            .Select(d => (Date: d, Expected: expected.GetValueOrDefault(d), Actual: descriptions.GetValueOrDefault(d)))
                            .Where(x => x.Actual != x.Expected)
                            .ToList();

        Assert.IsEmpty(wrong, $"{name}: {wrong.Count} descrições diferentes das regras.{Environment.NewLine}" +
                              string.Join(Environment.NewLine, wrong.Take(20).Select(x =>
                                  $"{x.Date:yyyy-MM-dd}: esperado '{x.Expected}', no arquivo '{x.Actual}'")));
    }

    private static (int First, int Last) Years(ICalendar calendar)
    {
        var first = Math.Max(FirstYear, calendar.MinDate.Year);
        // O último ano pode estar incompleto no arquivo; ele fica de fora.
        var last = calendar.MaxDate.Month == 12 ? calendar.MaxDate.Year : calendar.MaxDate.Year - 1;
        return (first, last);
    }

    private static IEnumerable<Holiday> Expected(string name, int first, int last)
    {
        Func<int, IEnumerable<Holiday>> rule = name switch
        {
            "br-BC" => BrazilNational,
            "br-SP" => B3,
            "us-NYC" => Nyse,
            "gb-LND" => England,
            _ => throw new ArgumentOutOfRangeException(nameof(name), name, null)
        };
        return Enumerable.Range(first, last - first + 1).SelectMany(rule);
    }

    /// <summary>
    ///     Feriados nacionais do Brasil, que são os dias sem expediente bancário do Banco Central.
    /// </summary>
    private static IEnumerable<Holiday> BrazilNational(int year)
    {
        var easter = Easter(year);
        Holiday[] holidays =
        [
            new(new DateTime(year, 1, 1), "Confraternização Universal"),
            new(easter.AddDays(-48), "Carnaval"),
            new(easter.AddDays(-47), "Carnaval"),
            new(easter.AddDays(-2), "Sexta-feira Santa"),
            new(new DateTime(year, 4, 21), "Tiradentes"),
            new(new DateTime(year, 5, 1), "Dia do Trabalho"),
            new(easter.AddDays(60), "Corpus Christi"),
            new(new DateTime(year, 9, 7), "Independência"),
            new(new DateTime(year, 10, 12), "Nossa Senhora Aparecida"),
            new(new DateTime(year, 11, 2), "Finados"),
            new(new DateTime(year, 11, 15), "Proclamação da República"),
            new(new DateTime(year, 12, 25), "Natal")
        ];
        // Lei 14.759/2023
        return year >= 2024
            ? holidays.Append(new Holiday(new DateTime(year, 11, 20), "Dia Nacional de Zumbi e da Consciência Negra"))
            : holidays;
    }

    /// <summary>
    ///     Dias sem negociação na B3: os feriados nacionais, as vésperas de Natal e de Ano Novo e, até 2021, os
    ///     feriados municipais e estaduais de São Paulo (25/01, 09/07 e 20/11).
    /// </summary>
    private static IEnumerable<Holiday> B3(int year)
    {
        var days = BrazilNational(year).Concat([
            new Holiday(new DateTime(year, 12, 24), "Véspera de Natal"),
            new Holiday(new DateTime(year, 12, 31), "Véspera de Ano Novo")
        ]);
        if (year <= 2021)
        {
            days = days.Concat([
                new Holiday(new DateTime(year, 1, 25), "Aniversário de São Paulo"),
                new Holiday(new DateTime(year, 7, 9), "Revolução Constitucionalista"),
                new Holiday(new DateTime(year, 11, 20), "Dia da Consciência Negra")
            ]);
        }

        return days.Concat(B3OneOff.Where(h => h.Date.Year == year));
    }

    /// <summary>
    ///     Dias sem negociação na NYSE. Feriado no sábado fecha a sexta anterior, e no domingo a segunda seguinte,
    ///     exceto o Ano Novo, que nunca fecha o 31 de dezembro.
    /// </summary>
    private static IEnumerable<Holiday> Nyse(int year)
    {
        var newYear = new DateTime(year, 1, 1);
        yield return newYear.DayOfWeek == DayOfWeek.Sunday
            ? new Holiday(newYear.AddDays(1), "New Year's Day (observed)")
            : new Holiday(newYear, "New Year's Day");
        yield return new Holiday(NthWeekday(year, 1, DayOfWeek.Monday, 3), "Martin Luther King Jr. Day");
        yield return new Holiday(NthWeekday(year, 2, DayOfWeek.Monday, 3), "Washington's Birthday");
        yield return new Holiday(Easter(year).AddDays(-2), "Good Friday");
        yield return new Holiday(NthWeekday(year, 5, DayOfWeek.Monday, -1), "Memorial Day");
        if (year >= 2022)
        {
            yield return Observed(new Holiday(new DateTime(year, 6, 19), "Juneteenth"));
        }

        yield return Observed(new Holiday(new DateTime(year, 7, 4), "Independence Day"));
        yield return new Holiday(NthWeekday(year, 9, DayOfWeek.Monday, 1), "Labor Day");
        yield return new Holiday(NthWeekday(year, 11, DayOfWeek.Thursday, 4), "Thanksgiving Day");
        yield return Observed(new Holiday(new DateTime(year, 12, 25), "Christmas Day"));

        foreach (var day in NyseOneOff.Where(h => h.Date.Year == year))
        {
            yield return day;
        }
    }

    /// <summary>
    ///     Feriados bancários da Inglaterra e do País de Gales, que a Bolsa de Londres segue.
    /// </summary>
    private static IEnumerable<Holiday> England(int year)
    {
        const string substitute = " (substitute day)";
        var easter = Easter(year);
        var newYear = new DateTime(year, 1, 1);
        var newYearHoliday = newYear.DayOfWeek switch
        {
            DayOfWeek.Saturday => new Holiday(newYear.AddDays(2), "New Year's Day" + substitute),
            DayOfWeek.Sunday => new Holiday(newYear.AddDays(1), "New Year's Day" + substitute),
            _ => new Holiday(newYear, "New Year's Day")
        };

        // Natal e Boxing Day em fim de semana passam para os próximos dias úteis livres.
        var christmas = new Holiday(new DateTime(year, 12, 25), "Christmas Day");
        var boxingDay = new Holiday(new DateTime(year, 12, 26), "Boxing Day");
        switch (christmas.Date.DayOfWeek)
        {
            case DayOfWeek.Saturday:
                christmas = new Holiday(christmas.Date.AddDays(2), christmas.Name + substitute);
                boxingDay = new Holiday(boxingDay.Date.AddDays(2), boxingDay.Name + substitute);
                break;
            case DayOfWeek.Sunday:
                christmas = new Holiday(christmas.Date.AddDays(2), christmas.Name + substitute);
                break;
            case DayOfWeek.Friday:
                boxingDay = new Holiday(boxingDay.Date.AddDays(2), boxingDay.Name + substitute);
                break;
        }

        var days = new List<Holiday>
        {
            newYearHoliday,
            new(easter.AddDays(-2), "Good Friday"),
            new(easter.AddDays(1), "Easter Monday"),
            new(NthWeekday(year, 5, DayOfWeek.Monday, 1), "Early May bank holiday"),
            new(NthWeekday(year, 5, DayOfWeek.Monday, -1), "Spring bank holiday"),
            new(NthWeekday(year, 8, DayOfWeek.Monday, -1), "Summer bank holiday"),
            christmas,
            boxingDay
        };

        foreach (var (regular, moved) in EnglandMoved.Where(m => m.Regular.Year == year))
        {
            days.RemoveAll(h => h.Date == regular);
            days.Add(moved);
        }

        return days.Concat(EnglandOneOff.Where(h => h.Date.Year == year));
    }

    /// <summary>
    ///     Domingo de Páscoa no calendário gregoriano (algoritmo de Meeus/Jones/Butcher).
    /// </summary>
    private static DateTime Easter(int year)
    {
        var a = year % 19;
        var b = year / 100;
        var c = year % 100;
        var d = b / 4;
        var e = b % 4;
        var f = (b + 8) / 25;
        var g = (b - f + 1) / 3;
        var h = (19 * a + b - d - g + 15) % 30;
        var i = c / 4;
        var k = c % 4;
        var l = (32 + 2 * e + 2 * i - h - k) % 7;
        var m = (a + 11 * h + 22 * l) / 451;
        var month = (h + l - 7 * m + 114) / 31;
        var day = (h + l - 7 * m + 114) % 31 + 1;
        return new DateTime(year, month, day);
    }

    /// <summary>
    ///     O n-ésimo dia da semana do mês; n = -1 é o último.
    /// </summary>
    private static DateTime NthWeekday(int year, int month, DayOfWeek dayOfWeek, int n)
    {
        if (n > 0)
        {
            var first = new DateTime(year, month, 1);
            return first.AddDays(((int)dayOfWeek - (int)first.DayOfWeek + 7) % 7 + 7 * (n - 1));
        }

        var last = new DateTime(year, month, DateTime.DaysInMonth(year, month));
        return last.AddDays(-(((int)last.DayOfWeek - (int)dayOfWeek + 7) % 7));
    }

    private static Holiday Observed(Holiday holiday) => holiday.Date.DayOfWeek switch
    {
        DayOfWeek.Saturday => new Holiday(holiday.Date.AddDays(-1), holiday.Name + " (observed)"),
        DayOfWeek.Sunday => new Holiday(holiday.Date.AddDays(1), holiday.Name + " (observed)"),
        _ => holiday
    };

    private static bool IsWeekday(DateTime date) => date.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday);

    private static string Join(IEnumerable<DateTime> dates)
    {
        var text = string.Join(" ", dates.Select(d => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));
        return text.Length == 0 ? "nada" : text;
    }

    private sealed record Holiday(DateTime Date, string Name);
}
