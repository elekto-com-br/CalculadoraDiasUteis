/*
    Calculadora de Dias �teis - Calcula Prazos e Datas Finais considerando os feriados
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
    ///     Interface para prover o m�nimo necess�rio para a constru��o de Calend�rios
    /// </summary>
    internal interface ICalendarInfoProvider
    {
        /// <summary>
        ///     O calend�rio padr�o do sistema
        /// </summary>
        string DefaultCalendarName { get; }

        /// <summary>
        ///     Retorna a lista de calend�rios cujos feriados s�o conhecidos
        /// </summary>
        IEnumerable<string> EnumCalendarNames();

        /// <summary>
        /// Retorna a informa��o m�nima necess�ria para a constru��o de um calend�rio
        /// </summary>
        CalendarInfo GetCalendarInfo(string calendarName);

        /// <summary>
        /// Enumera todos os calend�rios dispon�veis
        /// </summary>
        IEnumerable<CalendarInfo> All();
    }
}