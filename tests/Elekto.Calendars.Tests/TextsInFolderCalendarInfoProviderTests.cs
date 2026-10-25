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
using System.Text;

namespace Elekto.Calendars.Tests;

[TestClass]
public sealed class TextsInFolderCalendarInfoProviderTests
{
    /// <summary>
    ///     O formato aceita uma descrição depois da data (separada por ponto e vírgula, tabulação ou vírgula),
    ///     mas a 1.2 tentava converter a linha inteira em data e descartava esses feriados em silêncio.
    /// </summary>
    [TestMethod]
    public void FeriadoComDescricaoNaoEDescartado()
    {
        var folder = Directory.CreateTempSubdirectory("calendarios-");
        try
        {
            File.WriteAllLines(Path.Combine(folder.FullName, "0001.calendar.teste.txt"),
            [
                "Calendário de teste",
                "-- comentário",
                "20260101;Confraternização Universal",
                "20260216\tCarnaval",
                "20260217,Carnaval",
                "",
                "20260403",
                "isto não é data"
            ], new UTF8Encoding(true));

            var info = new TextsInFolderCalendarInfoProvider(folder.FullName).GetCalendarInfo("teste");

            CollectionAssert.AreEqual(
                new[] { new DateTime(2026, 1, 1), new DateTime(2026, 2, 16), new DateTime(2026, 2, 17), new DateTime(2026, 4, 3) },
                info.Holidays);
            Assert.AreEqual("Calendário de teste", info.Description);
        }
        finally
        {
            folder.Delete(true);
        }
    }
}
