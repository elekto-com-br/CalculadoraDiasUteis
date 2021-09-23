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
using System;
using System.Collections.Generic;

namespace Elekto.Code
{
    /// <summary>
    /// Interface para a obtenção de calendários
    /// </summary>
    internal interface ICalendarServer
    {
        /// <summary>
        /// Retorna o calendário padrão
        /// </summary>
        ICalendar Default { get; }

        /// <summary>
        /// Enumera o nome dos possíveis calendários
        /// </summary>
        IEnumerable<string> EnumCalendarNames();

        /// <summary>
        /// Retorna o calendário
        /// </summary>
        ICalendar GetCalendar(string name);

        /// <summary>
        /// Retorna o calendário construído com os feriados passados
        /// </summary>
        ICalendar GetCalendar(IEnumerable<DateTime> holidays);
    }
}