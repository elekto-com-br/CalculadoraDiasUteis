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
using System.Text.Json.Serialization;

namespace CalculadoraDiasUteis.Core;

/// <summary>
///     O arquivo de exportação dos calendários, em JSON.
/// </summary>
/// <remarks>
///     Cada calendário leva a herança (bases e alterações), que a importação usa para remontá-lo, e a lista completa
///     dos feriados em vigor, para quem quiser ler o arquivo sem conhecer a herança e para a importação, se uma base
///     faltar. As datas vão no formato yyyy-MM-dd.
/// </remarks>
internal sealed class ExchangeDocument
{
    public const string FormatName = "calculadora-dias-uteis";

    public const int CurrentVersion = 1;

    public string? Format { get; set; }

    public int Version { get; set; }

    public DateTimeOffset? Exported { get; set; }

    public string? Application { get; set; }

    public List<ExchangeCalendar>? Calendars { get; set; }
}

internal sealed class ExchangeCalendar
{
    public string? Code { get; set; }

    public string? Name { get; set; }

    public ExchangeKind Kind { get; set; }

    public List<string>? Bases { get; set; }

    public List<ExchangeChange>? Changes { get; set; }

    public DateOnly? MinDate { get; set; }

    public DateOnly? MaxDate { get; set; }

    public List<ExchangeHoliday>? Holidays { get; set; }
}

internal sealed class ExchangeChange
{
    public DateOnly Date { get; set; }

    public ExchangeChangeKind Change { get; set; }

    public string? Description { get; set; }
}

internal sealed class ExchangeHoliday
{
    public DateOnly Date { get; set; }

    public string? Description { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter<ExchangeKind>))]
internal enum ExchangeKind
{
    [JsonStringEnumMemberName("original")]
    Original = 1,

    [JsonStringEnumMemberName("custom")]
    Custom = 2
}

[JsonConverter(typeof(JsonStringEnumConverter<ExchangeChangeKind>))]
internal enum ExchangeChangeKind
{
    [JsonStringEnumMemberName("added")]
    Added = 1,

    [JsonStringEnumMemberName("removed")]
    Removed = 2,

    [JsonStringEnumMemberName("described")]
    Described = 3
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = true,
    AllowTrailingCommas = true,
    ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip)]
[JsonSerializable(typeof(ExchangeDocument))]
internal sealed partial class ExchangeJsonContext : JsonSerializerContext;
