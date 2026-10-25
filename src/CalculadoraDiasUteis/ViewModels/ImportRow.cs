using CalculadoraDiasUteis.Core;
using CalculadoraDiasUteis.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CalculadoraDiasUteis.ViewModels;

/// <summary>
///     Um calendário encontrado nos arquivos a importar, para o usuário marcar ou desmarcar.
/// </summary>
public partial class ImportRow : ObservableObject
{
    public ImportRow(ImportItem item)
    {
        Item = item;
        // antes da marcação, que só vale para o que pode ser marcado
        CanSelect = item.Action != ImportAction.Skip;
        IsSelected = item.IsSelected;

        var action = item.Action switch
        {
            ImportAction.Create when item.BaseNames.Count > 0 => Strings.Format(item.ChangeCount switch
            {
                0 => "ImportCreateDerivedNone",
                1 => "ImportCreateDerivedOne",
                _ => "ImportCreateDerivedMany"
            }, CatalogMessages.JoinNames(item.BaseNames), item.ChangeCount),
            ImportAction.Create => Strings.Format(item.HolidayCount == 1 ? "ImportCreateOne" : "ImportCreateMany",
                item.HolidayCount),
            ImportAction.Replace => Strings.Get("ImportReplace"),
            ImportAction.UpdateOriginal => Strings.Get("ImportUpdateOriginal"),
            _ => Strings.Get("ImportSkip")
        };
        var note = CatalogMessages.Get(item.Note, item.NoteDetail);
        Detail = Strings.Format("ImportDetail", note.Length > 0 ? Strings.Format("TextWithNote", action, note) : action,
            item.FileName);
    }

    public ImportItem Item { get; }

    public string Name => Item.Name;

    /// <summary>
    ///     O que será feito, a observação e o arquivo de origem.
    /// </summary>
    public string Detail { get; }

    /// <summary>
    ///     Um calendário sem nada a fazer aparece, mas não pode ser marcado.
    /// </summary>
    public bool CanSelect { get; }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    partial void OnIsSelectedChanged(bool value) => Item.IsSelected = value && CanSelect;
}
