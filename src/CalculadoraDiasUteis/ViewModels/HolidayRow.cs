using System.Globalization;
using CalculadoraDiasUteis.Core;
using CalculadoraDiasUteis.Services;

namespace CalculadoraDiasUteis.ViewModels;

/// <summary>
///     Uma linha da lista de feriados de um ano.
/// </summary>
public sealed class HolidayRow
{
    public HolidayRow(HolidayEntry holiday, CalendarKind kind)
    {
        var culture = CultureInfo.CurrentCulture;
        Date = holiday.Date;
        DateText = holiday.Date.ToString("d", culture);
        WeekDayText = holiday.Date.ToString("dddd", culture);
        Description = holiday.Description ?? string.Empty;
        IsRemoved = holiday.Origin == HolidayOrigin.Removed;
        CanRestore = IsRemoved || holiday.DescriptionChanged;
        OriginText = Strings.Get(holiday switch
        {
            { Origin: HolidayOrigin.Added } => "OriginAdded",
            { Origin: HolidayOrigin.Removed } => "OriginRemoved",
            { DescriptionChanged: true } => "OriginDescribed",
            _ => kind == CalendarKind.Original ? "OriginElekto" : "OriginInherited"
        });
        LongDateText = holiday.Date.ToString("D", culture);
    }

    public DateTime Date { get; }

    public string DateText { get; }

    public string WeekDayText { get; }

    public string Description { get; }

    public string OriginText { get; }

    /// <summary>
    ///     Feriado herdado que o usuário removeu: aparece riscado e não conta.
    /// </summary>
    public bool IsRemoved { get; }

    /// <summary>
    ///     Se há alteração do usuário a desfazer: a remoção ou a troca de descrição de um herdado.
    /// </summary>
    public bool CanRestore { get; }

    private string LongDateText { get; }

    /// <summary>
    ///     O que o Narrador lê na linha: a data por extenso, a descrição e a origem.
    /// </summary>
    public override string ToString() =>
        string.Join(Strings.Get("ListSeparator"),
            new[] { LongDateText, Description, OriginText }.Where(s => s.Length > 0));
}
