#:project ../../src/Elekto.Calendars/Elekto.Calendars.csproj
#:property PublishAot=false
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

// Calcula, com a biblioteca, os resultados que os testes de interface esperam ver na tela.
// Uso: dotnet run Expected.cs -- <hoje no formato yyyy-MM-dd>
// O "hoje" é a data do Windows onde o aplicativo roda, lida da própria tela pelo teste.

using System.Globalization;
using System.Text.Json;
using Elekto.Calendars;

var culture = CultureInfo.GetCultureInfo("pt-BR");
var today = DateTime.ParseExact(args[0], "yyyy-MM-dd", CultureInfo.InvariantCulture);

var provider = new TextsInFolderCalendarInfoProvider(Path.Combine(AppContext.BaseDirectory, "Calendars"));
var server = new CalendarServer(provider);
var brBc = server.GetCalendar("br-BC");
var usNyc = server.GetCalendar("us-NYC");
var brBcDescriptions = provider.GetCalendarInfo("br-BC").HolidayDescriptions;

// O teste escolhe o dia 15 nos seletores de data: é o único dia que aparece uma só vez em qualquer mês do calendário
var d0 = new DateTime(today.Year, today.Month, 15);
var d1 = d0.AddYears(1);

// Um prazo em dias corridos que termina num dia não útil do fim do mês, cujo dia útil seguinte já é do mês
// seguinte: assim os três ajustes dão três datas diferentes
var weekendTerm = Enumerable.Range(1, 400).First(n =>
    !brBc.IsWorkday(d0.AddDays(n)) &&
    brBc.AddActualDays(d0, n, FinalDateAdjust.Following).Month != d0.AddDays(n).Month);
var weekendDate = brBc.AddActualDays(d0, weekendTerm);

string Date(DateTime d) => d.ToString("dd/MM/yyyy", culture);
string Number(int n) => n.ToString("N0", culture);
string HolidayNote(DateTime d) =>
    brBcDescriptions.TryGetValue(d, out var description) ? $"feriado ({description})" : "feriado";

// O feriado local do calendário derivado: o primeiro dia útil do br-BC entre 15 e 22 do mês, dias que aparecem uma
// vez só na grade do calendário de qualquer mês
var localDate = Enumerable.Range(15, 8).Select(day => new DateTime(today.Year, today.Month, day)).First(brBc.IsWorkday);
var lapa = new Elekto.Calendars.Calendar(brBc.Holidays.Append(localDate), "forum-da-lapa", null, null);
var christmas = new DateTime(today.Year, 12, 25);

var expected = new Dictionary<string, object>
{
    // Estado inicial: hoje, um ano depois, financeiro, 252 dias úteis no br-BC
    ["todayWork"] = Number(brBc.GetDeltaWorkDays(today, today.AddYears(1), DeltaTerminalDayAdjust.Financial)),
    ["todayActual"] = Number(brBc.GetDeltaActualDays(today, today.AddYears(1), DeltaTerminalDayAdjust.Financial)),
    ["todayFinal"] = Date(brBc.AddWorkDays(today, 252)),
    ["d0"] = Date(d0),
    ["d1"] = Date(d1),
    ["financialWork"] = Number(brBc.GetDeltaWorkDays(d0, d1, DeltaTerminalDayAdjust.Financial)),
    ["financialActual"] = Number(brBc.GetDeltaActualDays(d0, d1, DeltaTerminalDayAdjust.Financial)),
    ["fullWork"] = Number(brBc.GetDeltaWorkDays(d0, d1, DeltaTerminalDayAdjust.Full)),
    ["fullActual"] = Number(brBc.GetDeltaActualDays(d0, d1, DeltaTerminalDayAdjust.Full)),
    ["work30"] = Date(brBc.AddWorkDays(d0, 30)),
    ["workMinus10"] = Date(brBc.AddWorkDays(d0, -10)),
    ["weekendTerm"] = weekendTerm,
    ["actualNone"] = Date(weekendDate),
    ["actualNoneDescription"] = "Data final: " + weekendDate.ToString("D", culture) + ", " +
                                (brBc.IsProperHoliday(weekendDate) ? HolidayNote(weekendDate) : "dia não útil"),
    ["actualFollowing"] = Date(brBc.AddActualDays(d0, weekendTerm, FinalDateAdjust.Following)),
    ["actualModified"] = Date(brBc.AddActualDays(d0, weekendTerm, FinalDateAdjust.ModifiedFollowing)),
    ["nyseFullWork"] = Number(usNyc.GetDeltaWorkDays(d0, d1, DeltaTerminalDayAdjust.Full)),
    ["nyseFullActual"] = Number(usNyc.GetDeltaActualDays(d0, d1, DeltaTerminalDayAdjust.Full)),
    ["nyseActualModified"] = Date(usNyc.AddActualDays(d0, weekendTerm, FinalDateAdjust.ModifiedFollowing)),
    ["holidaysThisYear"] = $"{brBc.Holidays.Count(h => h.Year == today.Year)} feriados em {today.Year}.",
    ["localDay"] = localDate.Day.ToString(CultureInfo.InvariantCulture),
    ["localLong"] = localDate.ToString("D", culture),
    ["christmasLong"] = christmas.ToString("D", culture),
    ["christmasDescription"] = brBcDescriptions.GetValueOrDefault(christmas) ?? "",
    ["brBcFinancialFromD0"] = Number(brBc.GetDeltaWorkDays(d0, today.AddYears(1), DeltaTerminalDayAdjust.Financial)),
    ["lapaFinancialFromD0"] = Number(lapa.GetDeltaWorkDays(d0, today.AddYears(1), DeltaTerminalDayAdjust.Financial)),
    ["lapaFinancialToday"] = Number(lapa.GetDeltaWorkDays(today, today.AddYears(1), DeltaTerminalDayAdjust.Financial)),
    ["periodDays"] = ((today.AddYears(1) - d0).Days + 1).ToString(CultureInfo.InvariantCulture),
    ["periodEnd"] = Date(today.AddYears(1)),
    ["periodFile"] = $"{d0:yyyy-MM-dd} a {today.AddYears(1):yyyy-MM-dd}",
    ["holidaysThisYearCount"] = brBc.Holidays.Count(h => h.Year == today.Year).ToString(CultureInfo.InvariantCulture),
    ["year"] = today.Year.ToString(CultureInfo.InvariantCulture)
};

Console.WriteLine(JsonSerializer.Serialize(expected));
