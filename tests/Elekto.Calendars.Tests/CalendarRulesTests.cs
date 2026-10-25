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
///     Confere os calendários enviados com a aplicação contra as regras de feriado de cada praça, de 2000 em diante.
/// </summary>
/// <remarks>
///     Só dias de semana entram na comparação: feriado em fim de semana não muda nenhuma conta. Quando uma lei ou uma
///     bolsa criar, mudar ou extinguir um feriado, atualize a regra aqui e o arquivo em src/Elekto.Calendars/Calendars.
/// </remarks>
[TestClass]
public sealed class CalendarRulesTests
{
    private const int FirstYear = 2000;

    /// <summary>
    ///     Fechamentos da B3 que nenhuma regra explica e que constam do calendário desde antes da 1.2.
    /// </summary>
    private static readonly DateTime[] B3OneOff =
    [
        new(2011, 12, 30), // véspera de um 31/12 em sábado
        new(2014, 6, 12) // abertura da Copa do Mundo em São Paulo
    ];

    private static readonly DateTime[] NyseOneOff =
    [
        new(2001, 9, 11), new(2001, 9, 12), new(2001, 9, 13), new(2001, 9, 14), // atentados de 11 de setembro
        new(2004, 6, 11), // luto por Ronald Reagan
        new(2007, 1, 2), // luto por Gerald Ford
        new(2012, 10, 29), new(2012, 10, 30), // furacão Sandy
        new(2018, 12, 5), // luto por George H. W. Bush
        new(2025, 1, 9) // luto por Jimmy Carter
    ];

    /// <summary>
    ///     Feriados bancários regulares da Inglaterra que foram remanejados em um ano: (data regular, data efetiva).
    /// </summary>
    private static readonly (DateTime Regular, DateTime Moved)[] EnglandMoved =
    [
        (new(2002, 5, 27), new(2002, 6, 4)), // Golden Jubilee
        (new(2012, 5, 28), new(2012, 6, 4)), // Diamond Jubilee
        (new(2020, 5, 4), new(2020, 5, 8)), // 75 anos do Dia da Vitória na Europa
        (new(2022, 5, 30), new(2022, 6, 2)) // Platinum Jubilee
    ];

    private static readonly DateTime[] EnglandOneOff =
    [
        new(2002, 6, 3), // Golden Jubilee
        new(2011, 4, 29), // casamento do príncipe William
        new(2012, 6, 5), // Diamond Jubilee
        new(2022, 6, 3), // Platinum Jubilee
        new(2022, 9, 19), // funeral da rainha Elizabeth II
        new(2023, 5, 8) // coroação do rei Charles III
    ];

    private static readonly Lazy<CalendarServer> Server = new(() =>
        new CalendarServer(new TextsInFolderCalendarInfoProvider(Path.Combine(AppContext.BaseDirectory, "Calendars"))));

    [TestMethod]
    [DataRow("br-BC")]
    [DataRow("br-SP")]
    [DataRow("us-NYC")]
    [DataRow("gb-LND")]
    public void CalendarioSegueAsRegrasDaPraca(string name)
    {
        Func<int, IEnumerable<DateTime>> rule = name switch
        {
            "br-BC" => BrazilNational,
            "br-SP" => B3,
            "us-NYC" => Nyse,
            "gb-LND" => England,
            _ => throw new ArgumentOutOfRangeException(nameof(name), name, null)
        };

        var calendar = Server.Value.GetCalendar(name);
        var first = Math.Max(FirstYear, calendar.MinDate.Year);
        // O último ano pode estar incompleto no arquivo; ele fica de fora.
        var last = calendar.MaxDate.Month == 12 ? calendar.MaxDate.Year : calendar.MaxDate.Year - 1;

        var expected = Enumerable.Range(first, last - first + 1).SelectMany(rule).Where(IsWeekday).ToHashSet();
        var actual = calendar.Holidays.Where(d => d.Year >= first && d.Year <= last && IsWeekday(d)).ToHashSet();

        var missing = expected.Except(actual).Order().ToList();
        var extra = actual.Except(expected).Order().ToList();
        Assert.IsTrue(missing.Count == 0 && extra.Count == 0,
            $"{name} ({first} a {last}) não bate com as regras.{Environment.NewLine}" +
            $"Faltam: {Join(missing)}{Environment.NewLine}Sobram: {Join(extra)}");
    }

    /// <summary>
    ///     Feriados nacionais do Brasil, que são os dias sem expediente bancário do Banco Central.
    /// </summary>
    private static IEnumerable<DateTime> BrazilNational(int year)
    {
        var easter = Easter(year);
        DateTime[] holidays =
        [
            new(year, 1, 1), // Confraternização Universal
            easter.AddDays(-48), easter.AddDays(-47), // Carnaval
            easter.AddDays(-2), // Sexta-feira Santa
            new(year, 4, 21), // Tiradentes
            new(year, 5, 1), // Dia do Trabalho
            easter.AddDays(60), // Corpus Christi
            new(year, 9, 7), // Independência
            new(year, 10, 12), // Nossa Senhora Aparecida
            new(year, 11, 2), // Finados
            new(year, 11, 15), // Proclamação da República
            new(year, 12, 25) // Natal
        ];
        // Lei 14.759/2023: Dia Nacional de Zumbi e da Consciência Negra
        return year >= 2024 ? holidays.Append(new DateTime(year, 11, 20)) : holidays;
    }

    /// <summary>
    ///     Dias sem negociação na B3: os feriados nacionais, as vésperas de Natal e de Ano Novo e, até 2021, os
    ///     feriados municipais e estaduais de São Paulo (25/01, 09/07 e 20/11).
    /// </summary>
    private static IEnumerable<DateTime> B3(int year)
    {
        var days = BrazilNational(year).Concat([new(year, 12, 24), new(year, 12, 31)]);
        if (year <= 2021)
        {
            days = days.Concat([new(year, 1, 25), new(year, 7, 9), new(year, 11, 20)]);
        }

        return days.Concat(B3OneOff.Where(d => d.Year == year));
    }

    /// <summary>
    ///     Dias sem negociação na NYSE. Feriado no sábado fecha a sexta anterior, e no domingo a segunda seguinte,
    ///     exceto o Ano Novo, que nunca fecha o 31 de dezembro.
    /// </summary>
    private static IEnumerable<DateTime> Nyse(int year)
    {
        var newYear = new DateTime(year, 1, 1);
        yield return newYear.DayOfWeek == DayOfWeek.Sunday ? newYear.AddDays(1) : newYear;
        yield return NthWeekday(year, 1, DayOfWeek.Monday, 3); // Martin Luther King Jr. Day
        yield return NthWeekday(year, 2, DayOfWeek.Monday, 3); // Washington's Birthday
        yield return Easter(year).AddDays(-2); // Good Friday
        yield return NthWeekday(year, 5, DayOfWeek.Monday, -1); // Memorial Day
        if (year >= 2022)
        {
            yield return Observed(new DateTime(year, 6, 19)); // Juneteenth
        }

        yield return Observed(new DateTime(year, 7, 4)); // Independence Day
        yield return NthWeekday(year, 9, DayOfWeek.Monday, 1); // Labor Day
        yield return NthWeekday(year, 11, DayOfWeek.Thursday, 4); // Thanksgiving
        yield return Observed(new DateTime(year, 12, 25)); // Christmas

        foreach (var day in NyseOneOff.Where(d => d.Year == year))
        {
            yield return day;
        }
    }

    /// <summary>
    ///     Feriados bancários da Inglaterra e do País de Gales, que a Bolsa de Londres segue.
    /// </summary>
    private static IEnumerable<DateTime> England(int year)
    {
        var easter = Easter(year);
        var newYear = new DateTime(year, 1, 1);
        newYear = newYear.DayOfWeek switch
        {
            DayOfWeek.Saturday => newYear.AddDays(2),
            DayOfWeek.Sunday => newYear.AddDays(1),
            _ => newYear
        };

        // Natal e Boxing Day em fim de semana passam para os próximos dias úteis livres.
        var christmas = new DateTime(year, 12, 25);
        var boxingDay = new DateTime(year, 12, 26);
        switch (christmas.DayOfWeek)
        {
            case DayOfWeek.Saturday:
                christmas = christmas.AddDays(2);
                boxingDay = boxingDay.AddDays(2);
                break;
            case DayOfWeek.Sunday:
                christmas = christmas.AddDays(2);
                break;
            case DayOfWeek.Friday:
                boxingDay = boxingDay.AddDays(2);
                break;
        }

        var days = new List<DateTime>
        {
            newYear,
            easter.AddDays(-2), // Good Friday
            easter.AddDays(1), // Easter Monday
            NthWeekday(year, 5, DayOfWeek.Monday, 1), // Early May
            NthWeekday(year, 5, DayOfWeek.Monday, -1), // Spring
            NthWeekday(year, 8, DayOfWeek.Monday, -1), // Summer
            christmas,
            boxingDay
        };

        foreach (var (regular, moved) in EnglandMoved.Where(m => m.Regular.Year == year))
        {
            days.Remove(regular);
            days.Add(moved);
        }

        return days.Concat(EnglandOneOff.Where(d => d.Year == year));
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

    private static DateTime Observed(DateTime date) => date.DayOfWeek switch
    {
        DayOfWeek.Saturday => date.AddDays(-1),
        DayOfWeek.Sunday => date.AddDays(1),
        _ => date
    };

    private static bool IsWeekday(DateTime date) => date.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday);

    private static string Join(IEnumerable<DateTime> dates)
    {
        var text = string.Join(" ", dates.Select(d => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));
        return text.Length == 0 ? "nada" : text;
    }
}
