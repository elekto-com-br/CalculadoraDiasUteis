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
    /// O que um calendário financeiro deve implementar
    /// </summary>
    internal interface ICalendar
    {
        /// <summary>
        /// Nome do calendário
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Descrição do calendário
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Maior data que o calendário alcança
        /// </summary>
        DateTime MaxDate { get; }

        /// <summary>
        /// Maior dia util que o calendário alcança
        /// </summary>
        DateTime MaxWorkDate { get; }

        /// <summary>
        /// Menor data que o calendário alcança
        /// </summary>
        DateTime MinDate { get; }

        /// <summary>
        /// Menor data útil que o calendário alcança
        /// </summary>
        DateTime MinWorkDate { get; }

        /// <summary>
        /// Os feriados todos
        /// </summary>
        IEnumerable<DateTime> Holidays { get; }

        /// <summary>
        /// Os dias não uteis da semana (normalmente apenas sábado e domingo)
        /// </summary>
        IEnumerable<DayOfWeek> NonWorkWeekDays { get; }

        /// <summary>
        /// Adiciona dias corridos
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="days">The days.</param>
        /// <param name="finalDateAdjust">The final date adjust.</param>
        /// <returns></returns>
        DateTime AddActualDays(DateTime date, int days, FinalDateAdjust finalDateAdjust = FinalDateAdjust.None);

        /// <summary>
        /// Adiciona dias úteis
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="workDays">The work days.</param>
        /// <param name="finalDateAdjust">The final date adjust.</param>
        /// <returns></returns>
        DateTime AddWorkDays(DateTime date, int workDays, FinalDateAdjust finalDateAdjust = FinalDateAdjust.Following);

        /// <summary>
        /// Adiciona <paramref name="days"/> em <paramref name="date"/> utilizando <paramref name="periodType"/>
        /// </summary>
        DateTime AddDays(DateTime date, int days, PeriodType periodType);

        /// <summary>
        /// Ajusta uma data final (de pagamento)
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="option">The option.</param>
        /// <returns></returns>
        DateTime AdjustFinalDate(DateTime date, FinalDateAdjust option);

        /// <summary>
        /// Retorna o primeiro dia do mês, contato relativamente, a data de referência
        /// </summary>
        /// <param name="referenceDate">The reference date.</param>
        /// <param name="monthsAhead">The months ahead.</param>
        /// <returns></returns>
        DateTime GetActualMonthHead(DateTime referenceDate, int monthsAhead);

        /// <summary>
        /// Retorna o último dia do mês, contato relativamente, a data de referência
        /// </summary>
        /// <param name="referenceDate">The reference date.</param>
        /// <param name="monthsAhead">The months ahead.</param>
        /// <returns></returns>
        DateTime GetActualMonthTail(DateTime referenceDate, int monthsAhead);

        /// <summary>
        /// Numero de dias corridos entre duas datas
        /// </summary>
        /// <param name="ini">The ini.</param>
        /// <param name="end">The end.</param>
        /// <param name="adjust">The adjust.</param>
        /// <returns></returns>
        int GetDeltaActualDays(DateTime ini, DateTime end, DeltaTerminalDayAdjust adjust = DeltaTerminalDayAdjust.Financial);

        /// <summary>
        /// Numero de dias úteis entre duas datas
        /// </summary>
        /// <param name="ini">The ini.</param>
        /// <param name="end">The end.</param>
        /// <param name="adjust">The adjust.</param>
        /// <returns></returns>
        int GetDeltaWorkDays(DateTime ini, DateTime end, DeltaTerminalDayAdjust adjust = DeltaTerminalDayAdjust.Financial);

        /// <summary>
        ///  Numero de dias úteis entre <paramref name="ini"/> e <paramref name="end"/> utilizando <paramref name="periodType"/>
        /// </summary>
        int GetDeltaDays(DateTime ini, DateTime end, PeriodType periodType);

        /// <summary>
        /// Caso a data seja útil retorna a mesma data, caso não seja, retorna o próximo dia útil
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        DateTime GetNextOrSameWorkday(DateTime date);

        /// <summary>
        /// Retorna o dia útil seguinte
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        DateTime GetNextWorkday(DateTime date);

        /// <summary>
        /// Caso a data seja útil retorna a mesma data, caso não seja, retorna o dia útil anterior
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        DateTime GetPrevOrSameWorkday(DateTime date);

        /// <summary>
        /// Retorna o dia útil anterior
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        DateTime GetPrevWorkday(DateTime date);

        /// <summary>
        /// Retorna somente os dias uteis dentro do intervalo
        /// </summary>
        /// <param name="ini">The ini.</param>
        /// <param name="end">The end.</param>
        /// <param name="deltaTerminalDayAdjust">The delta terminal day adjust.</param>
        /// <returns></returns>
        IEnumerable<DateTime> GetWorkDates(DateTime ini, DateTime end, DeltaTerminalDayAdjust deltaTerminalDayAdjust = DeltaTerminalDayAdjust.Financial);

        /// <summary>
        /// Retorna o primeiro dia útil do mês, contato relativamente, a data de referência
        /// </summary>
        /// <param name="referenceDate">The reference date.</param>
        /// <param name="monthsAhead">The months ahead.</param>
        /// <returns></returns>
        DateTime GetWorkingMonthHead(DateTime referenceDate, int monthsAhead);

        /// <summary>
        /// Retorna o último dia útil do mês, contato relativamente, a data de referência
        /// </summary>
        /// <param name="referenceDate">The reference date.</param>
        /// <param name="monthsAhead">The months ahead.</param>
        /// <returns></returns>
        DateTime GetWorkingMonthTail(DateTime referenceDate, int monthsAhead);

        /// <summary>
        /// Retorna o n-ésimo dia util do mes, contado relativamente a data de referencia
        /// </summary>
        /// <param name="referenceDate">The reference date.</param>
        /// <param name="monthsAhead">The months ahead.</param>
        /// <param name="n">The day.</param>
        /// <returns></returns>
        DateTime GetNthWorkingDay(DateTime referenceDate, int monthsAhead, int n);

        /// <summary>
        /// Gets the Nth to last working day.
        /// </summary>
        /// <param name="referenceDate">The reference date.</param>
        /// <param name="monthsAhead">The months ahead.</param>
        /// <param name="n">The n.</param>
        /// <returns></returns>
        DateTime GetNthToLastWorkingDay(DateTime referenceDate, int monthsAhead, int n);

        /// <summary>
        /// Testa se a data é um dia útil
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns>
        /// 	<c>true</c> if the specified date is workday; otherwise, <c>false</c>.
        /// </returns>
        bool IsWorkday(DateTime date);

        /// <summary>
        /// Testa se o dia está marcado como feriado
        /// </summary>
        /// <param name="date">A data</param>
        /// <returns>verdadeiro se a data for um feriado propriamente marcado.</returns>
        /// <remarks>NÃO é a inversa de IsWorkDay</remarks>
        bool IsProperHoliday(DateTime date);

        /// <summary>
        /// Retorna os dias Não-Uteis entre as datas (inclusive as pontas)
        /// </summary>
        IEnumerable<DateTime> GetNonWorkDates(DateTime minDate, DateTime maxDate);

        /// <summary>
        /// Devolve um novo calendário que é a união (em termos de feriados e dias não uteis) entre esta instância e a outra fornecida
        /// </summary>
        /// <param name="other">O outro calendário</param>
        /// <returns>A união entre os calendário</returns>
        ICalendar UnionWith(ICalendar other);
    }
}