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
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Validation;

namespace CalculadoraDiasUteis.Core.Tests;

[TestClass]
public sealed class SheetTests
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    private static readonly SheetTexts Texts = new("Dias", "Feriados", "Data", "Dia da semana", "É dia útil",
        "Dias úteis desde o início", "Feriado", "Descrição");

    [TestMethod]
    public void PeriodoTemTodasAsDatasComDiaUtilEFeriado()
    {
        using var temp = new TempStore();
        var entry = TestData.Catalog(temp.Open()).Get("br-BC");

        // de sábado, 14/02/2026, a quarta-feira de cinzas, 18/02/2026
        var table = SheetTables.Period(entry, new DateTime(2026, 2, 18), new DateTime(2026, 2, 14), Texts, PtBr);

        CollectionAssert.AreEqual(new[] { "Data", "Dia da semana", "É dia útil", "Dias úteis desde o início", "Feriado" },
            table.Columns.Select(c => c.Header).ToArray());
        Assert.HasCount(5, table.Rows);
        CollectionAssert.AreEqual(new object?[] { new DateTime(2026, 2, 14), "sábado", 0, 0, null }, table.Rows[0]);
        CollectionAssert.AreEqual(new object?[] { TestData.Carnaval, "segunda-feira", 0, 0, "Carnaval" }, table.Rows[2]);
        CollectionAssert.AreEqual(new object?[] { new DateTime(2026, 2, 18), "quarta-feira", 1, 1, null }, table.Rows[4]);
    }

    [TestMethod]
    public void PeriodoForaDoCalendarioERecusado()
    {
        using var temp = new TempStore();
        var entry = TestData.Catalog(temp.Open()).Get("br-BC");

        var e = Assert.ThrowsExactly<CatalogException>(() =>
            SheetTables.Period(entry, new DateTime(2025, 12, 1), new DateTime(2026, 1, 10), Texts, PtBr));
        Assert.AreEqual(CatalogError.DateOutsideRange, e.Error);
    }

    [TestMethod]
    public void FeriadosDoAnoSemOsRemovidos()
    {
        using var temp = new TempStore();
        var catalog = TestData.Catalog(temp.Open());
        catalog.RemoveHoliday("br-BC", TestData.Tiradentes);

        var year = SheetTables.Holidays(catalog.Get("br-BC"), 2026, Texts, PtBr);
        var all = SheetTables.Holidays(catalog.Get("br-BC"), null, Texts, PtBr);

        Assert.HasCount(5, year.Rows);
        Assert.HasCount(6, all.Rows);
        Assert.IsFalse(year.Rows.Any(r => (DateTime)r[0]! == TestData.Tiradentes));
        CollectionAssert.AreEqual(new object?[] { new DateTime(2026, 12, 25), "sexta-feira", "Natal" }, year.Rows[^1]);
    }

    [TestMethod]
    public void CsvComPontoEVirgulaDatasCurtasEAspasQuandoPrecisa()
    {
        var table = new SheetTable("Feriados",
            [new TableColumn("Data", ColumnKind.Date), new TableColumn("Descrição", ColumnKind.Text), new TableColumn("N", ColumnKind.Number)],
            [
                [new DateTime(2026, 12, 25), "Natal", 1],
                [new DateTime(2026, 1, 25), "São Paulo; capital", 1234],
                [new DateTime(2026, 7, 9), "Revolução \"Constitucionalista\"", null]
            ]);

        var csv = SheetWriter.ToCsv(table, PtBr);

        Assert.AreEqual(
            "Data;Descrição;N\r\n" +
            "25/12/2026;Natal;1\r\n" +
            "25/01/2026;\"São Paulo; capital\";1234\r\n" +
            "09/07/2026;\"Revolução \"\"Constitucionalista\"\"\";\r\n", csv);
    }

    [TestMethod]
    public void TextoComTabulacoesParaAreaDeTransferencia()
    {
        var table = new SheetTable("Feriados",
            [new TableColumn("Data", ColumnKind.Date), new TableColumn("Descrição", ColumnKind.Text)],
            [[new DateTime(2026, 12, 25), "Natal\tde novo"]]);

        Assert.AreEqual("Data\tDescrição\r\n25/12/2026\tNatal de novo\r\n", SheetWriter.ToTabSeparated(table, PtBr));
    }

    [TestMethod]
    public void PlanilhaValidaComDatasComoDatas()
    {
        var table = new SheetTable("Feriados",
            [new TableColumn("Data", ColumnKind.Date), new TableColumn("Descrição", ColumnKind.Text), new TableColumn("N", ColumnKind.Number)],
            [
                [new DateTime(2026, 12, 25), "Natal", 1],
                [new DateTime(2026, 1, 25), "  São Paulo  ", null]
            ]);

        using var stream = new MemoryStream();
        SheetWriter.ToXlsx(table, stream);
        stream.Position = 0;
        using var document = SpreadsheetDocument.Open(stream, false);

        var errors = new OpenXmlValidator().Validate(document).ToList();
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors.Select(e => $"{e.Path?.XPath}: {e.Description}")));
        var workbook = document.WorkbookPart!;
        Assert.AreEqual("Feriados", workbook.Workbook!.Descendants<Sheet>().Single().Name!.Value);
        var cells = workbook.WorksheetParts.Single().Worksheet!.Descendants<Cell>().ToDictionary(c => c.CellReference!.Value!);
        Assert.AreEqual("Data", cells["A1"].InlineString!.Text!.Text);
        Assert.AreEqual(new DateTime(2026, 12, 25).ToOADate().ToString(CultureInfo.InvariantCulture), cells["A2"].CellValue!.Text);
        Assert.AreEqual(1u, cells["A2"].StyleIndex!.Value, "A data deveria ter o estilo de data.");
        Assert.AreEqual("1", cells["C2"].CellValue!.Text);
        Assert.AreEqual("  São Paulo  ", cells["B3"].InlineString!.Text!.Text);
        Assert.IsFalse(cells.ContainsKey("C3"), "Valor vazio não vira célula.");
    }

    [TestMethod]
    public void HtmlComDatasIsoMarcadasComoData()
    {
        var table = new SheetTable("Feriados",
            [new TableColumn("Data", ColumnKind.Date), new TableColumn("Descrição", ColumnKind.Text), new TableColumn("N", ColumnKind.Number)],
            [[new DateTime(2026, 12, 25), "Natal & <Ano Novo>", 3]]);

        var html = SheetWriter.ToHtml(table);

        StringAssert.Contains(html, "<th>Descrição</th>");
        StringAssert.Contains(html, "<td style=\"mso-number-format:'Short Date'\">2026-12-25</td>");
        StringAssert.Contains(html, "Natal &amp; &lt;Ano Novo&gt;");
        StringAssert.Contains(html, "<td style=\"mso-number-format:'0'\">3</td>");
    }
}
