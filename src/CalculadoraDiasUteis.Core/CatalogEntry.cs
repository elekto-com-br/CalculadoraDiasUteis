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
using Elekto.Calendars;

namespace CalculadoraDiasUteis.Core;

/// <summary>
///     De onde vem um feriado de um calendário.
/// </summary>
public enum HolidayOrigin
{
    /// <summary>
    ///     Do arquivo da Elekto, num original, ou das bases, num derivado.
    /// </summary>
    Inherited,

    /// <summary>
    ///     Incluído pelo usuário.
    /// </summary>
    Added,

    /// <summary>
    ///     Herdado, mas removido pelo usuário: não conta nas contas.
    /// </summary>
    Removed
}

/// <summary>
///     Um feriado como aparece na lista do calendário.
/// </summary>
/// <param name="Date">A data.</param>
/// <param name="Description">A descrição em vigor, nula se não houver.</param>
/// <param name="Origin">De onde vem.</param>
/// <param name="DescriptionChanged">Se o usuário trocou a descrição de um feriado herdado.</param>
public sealed record HolidayEntry(DateTime Date, string? Description, HolidayOrigin Origin, bool DescriptionChanged);

/// <summary>
///     Um calendário do catálogo, já montado com as bases e as alterações do usuário.
/// </summary>
public sealed class CatalogEntry
{
    private readonly Dictionary<DateTime, HolidayEntry> _byDate;

    internal CatalogEntry(string code, string name, CalendarKind kind, IReadOnlyList<string> bases,
        IReadOnlyList<HolidayEntry> holidays, IReadOnlyDictionary<DateTime, string?> inherited, int changeCount,
        ICalendar? calendar, CatalogError? error, string? errorDetail)
    {
        Inherited = inherited;
        Code = code;
        Name = name;
        Kind = kind;
        Bases = bases;
        Holidays = holidays;
        ChangeCount = changeCount;
        Calendar = calendar;
        Error = error;
        ErrorDetail = errorDetail;
        _byDate = holidays.ToDictionary(h => h.Date);
    }

    /// <summary>
    ///     O código, que identifica o calendário e não muda.
    /// </summary>
    public string Code { get; }

    /// <summary>
    ///     O nome mostrado na tela.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Original ou do usuário.
    /// </summary>
    public CalendarKind Kind { get; }

    /// <summary>
    ///     Os códigos das bases, nos calendários derivados.
    /// </summary>
    public IReadOnlyList<string> Bases { get; }

    /// <summary>
    ///     Os feriados no período do calendário, em ordem de data, inclusive os removidos pelo usuário.
    /// </summary>
    public IReadOnlyList<HolidayEntry> Holidays { get; }

    /// <summary>
    ///     Quantas alterações o usuário guardou neste calendário.
    /// </summary>
    public int ChangeCount { get; }

    /// <summary>
    ///     O calendário pronto para as contas; nulo se ele não pôde ser montado.
    /// </summary>
    public ICalendar? Calendar { get; }

    /// <summary>
    ///     Por que o calendário não pôde ser montado; nulo se ele está bem.
    /// </summary>
    public CatalogError? Error { get; }

    /// <summary>
    ///     O complemento do erro, como o código da base que falta.
    /// </summary>
    public string? ErrorDetail { get; }

    /// <summary>
    ///     Os feriados herdados, do arquivo ou das bases, com a descrição de origem, antes das alterações do usuário.
    /// </summary>
    internal IReadOnlyDictionary<DateTime, string?> Inherited { get; }

    /// <summary>
    ///     Se o calendário pode ser usado nas contas.
    /// </summary>
    public bool IsUsable => Calendar != null;

    /// <summary>
    ///     O feriado da data, inclusive se removido; nulo se a data não é feriado.
    /// </summary>
    public HolidayEntry? HolidayAt(DateTime date) => _byDate.GetValueOrDefault(date.Date);

    public override string ToString() => Name;
}
