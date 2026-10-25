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
using System.Collections.ObjectModel;
using System.Globalization;
using CalculadoraDiasUteis.Core;
using CalculadoraDiasUteis.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Elekto.Calendars;

namespace CalculadoraDiasUteis.ViewModels;

/// <summary>
///     As duas contas da calculadora: os dias entre duas datas e a data final de um prazo.
/// </summary>
/// <remarks>
///     As contas usam exatamente as chamadas da 1.2, mas são refeitas a cada mudança de campo, em vez de esperar um botão.
/// </remarks>
public partial class MainPageViewModel : ObservableObject
{
    /// <summary>
    ///     O maior prazo aceito, em módulo; o mesmo limite da 1.2.
    /// </summary>
    public const int MaxTerm = 100_000;

    // Índices das opções nos grupos de botões de rádio, na ordem em que aparecem na tela
    private const int FinancialIndex = 0;
    private const int FullDaysIndex = 1;
    private const int WorkDaysIndex = 0;
    private const int ActualDaysIndex = 1;
    private const int AdjustNoneIndex = 0;
    private const int AdjustFollowingIndex = 1;
    private const int AdjustModifiedFollowingIndex = 2;

    private readonly AppSettings _settings = new();

    // Enquanto o construtor restaura as escolhas, nada é gravado nem calculado
    private readonly bool _isInitialized;

    // Enquanto a lista é refeita depois de uma edição, a seleção vazia que a caixa de escolha devolve não conta
    private bool _refreshing;

    public MainPageViewModel()
    {
        if (CalendarService.Catalog is not { } catalog)
        {
            LoadError = CalendarService.LoadError;
            return;
        }

        FillCalendars(catalog);
        catalog.Changed += Catalog_Changed;

        var today = DateTime.Today;
        PeriodStart = new DateTimeOffset(today);
        PeriodEnd = new DateTimeOffset(today.AddYears(1));
        TermStart = new DateTimeOffset(today);

        RestoreSettings();
        _isInitialized = true;

        ApplyCalendarLimits();
        RecalculatePeriod();
        RecalculateTerm();
    }

    /// <summary>
    ///     Os calendários que podem ser usados nas contas; muda quando o usuário edita os calendários.
    /// </summary>
    public ObservableCollection<CatalogEntry> Calendars { get; } = [];

    /// <summary>
    ///     Por que os calendários não puderam ser carregados; nulo se foram.
    /// </summary>
    public string? LoadError { get; }

    /// <summary>
    ///     Verdadeiro se há calendários e as contas podem ser feitas.
    /// </summary>
    public bool IsReady => LoadError == null;

    [ObservableProperty]
    public partial int SelectedCalendarIndex { get; set; } = -1;

    /// <summary>
    ///     A menor data do calendário escolhido.
    /// </summary>
    [ObservableProperty]
    public partial DateTimeOffset MinDate { get; set; } = DateTimeOffset.MinValue;

    /// <summary>
    ///     A maior data do calendário escolhido.
    /// </summary>
    [ObservableProperty]
    public partial DateTimeOffset MaxDate { get; set; } = DateTimeOffset.MaxValue;

    #region Dias entre duas datas

    [ObservableProperty]
    public partial DateTimeOffset? PeriodStart { get; set; }

    [ObservableProperty]
    public partial DateTimeOffset? PeriodEnd { get; set; }

    /// <summary>
    ///     O critério de contagem: financeiro (0) ou dias cheios (1).
    /// </summary>
    [ObservableProperty]
    public partial int CountingIndex { get; set; }

    [ObservableProperty]
    public partial string WorkDays { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string WorkDaysCaption { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ActualDays { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ActualDaysCaption { get; set; } = string.Empty;

    /// <summary>
    ///     Os dois resultados por extenso, para leitores de tela.
    /// </summary>
    [ObservableProperty]
    public partial string PeriodDescription { get; set; } = string.Empty;

    /// <summary>
    ///     Por que os dias entre as datas não puderam ser contados; vazio se puderam.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPeriodError))]
    public partial string PeriodError { get; set; } = string.Empty;

    public bool HasPeriodError => PeriodError.Length > 0;

    #endregion

    #region Data final de um prazo

    [ObservableProperty]
    public partial DateTimeOffset? TermStart { get; set; }

    /// <summary>
    ///     O prazo; <see cref="double.NaN" /> quando o campo está vazio.
    /// </summary>
    [ObservableProperty]
    public partial double Term { get; set; }

    /// <summary>
    ///     O tipo de prazo: dias úteis (0) ou corridos (1).
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAdjustEnabled))]
    public partial int TermTypeIndex { get; set; }

    /// <summary>
    ///     O ajuste da data final: nenhum (0), dia útil seguinte (1) ou seguinte modificado (2).
    /// </summary>
    [ObservableProperty]
    public partial int AdjustIndex { get; set; }

    /// <summary>
    ///     O ajuste só se aplica a prazos em dias corridos.
    /// </summary>
    public bool IsAdjustEnabled => TermTypeIndex == ActualDaysIndex;

    [ObservableProperty]
    public partial string FinalDate { get; set; } = string.Empty;

    /// <summary>
    ///     O dia da semana da data final e, se for o caso, o aviso de que ela não é dia útil.
    /// </summary>
    [ObservableProperty]
    public partial string FinalDateCaption { get; set; } = string.Empty;

    /// <summary>
    ///     A data final por extenso, para leitores de tela.
    /// </summary>
    [ObservableProperty]
    public partial string FinalDateDescription { get; set; } = string.Empty;

    /// <summary>
    ///     Por que a data final não pôde ser calculada; vazio se pôde.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasTermError))]
    public partial string TermError { get; set; } = string.Empty;

    public bool HasTermError => TermError.Length > 0;

    #endregion

    /// <summary>
    ///     O código do calendário escolhido, para a edição dos calendários abrir nele.
    /// </summary>
    public string? SelectedCalendarCode => Entry?.Code;

    private CatalogEntry? Entry =>
        SelectedCalendarIndex >= 0 && SelectedCalendarIndex < Calendars.Count ? Calendars[SelectedCalendarIndex] : null;

    private ICalendar? Calendar => Entry?.Calendar;

    partial void OnSelectedCalendarIndexChanged(int value)
    {
        if (!_isInitialized || _refreshing || Calendar is not { } calendar)
        {
            return;
        }

        _settings.LastCalendar = calendar.Name;
        ApplyCalendarLimits();
        RecalculatePeriod();
        RecalculateTerm();
    }

    partial void OnPeriodStartChanged(DateTimeOffset? value) => RecalculatePeriod();

    partial void OnPeriodEndChanged(DateTimeOffset? value) => RecalculatePeriod();

    partial void OnCountingIndexChanged(int value)
    {
        if (!_isInitialized || value < 0)
        {
            return;
        }

        _settings.LastCalculationType = Counting.ToString();
        RecalculatePeriod();
    }

    partial void OnTermStartChanged(DateTimeOffset? value) => RecalculateTerm();

    partial void OnTermChanged(double value)
    {
        if (!_isInitialized)
        {
            return;
        }

        if (!double.IsNaN(value))
        {
            _settings.LastTerm = TermDays;
        }

        RecalculateTerm();
    }

    partial void OnTermTypeIndexChanged(int value)
    {
        if (!_isInitialized || value < 0)
        {
            return;
        }

        _settings.LastPeriodType = TermType.ToString();
        RecalculateTerm();
    }

    partial void OnAdjustIndexChanged(int value)
    {
        if (!_isInitialized || value < 0)
        {
            return;
        }

        _settings.LastFinalDateAdjust = Adjust.ToString();
        RecalculateTerm();
    }

    private DeltaTerminalDayAdjust Counting =>
        CountingIndex == FinancialIndex ? DeltaTerminalDayAdjust.Financial : DeltaTerminalDayAdjust.Full;

    private PeriodType TermType => TermTypeIndex == WorkDaysIndex ? PeriodType.WorkDays : PeriodType.ActualDays;

    private FinalDateAdjust Adjust => AdjustIndex switch
    {
        AdjustFollowingIndex => FinalDateAdjust.Following,
        AdjustModifiedFollowingIndex => FinalDateAdjust.ModifiedFollowing,
        _ => FinalDateAdjust.None
    };

    private int TermDays => (int)Math.Clamp(Math.Round(Term), -MaxTerm, MaxTerm);

    private void FillCalendars(CalendarCatalog catalog)
    {
        Calendars.Clear();
        foreach (var entry in catalog.Calendars.Where(c => c.IsUsable))
        {
            Calendars.Add(entry);
        }
    }

    /// <summary>
    ///     Depois de uma edição, refaz a lista e as contas, no mesmo calendário se ele ainda existir.
    /// </summary>
    private void Catalog_Changed(object? sender, EventArgs e)
    {
        var code = Entry?.Code;
        _refreshing = true;
        FillCalendars((CalendarCatalog)sender!);
        var index = Calendars.ToList().FindIndex(c => string.Equals(c.Code, code, StringComparison.OrdinalIgnoreCase));
        SelectedCalendarIndex = -1;
        _refreshing = false;
        SelectedCalendarIndex = Math.Max(index, 0);
    }

    private void RestoreSettings()
    {
        var calendarIndex = Calendars.ToList().FindIndex(c =>
            string.Equals(c.Code, _settings.LastCalendar, StringComparison.OrdinalIgnoreCase));
        SelectedCalendarIndex = Math.Max(calendarIndex, 0);

        // Os padrões são os da 1.2: financeiro, dias úteis, sem ajuste e 252 dias
        CountingIndex = _settings.LastCalculationType == nameof(DeltaTerminalDayAdjust.Full)
            ? FullDaysIndex
            : FinancialIndex;

        TermTypeIndex = _settings.LastPeriodType == nameof(PeriodType.ActualDays) ? ActualDaysIndex : WorkDaysIndex;

        AdjustIndex = _settings.LastFinalDateAdjust switch
        {
            nameof(FinalDateAdjust.Following) => AdjustFollowingIndex,
            nameof(FinalDateAdjust.ModifiedFollowing) => AdjustModifiedFollowingIndex,
            _ => AdjustNoneIndex
        };

        Term = Math.Clamp(_settings.LastTerm ?? 252, -MaxTerm, MaxTerm);
    }

    /// <summary>
    ///     Traz as datas para dentro dos limites do calendário e os publica para os seletores de data.
    /// </summary>
    private void ApplyCalendarLimits()
    {
        if (Calendar is not { } calendar)
        {
            return;
        }

        var min = new DateTimeOffset(calendar.MinDate);
        var max = new DateTimeOffset(calendar.MaxDate);

        // As datas entram nos limites antes deles mudarem, para que nenhum seletor fique com data fora deles
        PeriodStart = Clamp(PeriodStart, min, max);
        PeriodEnd = Clamp(PeriodEnd, min, max);
        TermStart = Clamp(TermStart, min, max);

        MinDate = min;
        MaxDate = max;
    }

    private static DateTimeOffset? Clamp(DateTimeOffset? date, DateTimeOffset min, DateTimeOffset max)
    {
        if (date is not { } value)
        {
            return null;
        }

        if (value < min)
        {
            return min;
        }

        return value > max ? max : value;
    }

    private void RecalculatePeriod()
    {
        if (!_isInitialized)
        {
            return;
        }

        WorkDays = ActualDays = WorkDaysCaption = ActualDaysCaption = PeriodDescription = string.Empty;
        PeriodError = string.Empty;

        if (Calendar is not { } calendar || PeriodStart is not { } start || PeriodEnd is not { } end)
        {
            return;
        }

        try
        {
            var workDays = calendar.GetDeltaWorkDays(start.Date, end.Date, Counting);
            var actualDays = calendar.GetDeltaActualDays(start.Date, end.Date, Counting);

            WorkDays = workDays.ToString("N0", CultureInfo.CurrentCulture);
            WorkDaysCaption = Strings.Get(Math.Abs(workDays) == 1 ? "WorkDayCaption" : "WorkDaysCaption");
            ActualDays = actualDays.ToString("N0", CultureInfo.CurrentCulture);
            ActualDaysCaption = Strings.Get(Math.Abs(actualDays) == 1 ? "ActualDayCaption" : "ActualDaysCaption");
            PeriodDescription = Strings.Format("PeriodDescription", WorkDays, WorkDaysCaption, ActualDays,
                ActualDaysCaption);
        }
        catch (ArgumentOutOfRangeException)
        {
            PeriodError = OutsideCalendarMessage(calendar);
        }
        catch (Exception ex)
        {
            PeriodError = ex.Message;
        }
    }

    private void RecalculateTerm()
    {
        if (!_isInitialized)
        {
            return;
        }

        FinalDate = FinalDateCaption = FinalDateDescription = string.Empty;
        TermError = string.Empty;

        if (Calendar is not { } calendar || TermStart is not { } start || double.IsNaN(Term))
        {
            return;
        }

        try
        {
            // As mesmas chamadas da 1.2: o prazo em dias úteis usa o ajuste padrão da biblioteca
            var end = TermType == PeriodType.WorkDays
                ? calendar.AddWorkDays(start.Date, TermDays)
                : calendar.AddActualDays(start.Date, TermDays, Adjust);

            var culture = CultureInfo.CurrentCulture;
            var weekDay = end.ToString("dddd", culture);
            var description = Strings.Format("FinalDateDescription", end.ToString("D", culture));

            // Sem ajuste, a data final em dias corridos pode cair num dia não útil, o que vale avisar
            if (end >= calendar.MinDate && end <= calendar.MaxDate && !calendar.IsWorkday(end))
            {
                var holiday = Entry?.HolidayAt(end)?.Description;
                var note = calendar.IsProperHoliday(end)
                    ? holiday == null ? Strings.Get("HolidayNote") : Strings.Format("HolidayNoteWithDescription", holiday)
                    : Strings.Get("NonWorkDayNote");
                weekDay = Strings.Format("TextWithNote", weekDay, note);
                description = Strings.Format("TextWithNote", description, note);
            }

            FinalDate = end.ToString("d", culture);
            FinalDateCaption = weekDay;
            FinalDateDescription = description;
        }
        catch (ArgumentOutOfRangeException)
        {
            TermError = OutsideCalendarMessage(calendar);
        }
        catch (Exception ex)
        {
            TermError = ex.Message;
        }
    }

    private static string OutsideCalendarMessage(ICalendar calendar)
    {
        var culture = CultureInfo.CurrentCulture;
        return Strings.Format("ErrorOutsideCalendar", calendar.MinDate.ToString("d", culture),
            calendar.MaxDate.ToString("d", culture));
    }
}
