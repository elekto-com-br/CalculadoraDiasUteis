using CalculadoraDiasUteis.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace CalculadoraDiasUteis;

/// <summary>
///     O nome e as bases de um calendário, para criar, renomear ou trocar as bases.
/// </summary>
public sealed partial class CalendarDialog : ContentDialog
{
    private readonly IReadOnlyList<CatalogEntry> _bases;
    private readonly IReadOnlyCollection<string> _selected;

    /// <param name="title">O título.</param>
    /// <param name="primaryText">O texto do botão que confirma.</param>
    /// <param name="showName">Se o nome aparece para editar.</param>
    /// <param name="showBases">Se a escolha das bases aparece.</param>
    /// <param name="name">O nome inicial.</param>
    /// <param name="bases">Os calendários que podem ser base.</param>
    /// <param name="selected">Os códigos das bases já marcadas.</param>
    /// <param name="apply">Faz a operação; devolve a mensagem de erro, que mantém a caixa aberta, ou nulo.</param>
    public CalendarDialog(string title, string primaryText, bool showName, bool showBases, string name,
        IReadOnlyList<CatalogEntry> bases, IReadOnlyCollection<string> selected,
        Func<string, IReadOnlyList<string>, string?> apply)
    {
        InitializeComponent();

        Title = title;
        PrimaryButtonText = primaryText;
        NameTextBox.Text = name;
        NameTextBox.Visibility = showName ? Visibility.Visible : Visibility.Collapsed;
        BasesPanel.Visibility = showBases ? Visibility.Visible : Visibility.Collapsed;
        _bases = bases;
        _selected = selected;
        BasesListView.ItemsSource = bases;

        Opened += CalendarDialog_Opened;
        PrimaryButtonClick += (_, args) =>
        {
            // as bases vão na ordem da lista, que é a do catálogo
            var codes = _bases.Where(b => BasesListView.SelectedItems.Contains(b)).Select(b => b.Code).ToList();
            var error = apply(NameTextBox.Text, codes);
            if (error == null)
            {
                return;
            }

            args.Cancel = true;
            ErrorInfoBar.Message = error;
            ErrorInfoBar.IsOpen = true;
        };
    }

    private void CalendarDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
    {
        foreach (var entry in _bases.Where(b => _selected.Contains(b.Code, StringComparer.OrdinalIgnoreCase)))
        {
            BasesListView.SelectedItems.Add(entry);
        }

        if (NameTextBox.Visibility == Visibility.Visible)
        {
            NameTextBox.SelectAll();
            NameTextBox.Focus(FocusState.Programmatic);
        }
    }
}
