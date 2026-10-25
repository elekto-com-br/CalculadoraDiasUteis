using System.Globalization;
using CalculadoraDiasUteis.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace CalculadoraDiasUteis;

/// <summary>
///     A data e a descrição de um feriado, para incluir um novo ou trocar a descrição de um existente.
/// </summary>
public sealed partial class HolidayDialog : ContentDialog
{
    /// <param name="title">O título.</param>
    /// <param name="primaryText">O texto do botão que confirma.</param>
    /// <param name="date">A data inicial, ou a do feriado editado.</param>
    /// <param name="range">O período em que a data pode ser escolhida, ou nulo se não há limite.</param>
    /// <param name="description">A descrição inicial.</param>
    /// <param name="isDateFixed">Se a data só aparece, sem poder mudar.</param>
    /// <param name="apply">Faz a operação; devolve a mensagem de erro, que mantém a caixa aberta, ou nulo.</param>
    public HolidayDialog(string title, string primaryText, DateTime date, (DateTime Min, DateTime Max)? range,
        string? description, bool isDateFixed, Func<DateTime, string?, string?> apply)
    {
        InitializeComponent();

        Title = title;
        PrimaryButtonText = primaryText;
        DescriptionTextBox.Text = description ?? string.Empty;

        if (range is { } limits)
        {
            DatePicker.MinDate = new DateTimeOffset(limits.Min);
            DatePicker.MaxDate = new DateTimeOffset(limits.Max);
        }

        DatePicker.Date = new DateTimeOffset(date);
        DatePicker.Visibility = isDateFixed ? Visibility.Collapsed : Visibility.Visible;
        DateText.Text = date.ToString("D", CultureInfo.CurrentCulture);
        DateText.Visibility = isDateFixed ? Visibility.Visible : Visibility.Collapsed;
        ShowNote(date);

        Opened += (_, _) => (isDateFixed ? (Control)DescriptionTextBox : DatePicker).Focus(FocusState.Programmatic);
        PrimaryButtonClick += (_, args) =>
        {
            if (DatePicker.Date is not { } picked)
            {
                args.Cancel = true;
                ShowError(Strings.Get("ErrorNoDate"));
                return;
            }

            var error = apply(isDateFixed ? date : picked.Date, DescriptionTextBox.Text);
            if (error != null)
            {
                args.Cancel = true;
                ShowError(error);
            }
        };
    }

    private void DatePicker_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
    {
        ErrorInfoBar.IsOpen = false;
        if (args.NewDate is { } date)
        {
            ShowNote(date.Date);
        }
    }

    private void ShowNote(DateTime date)
    {
        var isWeekend = date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
        NoteInfoBar.Message = isWeekend ? Strings.Format("WeekendHolidayNote", date.ToString("dddd", CultureInfo.CurrentCulture)) : string.Empty;
        NoteInfoBar.IsOpen = isWeekend;
    }

    private void ShowError(string message)
    {
        ErrorInfoBar.Message = message;
        ErrorInfoBar.IsOpen = true;
    }
}
