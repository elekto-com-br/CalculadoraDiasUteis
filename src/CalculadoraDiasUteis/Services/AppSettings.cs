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
using Windows.Storage;

namespace CalculadoraDiasUteis.Services;

/// <summary>
///     As últimas escolhas do usuário, guardadas nas configurações locais do aplicativo.
/// </summary>
/// <remarks>
///     São as mesmas que a 1.2 guardava; os valores dos tipos enumerados ficam pelo nome, como lá.
/// </remarks>
public sealed class AppSettings
{
    private const string LastCalendarKey = "LastCalendar";
    private const string LastCalculationTypeKey = "LastCalculationType";
    private const string LastPeriodTypeKey = "LastPeriodType";
    private const string LastFinalDateAdjustKey = "LastFinalDateAdjust";
    private const string LastTermKey = "LastTerm";

    private readonly IDictionary<string, object> _values = ApplicationData.Current.LocalSettings.Values;

    /// <summary>
    ///     O nome (código) do último calendário escolhido, como <c>br-BC</c>.
    /// </summary>
    public string? LastCalendar
    {
        get => _values.TryGetValue(LastCalendarKey, out var value) ? value as string : null;
        set => Set(LastCalendarKey, value);
    }

    /// <summary>
    ///     O último critério de contagem de dias entre duas datas, por nome.
    /// </summary>
    public string? LastCalculationType
    {
        get => _values.TryGetValue(LastCalculationTypeKey, out var value) ? value as string : null;
        set => Set(LastCalculationTypeKey, value);
    }

    /// <summary>
    ///     O último tipo de prazo (dias úteis ou corridos), por nome.
    /// </summary>
    public string? LastPeriodType
    {
        get => _values.TryGetValue(LastPeriodTypeKey, out var value) ? value as string : null;
        set => Set(LastPeriodTypeKey, value);
    }

    /// <summary>
    ///     O último ajuste da data final de prazos em dias corridos, por nome.
    /// </summary>
    public string? LastFinalDateAdjust
    {
        get => _values.TryGetValue(LastFinalDateAdjustKey, out var value) ? value as string : null;
        set => Set(LastFinalDateAdjustKey, value);
    }

    /// <summary>
    ///     O último prazo digitado.
    /// </summary>
    public int? LastTerm
    {
        get => _values.TryGetValue(LastTermKey, out var value) && value is int term ? term : null;
        set => Set(LastTermKey, value);
    }

    private void Set(string key, object? value)
    {
        if (value == null)
        {
            _values.Remove(key);
        }
        else
        {
            _values[key] = value;
        }
    }
}
