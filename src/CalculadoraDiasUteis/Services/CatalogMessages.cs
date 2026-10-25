using System.Globalization;
using CalculadoraDiasUteis.Core;

namespace CalculadoraDiasUteis.Services;

/// <summary>
///     Os textos para o usuário dos motivos de recusa do catálogo, das observações da importação e de listas de nomes.
/// </summary>
public static class CatalogMessages
{
    public static string Get(CatalogException exception) => Get(exception.Error, exception.Detail);

    public static string Get(CatalogError error, string? detail) =>
        Strings.Format("CatalogError" + error, Detail(detail));

    /// <summary>
    ///     O texto da observação de um calendário a importar, ou vazio.
    /// </summary>
    public static string Get(ImportNote note, string? detail) =>
        note == ImportNote.None ? string.Empty : Strings.Format("ImportNote" + note, Detail(detail));

    /// <summary>
    ///     Junta nomes como numa frase: "A", "A e B", "A, B e C".
    /// </summary>
    public static string JoinNames(IReadOnlyList<string> names) => names.Count switch
    {
        0 => string.Empty,
        1 => names[0],
        _ => Strings.Format("ListLast", string.Join(Strings.Get("ListSeparator"), names.Take(names.Count - 1)), names[^1])
    };

    /// <summary>
    ///     As datas do catálogo vêm no formato yyyy-MM-dd; na mensagem, vão no formato do usuário.
    /// </summary>
    private static string Detail(string? detail)
    {
        if (detail != null && DateTime.TryParseExact(detail, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var date))
        {
            return date.ToString("d", CultureInfo.CurrentCulture);
        }

        return detail ?? string.Empty;
    }
}
