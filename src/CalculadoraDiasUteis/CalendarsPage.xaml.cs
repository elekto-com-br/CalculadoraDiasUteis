using System.Runtime.InteropServices;
using CalculadoraDiasUteis.Services;
using CalculadoraDiasUteis.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.Storage.Pickers;
using Windows.UI.Text;

namespace CalculadoraDiasUteis;

/// <summary>
///     A edição dos calendários. As contas ficam no ViewModel; aqui ficam as caixas de diálogo e os seletores de arquivo.
/// </summary>
public sealed partial class CalendarsPage : Page
{
    private const string ExportExtension = ".json";

    public CalendarsPage()
    {
        InitializeComponent();
    }

    public CalendarsPageViewModel ViewModel { get; } = new();

    public static Visibility VisibleIf(bool value) => value ? Visibility.Visible : Visibility.Collapsed;

    public static InfoBarSeverity SeverityOf(bool isError) => isError ? InfoBarSeverity.Error : InfoBarSeverity.Success;

    public static TextDecorations StrikeIf(bool value) => value ? TextDecorations.Strikethrough : TextDecorations.None;

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel.Select(e.Parameter as string);
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        ViewModel.Detach();
    }

    #region Calendários

    private async void NewCalendarButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new CalendarDialog(Strings.Get("NewCalendarTitle"), Strings.Get("CreateButtonText"), true, true,
            string.Empty, ViewModel.PossibleBases(null), [], (name, bases) => ViewModel.CreateCalendar(name, bases));
        await ShowAsync(dialog);
    }

    private async void DeriveCalendarButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedCalendar is not { } entry)
        {
            return;
        }

        var dialog = new CalendarDialog(Strings.Get("NewCalendarTitle"), Strings.Get("CreateButtonText"), true, true,
            string.Empty, ViewModel.PossibleBases(null), [entry.Code], (name, bases) => ViewModel.CreateCalendar(name, bases));
        await ShowAsync(dialog);
    }

    private async void RenameCalendarButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedCalendar is not { } entry)
        {
            return;
        }

        var dialog = new CalendarDialog(Strings.Get("RenameCalendarTitle"), Strings.Get("SaveButtonText"), true, false,
            entry.Name, [], [], (name, _) => ViewModel.RenameCalendar(name));
        await ShowAsync(dialog);
    }

    private async void BasesButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedCalendar is not { } entry)
        {
            return;
        }

        var dialog = new CalendarDialog(Strings.Format("BasesTitle", entry.Name), Strings.Get("SaveButtonText"), false,
            true, entry.Name, ViewModel.PossibleBases(entry.Code), entry.Bases, (_, bases) => ViewModel.SetBases(bases));
        await ShowAsync(dialog);
    }

    private async void DeleteCalendarButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedCalendar is not { } entry)
        {
            return;
        }

        var dependents = ViewModel.Dependents;
        if (dependents.Count > 0)
        {
            ViewModel.ShowStatus(Strings.Format("CatalogErrorHasDependents", CatalogMessages.JoinNames(dependents)), true);
            return;
        }

        if (await ConfirmAsync(Strings.Format("DeleteCalendarTitle", entry.Name), Strings.Get("DeleteCalendarMessage"),
                Strings.Get("DeleteButtonText")))
        {
            ViewModel.DeleteCalendar();
        }
    }

    private async void RestoreCalendarButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedCalendar is not { } entry)
        {
            return;
        }

        var message = entry.ChangeCount == 1
            ? Strings.Get("RestoreCalendarMessageOne")
            : Strings.Format("RestoreCalendarMessageMany", entry.ChangeCount);
        if (await ConfirmAsync(Strings.Format("RestoreCalendarTitle", entry.Name), message, Strings.Get("RestoreButtonText")))
        {
            ViewModel.RestoreCalendar();
        }
    }

    private async void RestoreAllButton_Click(object sender, RoutedEventArgs e)
    {
        var count = ViewModel.OriginalsWithChanges;
        if (count == 0)
        {
            ViewModel.ShowStatus(Strings.Get("StatusNothingToRestore"));
            return;
        }

        var message = count == 1
            ? Strings.Get("RestoreAllMessageOne")
            : Strings.Format("RestoreAllMessageMany", count);
        if (await ConfirmAsync(Strings.Get("RestoreAllTitle"), message, Strings.Get("RestoreButtonText")))
        {
            ViewModel.RestoreAllOriginals();
        }
    }

    #endregion

    #region Feriados

    private async void AddHolidayButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new HolidayDialog(Strings.Get("AddHolidayTitle"), Strings.Get("AddButtonText"),
            ViewModel.SuggestedHolidayDate, ViewModel.HolidayRange, null, false,
            (date, description) => ViewModel.AddHoliday(date, description));
        await ShowAsync(dialog);
    }

    private async void EditHolidayButton_Click(object sender, RoutedEventArgs e)
    {
        await EditHolidayAsync();
    }

    private async void EditHolidayAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        args.Handled = true;
        await EditHolidayAsync();
    }

    private async Task EditHolidayAsync()
    {
        if (!ViewModel.CanEditHoliday || ViewModel.SelectedHoliday is not { } holiday)
        {
            return;
        }

        var dialog = new HolidayDialog(Strings.Get("EditHolidayTitle"), Strings.Get("SaveButtonText"), holiday.Date, null,
            holiday.Description, true, (_, description) => ViewModel.SetDescription(description));
        await ShowAsync(dialog);
        HolidaysListView.Focus(FocusState.Programmatic);
    }

    private void RemoveHolidayButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.RemoveHoliday();
    }

    private void RemoveHolidayAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        args.Handled = true;
        ViewModel.RemoveHoliday();
    }

    private void RestoreHolidayButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.RestoreHoliday();
    }

    private void CopyHolidaysButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.CopyHolidays();
    }

    private async void ExportHolidaysButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (await SheetService.PickPathAsync(ViewModel.HolidaysFileName) is { } path)
            {
                ViewModel.ExportHolidays(path);
            }
        }
        catch (COMException ex)
        {
            ViewModel.ShowStatus(ex.Message, true);
        }
    }

    #endregion

    #region Exportação e importação

    private async void ExportAllButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ViewModel.HasUserData)
        {
            ViewModel.ShowStatus(Strings.Get("StatusNothingToExport"));
            return;
        }

        await ExportAsync(Strings.Get("ExportAllFileName"), true);
    }

    private async void ExportCalendarButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedCalendar is { } entry)
        {
            await ExportAsync(entry.Name, false);
        }
    }

    private async Task ExportAsync(string suggestedName, bool all)
    {
        var picker = new FileSavePicker(App.Window.AppWindow.Id)
        {
            SuggestedFileName = SheetService.SafeFileName(suggestedName),
            DefaultFileExtension = ExportExtension,
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary
        };
        picker.FileTypeChoices.Add(Strings.Get("ExportFileType"), [ExportExtension]);

        try
        {
            var file = await picker.PickSaveFileAsync();
            if (file != null)
            {
                ViewModel.Export(file.Path, all);
            }
        }
        catch (Exception ex) when (ex is COMException or UnauthorizedAccessException)
        {
            // a caixa do Windows pode falhar, por exemplo numa pasta sem permissão
            ViewModel.ShowStatus(ex.Message, true);
        }
    }

    private async void ImportButton_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker(App.Window.AppWindow.Id)
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary
        };
        picker.FileTypeFilter.Add(ExportExtension);
        picker.FileTypeFilter.Add(".txt");

        IReadOnlyList<PickFileResult> files;
        try
        {
            files = await picker.PickMultipleFilesAsync();
        }
        catch (Exception ex) when (ex is COMException or UnauthorizedAccessException)
        {
            ViewModel.ShowStatus(ex.Message, true);
            return;
        }

        if (files.Count == 0)
        {
            return;
        }

        var plan = ViewModel.PlanImport(files.Select(f => f.Path));
        if (plan.Items.Count == 0)
        {
            ViewModel.ShowStatus(ImportDialog.FailuresText(plan.Failures), true);
            return;
        }

        if (await ShowAsync(new ImportDialog(plan)) == ContentDialogResult.Primary)
        {
            ViewModel.ApplyImport(plan);
        }
    }

    #endregion

    // O WinUI só admite uma caixa de diálogo aberta por vez e lança exceção na segunda
    private bool _isDialogOpen;

    private async Task<ContentDialogResult> ShowAsync(ContentDialog dialog)
    {
        if (_isDialogOpen)
        {
            return ContentDialogResult.None;
        }

        _isDialogOpen = true;
        try
        {
            dialog.XamlRoot = XamlRoot;
            return await dialog.ShowAsync();
        }
        finally
        {
            _isDialogOpen = false;
        }
    }

    private async Task<bool> ConfirmAsync(string title, string message, string primaryText)
    {
        var dialog = new ContentDialog
        {
            Style = (Style)Application.Current.Resources["DefaultContentDialogStyle"],
            Title = title,
            Content = new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap },
            PrimaryButtonText = primaryText,
            CloseButtonText = Strings.Get("CancelButtonText"),
            DefaultButton = ContentDialogButton.Close
        };
        return await ShowAsync(dialog) == ContentDialogResult.Primary;
    }
}
