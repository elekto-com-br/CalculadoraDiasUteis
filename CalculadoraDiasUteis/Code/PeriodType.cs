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
using System.ComponentModel;

namespace Elekto.Code
{
    /// <summary>
    /// Tipos de períodos
    /// </summary>
    internal enum PeriodType
    {
        /// <summary>
        /// Dias úteis
        /// </summary>
        [Description("Dias Úteis")]
        WorkDays = 0,

        /// <summary>
        /// Dias corridos
        /// </summary>
        [Description("Dias Corridos")]
        ActualDays = 1
    }
}