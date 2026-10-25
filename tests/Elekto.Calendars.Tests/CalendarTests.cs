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
namespace Elekto.Calendars.Tests;

[TestClass]
public sealed class CalendarTests
{
    /// <summary>
    ///     Com um só feriado o último dia do calendário pode ser útil, e na 1.2 a busca binária do AddWorkDays
    ///     entrava em laço infinito ao chegar exatamente nele.
    /// </summary>
    [TestMethod]
    public void AddWorkDaysAteOUltimoDiaUtilNaoTrava()
    {
        var calendar = new Calendar(new[] { new DateTime(2026, 1, 3) });
        Assert.IsTrue(calendar.IsWorkday(calendar.MaxDate), "O cenário exige que o último dia do calendário seja útil.");
        var workDays = calendar.GetDeltaWorkDays(calendar.MinWorkDate, calendar.MaxDate);

        var task = Task.Run(() => calendar.AddWorkDays(calendar.MinWorkDate, workDays));

        Assert.IsTrue(task.Wait(TimeSpan.FromSeconds(5)), "AddWorkDays não terminou em 5 segundos.");
        Assert.AreEqual(calendar.MaxDate, task.Result);
    }

    /// <summary>
    ///     Na 1.2, um prazo negativo contado de um dia não útil depois do último dia útil do calendário só funcionava até
    ///     5 dias úteis; acima disso, o algoritmo rápido procurava o próximo dia útil, que não existe, e lançava exceção.
    /// </summary>
    [TestMethod]
    [DataRow(-1)]
    [DataRow(-5)]
    [DataRow(-6)]
    [DataRow(-300)]
    public void PrazoNegativoAPartirDoUltimoDiaDoCalendario(int workDays)
    {
        // 25/12/2027 é um sábado, o último dia do calendário
        var calendar = new Calendar([new DateTime(2026, 1, 1), new DateTime(2027, 12, 25)]);
        Assert.IsFalse(calendar.IsWorkday(calendar.MaxDate), "O cenário exige que o último dia do calendário não seja útil.");

        var expected = calendar.MaxDate;
        for (var i = 0; i > workDays; i--)
        {
            expected = calendar.GetPrevWorkday(expected);
        }

        Assert.AreEqual(expected, calendar.AddWorkDays(calendar.MaxDate, workDays));
    }

    [TestMethod]
    public void PeriodoExplicitoValeNoLugarDoDosFeriados()
    {
        var info = CalendarInfo.New("teste", [new DateTime(2026, 4, 21), new DateTime(2031, 1, 1)],
            minDate: new DateTime(2026, 1, 1), maxDate: new DateTime(2027, 12, 31));

        var calendar = new Calendar(info);

        Assert.AreEqual(new DateTime(2026, 1, 1), calendar.MinDate);
        Assert.AreEqual(new DateTime(2027, 12, 31), calendar.MaxDate);
        CollectionAssert.AreEqual(new[] { new DateTime(2026, 4, 21) }, calendar.Holidays.ToArray(),
            "O feriado fora do período não deveria contar.");
        Assert.IsFalse(calendar.IsWorkday(new DateTime(2026, 4, 21)));
    }

    [TestMethod]
    public void PeriodoExplicitoPrecisaDasDuasPontas()
    {
        Assert.ThrowsExactly<ArgumentException>(() =>
            CalendarInfo.New("teste", [], minDate: new DateTime(2026, 1, 1)));
        Assert.ThrowsExactly<ArgumentException>(() =>
            CalendarInfo.New("teste", [], minDate: new DateTime(2027, 1, 1), maxDate: new DateTime(2026, 1, 1)));
    }
}
