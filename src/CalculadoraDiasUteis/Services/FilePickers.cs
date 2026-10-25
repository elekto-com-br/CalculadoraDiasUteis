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
using Microsoft.Windows.Storage.Pickers;

namespace CalculadoraDiasUteis.Services;

/// <summary>
///     A caixa "Salvar como" do Windows, para as exportações.
/// </summary>
public static class FilePickers
{
    /// <summary>
    ///     Pergunta onde salvar, começando na pasta Documentos com o nome sugerido.
    /// </summary>
    /// <param name="suggestedName">O nome sugerido, sem extensão; o que o Windows não aceita em nome de arquivo vira hífen.</param>
    /// <param name="types">Os tipos de arquivo, o primeiro sendo o padrão.</param>
    /// <returns>O caminho escolhido, ou nulo se o usuário desistiu.</returns>
    public static async Task<string?> PickSavePathAsync(string suggestedName,
        params (string Label, string Extension)[] types)
    {
        var picker = new FileSavePicker(App.Window.AppWindow.Id)
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            SuggestedFileName = SafeFileName(suggestedName),
            DefaultFileExtension = types[0].Extension
        };
        foreach (var (label, extension) in types)
        {
            picker.FileTypeChoices.Add(label, [extension]);
        }

        var file = await picker.PickSaveFileAsync();
        return file?.Path;
    }

    /// <summary>
    ///     O nome sem os caracteres que o Windows não aceita em arquivos.
    /// </summary>
    public static string SafeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return new string(name.Select(c => invalid.Contains(c) ? '-' : c).ToArray()).Trim();
    }
}
