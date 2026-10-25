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
using Microsoft.Data.Sqlite;

namespace CalculadoraDiasUteis.Core.Tests;

[TestClass]
public sealed class CalendarStoreTests
{
    [TestMethod]
    public void GuardaELeDeVolta()
    {
        using var temp = new TempStore();
        StoredCalendar[] calendars =
        [
            new("forum-da-lapa", CalendarKind.Custom, "Fórum da Lapa", ["br-BC", "us-NYC"],
            [
                new HolidayChange(new DateTime(2026, 1, 25), HolidayChangeKind.Added, "Aniversário de São Paulo"),
                new HolidayChange(TestData.ConscienciaNegra, HolidayChangeKind.Removed),
                new HolidayChange(TestData.Carnaval, HolidayChangeKind.Described, null)
            ]),
            new("br-BC", CalendarKind.Original, null, [], [new HolidayChange(TestData.Tiradentes, HolidayChangeKind.Removed)])
        ];

        temp.Open().Apply(calendars, []);
        var loaded = temp.Open().Load();

        Assert.HasCount(2, loaded);
        var lapa = loaded.Single(c => c.Code == "forum-da-lapa");
        Assert.AreEqual("Fórum da Lapa", lapa.Name);
        Assert.AreEqual(CalendarKind.Custom, lapa.Kind);
        CollectionAssert.AreEqual(new[] { "br-BC", "us-NYC" }, lapa.Bases.ToArray());
        CollectionAssert.AreEqual(calendars[0].Changes.OrderBy(c => c.Date).ToArray(), lapa.Changes.ToArray());
        var original = loaded.Single(c => c.Code == "br-BC");
        Assert.IsNull(original.Name);
        Assert.AreEqual(CalendarKind.Original, original.Kind);
    }

    [TestMethod]
    public void GravarDeNovoSubstituiEApagarLevaAsAlteracoes()
    {
        using var temp = new TempStore();
        var store = temp.Open();
        store.Save(new StoredCalendar("x", CalendarKind.Custom, "X", ["br-BC"],
            [new HolidayChange(TestData.Carnaval, HolidayChangeKind.Removed)]));
        store.Save(new StoredCalendar("x", CalendarKind.Custom, "Xis", [], []));

        var loaded = store.Load().Single();
        Assert.AreEqual("Xis", loaded.Name);
        Assert.IsEmpty(loaded.Bases);
        Assert.IsEmpty(loaded.Changes);

        store.Delete("X");
        Assert.IsEmpty(store.Load());
        using var connection = new SqliteConnection($"Data Source={temp.Path};Pooling=False");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT (SELECT COUNT(*) FROM calendar_base) + (SELECT COUNT(*) FROM holiday_change)";
        Assert.AreEqual(0L, command.ExecuteScalar());
    }

    [TestMethod]
    public void RecusaBancoDeVersaoMaisNova()
    {
        using var temp = new TempStore();
        temp.Open();
        using (var connection = new SqliteConnection($"Data Source={temp.Path};Pooling=False"))
        {
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA user_version = 99";
            command.ExecuteNonQuery();
        }

        var e = Assert.ThrowsExactly<CatalogException>(() => temp.Open());
        Assert.AreEqual(CatalogError.NewerFormat, e.Error);
    }
}
