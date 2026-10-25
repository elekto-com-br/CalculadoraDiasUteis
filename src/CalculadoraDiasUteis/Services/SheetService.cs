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
using System.Runtime.InteropServices;
using CalculadoraDiasUteis.Core;
using Windows.ApplicationModel.DataTransfer;

namespace CalculadoraDiasUteis.Services;

/// <summary>
///     Leva as tabelas da calculadora para a planilha: pela área de transferência ou num arquivo .xlsx ou .csv.
/// </summary>
public static class SheetService
{
    private const string XlsxExtension = ".xlsx";
    private const string CsvExtension = ".csv";

    /// <summary>
    ///     Os textos das tabelas, no idioma do aplicativo.
    /// </summary>
    public static SheetTexts Texts => new(
        Strings.Get("SheetPeriod"), Strings.Get("SheetHolidays"), Strings.Get("ColumnDate"),
        Strings.Get("ColumnWeekDay"), Strings.Get("ColumnIsWorkday"), Strings.Get("ColumnWorkdayCount"),
        Strings.Get("ColumnHoliday"), Strings.Get("ColumnDescription"));

    /// <summary>
    ///     Copia a tabela em dois formatos: HTML, que o Excel cola com as datas como datas, e texto separado por
    ///     tabulações, para os programas que só aceitam texto. Ela continua na área de transferência depois de o
    ///     aplicativo fechar.
    /// </summary>
    public static void Copy(SheetTable table)
    {
        var package = new DataPackage { RequestedOperation = DataPackageOperation.Copy };
        package.SetText(SheetWriter.ToTabSeparated(table, App.RegionalCulture));
        package.SetHtmlFormat(HtmlFormatHelper.CreateHtmlFormat(SheetWriter.ToHtml(table)));
        Clipboard.SetContent(package);
        Clipboard.Flush();
    }

    /// <summary>
    ///     Pergunta onde gravar a planilha, em .xlsx ou .csv.
    /// </summary>
    /// <returns>O caminho escolhido, ou nulo se o usuário desistiu.</returns>
    /// <exception cref="COMException">A caixa do Windows falhou.</exception>
    public static Task<string?> PickPathAsync(string suggestedName) =>
        FilePickers.PickSavePathAsync(suggestedName, (Strings.Get("SheetFileTypeXlsx"), XlsxExtension),
            (Strings.Get("SheetFileTypeCsv"), CsvExtension));

    /// <summary>
    ///     Grava a tabela no formato da extensão do arquivo: CSV para .csv, planilha do Excel para os demais.
    /// </summary>
    public static void Save(SheetTable table, string path)
    {
        if (string.Equals(Path.GetExtension(path), CsvExtension, StringComparison.OrdinalIgnoreCase))
        {
            File.WriteAllText(path, SheetWriter.ToCsv(table, App.RegionalCulture), SheetWriter.CsvEncoding);
            return;
        }

        using var stream = File.Create(path);
        SheetWriter.ToXlsx(table, stream);
    }
}
