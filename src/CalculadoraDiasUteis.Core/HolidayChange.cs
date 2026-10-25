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
namespace CalculadoraDiasUteis.Core;

/// <summary>
///     De onde vem um calendário.
/// </summary>
public enum CalendarKind
{
    /// <summary>
    ///     Enviado pela Elekto dentro do pacote; o usuário só guarda exceções locais por cima dele.
    /// </summary>
    Original = 1,

    /// <summary>
    ///     Criado pelo usuário, vazio ou derivado de outros.
    /// </summary>
    Custom = 2
}

/// <summary>
///     O que o usuário mudou numa data.
/// </summary>
public enum HolidayChangeKind
{
    /// <summary>
    ///     Feriado incluído pelo usuário.
    /// </summary>
    Added = 1,

    /// <summary>
    ///     Feriado herdado que o usuário removeu.
    /// </summary>
    Removed = 2,

    /// <summary>
    ///     Feriado herdado cuja descrição o usuário trocou.
    /// </summary>
    Described = 3
}

/// <summary>
///     Uma alteração do usuário num calendário.
/// </summary>
/// <param name="Date">A data, sem hora nem tipo.</param>
/// <param name="Kind">O que mudou.</param>
/// <param name="Description">A descrição do feriado incluído ou a nova descrição do herdado; nula se não houver.</param>
public sealed record HolidayChange(DateTime Date, HolidayChangeKind Kind, string? Description = null);

/// <summary>
///     Um calendário como fica guardado: só o que o usuário definiu, sem os feriados herdados.
/// </summary>
/// <param name="Code">O código, que não muda depois de criado.</param>
/// <param name="Kind">Original ou do usuário.</param>
/// <param name="Name">O nome dado pelo usuário; nulo nos originais, que usam o do arquivo.</param>
/// <param name="Bases">Os códigos das bases, na ordem; vazio nos originais e nos calendários próprios sem base.</param>
/// <param name="Changes">As alterações, uma por data.</param>
public sealed record StoredCalendar(
    string Code,
    CalendarKind Kind,
    string? Name,
    IReadOnlyList<string> Bases,
    IReadOnlyList<HolidayChange> Changes);
