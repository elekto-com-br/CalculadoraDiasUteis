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

namespace CalculadoraDiasUteis.Core.Tests;

[TestClass]
public sealed class CalendarCatalogTests
{
    private static readonly DateTime AniversarioDeSaoPaulo = new(2026, 1, 25);
    private static readonly DateTime SaoPauloApostolo = new(2026, 6, 29);

    [TestMethod]
    public void OriginalSemAlteracaoEOProprioArquivo()
    {
        using var temp = new TempStore();
        var catalog = TestData.Catalog(temp.Open());

        var entry = catalog.Get("br-bc");
        var file = new Calendar(TestData.BrBc());

        Assert.AreEqual("br-BC", entry.Code);
        Assert.AreEqual("Brasil - Banco Central", entry.Name);
        Assert.AreEqual(CalendarKind.Original, entry.Kind);
        Assert.AreEqual(0, entry.ChangeCount);
        Assert.AreEqual(file.MinDate, entry.Calendar!.MinDate);
        Assert.AreEqual(file.MaxDate, entry.Calendar.MaxDate);
        CollectionAssert.AreEqual(file.Holidays.ToArray(), entry.Calendar.Holidays.ToArray());
        Assert.AreEqual("Tiradentes", entry.HolidayAt(TestData.Tiradentes)!.Description);
        CollectionAssert.AreEqual(new[] { "br-BC", "us-NYC" }, catalog.Calendars.Select(c => c.Code).ToArray());
    }

    [TestMethod]
    public void DerivadoTemOsFeriadosDaBaseMaisOsLocaisEVoltaAoReabrir()
    {
        using var temp = new TempStore();
        var catalog = TestData.Catalog(temp.Open());

        var lapa = catalog.Create("Fórum da Lapa", ["br-BC"]);
        catalog.AddHoliday(lapa.Code, AniversarioDeSaoPaulo.AddDays(1), "Aniversário de São Paulo, no dia útil");
        catalog.RemoveHoliday(lapa.Code, TestData.ConscienciaNegra);

        var reopened = TestData.Catalog(temp.Open()).Get("forum-da-lapa");
        Assert.AreEqual("Fórum da Lapa", reopened.Name);
        CollectionAssert.AreEqual(new[] { "br-BC" }, reopened.Bases.ToArray());
        Assert.IsFalse(reopened.Calendar!.IsWorkday(AniversarioDeSaoPaulo.AddDays(1)));
        Assert.IsFalse(reopened.Calendar.IsWorkday(TestData.Tiradentes), "O feriado da base deveria valer.");
        Assert.IsTrue(reopened.Calendar.IsWorkday(TestData.ConscienciaNegra), "O feriado removido deveria ser útil.");
        Assert.AreEqual(HolidayOrigin.Removed, reopened.HolidayAt(TestData.ConscienciaNegra)!.Origin);
        Assert.AreEqual(HolidayOrigin.Added, reopened.HolidayAt(AniversarioDeSaoPaulo.AddDays(1))!.Origin);
        Assert.AreEqual(2, reopened.ChangeCount);

        var brBc = catalog.Get("br-BC").Calendar!;
        Assert.AreEqual(brBc.MinDate, reopened.Calendar.MinDate, "O derivado vale no período da base.");
        Assert.AreEqual(brBc.MaxDate, reopened.Calendar.MaxDate);
    }

    [TestMethod]
    public void AtualizacaoDoOriginalChegaAoDerivadoEAsAlteracoesContinuam()
    {
        using var temp = new TempStore();
        var catalog = TestData.Catalog(temp.Open());
        var lapa = catalog.Create("Fórum da Lapa", ["br-BC"]);
        catalog.RemoveHoliday(lapa.Code, TestData.ConscienciaNegra);
        catalog.SetDescription("br-BC", TestData.Carnaval, "Carnaval (segunda-feira)");

        // a versão nova do app traz um feriado a mais no original
        var updated = TestData.BrBc((SaoPauloApostolo, "Feriado novo"));
        catalog = TestData.Catalog(temp.Open(), updated, TestData.UsNyc());

        var entry = catalog.Get(lapa.Code);
        Assert.IsFalse(entry.Calendar!.IsWorkday(SaoPauloApostolo), "O feriado novo do original deveria chegar.");
        Assert.IsTrue(entry.Calendar.IsWorkday(TestData.ConscienciaNegra), "A remoção do usuário deveria continuar.");
        Assert.AreEqual("Carnaval (segunda-feira)", entry.HolidayAt(TestData.Carnaval)!.Description,
            "A descrição trocada no original deveria chegar ao derivado.");
        Assert.AreEqual(HolidayOrigin.Inherited, entry.HolidayAt(SaoPauloApostolo)!.Origin);
    }

    [TestMethod]
    public void VariasBasesSomamOsFeriadosNoPeriodoComum()
    {
        using var temp = new TempStore();
        var catalog = TestData.Catalog(temp.Open());

        var cambio = catalog.Create("Câmbio Brasil e Estados Unidos", ["br-BC", "us-NYC"]);

        Assert.IsFalse(cambio.Calendar!.IsWorkday(TestData.Carnaval));
        Assert.IsFalse(cambio.Calendar.IsWorkday(TestData.Thanksgiving));
        Assert.AreEqual(new DateTime(2026, 1, 1), cambio.Calendar.MinDate);
        Assert.AreEqual(new DateTime(2026, 12, 25), cambio.Calendar.MaxDate, "O período é o que as duas bases cobrem.");
        Assert.AreEqual("Confraternização Universal", cambio.HolidayAt(new DateTime(2026, 1, 1))!.Description,
            "A descrição vem da primeira base que tiver uma.");
        Assert.IsNull(cambio.HolidayAt(new DateTime(2027, 1, 1)), "Fora do período comum não há feriado.");
    }

    [TestMethod]
    public void DerivadoDeDerivado()
    {
        using var temp = new TempStore();
        var catalog = TestData.Catalog(temp.Open());
        var saoPaulo = catalog.Create("Cidade de São Paulo", ["br-BC"]);
        catalog.AddHoliday(saoPaulo.Code, AniversarioDeSaoPaulo.AddDays(1), "Aniversário de São Paulo");
        var lapa = catalog.Create("Fórum da Lapa", [saoPaulo.Code]);
        catalog.AddHoliday(lapa.Code, SaoPauloApostolo, "Feriado do fórum");

        var entry = catalog.Get(lapa.Code);
        Assert.IsFalse(entry.Calendar!.IsWorkday(TestData.Tiradentes));
        Assert.IsFalse(entry.Calendar.IsWorkday(AniversarioDeSaoPaulo.AddDays(1)));
        Assert.IsFalse(entry.Calendar.IsWorkday(SaoPauloApostolo));
        Assert.AreEqual(HolidayOrigin.Inherited, entry.HolidayAt(AniversarioDeSaoPaulo.AddDays(1))!.Origin);

        var e = Assert.ThrowsExactly<CatalogException>(() => catalog.SetBases(saoPaulo.Code, [lapa.Code]));
        Assert.AreEqual(CatalogError.BaseCycle, e.Error);
        e = Assert.ThrowsExactly<CatalogException>(() => catalog.Delete(saoPaulo.Code));
        Assert.AreEqual(CatalogError.HasDependents, e.Error);
        Assert.AreEqual("Fórum da Lapa", e.Detail);

        catalog.Delete(lapa.Code);
        catalog.Delete(saoPaulo.Code);
        Assert.HasCount(2, catalog.Calendars);
    }

    [TestMethod]
    public void CalendarioSemBaseValeDoPrimeiroAoUltimoFeriado()
    {
        using var temp = new TempStore();
        var catalog = TestData.Catalog(temp.Open());

        var proprio = catalog.Create("Meu calendário", []);
        catalog.AddHoliday(proprio.Code, new DateTime(2030, 3, 4), null);
        catalog.AddHoliday(proprio.Code, new DateTime(2031, 3, 4), "Outro");

        var entry = catalog.Get(proprio.Code);
        Assert.AreEqual(new DateTime(2030, 3, 4), entry.Calendar!.MinDate);
        Assert.AreEqual(new DateTime(2031, 3, 4), entry.Calendar.MaxDate);
        Assert.IsNull(entry.HolidayAt(new DateTime(2030, 3, 4))!.Description);
    }

    [TestMethod]
    public void DescricaoDoFeriadoHerdado()
    {
        using var temp = new TempStore();
        var catalog = TestData.Catalog(temp.Open());

        catalog.SetDescription("br-BC", TestData.Tiradentes, "Dia de Tiradentes");
        var entry = catalog.Get("br-BC");
        Assert.AreEqual("Dia de Tiradentes", entry.HolidayAt(TestData.Tiradentes)!.Description);
        Assert.IsTrue(entry.HolidayAt(TestData.Tiradentes)!.DescriptionChanged);
        Assert.AreEqual(1, entry.ChangeCount);

        // voltar à descrição original desfaz a alteração
        catalog.SetDescription("br-BC", TestData.Tiradentes, " Tiradentes ");
        Assert.AreEqual(0, catalog.Get("br-BC").ChangeCount);

        catalog.SetDescription("br-BC", TestData.Tiradentes, "");
        Assert.IsNull(catalog.Get("br-BC").HolidayAt(TestData.Tiradentes)!.Description);
        catalog.RestoreHoliday("br-BC", TestData.Tiradentes);
        Assert.AreEqual("Tiradentes", catalog.Get("br-BC").HolidayAt(TestData.Tiradentes)!.Description);
    }

    [TestMethod]
    public void RestaurarOsOriginais()
    {
        using var temp = new TempStore();
        var catalog = TestData.Catalog(temp.Open());
        catalog.RemoveHoliday("br-BC", TestData.Tiradentes);
        catalog.AddHoliday("br-BC", SaoPauloApostolo, "Local");
        catalog.RemoveHoliday("us-NYC", TestData.Thanksgiving);
        Assert.AreEqual(2, catalog.Get("br-BC").ChangeCount);

        catalog.Restore("br-BC");
        Assert.AreEqual(0, catalog.Get("br-BC").ChangeCount);
        Assert.IsFalse(catalog.Get("br-BC").Calendar!.IsWorkday(TestData.Tiradentes));
        Assert.AreEqual(1, catalog.Get("us-NYC").ChangeCount);

        catalog.RestoreAllOriginals();
        Assert.AreEqual(0, TestData.Catalog(temp.Open()).Get("us-NYC").ChangeCount);
        Assert.IsEmpty(temp.Open().Load(), "Sem alteração, nada fica guardado.");
    }

    [TestMethod]
    public void IncluirDeNovoUmFeriadoRemovidoDesfazARemocao()
    {
        using var temp = new TempStore();
        var catalog = TestData.Catalog(temp.Open());
        catalog.RemoveHoliday("br-BC", TestData.Tiradentes);

        catalog.AddHoliday("br-BC", TestData.Tiradentes, null);

        var entry = catalog.Get("br-BC");
        Assert.AreEqual(HolidayOrigin.Inherited, entry.HolidayAt(TestData.Tiradentes)!.Origin);
        Assert.AreEqual(0, entry.ChangeCount);
    }

    [TestMethod]
    public void RemoverFeriadoIncluidoApagaAAlteracao()
    {
        using var temp = new TempStore();
        var catalog = TestData.Catalog(temp.Open());
        catalog.AddHoliday("br-BC", SaoPauloApostolo, "Local");

        catalog.RemoveHoliday("br-BC", SaoPauloApostolo);

        Assert.IsNull(catalog.Get("br-BC").HolidayAt(SaoPauloApostolo));
        Assert.AreEqual(0, catalog.Get("br-BC").ChangeCount);
    }

    [TestMethod]
    [DataRow("", CatalogError.NameEmpty)]
    [DataRow("   ", CatalogError.NameEmpty)]
    [DataRow("Brasil - Banco Central", CatalogError.NameInUse)]
    [DataRow("brasil - banco central", CatalogError.NameInUse)]
    [DataRow("Um nome comprido demais para caber na lista de calendários", CatalogError.NameTooLong)]
    public void NomeInvalido(string name, CatalogError error)
    {
        using var temp = new TempStore();
        var catalog = TestData.Catalog(temp.Open());

        var e = Assert.ThrowsExactly<CatalogException>(() => catalog.Create(name, []));
        Assert.AreEqual(error, e.Error);
    }

    [TestMethod]
    public void OperacoesRecusadas()
    {
        using var temp = new TempStore();
        var catalog = TestData.Catalog(temp.Open());
        var lapa = catalog.Create("Fórum da Lapa", ["br-BC"]);

        Assert.AreEqual(CatalogError.AlreadyHoliday,
            Assert.ThrowsExactly<CatalogException>(() => catalog.AddHoliday(lapa.Code, TestData.Tiradentes, null)).Error);
        Assert.AreEqual(CatalogError.DateOutsideRange,
            Assert.ThrowsExactly<CatalogException>(() => catalog.AddHoliday(lapa.Code, new DateTime(2030, 1, 2), null)).Error);
        Assert.AreEqual(CatalogError.NotHoliday,
            Assert.ThrowsExactly<CatalogException>(() => catalog.RemoveHoliday(lapa.Code, SaoPauloApostolo)).Error);
        Assert.AreEqual(CatalogError.NotCustom,
            Assert.ThrowsExactly<CatalogException>(() => catalog.Delete("br-BC")).Error);
        Assert.AreEqual(CatalogError.NotOriginal,
            Assert.ThrowsExactly<CatalogException>(() => catalog.Restore(lapa.Code)).Error);
        Assert.AreEqual(CatalogError.BaseNotFound,
            Assert.ThrowsExactly<CatalogException>(() => catalog.Create("Outro", ["xx-XX"])).Error);
        Assert.AreEqual(CatalogError.DescriptionTooLong,
            Assert.ThrowsExactly<CatalogException>(() =>
                catalog.SetDescription(lapa.Code, TestData.Tiradentes, new string('x', 101))).Error);
    }

    [TestMethod]
    public void RenomearMantemOCodigo()
    {
        using var temp = new TempStore();
        var catalog = TestData.Catalog(temp.Open());
        var lapa = catalog.Create("Fórum da Lapa", ["br-BC"]);
        var outro = catalog.Create("Fórum da Lapa!", []);

        catalog.Rename(lapa.Code, "Fórum Regional da Lapa");

        Assert.AreEqual("forum-da-lapa", lapa.Code);
        Assert.AreEqual("forum-da-lapa-2", outro.Code, "O código repetido ganha um número.");
        Assert.AreEqual("Fórum Regional da Lapa", catalog.Get("forum-da-lapa").Name);
        CollectionAssert.AreEqual(new[] { "br-BC", "us-NYC", "forum-da-lapa-2", "forum-da-lapa" },
            catalog.Calendars.Select(c => c.Code).ToArray(), "Os do usuário vêm depois dos originais, pelo nome.");
    }

    [TestMethod]
    public void BaseQueSumiuDeixaOCalendarioComErro()
    {
        using var temp = new TempStore();
        temp.Open().Save(new StoredCalendar("orfao", CalendarKind.Custom, "Órfão", ["xx-XX"], []));

        var entry = TestData.Catalog(temp.Open()).Get("orfao");

        Assert.IsFalse(entry.IsUsable);
        Assert.AreEqual(CatalogError.BaseNotFound, entry.Error);
        Assert.AreEqual("xx-XX", entry.ErrorDetail);
    }

    [TestMethod]
    public void AvisaQuandoMuda()
    {
        using var temp = new TempStore();
        var catalog = TestData.Catalog(temp.Open());
        var changes = 0;
        catalog.Changed += (_, _) => changes++;

        catalog.Create("Fórum da Lapa", ["br-BC"]);
        catalog.RemoveHoliday("br-BC", TestData.Tiradentes);

        Assert.AreEqual(2, changes);
    }

    [TestMethod]
    [DataRow("Fórum da Lapa", "forum-da-lapa")]
    [DataRow("  São Paulo / Capital  ", "sao-paulo-capital")]
    [DataRow("!!!", "calendario")]
    [DataRow("Ação Cível nº 1", "acao-civel-n-1")]
    public void CodigoTiradoDoNome(string name, string code)
    {
        Assert.AreEqual(code, CalendarCodes.New(name, _ => false));
    }
}
