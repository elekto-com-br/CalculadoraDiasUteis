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
using System.Text;
using System.Text.Json;

namespace CalculadoraDiasUteis.Core.Tests;

[TestClass]
public sealed class CalendarExchangeTests
{
    private static readonly DateTime Local = new(2026, 1, 26);
    private static readonly DateTimeOffset Now = new(2026, 10, 25, 14, 30, 0, TimeSpan.FromHours(-3));

    [TestMethod]
    public void ExportadoEImportadoNumaInstalacaoNovaVoltaComAHeranca()
    {
        using var origin = new TempStore();
        var catalog = TestData.Catalog(origin.Open());
        var saoPaulo = catalog.Create("Cidade de São Paulo", ["br-BC"]);
        catalog.AddHoliday(saoPaulo.Code, Local, "Aniversário de São Paulo");
        var lapa = catalog.Create("Fórum da Lapa", [saoPaulo.Code, "us-NYC"]);
        catalog.RemoveHoliday(lapa.Code, TestData.Thanksgiving);
        catalog.SetDescription("br-BC", TestData.Carnaval, "Carnaval (segunda-feira)");

        var json = Export(s => CalendarExchange.Export(catalog, [lapa.Code], s, "2.0.0", Now));

        using var target = new TempStore();
        var fresh = TestData.Catalog(target.Open());
        var plan = CalendarExchange.Plan(fresh, [new ImportSource("calendarios.json", json)]);
        Assert.IsEmpty(plan.Failures);
        CollectionAssert.AreEqual(
            new[] { ImportAction.UpdateOriginal, ImportAction.Create, ImportAction.Create },
            plan.Items.Select(i => i.Action).ToArray(), "Vão o original alterado e a base do usuário, antes do derivado.");

        Assert.AreEqual(3, CalendarExchange.Apply(fresh, plan));

        var imported = fresh.Get("forum-da-lapa");
        CollectionAssert.AreEqual(new[] { "cidade-de-sao-paulo", "us-NYC" }, imported.Bases.ToArray());
        Assert.IsFalse(imported.Calendar!.IsWorkday(Local));
        Assert.IsTrue(imported.Calendar.IsWorkday(TestData.Thanksgiving));
        Assert.AreEqual("Carnaval (segunda-feira)", imported.HolidayAt(TestData.Carnaval)!.Description);
        Assert.AreEqual(1, fresh.Get("br-BC").ChangeCount);
        CollectionAssert.AreEqual(catalog.Get(lapa.Code).Calendar!.Holidays.ToArray(), imported.Calendar.Holidays.ToArray());
    }

    [TestMethod]
    public void ArquivoExportadoTrazAListaCompletaDeFeriados()
    {
        using var temp = new TempStore();
        var catalog = TestData.Catalog(temp.Open());
        var lapa = catalog.Create("Fórum da Lapa", ["br-BC"]);
        catalog.AddHoliday(lapa.Code, Local, "Local");

        using var document = JsonDocument.Parse(Export(s => CalendarExchange.ExportAll(catalog, s, "2.0.0", Now)));

        var root = document.RootElement;
        Assert.AreEqual("calculadora-dias-uteis", root.GetProperty("format").GetString());
        Assert.AreEqual(1, root.GetProperty("version").GetInt32());
        var calendar = root.GetProperty("calendars").EnumerateArray().Single();
        Assert.AreEqual("custom", calendar.GetProperty("kind").GetString());
        Assert.AreEqual("br-BC", calendar.GetProperty("bases")[0].GetString());
        var change = calendar.GetProperty("changes")[0];
        Assert.AreEqual("2026-01-26", change.GetProperty("date").GetString());
        Assert.AreEqual("added", change.GetProperty("change").GetString());
        var holidays = calendar.GetProperty("holidays").EnumerateArray().Select(h => h.GetProperty("date").GetString()).ToList();
        CollectionAssert.Contains(holidays, "2026-01-26");
        CollectionAssert.Contains(holidays, "2026-04-21");
        Assert.AreEqual("2026-01-01", calendar.GetProperty("minDate").GetString());
    }

    [TestMethod]
    public void ImportarDeNovoSubstituiEmVezDeDuplicar()
    {
        using var temp = new TempStore();
        var catalog = TestData.Catalog(temp.Open());
        var lapa = catalog.Create("Fórum da Lapa", ["br-BC"]);
        var json = Export(s => CalendarExchange.ExportAll(catalog, s, "2.0.0", Now));
        catalog.AddHoliday(lapa.Code, Local, "Incluído depois da exportação");

        var plan = CalendarExchange.Plan(catalog, [new ImportSource("calendarios.json", json)]);
        Assert.AreEqual(ImportAction.Replace, plan.Items.Single().Action);
        Assert.AreEqual("Fórum da Lapa", plan.Items.Single().Name);
        CalendarExchange.Apply(catalog, plan);

        Assert.HasCount(3, catalog.Calendars);
        Assert.IsNull(catalog.Get(lapa.Code).HolidayAt(Local), "O arquivo volta o calendário ao que era.");
    }

    [TestMethod]
    public void BaseQueNaoExisteVemSemHeranca()
    {
        using var origin = new TempStore();
        var catalog = TestData.Catalog(origin.Open());
        var lapa = catalog.Create("Fórum da Lapa", ["us-NYC"]);
        catalog.AddHoliday(lapa.Code, Local, "Local");
        var json = Export(s => CalendarExchange.Export(catalog, [lapa.Code], s, "2.0.0", Now));

        // uma instalação onde o us-NYC não existe
        using var target = new TempStore();
        var other = TestData.Catalog(target.Open(), TestData.BrBc());
        var plan = CalendarExchange.Plan(other, [new ImportSource("calendarios.json", json)]);
        var item = plan.Items.Single();
        Assert.AreEqual(ImportNote.Flattened, item.Note);
        Assert.AreEqual("us-NYC", item.NoteDetail);
        CalendarExchange.Apply(other, plan);

        var imported = other.Get(lapa.Code);
        Assert.IsEmpty(imported.Bases);
        Assert.IsFalse(imported.Calendar!.IsWorkday(TestData.Thanksgiving));
        Assert.IsFalse(imported.Calendar.IsWorkday(Local));
    }

    [TestMethod]
    public void BaseDesmarcadaFazODerivadoVirSemHeranca()
    {
        using var origin = new TempStore();
        var catalog = TestData.Catalog(origin.Open());
        var saoPaulo = catalog.Create("Cidade de São Paulo", ["br-BC"]);
        catalog.AddHoliday(saoPaulo.Code, Local, "Local");
        var lapa = catalog.Create("Fórum da Lapa", [saoPaulo.Code]);
        var json = Export(s => CalendarExchange.Export(catalog, [lapa.Code], s, "2.0.0", Now));

        using var target = new TempStore();
        var fresh = TestData.Catalog(target.Open());
        var plan = CalendarExchange.Plan(fresh, [new ImportSource("calendarios.json", json)]);
        plan.Items.Single(i => i.Name == "Cidade de São Paulo").IsSelected = false;
        Assert.AreEqual(1, CalendarExchange.Apply(fresh, plan));

        var imported = fresh.Get(lapa.Code);
        Assert.IsTrue(imported.IsUsable);
        Assert.IsEmpty(imported.Bases);
        Assert.IsFalse(imported.Calendar!.IsWorkday(Local));
    }

    [TestMethod]
    public void ImportaArquivoDa1x()
    {
        using var temp = new TempStore();
        var catalog = TestData.Catalog(temp.Open());
        var text = string.Join("\r\n",
            "\uFEFFFórum da Lapa", "-- feriados locais", "20260125;Aniversário de São Paulo", "20260709", "20261120");

        var plan = CalendarExchange.Plan(catalog, [new ImportSource(@"C:\Calendarios\00A1.calendar.fórum-lapa.txt", text)]);
        var item = plan.Items.Single();
        Assert.AreEqual(ImportAction.Create, item.Action);
        Assert.AreEqual("Fórum da Lapa", item.Name);
        Assert.AreEqual(3, item.HolidayCount);
        Assert.AreEqual("00A1.calendar.fórum-lapa.txt", item.FileName);
        CalendarExchange.Apply(catalog, plan);

        var imported = catalog.Get("forum-lapa");
        Assert.IsEmpty(imported.Bases);
        Assert.AreEqual("Aniversário de São Paulo", imported.HolidayAt(new DateTime(2026, 1, 25))!.Description);
        Assert.AreEqual(new DateTime(2026, 11, 20), imported.Calendar!.MaxDate);
    }

    [TestMethod]
    public void ArquivoDa1xIgualAoOriginalFicaDeFora()
    {
        using var temp = new TempStore();
        var catalog = TestData.Catalog(temp.Open());
        var lines = new List<string> { "Brasil - Banco Central" };
        lines.AddRange(TestData.BrBc().Holidays.Where(d => d.Year == 2026).Select(d => d.ToString("yyyyMMdd")));
        var different = lines.Take(3).Append("20260709").ToList();

        var plan = CalendarExchange.Plan(catalog,
        [
            new ImportSource("0010.calendar.br-BC.txt", string.Join("\n", lines)),
            new ImportSource("copia/0010.calendar.br-BC.txt", string.Join("\n", different))
        ]);

        Assert.AreEqual(ImportAction.Skip, plan.Items[0].Action);
        Assert.AreEqual(ImportNote.SameAsOriginal, plan.Items[0].Note);
        Assert.IsFalse(plan.Items[0].IsSelected);
        Assert.AreEqual(ImportAction.Create, plan.Items[1].Action);
        Assert.AreEqual(ImportNote.CopyOfOriginal, plan.Items[1].Note);
        Assert.AreEqual("Brasil - Banco Central (2)", plan.Items[1].Name);
    }

    [TestMethod]
    public void NomesRepetidosGanhamNumero()
    {
        using var temp = new TempStore();
        var catalog = TestData.Catalog(temp.Open());
        catalog.Create("Fórum", []);

        var plan = CalendarExchange.Plan(catalog,
        [
            new ImportSource("0001.calendar.a.txt", "Fórum\n20260125"),
            new ImportSource("0002.calendar.b.txt", "Fórum\n20260126")
        ]);

        CollectionAssert.AreEqual(new[] { "Fórum (2)", "Fórum (3)" }, plan.Items.Select(i => i.Name).ToArray());
        Assert.IsTrue(plan.Items.All(i => i.Note == ImportNote.Renamed));
        CalendarExchange.Apply(catalog, plan);
        Assert.HasCount(5, catalog.Calendars);
    }

    [TestMethod]
    public void ArquivosInvalidos()
    {
        using var temp = new TempStore();
        var catalog = TestData.Catalog(temp.Open());

        var plan = CalendarExchange.Plan(catalog,
        [
            new ImportSource("vazio.txt", ""),
            new ImportSource("outro.json", """{ "format": "outra-coisa", "version": 1 }"""),
            new ImportSource("futuro.json", """{ "format": "calculadora-dias-uteis", "version": 2, "calendars": [] }"""),
            new ImportSource("quebrado.json", "{ \"format\": ")
        ]);

        Assert.IsEmpty(plan.Items);
        CollectionAssert.AreEqual(
            new[] { CatalogError.UnknownFormat, CatalogError.UnknownFormat, CatalogError.NewerFormat, CatalogError.UnknownFormat },
            plan.Failures.Select(f => f.Error).ToArray());
    }

    private static string Export(Action<Stream> export)
    {
        using var stream = new MemoryStream();
        export(stream);
        return Encoding.UTF8.GetString(stream.ToArray());
    }
}
