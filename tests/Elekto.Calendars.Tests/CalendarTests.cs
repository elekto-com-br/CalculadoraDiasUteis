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
}
