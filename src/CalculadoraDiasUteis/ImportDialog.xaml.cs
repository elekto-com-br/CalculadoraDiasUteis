using System.ComponentModel;
using CalculadoraDiasUteis.Core;
using CalculadoraDiasUteis.Services;
using CalculadoraDiasUteis.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace CalculadoraDiasUteis;

/// <summary>
///     O que a importação vai fazer com cada calendário, para o usuário conferir e desmarcar o que não quiser.
/// </summary>
public sealed partial class ImportDialog : ContentDialog
{
    public ImportDialog(ImportPlan plan)
    {
        Rows = plan.Items.Select(i => new ImportRow(i)).ToList();
        InitializeComponent();

        FailuresInfoBar.Message = FailuresText(plan.Failures);
        FailuresInfoBar.IsOpen = plan.Failures.Count > 0;

        foreach (var row in Rows)
        {
            row.PropertyChanged += Row_PropertyChanged;
        }

        UpdatePrimaryButton();
    }

    public IReadOnlyList<ImportRow> Rows { get; }

    /// <summary>
    ///     Os arquivos que não puderam ser lidos, um por linha, com o motivo.
    /// </summary>
    public static string FailuresText(IReadOnlyList<ImportFailure> failures) =>
        string.Join(Environment.NewLine, failures.Select(f =>
            Strings.Format("ImportFailure", f.FileName, CatalogMessages.Get(f.Error, f.Detail))));

    private void Row_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ImportRow.IsSelected))
        {
            UpdatePrimaryButton();
        }
    }

    private void UpdatePrimaryButton()
    {
        IsPrimaryButtonEnabled = Rows.Any(r => r.IsSelected && r.CanSelect);
    }
}
