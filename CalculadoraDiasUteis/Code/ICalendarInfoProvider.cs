/*
    Calculadora de Dias Úteis - Calcula Prazos e Datas Finais considerando os feriados
    Copyright (C) 2021 by Elekto Produtos Financeiros Ltda.

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
using System.Collections.Generic;

namespace Elekto.Code
{
    /// <summary>
    ///     Interface para prover o mínimo necessário para a construção de Calendários
    /// </summary>
    internal interface ICalendarInfoProvider
    {
        /// <summary>
        ///     O calendário padrão do sistema
        /// </summary>
        string DefaultCalendarName { get; }

        /// <summary>
        ///     Retorna a lista de calendários cujos feriados são conhecidos
        /// </summary>
        IEnumerable<string> EnumCalendarNames();

        /// <summary>
        /// Retorna a informação mínima necessária para a construção de um calendário
        /// </summary>
        CalendarInfo GetCalendarInfo(string calendarName);

        /// <summary>
        /// Enumera todos os calendários disponíveis
        /// </summary>
        IEnumerable<CalendarInfo> All();
    }
}