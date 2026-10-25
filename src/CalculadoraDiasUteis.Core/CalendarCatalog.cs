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
using Elekto.Calendars;
using Calendar = Elekto.Calendars.Calendar;

namespace CalculadoraDiasUteis.Core;

/// <summary>
///     Todos os calendários disponíveis: os originais, com as exceções locais do usuário, e os do usuário.
/// </summary>
/// <remarks>
///     <para>
///         Um original é o arquivo da Elekto mais as alterações do usuário. Um calendário do usuário é a união das
///         bases, se houver, mais as alterações dele. Como só as alterações ficam guardadas, uma versão nova do app com
///         feriados novos nos originais chega também aos derivados, e as alterações continuam valendo por cima.
///     </para>
///     <para>
///         O período de um derivado é o trecho em que todas as bases valem: fora dele, os feriados de alguma base não
///         são conhecidos. Um calendário do usuário sem base vale do primeiro ao último feriado, como na 1.x.
///     </para>
/// </remarks>
public sealed class CalendarCatalog
{
    /// <summary>
    ///     Tamanho máximo do nome de um calendário, o mesmo da descrição na 1.x.
    /// </summary>
    public const int MaxNameLength = 50;

    /// <summary>
    ///     Tamanho máximo da descrição de um feriado.
    /// </summary>
    public const int MaxDescriptionLength = 100;

    private readonly IReadOnlyList<CalendarInfo> _originals;
    private readonly Dictionary<string, CalendarInfo> _originalsByCode;
    private readonly Dictionary<string, StoredCalendar> _stored = new(StringComparer.OrdinalIgnoreCase);
    private readonly CalendarStore _store;

    /// <summary>
    ///     Monta o catálogo.
    /// </summary>
    /// <param name="originals">Os calendários do pacote, na ordem em que aparecem.</param>
    /// <param name="store">Onde ficam as alterações do usuário.</param>
    public CalendarCatalog(IEnumerable<CalendarInfo> originals, CalendarStore store)
    {
        _originals = originals.ToList();
        _originalsByCode = _originals.ToDictionary(o => o.Name, StringComparer.OrdinalIgnoreCase);
        _store = store;

        // exceções num original que deixou de vir no pacote continuam guardadas, mas sem efeito
        foreach (var stored in store.Load())
        {
            _stored[stored.Code] = stored;
        }

        Calendars = Rebuild();
    }

    /// <summary>
    ///     Os calendários: primeiro os originais, na ordem do pacote, depois os do usuário, pelo nome.
    /// </summary>
    public IReadOnlyList<CatalogEntry> Calendars { get; private set; }

    /// <summary>
    ///     Avisa que algum calendário mudou; a lista <see cref="Calendars" /> já é a nova.
    /// </summary>
    public event EventHandler? Changed;

    /// <summary>
    ///     O calendário com o código, ou nulo.
    /// </summary>
    public CatalogEntry? Find(string code) =>
        Calendars.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    ///     O calendário com o código.
    /// </summary>
    /// <exception cref="CatalogException">Não existe.</exception>
    public CatalogEntry Get(string code) =>
        Find(code) ?? throw new CatalogException(CatalogError.CalendarNotFound, code);

    /// <summary>
    ///     Os calendários que têm este como base.
    /// </summary>
    public IReadOnlyList<CatalogEntry> DependentsOf(string code) =>
        Calendars.Where(c => c.Bases.Contains(code, StringComparer.OrdinalIgnoreCase)).ToList();

    /// <summary>
    ///     As alterações guardadas, como estão, para a exportação.
    /// </summary>
    internal IReadOnlyCollection<StoredCalendar> Stored => _stored.Values;

    #region Calendários

    /// <summary>
    ///     Cria um calendário do usuário.
    /// </summary>
    /// <param name="name">O nome.</param>
    /// <param name="bases">Os códigos das bases; sem nenhuma, o calendário começa só com os fins de semana.</param>
    public CatalogEntry Create(string name, IEnumerable<string> bases)
    {
        name = ValidateName(name, null);
        var baseCodes = ValidateBases(null, bases);
        var code = CalendarCodes.New(name, IsCodeInUse);
        Save(new StoredCalendar(code, CalendarKind.Custom, name, baseCodes, []));
        return Get(code);
    }

    /// <summary>
    ///     Troca o nome de um calendário do usuário; o código continua o mesmo.
    /// </summary>
    public void Rename(string code, string name)
    {
        var stored = Custom(code);
        Save(stored with { Name = ValidateName(name, stored.Code) });
    }

    /// <summary>
    ///     Troca as bases de um calendário do usuário. As alterações dele continuam guardadas.
    /// </summary>
    public void SetBases(string code, IEnumerable<string> bases)
    {
        var stored = Custom(code);
        Save(stored with { Bases = ValidateBases(stored.Code, bases) });
    }

    /// <summary>
    ///     Apaga um calendário do usuário, se ele não for base de nenhum outro.
    /// </summary>
    public void Delete(string code)
    {
        var stored = Custom(code);
        var dependents = DependentsOf(stored.Code);
        if (dependents.Count > 0)
        {
            throw new CatalogException(CatalogError.HasDependents, string.Join(", ", dependents.Select(d => d.Name)));
        }

        _store.Delete(stored.Code);
        _stored.Remove(stored.Code);
        Refresh();
    }

    /// <summary>
    ///     Desfaz todas as alterações do usuário num calendário original.
    /// </summary>
    public void Restore(string code)
    {
        var entry = Get(code);
        if (entry.Kind != CalendarKind.Original)
        {
            throw new CatalogException(CatalogError.NotOriginal, entry.Name);
        }

        if (!_stored.ContainsKey(entry.Code))
        {
            return;
        }

        _store.Delete(entry.Code);
        _stored.Remove(entry.Code);
        Refresh();
    }

    /// <summary>
    ///     Desfaz as alterações do usuário em todos os calendários originais.
    /// </summary>
    public void RestoreAllOriginals()
    {
        var codes = _stored.Values.Where(s => s.Kind == CalendarKind.Original).Select(s => s.Code).ToList();
        if (codes.Count == 0)
        {
            return;
        }

        _store.Apply([], codes);
        foreach (var code in codes)
        {
            _stored.Remove(code);
        }

        Refresh();
    }

    #endregion

    #region Feriados

    /// <summary>
    ///     Inclui um feriado. Num feriado herdado que tinha sido removido, desfaz a remoção.
    /// </summary>
    public void AddHoliday(string code, DateTime date, string? description)
    {
        var entry = Usable(code);
        date = date.Date;
        description = ValidateDescription(description);

        var holiday = entry.HolidayAt(date);
        if (holiday is { Origin: not HolidayOrigin.Removed })
        {
            throw new CatalogException(CatalogError.AlreadyHoliday, Format(date));
        }

        var hasFixedRange = entry.Kind == CalendarKind.Original || entry.Bases.Count > 0;
        if (hasFixedRange && (date < entry.Calendar!.MinDate || date > entry.Calendar.MaxDate))
        {
            throw new CatalogException(CatalogError.DateOutsideRange, Format(date));
        }

        var changes = ChangesOf(entry);
        if (holiday != null)
        {
            changes.Remove(date);
            if (description != null && description != holiday.Description)
            {
                changes[date] = new HolidayChange(date, HolidayChangeKind.Described, description);
            }
        }
        else
        {
            changes[date] = new HolidayChange(date, HolidayChangeKind.Added, description);
        }

        SaveChanges(entry, changes);
    }

    /// <summary>
    ///     Troca a descrição de um feriado; nula ou vazia deixa o feriado sem descrição.
    /// </summary>
    public void SetDescription(string code, DateTime date, string? description)
    {
        var entry = Usable(code);
        date = date.Date;
        description = ValidateDescription(description);

        var holiday = entry.HolidayAt(date);
        if (holiday is null or { Origin: HolidayOrigin.Removed })
        {
            throw new CatalogException(CatalogError.NotHoliday, Format(date));
        }

        var changes = ChangesOf(entry);
        if (holiday.Origin == HolidayOrigin.Added)
        {
            changes[date] = new HolidayChange(date, HolidayChangeKind.Added, description);
        }
        else if (description == entry.Inherited[date])
        {
            changes.Remove(date);
        }
        else
        {
            changes[date] = new HolidayChange(date, HolidayChangeKind.Described, description);
        }

        SaveChanges(entry, changes);
    }

    /// <summary>
    ///     Remove um feriado: o incluído pelo usuário some; o herdado fica marcado como removido e pode ser restaurado.
    /// </summary>
    public void RemoveHoliday(string code, DateTime date)
    {
        var entry = Usable(code);
        date = date.Date;

        var holiday = entry.HolidayAt(date);
        if (holiday is null or { Origin: HolidayOrigin.Removed })
        {
            throw new CatalogException(CatalogError.NotHoliday, Format(date));
        }

        var changes = ChangesOf(entry);
        if (entry.Inherited.ContainsKey(date))
        {
            changes[date] = new HolidayChange(date, HolidayChangeKind.Removed);
        }
        else
        {
            changes.Remove(date);
        }

        SaveChanges(entry, changes);
    }

    /// <summary>
    ///     Desfaz a remoção ou a troca de descrição de um feriado herdado.
    /// </summary>
    public void RestoreHoliday(string code, DateTime date)
    {
        var entry = Get(code);
        date = date.Date;

        var changes = ChangesOf(entry);
        if (!changes.TryGetValue(date, out var change) || change.Kind == HolidayChangeKind.Added)
        {
            throw new CatalogException(CatalogError.NoChange, Format(date));
        }

        changes.Remove(date);
        SaveChanges(entry, changes);
    }

    #endregion

    /// <summary>
    ///     Grava de uma vez os calendários importados.
    /// </summary>
    internal void Import(IReadOnlyCollection<StoredCalendar> calendars)
    {
        if (calendars.Count == 0)
        {
            return;
        }

        _store.Apply(calendars, []);
        foreach (var calendar in calendars)
        {
            _stored[calendar.Code] = calendar;
        }

        Refresh();
    }

    /// <summary>
    ///     Se o código já é de algum calendário, original ou do usuário.
    /// </summary>
    internal bool IsCodeInUse(string code) => _originalsByCode.ContainsKey(code) || _stored.ContainsKey(code);

    /// <summary>
    ///     Se o nome já é de algum calendário que não o do código indicado.
    /// </summary>
    internal bool IsNameInUse(string name, string? exceptCode) =>
        Calendars.Any(c => string.Equals(c.Name, name.Trim(), StringComparison.CurrentCultureIgnoreCase) &&
                           !string.Equals(c.Code, exceptCode, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    ///     Se o código é de um original enviado no pacote.
    /// </summary>
    internal bool IsOriginal(string code) => _originalsByCode.ContainsKey(code);

    private static string Format(DateTime date) => date.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);

    private string ValidateName(string name, string? exceptCode)
    {
        name = name.Trim();
        if (name.Length == 0)
        {
            throw new CatalogException(CatalogError.NameEmpty);
        }

        if (name.Length > MaxNameLength)
        {
            throw new CatalogException(CatalogError.NameTooLong, name);
        }

        if (IsNameInUse(name, exceptCode))
        {
            throw new CatalogException(CatalogError.NameInUse, name);
        }

        return name;
    }

    private static string? ValidateDescription(string? description)
    {
        description = description?.Trim();
        if (string.IsNullOrEmpty(description))
        {
            return null;
        }

        if (description.Length > MaxDescriptionLength)
        {
            throw new CatalogException(CatalogError.DescriptionTooLong, description);
        }

        return description;
    }

    private List<string> ValidateBases(string? code, IEnumerable<string> bases)
    {
        var result = new List<string>();
        foreach (var baseCode in bases)
        {
            var entry = Find(baseCode) ?? throw new CatalogException(CatalogError.BaseNotFound, baseCode);
            if (result.Contains(entry.Code, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            if (code != null && DependsOn(entry, code, []))
            {
                throw new CatalogException(CatalogError.BaseCycle, entry.Name);
            }

            result.Add(entry.Code);
        }

        return result;
    }

    private bool DependsOn(CatalogEntry entry, string code, HashSet<string> visited)
    {
        if (entry.Code.Equals(code, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!visited.Add(entry.Code))
        {
            return false;
        }

        return entry.Bases.Select(Find).Any(b => b != null && DependsOn(b, code, visited));
    }

    private StoredCalendar Custom(string code)
    {
        var entry = Get(code);
        if (entry.Kind != CalendarKind.Custom)
        {
            throw new CatalogException(CatalogError.NotCustom, entry.Name);
        }

        return _stored[entry.Code];
    }

    private CatalogEntry Usable(string code)
    {
        var entry = Get(code);
        return entry.IsUsable ? entry : throw new CatalogException(entry.Error!.Value, entry.ErrorDetail);
    }

    private Dictionary<DateTime, HolidayChange> ChangesOf(CatalogEntry entry) =>
        _stored.TryGetValue(entry.Code, out var stored)
            ? stored.Changes.ToDictionary(c => c.Date)
            : new Dictionary<DateTime, HolidayChange>();

    private void SaveChanges(CatalogEntry entry, Dictionary<DateTime, HolidayChange> changes)
    {
        var ordered = changes.Values.OrderBy(c => c.Date).ToList();
        if (entry.Kind == CalendarKind.Custom)
        {
            Save(_stored[entry.Code] with { Changes = ordered });
        }
        else if (ordered.Count > 0)
        {
            Save(new StoredCalendar(entry.Code, CalendarKind.Original, null, [], ordered));
        }
        else
        {
            // sem alteração nenhuma, o original volta a ser só o arquivo
            _store.Delete(entry.Code);
            _stored.Remove(entry.Code);
            Refresh();
        }
    }

    private void Save(StoredCalendar calendar)
    {
        _store.Save(calendar);
        _stored[calendar.Code] = calendar;
        Refresh();
    }

    private void Refresh()
    {
        Calendars = Rebuild();
        Changed?.Invoke(this, EventArgs.Empty);
    }

    #region Montagem

    private List<CatalogEntry> Rebuild()
    {
        var built = new Dictionary<string, CatalogEntry>(StringComparer.OrdinalIgnoreCase);
        var originals = _originals.Select(o => Build(o.Name, built, [])!).ToList();
        var custom = _stored.Values
                            .Where(s => s.Kind == CalendarKind.Custom)
                            .Select(s => Build(s.Code, built, [])!)
                            .OrderBy(e => e.Name, StringComparer.CurrentCultureIgnoreCase)
                            .ThenBy(e => e.Code, StringComparer.OrdinalIgnoreCase);
        return [.. originals, .. custom];
    }

    private CatalogEntry? Build(string code, Dictionary<string, CatalogEntry> built, HashSet<string> visiting)
    {
        if (built.TryGetValue(code, out var done))
        {
            return done;
        }

        CatalogEntry entry;
        if (_originalsByCode.TryGetValue(code, out var info))
        {
            entry = BuildOriginal(info);
        }
        else if (_stored.TryGetValue(code, out var stored) && stored.Kind == CalendarKind.Custom)
        {
            if (!visiting.Add(stored.Code))
            {
                // voltou a um calendário que ainda está sendo montado; quem chamou marca o erro
                return Failed(stored, CatalogError.BaseCycle, stored.Name);
            }

            entry = BuildCustom(stored, built, visiting);
            visiting.Remove(stored.Code);
        }
        else
        {
            return null;
        }

        built[entry.Code] = entry;
        return entry;
    }

    private CatalogEntry BuildOriginal(CalendarInfo info)
    {
        var name = info.Description ?? info.Name;
        var file = new Calendar(info);
        var inherited = info.Holidays.ToDictionary(d => d, d => info.HolidayDescriptions.GetValueOrDefault(d));

        if (!_stored.TryGetValue(info.Name, out var stored) || stored.Changes.Count == 0)
        {
            // sem alteração, é o calendário do arquivo, exatamente como na 1.x
            var holidays = inherited.Select(h => new HolidayEntry(h.Key, h.Value, HolidayOrigin.Inherited, false))
                                    .OrderBy(h => h.Date)
                                    .ToList();
            return new CatalogEntry(info.Name, name, CalendarKind.Original, [], holidays, inherited, 0, file, null,
                null);
        }

        return Compose(info.Name, name, CalendarKind.Original, [], inherited, stored.Changes,
            (file.MinDate, file.MaxDate));
    }

    private CatalogEntry BuildCustom(StoredCalendar stored, Dictionary<string, CatalogEntry> built,
        HashSet<string> visiting)
    {
        var name = stored.Name ?? stored.Code;
        if (stored.Bases.Count == 0)
        {
            return Compose(stored.Code, name, CalendarKind.Custom, stored.Bases, new Dictionary<DateTime, string?>(),
                stored.Changes, null);
        }

        var inherited = new Dictionary<DateTime, string?>();
        var min = DateTime.MinValue;
        var max = DateTime.MaxValue;
        foreach (var baseCode in stored.Bases)
        {
            var baseEntry = Build(baseCode, built, visiting);
            if (baseEntry == null)
            {
                return Failed(stored, CatalogError.BaseNotFound, baseCode);
            }

            if (baseEntry.Calendar == null)
            {
                return Failed(stored,
                    baseEntry.Error == CatalogError.BaseCycle ? CatalogError.BaseCycle : CatalogError.BaseUnusable,
                    baseEntry.Name);
            }

            if (baseEntry.Calendar.MinDate > min)
            {
                min = baseEntry.Calendar.MinDate;
            }

            if (baseEntry.Calendar.MaxDate < max)
            {
                max = baseEntry.Calendar.MaxDate;
            }

            // a descrição é a da primeira base que tiver uma
            foreach (var holiday in baseEntry.Holidays.Where(h => h.Origin != HolidayOrigin.Removed))
            {
                if (inherited.GetValueOrDefault(holiday.Date) == null)
                {
                    inherited[holiday.Date] = holiday.Description;
                }
            }
        }

        if (min > max)
        {
            return Failed(stored, CatalogError.EmptyRange, null);
        }

        var inRange = inherited.Where(h => h.Key >= min && h.Key <= max).ToDictionary(h => h.Key, h => h.Value);
        return Compose(stored.Code, name, CalendarKind.Custom, stored.Bases, inRange, stored.Changes, (min, max));
    }

    private static CatalogEntry Compose(string code, string name, CalendarKind kind, IReadOnlyList<string> bases,
        IReadOnlyDictionary<DateTime, string?> inherited, IEnumerable<HolidayChange> changes,
        (DateTime Min, DateTime Max)? range)
    {
        var effective = new Dictionary<DateTime, string?>(inherited);
        var added = new HashSet<DateTime>();
        var removed = new HashSet<DateTime>();
        var described = new HashSet<DateTime>();

        // alteração que não se aplica mais (fora do período, ou num feriado que a base deixou de ter) fica sem efeito
        foreach (var change in changes)
        {
            var date = change.Date.Date;
            switch (change.Kind)
            {
                case HolidayChangeKind.Added when range == null || (date >= range.Value.Min && date <= range.Value.Max):
                    effective[date] = change.Description;
                    added.Add(date);
                    break;
                case HolidayChangeKind.Removed when inherited.ContainsKey(date):
                    effective.Remove(date);
                    removed.Add(date);
                    break;
                case HolidayChangeKind.Described when inherited.ContainsKey(date):
                    effective[date] = change.Description;
                    described.Add(date);
                    break;
            }
        }

        var holidays = effective.Keys.Concat(removed)
                                .Order()
                                .Select(d => removed.Contains(d)
                                    ? new HolidayEntry(d, inherited[d], HolidayOrigin.Removed, false)
                                    : new HolidayEntry(d, effective[d],
                                        added.Contains(d) ? HolidayOrigin.Added : HolidayOrigin.Inherited,
                                        described.Contains(d)))
                                .ToList();
        var changeCount = added.Count + removed.Count + described.Count;

        try
        {
            var info = CalendarInfo.New(code, effective.Keys, name,
                holidayDescriptions: effective.Where(h => h.Value != null)
                                              .Select(h => new KeyValuePair<DateTime, string>(h.Key, h.Value!)),
                minDate: range?.Min, maxDate: range?.Max);
            return new CatalogEntry(code, name, kind, bases, holidays, inherited, changeCount, new Calendar(info),
                null, null);
        }
        catch (ArgumentException)
        {
            // um período só de dias não úteis não tem conta possível
            return new CatalogEntry(code, name, kind, bases, holidays, inherited, changeCount, null,
                CatalogError.NoWorkDays, name);
        }
    }

    private static CatalogEntry Failed(StoredCalendar stored, CatalogError error, string? detail) =>
        new(stored.Code, stored.Name ?? stored.Code, stored.Kind, stored.Bases, [],
            new Dictionary<DateTime, string?>(), stored.Changes.Count, null, error, detail);

    #endregion
}
