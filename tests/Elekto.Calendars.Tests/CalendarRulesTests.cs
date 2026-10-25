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
    [DataRow("ar-BA")]
    [DataRow("py-ASU")]
    [DataRow("uy-MVD")]
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
    [DataRow("ar-BA")]
    [DataRow("py-ASU")]
    [DataRow("uy-MVD")]
    public void FeriadoTemADescricaoDaRegra(string name)
    {
        var calendar = Server.Value.GetCalendar(name);
        var descriptions = Provider.Value.GetCalendarInfo(name).HolidayDescriptions;
        var (first, last) = Years(calendar);

        // uma data com duas regras, como a Sexta-feira Santa num 21 de abril, leva as duas descrições, unidas na
        // língua do calendário
        var and = name is "ar-BA" or "py-ASU" or "uy-MVD" ? " y " : " e ";
        var expected = Expected(name, first, last)
                       .GroupBy(h => h.Date)
                       .ToDictionary(g => g.Key, g => string.Join(and, g.Select(h => h.Name).Distinct()));
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
            "ar-BA" => Argentina,
            "py-ASU" => Paraguay,
            "uy-MVD" => Uruguay,
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
    ///     Feriados da Argentina levados por lei ou decreto a uma data que a regra do período não daria: a data legal e a
    ///     efetiva. As fontes estão em docs/calendarios/ar-BA.md.
    /// </summary>
    private static readonly (DateTime Legal, DateTime Moved)[] ArgentinaMoved =
    [
        (new DateTime(2001, 10, 12), new DateTime(2001, 10, 8)), // Ley 25.442
        (new DateTime(2002, 10, 12), new DateTime(2002, 10, 14)), // Decreto 1932/2002
        (new DateTime(2003, 10, 12), new DateTime(2003, 10, 13)), // Decreto 810/2003
        (new DateTime(2011, 8, 17), new DateTime(2011, 8, 22)), // Decreto 521/2011
        (new DateTime(2015, 11, 20), new DateTime(2015, 11, 27)), // Decreto 2226/2015
        (new DateTime(2020, 4, 2), new DateTime(2020, 3, 31)), // Decreto 297/2020
        (new DateTime(2025, 10, 12), new DateTime(2025, 10, 10)) // Decreto 614/2025 e Resolución JGM 139/2025
    ];

    private static readonly Holiday[] ArgentinaOneOff =
    [
        new(new DateTime(2000, 12, 29), "Feriado bancario de fin de año"), // Decreto 24.879/1953, abrogado em 2001
        new(new DateTime(2001, 11, 17), "Censo Nacional 2001"),
        new(new DateTime(2001, 11, 18), "Censo Nacional 2001"),
        new(new DateTime(2001, 12, 21), "Feriado bancario y cambiario"),
        new(new DateTime(2001, 12, 24), "Feriado bancario y cambiario"),
        new(new DateTime(2001, 12, 26), "Feriado bancario y cambiario"),
        new(new DateTime(2002, 1, 7), "Feriado bancario y cambiario"),
        new(new DateTime(2002, 1, 8), "Feriado bancario y cambiario"),
        new(new DateTime(2002, 1, 9), "Feriado bancario y cambiario"),
        new(new DateTime(2002, 1, 10), "Feriado bancario y cambiario"),
        new(new DateTime(2002, 2, 4), "Feriado bancario y cambiario"),
        new(new DateTime(2002, 2, 5), "Feriado bancario y cambiario"),
        new(new DateTime(2002, 2, 6), "Feriado bancario y cambiario"),
        new(new DateTime(2002, 2, 7), "Feriado bancario y cambiario"),
        new(new DateTime(2002, 4, 22), "Feriado bancario y cambiario"),
        new(new DateTime(2002, 4, 23), "Feriado bancario y cambiario"),
        new(new DateTime(2002, 4, 24), "Feriado bancario y cambiario"),
        new(new DateTime(2002, 4, 25), "Feriado bancario y cambiario"),
        new(new DateTime(2002, 4, 26), "Feriado bancario y cambiario"),
        new(new DateTime(2007, 12, 24), "Asueto bancario"),
        new(new DateTime(2007, 12, 31), "Asueto bancario"),
        new(new DateTime(2009, 7, 10), "Asueto por la gripe A (H1N1)"),
        new(new DateTime(2010, 5, 24), "Bicentenario de la Revolución de Mayo"),
        new(new DateTime(2010, 10, 27), "Censo Nacional 2010"),
        new(new DateTime(2010, 12, 24), "Asueto bancario"),
        new(new DateTime(2010, 12, 31), "Asueto bancario"),
        new(new DateTime(2011, 12, 26), "Asueto bancario"),
        new(new DateTime(2012, 1, 2), "Asueto bancario"),
        new(new DateTime(2012, 2, 27), "Bicentenario de la Creación y Primera Jura de la Bandera"),
        new(new DateTime(2012, 9, 24), "Bicentenario de la Batalla de Tucumán"),
        new(new DateTime(2013, 1, 31), "Bicentenario de la Asamblea General Constituyente de 1813"),
        new(new DateTime(2013, 2, 20), "Bicentenario de la Batalla de Salta"),
        new(new DateTime(2013, 12, 24), "Asueto bancario"),
        new(new DateTime(2013, 12, 31), "Asueto bancario"),
        new(new DateTime(2014, 12, 31), "Asueto bancario"),
        new(new DateTime(2018, 11, 30), "Cumbre de Líderes del G20"),
        new(new DateTime(2019, 12, 24), "Asueto bancario"),
        new(new DateTime(2019, 12, 31), "Asueto bancario"),
        new(new DateTime(2020, 12, 24), "Asueto bancario"),
        new(new DateTime(2020, 12, 31), "Asueto bancario"),
        new(new DateTime(2021, 12, 24), "Asueto bancario"),
        new(new DateTime(2021, 12, 31), "Asueto bancario"),
        new(new DateTime(2022, 5, 18), "Censo Nacional 2022"),
        new(new DateTime(2022, 9, 2), "Feriado nacional extraordinario (DNU 573/2022)"),
        new(new DateTime(2024, 12, 24), "Asueto bancario"),
        new(new DateTime(2024, 12, 31), "Asueto bancario"),
        new(new DateTime(2025, 12, 24), "Asueto bancario"),
        new(new DateTime(2025, 12, 31), "Asueto bancario"),
        new(new DateTime(2026, 11, 9), "Visita de Su Santidad el Papa León XIV"),
        new(new DateTime(2026, 11, 10), "Feriado bancario por la visita de Su Santidad el Papa León XIV")
    ];

    /// <summary>
    ///     Os feriados ou dias não laboráveis com fins turísticos de cada ano, nos quais os bancos não abrem.
    /// </summary>
    private static readonly DateTime[] ArgentinaTourism =
    [
        new(2011, 3, 25), new(2011, 12, 9), new(2012, 4, 30), new(2012, 12, 24), new(2013, 4, 1), new(2013, 6, 21),
        new(2014, 5, 2), new(2014, 12, 26), new(2015, 3, 23), new(2015, 12, 7), new(2016, 7, 8), new(2016, 12, 9),
        new(2018, 4, 30), new(2018, 12, 24), new(2018, 12, 31), new(2019, 7, 8), new(2019, 8, 19), new(2019, 10, 14),
        new(2020, 3, 23), new(2020, 7, 10), new(2020, 12, 7), new(2021, 5, 24), new(2021, 10, 8), new(2021, 11, 22),
        new(2022, 10, 7), new(2022, 11, 21), new(2022, 12, 9), new(2023, 5, 26), new(2023, 6, 19), new(2023, 10, 13),
        new(2024, 4, 1), new(2024, 6, 21), new(2024, 10, 11), new(2025, 5, 2), new(2025, 8, 15), new(2025, 11, 21),
        new(2026, 3, 23), new(2026, 7, 10), new(2026, 12, 7)
    ];

    /// <summary>
    ///     Dias sem expediente bancário na Argentina, segundo o BCRA: os feriados nacionais na data efetiva, a quinta-feira
    ///     santa, o Día del Bancario, os dias turísticos, os feriados bancários e os asuetos de dia inteiro.
    /// </summary>
    private static IEnumerable<Holiday> Argentina(int year)
    {
        var easter = Easter(year);
        var days = new List<Holiday> { new(new DateTime(year, 1, 1), "Año Nuevo") };

        // Decreto 1584/2010 e Ley 27.399
        if (year >= 2011)
        {
            days.Add(new Holiday(easter.AddDays(-48), "Carnaval"));
            days.Add(new Holiday(easter.AddDays(-47), "Carnaval"));
        }

        // Ley 26.085
        if (year >= 2006)
        {
            days.Add(new Holiday(new DateTime(year, 3, 24), "Día Nacional de la Memoria por la Verdad y la Justicia"));
        }

        // Ley 25.370; trasladável pela Ley 23.555 até a Ley 26.110, de 2006
        if (year >= 2001)
        {
            var malvinas = new DateTime(year, 4, 2);
            days.Add(new Holiday(ArgentinaEffective(malvinas, year <= 2006 ? ArgentinaMovable(malvinas) : malvinas),
                "Día del Veterano y de los Caídos en la Guerra de Malvinas"));
        }

        days.Add(new Holiday(easter.AddDays(-3), "Jueves Santo"));
        days.Add(new Holiday(easter.AddDays(-2), "Viernes Santo"));
        days.Add(new Holiday(new DateTime(year, 5, 1), "Día del Trabajo"));
        days.Add(new Holiday(new DateTime(year, 5, 25), "Día de la Revolución de Mayo"));

        // revogado pela Ley 25.370
        if (year <= 2000)
        {
            var afirmacion = new DateTime(year, 6, 10);
            days.Add(new Holiday(ArgentinaMovable(afirmacion),
                "Día de la Afirmación de los Derechos Argentinos sobre las Malvinas, Islas y Sector Antártico"));
        }

        // Ley 27.258: fixo em 2016; depois, trasladável, mas numa sexta fica (Decreto 789/2021)
        if (year >= 2016)
        {
            var guemes = new DateTime(year, 6, 17);
            days.Add(new Holiday(year == 2016 || guemes.DayOfWeek == DayOfWeek.Friday ? guemes : ArgentinaMovable(guemes),
                "Paso a la Inmortalidad del General Don Martín Miguel de Güemes"));
        }

        // Ley 24.445: terceira segunda-feira de junho até 2010
        days.Add(new Holiday(year <= 2010 ? NthWeekday(year, 6, DayOfWeek.Monday, 3) : new DateTime(year, 6, 20),
            "Paso a la Inmortalidad del General Don Manuel Belgrano"));
        days.Add(new Holiday(new DateTime(year, 7, 9), "Día de la Independencia"));

        // terceira segunda-feira de agosto até 2016; depois, Ley 27.399
        var sanMartin = new DateTime(year, 8, 17);
        days.Add(new Holiday(ArgentinaEffective(sanMartin,
                year <= 2016 ? NthWeekday(year, 8, DayOfWeek.Monday, 3) : ArgentinaMovable(sanMartin)),
            "Paso a la Inmortalidad del General Don José de San Martín"));

        var raza = new DateTime(year, 10, 12);
        var razaDate = year switch
        {
            <= 2007 => ArgentinaMovable(raza),
            <= 2010 => raza.DayOfWeek switch // Ley 26.416
            {
                DayOfWeek.Tuesday => raza.AddDays(-1),
                DayOfWeek.Wednesday => raza.AddDays(-2),
                DayOfWeek.Monday => raza,
                _ => raza.AddDays(((int)DayOfWeek.Monday - (int)raza.DayOfWeek + 7) % 7)
            },
            <= 2016 => NthWeekday(year, 10, DayOfWeek.Monday, 2), // Decreto 1584/2010
            _ => ArgentinaMovable(raza)
        };
        days.Add(new Holiday(ArgentinaEffective(raza, razaDate),
            year <= 2010 ? "Día de la Raza" : "Día del Respeto a la Diversidad Cultural"));

        // Convenio Colectivo 18/75, art. 50
        days.Add(new Holiday(new DateTime(year, 11, 6), "Día del Bancario"));

        // Decreto 1584/2010: quarta segunda-feira de novembro até 2016; depois, Ley 27.399
        if (year >= 2010)
        {
            var soberania = new DateTime(year, 11, 20);
            days.Add(new Holiday(ArgentinaEffective(soberania,
                    year <= 2016 ? NthWeekday(year, 11, DayOfWeek.Monday, 4) : ArgentinaMovable(soberania)),
                "Día de la Soberanía Nacional"));
        }

        days.Add(new Holiday(new DateTime(year, 12, 8), "Día de la Inmaculada Concepción de María"));
        days.Add(new Holiday(new DateTime(year, 12, 25), "Navidad"));

        // a descrição segue a norma de cada ano
        var tourism = year is 2018 or 2019 or >= 2025 ? "Día no laborable con fines turísticos" : "Feriado con fines turísticos";
        days.AddRange(ArgentinaTourism.Where(d => d.Year == year).Select(d => new Holiday(d, tourism)));
        days.AddRange(ArgentinaOneOff.Where(h => h.Date.Year == year));
        return days;
    }

    /// <summary>
    ///     A data efetiva: a da tabela de exceções, se houver, ou a da regra do período.
    /// </summary>
    private static DateTime ArgentinaEffective(DateTime legal, DateTime byRule) =>
        ArgentinaMoved.Where(m => m.Legal == legal).Select(m => m.Moved).DefaultIfEmpty(byRule).First();

    /// <summary>
    ///     O traslado da Ley 23.555 (até 2010) e da Ley 27.399 (desde 2017): terça e quarta vão para a segunda anterior,
    ///     quinta e sexta para a segunda seguinte, sábado e domingo ficam.
    /// </summary>
    private static DateTime ArgentinaMovable(DateTime date) => date.DayOfWeek switch
    {
        DayOfWeek.Tuesday => date.AddDays(-1),
        DayOfWeek.Wednesday => date.AddDays(-2),
        DayOfWeek.Thursday => date.AddDays(4),
        DayOfWeek.Friday => date.AddDays(3),
        _ => date
    };

    /// <summary>
    ///     Traslados de feriados do Paraguai decretados ano a ano (Ley 1.723/2001 e Ley 7.544/2025): a data legal e a
    ///     efetiva. As fontes estão em docs/calendarios/py-ASU.md.
    /// </summary>
    private static readonly (DateTime Legal, DateTime Moved)[] ParaguayMoved =
    [
        (new DateTime(2002, 3, 1), new DateTime(2002, 3, 4)),
        (new DateTime(2013, 3, 1), new DateTime(2013, 3, 4)),
        (new DateTime(2013, 6, 12), new DateTime(2013, 6, 10)),
        (new DateTime(2014, 6, 12), new DateTime(2014, 6, 16)),
        (new DateTime(2015, 9, 29), new DateTime(2015, 9, 28)),
        (new DateTime(2016, 3, 1), new DateTime(2016, 2, 29)),
        (new DateTime(2016, 9, 29), new DateTime(2016, 10, 3)),
        (new DateTime(2017, 3, 1), new DateTime(2017, 2, 27)),
        (new DateTime(2017, 9, 29), new DateTime(2017, 10, 2)),
        (new DateTime(2018, 3, 1), new DateTime(2018, 2, 26)),
        (new DateTime(2018, 6, 12), new DateTime(2018, 6, 11)),
        (new DateTime(2019, 3, 1), new DateTime(2019, 3, 4)),
        (new DateTime(2019, 6, 12), new DateTime(2019, 6, 17)),
        (new DateTime(2021, 9, 29), new DateTime(2021, 9, 27)),
        (new DateTime(2022, 3, 1), new DateTime(2022, 2, 28)),
        (new DateTime(2022, 5, 1), new DateTime(2022, 5, 2)),
        (new DateTime(2022, 9, 29), new DateTime(2022, 10, 3)),
        (new DateTime(2023, 3, 1), new DateTime(2023, 2, 27)),
        (new DateTime(2024, 6, 12), new DateTime(2024, 6, 10)),
        (new DateTime(2024, 9, 29), new DateTime(2024, 9, 30)),
        (new DateTime(2025, 3, 1), new DateTime(2025, 3, 3)),
        (new DateTime(2025, 6, 12), new DateTime(2025, 6, 16)),
        (new DateTime(2026, 3, 1), new DateTime(2026, 3, 2)),
        (new DateTime(2026, 6, 20), new DateTime(2026, 6, 22)),
        (new DateTime(2026, 9, 29), new DateTime(2026, 9, 28))
    ];

    private static readonly Holiday[] ParaguayOneOff =
    [
        new(new DateTime(2002, 8, 28), "Censo Nacional de Población y Viviendas"),
        new(new DateTime(2011, 5, 14), "Bicentenario de la Independencia Nacional"),
        new(new DateTime(2011, 5, 16), "Bicentenario de la Independencia Nacional"),
        new(new DateTime(2015, 7, 10), "Visita del Papa Francisco"),
        new(new DateTime(2015, 7, 11), "Visita del Papa Francisco"),
        new(new DateTime(2022, 11, 9), "Censo Nacional de Población y Viviendas"),
        new(new DateTime(2025, 9, 5), "Clasificación de la Selección Paraguaya al Mundial 2026"),
        new(new DateTime(2025, 12, 26), "Feriado adicional de Navidad"),
        new(new DateTime(2026, 6, 30), "Victoria de la Selección Paraguaya en el Mundial 2026")
    ];

    /// <summary>
    ///     Dias sem expediente bancário no Paraguai: os feriados nacionais na data efetiva, a quinta e a sexta-feira
    ///     santa, os feriados extraordinários e o feriado bancário do último dia útil de dezembro (Decreto 9.131/1949).
    /// </summary>
    private static IEnumerable<Holiday> Paraguay(int year)
    {
        var easter = Easter(year);
        var days = new List<Holiday>
        {
            new(new DateTime(year, 1, 1), "Año Nuevo"),
            new(easter.AddDays(-3), "Jueves Santo"),
            new(easter.AddDays(-2), "Viernes Santo"),
            new(new DateTime(year, 5, 15), "Día de la Independencia Nacional"),
            new(new DateTime(year, 12, 8), "Día de la Virgen de Caacupé"),
            new(new DateTime(year, 12, 25), "Navidad")
        };

        // Ley 4.531/2011
        if (year >= 2012)
        {
            days.Add(new Holiday(new DateTime(year, 5, 14), "Día de la Independencia Nacional"));
        }

        // os que podem ser trasladados por decreto
        var movable = new List<Holiday>
        {
            new(new DateTime(year, 3, 1), "Día de los Héroes de la Patria"),
            new(new DateTime(year, 5, 1), "Día de los Trabajadores"),
            new(new DateTime(year, 6, 12), "Día de la Paz del Chaco"),
            new(new DateTime(year, 8, 15), "Día de la Fundación de Asunción"),
            new(new DateTime(year, 9, 29), "Día de la Batalla de Boquerón")
        };

        // Ley 7.544/2025
        if (year >= 2026)
        {
            movable.Add(new Holiday(new DateTime(year, 6, 20), "Día de la Jura de la Constitución Nacional"));
        }

        foreach (var holiday in movable)
        {
            var moved = ParaguayMoved.Where(m => m.Legal == holiday.Date).Select(m => m.Moved).ToList();
            days.Add(moved.Count == 0 ? holiday : new Holiday(moved[0], holiday.Name + " (trasladado)"));
        }

        days.AddRange(ParaguayOneOff.Where(h => h.Date.Year == year));

        // o último dia de segunda a sexta de dezembro que não é feriado
        var bankHoliday = new DateTime(year, 12, 31);
        while (!IsWeekday(bankHoliday) || days.Any(h => h.Date == bankHoliday))
        {
            bankHoliday = bankHoliday.AddDays(-1);
        }

        days.Add(new Holiday(bankHoliday, "Feriado bancario (último día hábil del año)"));
        return days;
    }

    private static readonly Holiday[] UruguayOneOff =
    [
        new(new DateTime(2000, 9, 23), "Sesquicentenario del fallecimiento de Artigas"), // Ley 17.262
        new(new DateTime(2002, 7, 30), "Feriado bancario"), // Decreto 288/002
        new(new DateTime(2002, 7, 31), "Feriado bancario"), // Decreto 290/002
        new(new DateTime(2002, 8, 1), "Feriado bancario"),
        new(new DateTime(2002, 8, 2), "Feriado bancario"),
        new(new DateTime(2004, 9, 10), "Centenario del fallecimiento de Aparicio Saravia") // Ley 17.798
    ];

    /// <summary>
    ///     Dias de greve em que o Banco Central del Uruguay não operou, sem liquidação interbancária nem compensação de
    ///     cheques, segundo documento do próprio BCU.
    /// </summary>
    private static readonly Holiday[] UruguayStrikes =
    [
        new(new DateTime(2000, 6, 8), "Paro general (sin operaciones en el BCU)"),
        new(new DateTime(2000, 11, 16), "Sin operaciones en el BCU"),
        new(new DateTime(2000, 12, 6), "Paro general (sin operaciones en el BCU)"),
        new(new DateTime(2001, 7, 11), "Sin operaciones en el BCU"),
        new(new DateTime(2001, 7, 25), "Paro general (sin operaciones en el BCU)"),
        new(new DateTime(2002, 5, 24), "Paro general (sin operaciones en el BCU)"),
        new(new DateTime(2002, 6, 12), "Paro general (sin operaciones en el BCU)"),
        new(new DateTime(2002, 8, 7), "Paro general (sin operaciones en el BCU)"),
        new(new DateTime(2002, 12, 19), "Sin operaciones en el BCU"),
        new(new DateTime(2003, 6, 17), "Sin operaciones en el BCU"),
        new(new DateTime(2003, 8, 28), "Paro general (sin operaciones en el BCU)"),
        new(new DateTime(2015, 8, 6), "Paro general (sin operaciones en el BCU)"),
        new(new DateTime(2016, 7, 14), "Paro general (sin operaciones en el BCU)"),
        new(new DateTime(2016, 9, 14), "Sin operaciones en el BCU"),
        new(new DateTime(2017, 7, 20), "Paro general (sin operaciones en el BCU)"),
        new(new DateTime(2018, 8, 22), "Paro general (sin operaciones en el BCU)"),
        new(new DateTime(2019, 6, 25), "Paro general (sin operaciones en el BCU)")
    ];

    /// <summary>
    ///     Dias sem expediente bancário no Uruguai: os feriados da Ley 16.805, laborables ou no laborables, nos quais os
    ///     bancos fecham (RNRCSF, art. 28), a quinta e a sexta da Semana de Turismo, o fechamento de 31 de dezembro e os
    ///     fechamentos extraordinários. As fontes estão em docs/calendarios/uy-MVD.md.
    /// </summary>
    private static IEnumerable<Holiday> Uruguay(int year)
    {
        var easter = Easter(year);
        var days = new List<Holiday>
        {
            new(new DateTime(year, 1, 1), "Año Nuevo"),
            new(new DateTime(year, 1, 6), "Día de Reyes"),
            new(easter.AddDays(-48), "Carnaval"),
            new(easter.AddDays(-47), "Carnaval"),
            new(easter.AddDays(-2), "Semana de Turismo"),
            new(new DateTime(year, 5, 1), "Día de los Trabajadores"),
            new(new DateTime(year, 7, 18), "Jura de la Constitución"),
            new(new DateTime(year, 8, 25), "Declaratoria de la Independencia"),
            new(new DateTime(year, 12, 25), "Navidad")
        };

        // Comunicação BCU 2009/053: na quinta de Turismo o BCU não opera
        if (year >= 2009)
        {
            days.Add(new Holiday(easter.AddDays(-3), "Semana de Turismo"));
        }

        // Resolução do BCU de 23/12/2003 e Circular 1.888
        if (year >= 2003)
        {
            days.Add(new Holiday(new DateTime(year, 12, 31), "Cierre bancario de fin de año"));
        }

        // Ley 7.556: a posse presidencial, no próprio dia, a cada cinco anos
        if (year % 5 == 0)
        {
            days.Add(new Holiday(new DateTime(year, 3, 1), "Transmisión del mando presidencial"));
        }

        var movable = new List<Holiday>
        {
            new(new DateTime(year, 4, 19), "Desembarco de los 33 Orientales"),
            new(new DateTime(year, 5, 18), "Batalla de las Piedras"),
            new(new DateTime(year, 10, 12), "Día de la Raza")
        };

        // Ley 17.414, de novembro de 2001: 19/6 e 2/11 passam a ficar no dia
        var artigas = new Holiday(new DateTime(year, 6, 19), "Natalicio de Artigas");
        var difuntos = new Holiday(new DateTime(year, 11, 2), "Día de los Difuntos");
        if (year >= 2002)
        {
            days.Add(artigas);
            days.Add(difuntos);
        }
        else
        {
            movable.Add(artigas);
            movable.Add(difuntos);
        }

        foreach (var holiday in movable)
        {
            // Ley 18.748: o bicentenário da Batalla de las Piedras ficou na quarta
            var moved = year == 2011 && holiday.Date.Month == 5 ? holiday.Date : UruguayMoved(holiday.Date);
            days.Add(moved == holiday.Date ? holiday : new Holiday(moved, holiday.Name + " (trasladado)"));
        }

        return days.Concat(UruguayOneOff.Where(h => h.Date.Year == year))
                   .Concat(UruguayStrikes.Where(h => h.Date.Year == year));
    }

    /// <summary>
    ///     Ley 16.805, art. 1: em terça ou quarta, o feriado passa para a segunda anterior; em quinta ou sexta, para a
    ///     segunda seguinte; em sábado, domingo ou segunda, fica no dia.
    /// </summary>
    private static DateTime UruguayMoved(DateTime date) => date.DayOfWeek switch
    {
        DayOfWeek.Tuesday => date.AddDays(-1),
        DayOfWeek.Wednesday => date.AddDays(-2),
        DayOfWeek.Thursday => date.AddDays(4),
        DayOfWeek.Friday => date.AddDays(3),
        _ => date
    };

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
