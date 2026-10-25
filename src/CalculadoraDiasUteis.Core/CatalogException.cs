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
///     Os motivos pelos quais uma operação nos calendários pode ser recusada, ou um calendário não pode ser montado.
/// </summary>
/// <remarks>
///     O texto mostrado ao usuário fica nos recursos do aplicativo, pelo nome do motivo, para poder ser traduzido.
/// </remarks>
public enum CatalogError
{
    /// <summary>O nome está vazio.</summary>
    NameEmpty,

    /// <summary>O nome passa do tamanho máximo.</summary>
    NameTooLong,

    /// <summary>Outro calendário já tem esse nome.</summary>
    NameInUse,

    /// <summary>Não existe calendário com esse código.</summary>
    CalendarNotFound,

    /// <summary>A operação só vale para calendários do usuário.</summary>
    NotCustom,

    /// <summary>A operação só vale para os calendários originais.</summary>
    NotOriginal,

    /// <summary>A base indicada não existe.</summary>
    BaseNotFound,

    /// <summary>A base depende do próprio calendário, o que daria uma volta.</summary>
    BaseCycle,

    /// <summary>Uma das bases não pôde ser montada.</summary>
    BaseUnusable,

    /// <summary>O calendário é base de outros e não pode ser apagado.</summary>
    HasDependents,

    /// <summary>A data está fora do período do calendário.</summary>
    DateOutsideRange,

    /// <summary>A data já é feriado no calendário.</summary>
    AlreadyHoliday,

    /// <summary>A data não é feriado no calendário.</summary>
    NotHoliday,

    /// <summary>Não há alteração do usuário a desfazer nessa data.</summary>
    NoChange,

    /// <summary>A descrição passa do tamanho máximo.</summary>
    DescriptionTooLong,

    /// <summary>Os períodos das bases não se sobrepõem.</summary>
    EmptyRange,

    /// <summary>O calendário não tem nenhum dia útil no período.</summary>
    NoWorkDays,

    /// <summary>O arquivo não está num formato conhecido.</summary>
    UnknownFormat,

    /// <summary>O arquivo foi gravado por uma versão mais nova do aplicativo.</summary>
    NewerFormat
}

/// <summary>
///     Uma operação recusada nos calendários.
/// </summary>
public sealed class CatalogException(CatalogError error, string? detail = null)
    : Exception(detail == null ? error.ToString() : $"{error}: {detail}")
{
    /// <summary>
    ///     O motivo.
    /// </summary>
    public CatalogError Error { get; } = error;

    /// <summary>
    ///     O complemento da mensagem, como o nome do calendário envolvido; nulo se não houver.
    /// </summary>
    public string? Detail { get; } = detail;
}
