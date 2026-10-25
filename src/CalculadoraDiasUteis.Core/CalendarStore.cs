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
using System.Globalization;
using Microsoft.Data.Sqlite;

namespace CalculadoraDiasUteis.Core;

/// <summary>
///     Guarda num arquivo SQLite os calendários do usuário e as exceções dele nos originais.
/// </summary>
/// <remarks>
///     Cada gravação é uma transação: se o computador desligar no meio, o arquivo fica como estava antes dela.
/// </remarks>
public sealed class CalendarStore
{
    private const int SchemaVersion = 1;
    private const string DateFormat = "yyyy-MM-dd";

    private const string Schema = """
        CREATE TABLE calendar (
            code TEXT NOT NULL PRIMARY KEY COLLATE NOCASE,
            kind INTEGER NOT NULL,
            name TEXT
        );
        CREATE TABLE calendar_base (
            code TEXT NOT NULL COLLATE NOCASE REFERENCES calendar (code) ON DELETE CASCADE,
            position INTEGER NOT NULL,
            base TEXT NOT NULL COLLATE NOCASE,
            PRIMARY KEY (code, position)
        );
        CREATE TABLE holiday_change (
            code TEXT NOT NULL COLLATE NOCASE REFERENCES calendar (code) ON DELETE CASCADE,
            date TEXT NOT NULL,
            kind INTEGER NOT NULL,
            description TEXT,
            PRIMARY KEY (code, date)
        );
        """;

    private readonly string _connectionString;

    /// <summary>
    ///     Abre o arquivo, criando-o se ainda não existir.
    /// </summary>
    /// <param name="path">O caminho do arquivo.</param>
    /// <exception cref="CatalogException">O arquivo foi gravado por uma versão mais nova do aplicativo.</exception>
    public CalendarStore(string path)
    {
        // sem pool, a conexão fecha de verdade ao fim de cada operação e o arquivo não fica preso
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = path,
            ForeignKeys = true,
            Pooling = false
        }.ToString();

        Migrate(path);
    }

    /// <summary>
    ///     Lê tudo o que está guardado.
    /// </summary>
    public IReadOnlyList<StoredCalendar> Load()
    {
        using var connection = Open();

        var rows = new List<(string Code, CalendarKind Kind, string? Name)>();
        using (var command = Command(connection, "SELECT code, kind, name FROM calendar ORDER BY code"))
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                rows.Add((reader.GetString(0), (CalendarKind)reader.GetInt32(1),
                    reader.IsDBNull(2) ? null : reader.GetString(2)));
            }
        }

        var bases = rows.ToDictionary(r => r.Code, _ => new List<string>(), StringComparer.OrdinalIgnoreCase);
        using (var command = Command(connection, "SELECT code, base FROM calendar_base ORDER BY code, position"))
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                bases[reader.GetString(0)].Add(reader.GetString(1));
            }
        }

        var changes = rows.ToDictionary(r => r.Code, _ => new List<HolidayChange>(), StringComparer.OrdinalIgnoreCase);
        using (var command = Command(connection, "SELECT code, date, kind, description FROM holiday_change ORDER BY code, date"))
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                var date = DateTime.ParseExact(reader.GetString(1), DateFormat, CultureInfo.InvariantCulture);
                changes[reader.GetString(0)].Add(new HolidayChange(date, (HolidayChangeKind)reader.GetInt32(2),
                    reader.IsDBNull(3) ? null : reader.GetString(3)));
            }
        }

        return rows.Select(r => new StoredCalendar(r.Code, r.Kind, r.Name, bases[r.Code], changes[r.Code])).ToList();
    }

    /// <summary>
    ///     Grava um calendário, no lugar do que tiver o mesmo código.
    /// </summary>
    public void Save(StoredCalendar calendar) => Apply([calendar], []);

    /// <summary>
    ///     Apaga um calendário e as alterações dele.
    /// </summary>
    public void Delete(string code) => Apply([], [code]);

    /// <summary>
    ///     Grava e apaga vários calendários numa transação só.
    /// </summary>
    public void Apply(IEnumerable<StoredCalendar> save, IEnumerable<string> delete)
    {
        using var connection = Open();
        using var transaction = connection.BeginTransaction();

        foreach (var code in delete)
        {
            DeleteRows(connection, transaction, code);
        }

        foreach (var calendar in save)
        {
            DeleteRows(connection, transaction, calendar.Code);

            using (var command = Command(connection, "INSERT INTO calendar (code, kind, name) VALUES ($code, $kind, $name)", transaction))
            {
                command.Parameters.AddWithValue("$code", calendar.Code);
                command.Parameters.AddWithValue("$kind", (int)calendar.Kind);
                command.Parameters.AddWithValue("$name", (object?)calendar.Name ?? DBNull.Value);
                command.ExecuteNonQuery();
            }

            for (var i = 0; i < calendar.Bases.Count; i++)
            {
                using var command = Command(connection,
                    "INSERT INTO calendar_base (code, position, base) VALUES ($code, $position, $base)", transaction);
                command.Parameters.AddWithValue("$code", calendar.Code);
                command.Parameters.AddWithValue("$position", i);
                command.Parameters.AddWithValue("$base", calendar.Bases[i]);
                command.ExecuteNonQuery();
            }

            foreach (var change in calendar.Changes)
            {
                using var command = Command(connection,
                    "INSERT INTO holiday_change (code, date, kind, description) VALUES ($code, $date, $kind, $description)",
                    transaction);
                command.Parameters.AddWithValue("$code", calendar.Code);
                command.Parameters.AddWithValue("$date", change.Date.ToString(DateFormat, CultureInfo.InvariantCulture));
                command.Parameters.AddWithValue("$kind", (int)change.Kind);
                command.Parameters.AddWithValue("$description", (object?)change.Description ?? DBNull.Value);
                command.ExecuteNonQuery();
            }
        }

        transaction.Commit();
    }

    private void Migrate(string path)
    {
        using var connection = Open();
        using var command = Command(connection, "PRAGMA user_version");
        var version = Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
        if (version > SchemaVersion)
        {
            throw new CatalogException(CatalogError.NewerFormat, path);
        }

        if (version == SchemaVersion)
        {
            return;
        }

        using var transaction = connection.BeginTransaction();
        using (var create = Command(connection, Schema, transaction))
        {
            create.ExecuteNonQuery();
        }

        using (var setVersion = Command(connection, $"PRAGMA user_version = {SchemaVersion}", transaction))
        {
            setVersion.ExecuteNonQuery();
        }

        transaction.Commit();
    }

    private SqliteConnection Open()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }

    private static SqliteCommand Command(SqliteConnection connection, string sql, SqliteTransaction? transaction = null)
    {
        var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Transaction = transaction;
        return command;
    }

    private static void DeleteRows(SqliteConnection connection, SqliteTransaction transaction, string code)
    {
        // as alterações e as bases vão junto, pela chave estrangeira
        using var command = Command(connection, "DELETE FROM calendar WHERE code = $code", transaction);
        command.Parameters.AddWithValue("$code", code);
        command.ExecuteNonQuery();
    }
}
