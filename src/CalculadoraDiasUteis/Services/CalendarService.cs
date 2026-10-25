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
using Elekto.Calendars;
using Windows.Storage;

namespace CalculadoraDiasUteis.Services;

/// <summary>
///     O catálogo de calendários do aplicativo: os originais do pacote e as alterações do usuário, no banco local.
/// </summary>
/// <remarks>
///     É montado uma vez, na primeira consulta, e compartilhado pelas páginas.
/// </remarks>
public static class CalendarService
{
    private const string DatabaseFileName = "calendarios.db";

    private static readonly Lazy<(CalendarCatalog? Catalog, string? Error)> Loaded = new(Load);

    /// <summary>
    ///     O catálogo; nulo se os calendários não puderam ser lidos.
    /// </summary>
    public static CalendarCatalog? Catalog => Loaded.Value.Catalog;

    /// <summary>
    ///     Por que os calendários não puderam ser lidos; nulo se foram.
    /// </summary>
    public static string? LoadError => Loaded.Value.Error;

    private static (CalendarCatalog?, string?) Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Calendars");
        if (!Directory.Exists(path))
        {
            return (null, Strings.Format("ErrorCalendarsFolderMissing", path));
        }

        var provider = new TextsInFolderCalendarInfoProvider(path);
        var originals = provider.All().Select(Localize).ToList();
        if (originals.Count == 0)
        {
            return (null, Strings.Format("ErrorNoCalendars", path));
        }

        var database = Path.Combine(ApplicationData.Current.LocalFolder.Path, DatabaseFileName);
        try
        {
            return (new CalendarCatalog(originals, new CalendarStore(database)), null);
        }
        catch (CatalogException ex)
        {
            return (null, CatalogMessages.Get(ex));
        }
        catch (Exception ex)
        {
            return (null, Strings.Format("ErrorDatabase", database, ex.Message));
        }
    }

    /// <summary>
    ///     O nome do calendário da Elekto no idioma do aplicativo, que vem dos recursos pelo código; o arquivo traz o
    ///     nome em português, que fica valendo se o recurso não existir.
    /// </summary>
    private static CalendarInfo Localize(CalendarInfo info)
    {
        string name;
        try
        {
            name = Strings.Get("CalendarName_" + info.Name.Replace('-', '_'));
        }
        catch (COMException)
        {
            return info;
        }

        return string.IsNullOrEmpty(name)
            ? info
            : CalendarInfo.New(info.Name, info.Holidays, name, info.NonWorkDays, info.HolidayDescriptions);
    }
}
