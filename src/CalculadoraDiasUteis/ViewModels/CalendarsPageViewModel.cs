using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using CalculadoraDiasUteis.Core;
using CalculadoraDiasUteis.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Windows.ApplicationModel;

namespace CalculadoraDiasUteis.ViewModels;

/// <summary>
///     A edição dos calendários: criar, derivar, renomear, apagar e restaurar, os feriados de cada ano, e a
///     exportação e a importação.
/// </summary>
public partial class CalendarsPageViewModel : ObservableObject
{
    private readonly CalendarCatalog? _catalog = CalendarService.Catalog;

    // Enquanto a lista é refeita, a seleção vazia que a caixa de escolha devolve não conta
    private bool _refreshing;

    public CalendarsPageViewModel()
    {
        if (_catalog == null)
        {
            return;
        }

        _catalog.Changed += Catalog_Changed;
        FillCalendars(null);
    }

    public ObservableCollection<CatalogEntry> Calendars { get; } = [];

    public ObservableCollection<HolidayRow> Holidays { get; } = [];

    public string? LoadError => CalendarService.LoadError;

    public bool IsReady => _catalog != null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCustom), nameof(IsOriginal), nameof(CanRestoreCalendar), nameof(CanEditHolidays),
        nameof(CanEditHoliday), nameof(CanRestoreHoliday))]
    public partial CatalogEntry? SelectedCalendar { get; set; }

    [ObservableProperty]
    public partial double Year { get; set; } = DateTime.Today.Year;

    [ObservableProperty]
    public partial double MinYear { get; set; } = 1;

    [ObservableProperty]
    public partial double MaxYear { get; set; } = 9999;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanEditHoliday), nameof(CanRestoreHoliday))]
    public partial HolidayRow? SelectedHoliday { get; set; }

    [ObservableProperty]
    public partial string CalendarKindText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CalendarPeriodText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasCalendarError))]
    public partial string CalendarError { get; set; } = string.Empty;

    public bool HasCalendarError => CalendarError.Length > 0;

    [ObservableProperty]
    public partial string HolidaysSummary { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsStatusError { get; set; }

    [ObservableProperty]
    public partial bool IsStatusOpen { get; set; }

    public bool IsCustom => SelectedCalendar?.Kind == CalendarKind.Custom;

    public bool IsOriginal => SelectedCalendar?.Kind == CalendarKind.Original;

    public bool CanRestoreCalendar => SelectedCalendar is { Kind: CalendarKind.Original, ChangeCount: > 0 };

    public bool CanEditHolidays => SelectedCalendar?.IsUsable == true;

    public bool CanEditHoliday => CanEditHolidays && SelectedHoliday is { IsRemoved: false };

    public bool CanRestoreHoliday => SelectedHoliday is { CanRestore: true };

    /// <summary>
    ///     Os calendários que podem servir de base, para a escolha na criação e na troca de bases: os que podem ser
    ///     usados e não dependem do próprio calendário.
    /// </summary>
    public IReadOnlyList<CatalogEntry> PossibleBases(string? exceptCode) =>
        _catalog?.Calendars.Where(c => c.IsUsable && (exceptCode == null || !_catalog.DependsOn(c.Code, exceptCode)))
                .ToList() ?? [];

    /// <summary>
    ///     O período em que se pode incluir feriado: o do calendário, menos nos próprios sem base, que crescem com eles.
    /// </summary>
    public (DateTime Min, DateTime Max)? HolidayRange =>
        SelectedCalendar is { Calendar: { } calendar } entry &&
        (entry.Kind == CalendarKind.Original || entry.Bases.Count > 0)
            ? (calendar.MinDate, calendar.MaxDate)
            : null;

    /// <summary>
    ///     A data sugerida para um feriado novo: hoje, se for do ano mostrado, ou o primeiro dia dele.
    /// </summary>
    public DateTime SuggestedHolidayDate
    {
        get
        {
            var year = (int)Year;
            var date = DateTime.Today.Year == year ? DateTime.Today : new DateTime(year, 1, 1);
            if (HolidayRange is { } range)
            {
                date = date < range.Min ? range.Min : date > range.Max ? range.Max : date;
            }

            return date;
        }
    }

    /// <summary>
    ///     Deixa de acompanhar o catálogo, quando a página sai de cena.
    /// </summary>
    public void Detach()
    {
        if (_catalog != null)
        {
            _catalog.Changed -= Catalog_Changed;
        }
    }

    public void Select(string? code)
    {
        if (code != null && Calendars.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase)) is { } entry)
        {
            SelectedCalendar = entry;
        }
    }

    #region Calendários

    public string? CreateCalendar(string name, IReadOnlyList<string> bases)
    {
        return Run(catalog =>
        {
            var entry = catalog.Create(name, bases);
            Select(entry.Code);
            ShowStatus(Strings.Format("StatusCreated", entry.Name));
        });
    }

    public string? RenameCalendar(string name)
    {
        return Run(catalog =>
        {
            catalog.Rename(SelectedCalendar!.Code, name);
            ShowStatus(Strings.Format("StatusRenamed", SelectedCalendar!.Name));
        });
    }

    public string? SetBases(IReadOnlyList<string> bases)
    {
        return Run(catalog =>
        {
            catalog.SetBases(SelectedCalendar!.Code, bases);
            ShowStatus(Strings.Format("StatusBasesChanged", SelectedCalendar!.Name));
        });
    }

    /// <summary>
    ///     Os nomes dos calendários que têm o selecionado como base, que impedem apagá-lo.
    /// </summary>
    public IReadOnlyList<string> Dependents =>
        SelectedCalendar == null || _catalog == null ? [] : _catalog.DependentsOf(SelectedCalendar.Code).Select(d => d.Name).ToList();

    public void DeleteCalendar()
    {
        var name = SelectedCalendar!.Name;
        ShowResult(Run(catalog =>
        {
            catalog.Delete(SelectedCalendar!.Code);
            ShowStatus(Strings.Format("StatusDeleted", name));
        }));
    }

    public void RestoreCalendar()
    {
        var name = SelectedCalendar!.Name;
        ShowResult(Run(catalog =>
        {
            catalog.Restore(SelectedCalendar!.Code);
            ShowStatus(Strings.Format("StatusRestored", name));
        }));
    }

    /// <summary>
    ///     Quantos calendários da Elekto têm alteração do usuário.
    /// </summary>
    public int OriginalsWithChanges =>
        _catalog?.Calendars.Count(c => c.Kind == CalendarKind.Original && c.ChangeCount > 0) ?? 0;

    public void RestoreAllOriginals()
    {
        ShowResult(Run(catalog =>
        {
            catalog.RestoreAllOriginals();
            ShowStatus(Strings.Get("StatusRestoredAll"));
        }));
    }

    #endregion

    #region Feriados

    public string? AddHoliday(DateTime date, string? description)
    {
        return Run(catalog =>
        {
            catalog.AddHoliday(SelectedCalendar!.Code, date, description);
            Year = date.Year;
            SelectedHoliday = Holidays.FirstOrDefault(h => h.Date == date.Date);
            ShowStatus(Strings.Format("StatusHolidayAdded", date.ToString("d", CultureInfo.CurrentCulture)));
        });
    }

    public string? SetDescription(string? description)
    {
        var date = SelectedHoliday!.Date;
        return Run(catalog =>
        {
            catalog.SetDescription(SelectedCalendar!.Code, date, description);
            ShowStatus(Strings.Format("StatusDescriptionChanged", date.ToString("d", CultureInfo.CurrentCulture)));
        });
    }

    public void RemoveHoliday()
    {
        if (!CanEditHoliday)
        {
            return;
        }

        var date = SelectedHoliday!.Date;
        ShowResult(Run(catalog =>
        {
            catalog.RemoveHoliday(SelectedCalendar!.Code, date);
            ShowStatus(Strings.Format("StatusHolidayRemoved", date.ToString("d", CultureInfo.CurrentCulture)));
        }));
    }

    public void RestoreHoliday()
    {
        if (!CanRestoreHoliday)
        {
            return;
        }

        var date = SelectedHoliday!.Date;
        ShowResult(Run(catalog =>
        {
            catalog.RestoreHoliday(SelectedCalendar!.Code, date);
            ShowStatus(Strings.Format("StatusHolidayRestored", date.ToString("d", CultureInfo.CurrentCulture)));
        }));
    }

    #endregion

    #region Exportação e importação

    /// <summary>
    ///     Se há o que exportar em "Exportar tudo": algum calendário do usuário ou alteração num da Elekto.
    /// </summary>
    public bool HasUserData =>
        _catalog?.Calendars.Any(c => c.Kind == CalendarKind.Custom || c.ChangeCount > 0) == true;

    /// <summary>
    ///     Se o calendário selecionado tem o que exportar: é do usuário ou tem alteração.
    /// </summary>
    public bool CanExportCalendar => SelectedCalendar is { Kind: CalendarKind.Custom } or { ChangeCount: > 0 };

    public void Export(string path, bool all)
    {
        try
        {
            using (var stream = File.Create(path))
            {
                var version = Package.Current.Id.Version;
                var application = $"{version.Major}.{version.Minor}.{version.Build}";
                if (all)
                {
                    CalendarExchange.ExportAll(_catalog!, stream, application, DateTimeOffset.Now);
                }
                else
                {
                    CalendarExchange.Export(_catalog!, [SelectedCalendar!.Code], stream, application, DateTimeOffset.Now);
                }
            }

            ShowStatus(Strings.Format("StatusExported", path));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or CatalogException)
        {
            ShowStatus(Strings.Format("ErrorExport", path, ex is CatalogException c ? CatalogMessages.Get(c) : ex.Message),
                true);
        }
    }

    /// <summary>
    ///     Lê os arquivos e diz o que a importação faria.
    /// </summary>
    public ImportPlan PlanImport(IEnumerable<string> paths)
    {
        var sources = new List<ImportSource>();
        var unreadable = new List<ImportFailure>();
        foreach (var path in paths)
        {
            try
            {
                // UTF-8, como na 1.x, a menos que o arquivo diga outra coisa no começo
                sources.Add(new ImportSource(path, File.ReadAllText(path, Encoding.UTF8)));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                unreadable.Add(new ImportFailure(Path.GetFileName(path), CatalogError.UnknownFormat, ex.Message));
            }
        }

        var plan = CalendarExchange.Plan(_catalog!, sources);
        return unreadable.Count == 0 ? plan : CalendarExchange.WithFailures(plan, unreadable);
    }

    public void ApplyImport(ImportPlan plan)
    {
        ShowResult(Run(catalog =>
        {
            var count = CalendarExchange.Apply(catalog, plan);
            Select(plan.Items.FirstOrDefault(i => i.IsSelected && i.Code != null)?.Code);

            ShowStatus(count == 1 ? Strings.Get("StatusImportedOne") : Strings.Format("StatusImportedMany", count));
        }));
    }

    #endregion

    public void ShowStatus(string message, bool isError = false)
    {
        StatusMessage = message;
        IsStatusError = isError;
        IsStatusOpen = true;
    }

    partial void OnSelectedCalendarChanged(CatalogEntry? value)
    {
        if (!_refreshing)
        {
            ShowCalendar();
        }
    }

    partial void OnYearChanged(double value)
    {
        if (double.IsNaN(value))
        {
            return;
        }

        var year = Math.Clamp(Math.Round(value), MinYear, MaxYear);
        if (year != value)
        {
            Year = year;
            return;
        }

        ShowHolidays();
    }

    private string? Run(Action<CalendarCatalog> action)
    {
        if (_catalog == null)
        {
            return LoadError;
        }

        try
        {
            action(_catalog);
            return null;
        }
        catch (CatalogException ex)
        {
            return CatalogMessages.Get(ex);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or Microsoft.Data.Sqlite.SqliteException)
        {
            return Strings.Format("ErrorSave", ex.Message);
        }
    }

    private void ShowResult(string? error)
    {
        if (error != null)
        {
            ShowStatus(error, true);
        }
    }

    private void Catalog_Changed(object? sender, EventArgs e) => FillCalendars(SelectedCalendar?.Code);

    private void FillCalendars(string? code)
    {
        _refreshing = true;
        Calendars.Clear();
        foreach (var entry in _catalog!.Calendars)
        {
            Calendars.Add(entry);
        }

        _refreshing = false;

        var selected = (code == null ? null : Calendars.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase)))
                       ?? Calendars.FirstOrDefault();
        if (ReferenceEquals(selected, SelectedCalendar))
        {
            ShowCalendar();
        }
        else
        {
            SelectedCalendar = selected;
        }
    }

    private void ShowCalendar()
    {
        var entry = SelectedCalendar;
        CalendarKindText = entry == null ? string.Empty : KindText(entry);
        CalendarError = entry is { IsUsable: false } ? CatalogMessages.Get(entry.Error!.Value, entry.ErrorDetail) : string.Empty;

        var culture = CultureInfo.CurrentCulture;
        if (entry?.Calendar is { } calendar)
        {
            CalendarPeriodText = Strings.Format("CalendarPeriod", calendar.MinDate.ToString("d", culture),
                calendar.MaxDate.ToString("d", culture));
            MinYear = calendar.MinDate.Year;
            MaxYear = calendar.MaxDate.Year;
        }
        else
        {
            CalendarPeriodText = string.Empty;
            MinYear = MaxYear = DateTime.Today.Year;
        }

        var year = Math.Clamp(double.IsNaN(Year) ? DateTime.Today.Year : Year, MinYear, MaxYear);
        if (year != Year)
        {
            Year = year;
        }
        else
        {
            ShowHolidays();
        }

        OnPropertyChanged(nameof(IsCustom));
        OnPropertyChanged(nameof(IsOriginal));
        OnPropertyChanged(nameof(CanRestoreCalendar));
        OnPropertyChanged(nameof(CanEditHolidays));
        OnPropertyChanged(nameof(CanExportCalendar));
    }

    private void ShowHolidays()
    {
        var selectedDate = SelectedHoliday?.Date;
        var year = (int)Year;
        Holidays.Clear();
        if (SelectedCalendar is not { } entry)
        {
            HolidaysSummary = string.Empty;
            return;
        }

        foreach (var holiday in entry.Holidays.Where(h => h.Date.Year == year))
        {
            Holidays.Add(new HolidayRow(holiday, entry.Kind));
        }

        SelectedHoliday = Holidays.FirstOrDefault(h => h.Date == selectedDate);

        var count = Holidays.Count(h => !h.IsRemoved);
        HolidaysSummary = count switch
        {
            0 => Strings.Format("HolidaysNone", year),
            1 => Strings.Format("HolidaysOne", year),
            _ => Strings.Format("HolidaysMany", count, year)
        };
    }

    private string KindText(CatalogEntry entry)
    {
        var changes = entry.ChangeCount switch
        {
            0 => Strings.Get("ChangesNone"),
            1 => Strings.Get("ChangesOne"),
            _ => Strings.Format("ChangesMany", entry.ChangeCount)
        };

        if (entry.Kind == CalendarKind.Original)
        {
            return Strings.Format("TextWithNote", Strings.Get("KindOriginal"), changes);
        }

        if (entry.Bases.Count > 0)
        {
            var names = entry.Bases.Select(b => _catalog!.Find(b)?.Name ?? b).ToList();
            return Strings.Format("TextWithNote", Strings.Format("KindDerived", CatalogMessages.JoinNames(names)), changes);
        }

        var holidays = entry.ChangeCount switch
        {
            0 => Strings.Get("OwnHolidaysNone"),
            1 => Strings.Get("OwnHolidaysOne"),
            _ => Strings.Format("OwnHolidaysMany", entry.ChangeCount)
        };
        return Strings.Format("TextWithNote", Strings.Get("KindOwn"), holidays);
    }
}
