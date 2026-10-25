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
using System.Text.Json;
using Elekto.Calendars;

namespace CalculadoraDiasUteis.Core;

/// <summary>
///     O que a importação vai fazer com um calendário do arquivo.
/// </summary>
public enum ImportAction
{
    /// <summary>Criar um calendário do usuário.</summary>
    Create,

    /// <summary>Substituir o calendário do usuário que tem o mesmo código, de uma exportação anterior.</summary>
    Replace,

    /// <summary>Juntar as alterações às que o usuário já tem num original; no mesmo dia, vale a do arquivo.</summary>
    UpdateOriginal,

    /// <summary>Nada a fazer.</summary>
    Skip
}

/// <summary>
///     Observações sobre um calendário importado, para o usuário decidir.
/// </summary>
public enum ImportNote
{
    /// <summary>Nada a observar.</summary>
    None,

    /// <summary>Já existe calendário com esse nome; o importado ganha um número no fim.</summary>
    Renamed,

    /// <summary>O arquivo da 1.x tem os mesmos feriados do original, no período dele.</summary>
    SameAsOriginal,

    /// <summary>O arquivo da 1.x tem o código de um original, mas feriados diferentes; vira um calendário próprio.</summary>
    CopyOfOriginal,

    /// <summary>Já existe um calendário do usuário igual a este.</summary>
    AlreadyExists,

    /// <summary>Uma base não existe aqui; o calendário vem com os feriados todos, sem a herança.</summary>
    Flattened,

    /// <summary>O original não vem mais no aplicativo; o calendário vem como próprio, com os feriados todos.</summary>
    OriginalMissing,

    /// <summary>O arquivo não traz alteração nenhuma para o original.</summary>
    NoChanges
}

/// <summary>
///     Um calendário que a importação encontrou num arquivo.
/// </summary>
public sealed class ImportItem
{
    internal ImportItem(string fileName, string name, ImportAction action, ImportNote note, string? noteDetail,
        int holidayCount, StoredCalendar? calendar, StoredCalendar? flat)
    {
        FileName = fileName;
        Name = name;
        Action = action;
        Note = note;
        NoteDetail = noteDetail;
        HolidayCount = holidayCount;
        Calendar = calendar;
        Flat = flat;
        IsSelected = action != ImportAction.Skip;
    }

    /// <summary>O nome do arquivo de onde ele veio.</summary>
    public string FileName { get; }

    /// <summary>O nome que o calendário terá.</summary>
    public string Name { get; }

    /// <summary>O que será feito.</summary>
    public ImportAction Action { get; }

    /// <summary>Uma observação, se houver.</summary>
    public ImportNote Note { get; }

    /// <summary>O complemento da observação, como o nome da base que falta.</summary>
    public string? NoteDetail { get; }

    /// <summary>Quantos feriados o calendário tem no arquivo.</summary>
    public int HolidayCount { get; }

    /// <summary>Os nomes das bases, num calendário que vem com a herança; vazio nos outros.</summary>
    public IReadOnlyList<string> BaseNames { get; internal init; } = [];

    /// <summary>Quantas alterações o calendário traz por cima das bases.</summary>
    public int ChangeCount => Calendar?.Changes.Count ?? 0;

    /// <summary>O código que o calendário terá; nulo quando não há nada a fazer.</summary>
    public string? Code => Calendar?.Code;

    /// <summary>Se o usuário quer importá-lo; começa marcado, menos quando não há nada a fazer.</summary>
    public bool IsSelected { get; set; }

    /// <summary>O que será gravado.</summary>
    internal StoredCalendar? Calendar { get; }

    /// <summary>A versão sem herança, com os feriados todos, para quando a base dele ficar de fora da importação.</summary>
    internal StoredCalendar? Flat { get; }
}

/// <summary>
///     Um arquivo que não pôde ser lido.
/// </summary>
/// <param name="FileName">O nome do arquivo.</param>
/// <param name="Error">O motivo.</param>
/// <param name="Detail">O complemento, como a mensagem do leitor de JSON.</param>
public sealed record ImportFailure(string FileName, CatalogError Error, string? Detail);

/// <summary>
///     O que a importação de um ou mais arquivos vai fazer, para o usuário conferir antes.
/// </summary>
public sealed class ImportPlan
{
    internal ImportPlan(IReadOnlyList<ImportItem> items, IReadOnlyList<ImportFailure> failures)
    {
        Items = items;
        Failures = failures;
    }

    /// <summary>Os calendários encontrados.</summary>
    public IReadOnlyList<ImportItem> Items { get; }

    /// <summary>Os arquivos que não puderam ser lidos.</summary>
    public IReadOnlyList<ImportFailure> Failures { get; }
}

/// <summary>
///     Um arquivo a importar.
/// </summary>
/// <param name="FileName">O nome, com ou sem o caminho; nos da 1.x, ele dá o código do calendário.</param>
/// <param name="Content">O conteúdo, já decodificado.</param>
public sealed record ImportSource(string FileName, string Content);

/// <summary>
///     Exporta os calendários num arquivo completo, com a herança, e importa esses arquivos e os da 1.x.
/// </summary>
public static class CalendarExchange
{
    private const string DateFormat = "yyyy-MM-dd";

    #region Exportação

    /// <summary>
    ///     Exporta tudo o que o usuário criou ou alterou.
    /// </summary>
    public static void ExportAll(CalendarCatalog catalog, Stream output, string? application, DateTimeOffset now)
    {
        var codes = catalog.Calendars.Where(c => c.Kind == CalendarKind.Custom || c.ChangeCount > 0)
                           .Select(c => c.Code);
        Export(catalog, codes, output, application, now);
    }

    /// <summary>
    ///     Exporta os calendários indicados, com as bases de que eles dependem.
    /// </summary>
    /// <remarks>
    ///     As bases do usuário vão junto, e os originais vão quando o usuário os alterou: sem eles, o calendário não
    ///     seria remontado igual. Um original sem alteração não vai, porque todo aplicativo já o tem.
    /// </remarks>
    public static void Export(CalendarCatalog catalog, IEnumerable<string> codes, Stream output, string? application,
        DateTimeOffset now)
    {
        var included = new List<CatalogEntry>();
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var code in codes)
        {
            Include(catalog, catalog.Get(code), included, visited);
        }

        var stored = catalog.Stored.ToDictionary(s => s.Code, StringComparer.OrdinalIgnoreCase);
        var document = new ExchangeDocument
        {
            Format = ExchangeDocument.FormatName,
            Version = ExchangeDocument.CurrentVersion,
            Exported = now,
            Application = application,
            Calendars = included.Select(e => ToExchange(e, stored.GetValueOrDefault(e.Code))).ToList()
        };

        JsonSerializer.Serialize(output, document, ExchangeJsonContext.Default.ExchangeDocument);
    }

    private static void Include(CalendarCatalog catalog, CatalogEntry entry, List<CatalogEntry> included,
        HashSet<string> visited)
    {
        if (!visited.Add(entry.Code))
        {
            return;
        }

        // as bases vêm antes, para quem ler o arquivo em ordem
        foreach (var baseEntry in entry.Bases.Select(catalog.Find).OfType<CatalogEntry>())
        {
            if (baseEntry.Kind == CalendarKind.Custom || baseEntry.ChangeCount > 0)
            {
                Include(catalog, baseEntry, included, visited);
            }
        }

        if (entry.Kind == CalendarKind.Custom || entry.ChangeCount > 0)
        {
            included.Add(entry);
        }
    }

    private static ExchangeCalendar ToExchange(CatalogEntry entry, StoredCalendar? stored)
    {
        return new ExchangeCalendar
        {
            Code = entry.Code,
            Name = entry.Name,
            Kind = entry.Kind == CalendarKind.Original ? ExchangeKind.Original : ExchangeKind.Custom,
            Bases = entry.Kind == CalendarKind.Custom ? entry.Bases.ToList() : null,
            Changes = (stored?.Changes ?? []).Select(c => new ExchangeChange
            {
                Date = DateOnly.FromDateTime(c.Date),
                Change = (ExchangeChangeKind)c.Kind,
                Description = c.Description
            }).ToList(),
            MinDate = entry.Calendar == null ? null : DateOnly.FromDateTime(entry.Calendar.MinDate),
            MaxDate = entry.Calendar == null ? null : DateOnly.FromDateTime(entry.Calendar.MaxDate),
            Holidays = entry.Holidays.Where(h => h.Origin != HolidayOrigin.Removed)
                            .Select(h => new ExchangeHoliday { Date = DateOnly.FromDateTime(h.Date), Description = h.Description })
                            .ToList()
        };
    }

    #endregion

    #region Importação

    /// <summary>
    ///     Lê os arquivos e diz o que a importação faria, sem gravar nada.
    /// </summary>
    /// <remarks>
    ///     Aceita os arquivos exportados pela 2.0 (JSON) e os arquivos texto de calendário da 1.x.
    /// </remarks>
    public static ImportPlan Plan(CalendarCatalog catalog, IEnumerable<ImportSource> sources)
    {
        var context = new PlanContext(catalog);
        var failures = new List<ImportFailure>();
        foreach (var source in sources)
        {
            try
            {
                if (source.Content.TrimStart('﻿', ' ', '\t', '\r', '\n').StartsWith('{'))
                {
                    PlanExchange(context, source);
                }
                else
                {
                    PlanText(context, source);
                }
            }
            catch (CatalogException e)
            {
                failures.Add(new ImportFailure(source.FileName, e.Error, e.Detail));
            }
            catch (Exception e) when (e is JsonException or FormatException)
            {
                failures.Add(new ImportFailure(source.FileName, CatalogError.UnknownFormat, e.Message));
            }
        }

        return new ImportPlan(context.Items, failures);
    }

    /// <summary>
    ///     O mesmo plano, com mais arquivos que não puderam ser lidos, como os que o sistema não deixou abrir.
    /// </summary>
    public static ImportPlan WithFailures(ImportPlan plan, IEnumerable<ImportFailure> failures) =>
        new(plan.Items, [.. failures, .. plan.Failures]);

    /// <summary>
    ///     Grava os calendários marcados.
    /// </summary>
    /// <returns>Quantos calendários foram gravados.</returns>
    public static int Apply(CalendarCatalog catalog, ImportPlan plan)
    {
        var selected = plan.Items.Where(i => i.IsSelected && i.Calendar != null).ToList();

        // um derivado cuja base nova, do mesmo arquivo, ficou desmarcada vem sem a herança
        var skippedCodes = plan.Items.Where(i => !i.IsSelected && i.Action == ImportAction.Create && i.Calendar != null)
                               .Select(i => i.Calendar!.Code)
                               .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var calendars = selected.Select(i =>
                                    i.Flat != null && i.Calendar!.Bases.Any(skippedCodes.Contains) ? i.Flat : i.Calendar!)
                                .ToList();

        catalog.Import(calendars);
        return calendars.Count;
    }

    private static void PlanText(PlanContext context, ImportSource source)
    {
        var fileName = Path.GetFileName(source.FileName);
        var fileCode = CalendarTextFile.NameFromFileName(fileName);
        CalendarInfo info;
        using (var reader = new StringReader(source.Content.TrimStart('﻿')))
        {
            info = CalendarTextFile.Read(reader, fileCode ?? Path.GetFileNameWithoutExtension(fileName));
        }

        var holidays = info.Holidays;
        var changes = holidays.Select(d => new HolidayChange(d, HolidayChangeKind.Added,
                                  Truncate(info.HolidayDescriptions.GetValueOrDefault(d), CalendarCatalog.MaxDescriptionLength)))
                              .ToList();
        var name = Truncate(info.Description, CalendarCatalog.MaxNameLength)!;

        var note = ImportNote.None;
        string? noteDetail = null;
        if (fileCode != null && context.Catalog.IsOriginal(fileCode))
        {
            var original = context.Catalog.Get(fileCode);
            if (SameHolidaysInRange(holidays, original))
            {
                context.Add(new ImportItem(fileName, original.Name, ImportAction.Skip, ImportNote.SameAsOriginal,
                    original.Name, holidays.Length, null, null));
                return;
            }

            note = ImportNote.CopyOfOriginal;
            noteDetail = original.Name;
        }

        var existing = context.Catalog.Calendars.FirstOrDefault(c =>
            c.Kind == CalendarKind.Custom && string.Equals(c.Name, name, StringComparison.CurrentCultureIgnoreCase));
        if (existing != null && existing.Bases.Count == 0 &&
            existing.Holidays.Where(h => h.Origin != HolidayOrigin.Removed).Select(h => h.Date).SequenceEqual(holidays))
        {
            context.Add(new ImportItem(fileName, existing.Name, ImportAction.Skip, ImportNote.AlreadyExists,
                existing.Name, holidays.Length, null, null));
            return;
        }

        var uniqueName = context.UniqueName(name, null);
        if (uniqueName != name && note == ImportNote.None)
        {
            note = ImportNote.Renamed;
            noteDetail = name;
        }

        var code = context.NewCode(fileCode ?? name);
        var calendar = new StoredCalendar(code, CalendarKind.Custom, uniqueName, [], changes);
        context.Add(new ImportItem(fileName, uniqueName, ImportAction.Create, note, noteDetail, holidays.Length,
            calendar, null));
    }

    /// <summary>
    ///     Se o arquivo tem exatamente os feriados do original, no trecho em que os dois se sobrepõem.
    /// </summary>
    private static bool SameHolidaysInRange(IReadOnlyList<DateTime> holidays, CatalogEntry original)
    {
        if (original.Calendar == null || holidays.Count == 0)
        {
            return false;
        }

        var min = holidays[0] > original.Calendar.MinDate ? holidays[0] : original.Calendar.MinDate;
        var max = holidays[^1] < original.Calendar.MaxDate ? holidays[^1] : original.Calendar.MaxDate;
        var fromFile = holidays.Where(d => d >= min && d <= max);
        var fromOriginal = original.Holidays.Where(h => h.Origin != HolidayOrigin.Removed && h.Date >= min && h.Date <= max)
                                   .Select(h => h.Date);
        return fromFile.SequenceEqual(fromOriginal);
    }

    private static void PlanExchange(PlanContext context, ImportSource source)
    {
        var fileName = Path.GetFileName(source.FileName);
        var document = JsonSerializer.Deserialize(source.Content.TrimStart('﻿'),
                           ExchangeJsonContext.Default.ExchangeDocument)
                       ?? throw new CatalogException(CatalogError.UnknownFormat, fileName);
        if (document.Format != ExchangeDocument.FormatName || document.Version < 1)
        {
            throw new CatalogException(CatalogError.UnknownFormat, fileName);
        }

        if (document.Version > ExchangeDocument.CurrentVersion)
        {
            throw new CatalogException(CatalogError.NewerFormat, fileName);
        }

        var calendars = (document.Calendars ?? []).Where(c => !string.IsNullOrWhiteSpace(c.Code)).ToList();

        // primeiro decide o código de cada calendário do usuário, porque as bases dos outros apontam para eles
        var codes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var names = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var calendar in calendars.Where(c => c.Kind == ExchangeKind.Custom))
        {
            var existing = context.Catalog.Find(calendar.Code!);
            var code = existing is { Kind: CalendarKind.Custom } ? existing.Code : context.NewCode(calendar.Code!);
            codes[calendar.Code!] = code;
            names[code] = Truncate(calendar.Name, CalendarCatalog.MaxNameLength) ?? calendar.Code!;
        }

        foreach (var calendar in calendars)
        {
            var changes = ToChanges(calendar.Changes);
            var flatChanges = (calendar.Holidays ?? [])
                              .GroupBy(h => h.Date)
                              .Select(g => new HolidayChange(g.Key.ToDateTime(TimeOnly.MinValue), HolidayChangeKind.Added,
                                  Truncate(g.First().Description, CalendarCatalog.MaxDescriptionLength)))
                              .OrderBy(c => c.Date)
                              .ToList();
            var name = Truncate(calendar.Name, CalendarCatalog.MaxNameLength) ?? calendar.Code!;

            if (calendar.Kind == ExchangeKind.Original)
            {
                PlanOriginal(context, fileName, calendar, name, changes, flatChanges);
                continue;
            }

            var code = codes[calendar.Code!];
            var replacing = context.Catalog.Find(code) is { Kind: CalendarKind.Custom };
            var uniqueName = context.UniqueName(name, replacing ? code : null);
            var note = uniqueName != name ? ImportNote.Renamed : ImportNote.None;
            var noteDetail = uniqueName != name ? name : null;

            // a base pode vir no próprio arquivo ou já existir aqui; se não, o calendário vem sem a herança
            var bases = new List<string>();
            string? missing = null;
            foreach (var baseCode in calendar.Bases ?? [])
            {
                if (codes.TryGetValue(baseCode, out var mapped))
                {
                    bases.Add(mapped);
                }
                else if (context.Catalog.Find(baseCode) is { } found)
                {
                    bases.Add(found.Code);
                }
                else
                {
                    missing ??= baseCode;
                }
            }

            var flat = new StoredCalendar(code, CalendarKind.Custom, uniqueName, [], flatChanges);
            StoredCalendar stored;
            if (missing != null)
            {
                stored = flat;
                note = ImportNote.Flattened;
                noteDetail = missing;
            }
            else
            {
                stored = new StoredCalendar(code, CalendarKind.Custom, uniqueName, bases, changes);
            }

            context.Add(new ImportItem(fileName, uniqueName, replacing ? ImportAction.Replace : ImportAction.Create,
                note, noteDetail, flatChanges.Count, stored, bases.Count > 0 ? flat : null)
            {
                BaseNames = stored.Bases.Select(b => context.Catalog.Find(b)?.Name ?? names.GetValueOrDefault(b, b)).ToList()
            });
        }
    }

    private static void PlanOriginal(PlanContext context, string fileName, ExchangeCalendar calendar, string name,
        List<HolidayChange> changes, List<HolidayChange> flatChanges)
    {
        if (!context.Catalog.IsOriginal(calendar.Code!))
        {
            var code = context.NewCode(calendar.Code!);
            var uniqueName = context.UniqueName(name, null);
            var flat = new StoredCalendar(code, CalendarKind.Custom, uniqueName, [], flatChanges);
            context.Add(new ImportItem(fileName, uniqueName, ImportAction.Create, ImportNote.OriginalMissing,
                calendar.Code, flatChanges.Count, flat, null));
            return;
        }

        var original = context.Catalog.Get(calendar.Code!);
        if (changes.Count == 0)
        {
            context.Add(new ImportItem(fileName, original.Name, ImportAction.Skip, ImportNote.NoChanges, null,
                flatChanges.Count, null, null));
            return;
        }

        // no mesmo dia, vale a alteração do arquivo
        var merged = (context.Catalog.Stored.FirstOrDefault(s => s.Code.Equals(original.Code, StringComparison.OrdinalIgnoreCase))
                             ?.Changes ?? [])
                     .ToDictionary(c => c.Date);
        foreach (var change in changes)
        {
            merged[change.Date] = change;
        }

        var stored = new StoredCalendar(original.Code, CalendarKind.Original, null, [],
            merged.Values.OrderBy(c => c.Date).ToList());
        context.Add(new ImportItem(fileName, original.Name, ImportAction.UpdateOriginal, ImportNote.None, null,
            flatChanges.Count, stored, null));
    }

    private static List<HolidayChange> ToChanges(List<ExchangeChange>? changes)
    {
        var result = new Dictionary<DateTime, HolidayChange>();
        foreach (var change in changes ?? [])
        {
            if (!Enum.IsDefined(change.Change))
            {
                throw new CatalogException(CatalogError.UnknownFormat, change.Change.ToString());
            }

            var date = change.Date.ToDateTime(TimeOnly.MinValue);
            result[date] = new HolidayChange(date, (HolidayChangeKind)change.Change,
                Truncate(change.Description, CalendarCatalog.MaxDescriptionLength));
        }

        return result.Values.OrderBy(c => c.Date).ToList();
    }

    private static string? Truncate(string? text, int length)
    {
        text = text?.Trim();
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        return text.Length <= length ? text : text[..length].TrimEnd();
    }

    /// <summary>
    ///     Os nomes e códigos já reservados pelos itens do plano, além dos do catálogo.
    /// </summary>
    private sealed class PlanContext(CalendarCatalog catalog)
    {
        private readonly HashSet<string> _codes = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<string> _names = [];

        public CalendarCatalog Catalog { get; } = catalog;

        public List<ImportItem> Items { get; } = [];

        public void Add(ImportItem item)
        {
            Items.Add(item);
            if (item.Calendar is { Kind: CalendarKind.Custom } calendar)
            {
                _codes.Add(calendar.Code);
                _names.Add(calendar.Name!);
            }
        }

        public string NewCode(string name)
        {
            var code = CalendarCodes.New(name, c => Catalog.IsCodeInUse(c) || _codes.Contains(c));
            _codes.Add(code);
            return code;
        }

        public string UniqueName(string name, string? replacingCode)
        {
            if (!InUse(name, replacingCode))
            {
                return name;
            }

            for (var i = 2;; i++)
            {
                var suffix = $" ({i})";
                var candidate = (name.Length + suffix.Length > CalendarCatalog.MaxNameLength
                    ? name[..(CalendarCatalog.MaxNameLength - suffix.Length)].TrimEnd()
                    : name) + suffix;
                if (!InUse(candidate, replacingCode))
                {
                    return candidate;
                }
            }
        }

        private bool InUse(string name, string? replacingCode) =>
            Catalog.IsNameInUse(name, replacingCode) ||
            _names.Contains(name, StringComparer.CurrentCultureIgnoreCase);
    }

    #endregion
}
