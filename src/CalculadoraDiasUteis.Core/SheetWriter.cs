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
using System.Text;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace CalculadoraDiasUteis.Core;

/// <summary>
///     Grava uma tabela como planilha do Excel, como texto separado (CSV, ou tabulações para a área de transferência) e
///     como HTML, que é o que o Excel lê ao colar.
/// </summary>
public static class SheetWriter
{
    /// <summary>
    ///     A codificação dos arquivos CSV: UTF-8 com a marca no começo, sem a qual o Excel lê os acentos errado.
    /// </summary>
    public static readonly Encoding CsvEncoding = new UTF8Encoding(true);

    /// <summary>
    ///     A tabela em CSV, com o separador de listas e a data curta da cultura. Ela deve ser a do formato regional do
    ///     Windows, com as personalizações do usuário: é com ele que o Excel separa as colunas e reconhece as datas.
    /// </summary>
    public static string ToCsv(SheetTable table, CultureInfo culture)
    {
        var separator = culture.TextInfo.ListSeparator;
        return ToDelimited(table, culture, separator, value => Quote(value, separator));
    }

    /// <summary>
    ///     A tabela separada por tabulações, o texto simples da área de transferência, com as datas no formato curto da
    ///     cultura, que deve ser a do formato regional do Windows, como no CSV.
    /// </summary>
    public static string ToTabSeparated(SheetTable table, CultureInfo culture)
    {
        return ToDelimited(table, culture, "\t", value => value.Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' '));
    }

    /// <summary>
    ///     A tabela em HTML para a área de transferência. As datas vão no formato ISO, que o Excel reconhece em qualquer
    ///     idioma, marcadas para aparecer no formato de data curta do usuário; assim elas colam como datas, e não como texto.
    /// </summary>
    public static string ToHtml(SheetTable table)
    {
        var html = new StringBuilder();
        html.Append("<table>");
        html.Append("<tr>");
        foreach (var column in table.Columns)
        {
            html.Append("<th>").Append(Encode(column.Header)).Append("</th>");
        }

        html.Append("</tr>");
        foreach (var row in table.Rows)
        {
            html.Append("<tr>");
            for (var i = 0; i < table.Columns.Count; i++)
            {
                switch (row[i])
                {
                    case DateTime date:
                        html.Append("<td style=\"mso-number-format:'Short Date'\">")
                            .Append(date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)).Append("</td>");
                        break;
                    case int number:
                        html.Append("<td style=\"mso-number-format:'0'\">")
                            .Append(number.ToString(CultureInfo.InvariantCulture)).Append("</td>");
                        break;
                    case string text:
                        html.Append("<td style=\"mso-number-format:'\\@'\">").Append(Encode(text))
                            .Append("</td>");
                        break;
                    default:
                        html.Append("<td></td>");
                        break;
                }
            }

            html.Append("</tr>");
        }

        html.Append("</table>");
        return html.ToString();
    }

    /// <summary>
    ///     A tabela numa planilha do Excel (.xlsx), com as datas gravadas como datas, no formato de data curta de quem
    ///     abrir o arquivo, o cabeçalho em negrito e fixo no alto, e o filtro ligado.
    /// </summary>
    public static void ToXlsx(SheetTable table, Stream stream)
    {
        using var document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook);
        var workbookPart = document.AddWorkbookPart();
        workbookPart.Workbook = new Workbook();

        // Estilos: 0 é o padrão, 1 é data (o formato 14 do Excel, a data curta no idioma de quem abre) e 2, o cabeçalho
        var stylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();
        stylesPart.Stylesheet = new Stylesheet(
            new Fonts(new Font(), new Font(new Bold())) { Count = 2 },
            new Fills(new Fill(new PatternFill { PatternType = PatternValues.None }),
                new Fill(new PatternFill { PatternType = PatternValues.Gray125 })) { Count = 2 },
            new Borders(new Border()) { Count = 1 },
            new CellStyleFormats(new CellFormat()) { Count = 1 },
            new CellFormats(
                new CellFormat(),
                new CellFormat { NumberFormatId = 14, ApplyNumberFormat = true },
                new CellFormat { FontId = 1, ApplyFont = true }) { Count = 3 });

        var last = ColumnName(table.Columns.Count - 1);
        var sheetData = new SheetData();
        var header = new Row { RowIndex = 1 };
        for (var i = 0; i < table.Columns.Count; i++)
        {
            header.Append(TextCell($"{ColumnName(i)}1", table.Columns[i].Header, 2));
        }

        sheetData.Append(header);
        for (var r = 0; r < table.Rows.Count; r++)
        {
            var index = (uint)(r + 2);
            var row = new Row { RowIndex = index };
            for (var i = 0; i < table.Columns.Count; i++)
            {
                var reference = $"{ColumnName(i)}{index}";
                switch (table.Rows[r][i])
                {
                    case DateTime date:
                        row.Append(NumberCell(reference, date.ToOADate().ToString(CultureInfo.InvariantCulture), 1));
                        break;
                    case int number:
                        row.Append(NumberCell(reference, number.ToString(CultureInfo.InvariantCulture), 0));
                        break;
                    case string text:
                        row.Append(TextCell(reference, text, 0));
                        break;
                }
            }

            sheetData.Append(row);
        }

        var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
        worksheetPart.Worksheet = new Worksheet(
            new SheetViews(new SheetView(new Pane
            {
                VerticalSplit = 1, TopLeftCell = "A2", ActivePane = PaneValues.BottomLeft, State = PaneStateValues.Frozen
            }) { WorkbookViewId = 0, TabSelected = true }),
            new Columns(table.Columns.Select((column, i) => new Column
            {
                Min = (uint)(i + 1), Max = (uint)(i + 1), Width = Width(table, i), CustomWidth = true
            })),
            sheetData,
            new AutoFilter { Reference = $"A1:{last}{table.Rows.Count + 1}" });

        workbookPart.Workbook.Append(new Sheets(new Sheet
        {
            Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = table.Name
        }));
        workbookPart.Workbook.Save();
    }

    private static Cell NumberCell(string reference, string value, uint style) =>
        new() { CellReference = reference, StyleIndex = style, CellValue = new CellValue(value) };

    private static Cell TextCell(string reference, string text, uint style) =>
        new(new InlineString(new Text(text) { Space = SpaceProcessingModeValues.Preserve }))
        {
            CellReference = reference, StyleIndex = style, DataType = CellValues.InlineString
        };

    // A, B, ..., Z, AA...; as tabelas daqui têm poucas colunas
    private static string ColumnName(int index) =>
        index < 26 ? ((char)('A' + index)).ToString() : ColumnName(index / 26 - 1) + (char)('A' + index % 26);

    // em caracteres, pelo texto mais comprido da coluna; datas cabem em 12
    private static double Width(SheetTable table, int column)
    {
        var longest = table.Rows.Select(r => r[column] switch
            {
                DateTime => 10,
                int number => number.ToString(CultureInfo.InvariantCulture).Length,
                string text => text.Length,
                _ => 0
            })
            .Append(table.Columns[column].Header.Length)
            .Max();
        return Math.Min(longest + 3, 60);
    }

    // só o que o HTML exige; os acentos vão como estão, porque a área de transferência leva o HTML em UTF-8
    private static string Encode(string text) =>
        text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");

    private static string ToDelimited(SheetTable table, CultureInfo culture, string separator, Func<string, string> escape)
    {
        var text = new StringBuilder();
        text.AppendJoin(separator, table.Columns.Select(c => escape(c.Header))).Append("\r\n");
        foreach (var row in table.Rows)
        {
            text.AppendJoin(separator, row.Select(value => escape(Format(value, culture)))).Append("\r\n");
        }

        return text.ToString();
    }

    private static string Format(object? value, CultureInfo culture) => value switch
    {
        DateTime date => date.ToString("d", culture),
        int number => number.ToString(CultureInfo.InvariantCulture),
        string text => text,
        _ => string.Empty
    };

    private static string Quote(string value, string separator)
    {
        if (!value.Contains(separator, StringComparison.Ordinal) && value.IndexOfAny(['"', '\r', '\n']) < 0)
        {
            return value;
        }

        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}
