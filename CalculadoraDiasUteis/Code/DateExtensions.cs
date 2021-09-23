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
using System.Linq;
using System.Runtime.CompilerServices;

namespace Elekto.Code
{
    /// <summary>
    /// Utilitários para lidar com Datas, em especial removendo a informação de localidade usualmente presente.
    /// </summary>
    internal static class DateExtensions
    {
        /// <summary>
        /// Hoje, mas sem informação de localidade (DateTimeKind.Unspecified)
        /// </summary>
        /// <value>The today.</value>
        public static DateTime Today => DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Unspecified);

        /// <summary>
        /// Remove a informação de tipo da data. Passo importante caso as datas tenham de trafegar via SOAP entre máquinas em diferentes TimeZones.
        /// </summary>
        public static DateTime RemoveKind(this DateTime date)
        {
            return DateTime.SpecifyKind(date, DateTimeKind.Unspecified);
        }

        /// <summary>
        /// Retorna somente a parte de data e sem especificação de tipo
        /// </summary>
        public static DateTime NormalizeDate(this DateTime date)
        {
            return DateTime.SpecifyKind(date.Date, DateTimeKind.Unspecified);
        }

        /// <summary>
        /// Retorna uma versão certamente não-nula do original.
        /// </summary>
        /// <remarks>
        /// Use quando os dados vierem de fonte externa (chamada de cliente do WS) e existe a chance de ser enviado um nulo ao invés de coleção vazia.
        /// </remarks>
        public static IEnumerable<T> Normalize<T>(this IEnumerable<T> original)
        {
            return original ?? Enumerable.Empty<T>();
        }

        /// <summary>
        /// Cria uma data a partir de ser componentes, mas sem informação de localidade (DateTimeKind.Unspecified)
        /// </summary>
        /// <param name="year">The year.</param>
        /// <param name="month">The month.</param>
        /// <param name="day">The day.</param>
        /// <param name="toPastDirection">Se true o dia existente anterior (no caso de dadas impossíveis) é retornado</param>
        /// <returns>O dia</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static DateTime CreateDate(int year, int month, int day, bool toPastDirection = false)
        {
            if (day < 1)
            {
                day = 1;
            }

            NormalizeYearAndMonth(ref year, ref month);

            if (day > DateTime.DaysInMonth(year, month))
            {
                // Dia impossível
                var adjustedDate = CreateDate(year, month + 1, 1);
                if (toPastDirection)
                {
                    adjustedDate = adjustedDate.AddDays(-1);
                }
                return adjustedDate;
            }

            // Dia possível
            return new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Unspecified);
        }

        private static void NormalizeYearAndMonth(ref int year, ref int month)
        {
            var serialMonth = GetSerialMonth(year, month);
            GetYearAndMonthFromSerialMonth(serialMonth, out year, out month);
        }

        /// <summary>
        /// Gets the serial month.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        public static int GetSerialMonth(this DateTime date)
        {
            return GetSerialMonth(date.Year, date.Month);
        }

        private static int GetSerialMonth(int year, int month)
        {
            return year * 12 + (month - 1);
        }

        private static void GetYearAndMonthFromSerialMonth(int serialMonth, out int year, out int month)
        {
            year = serialMonth / 12;
            month = serialMonth - year * 12 + 1;
        }

        /// <summary>
        /// Retorna a mesma data, substituindo o dia pelo definido
        /// </summary>
        /// <param name="date">A data a ter o dia substituído</param>
        /// <param name="day">O dia [1; 31]</param>
        /// <param name="toPastDirection">if set to <c>true</c> [to past direction].</param>
        /// <returns>
        /// A data com o dia substituído
        /// </returns>
        public static DateTime ChangeDay(this DateTime date, int day, bool toPastDirection = true)
        {
            return CreateDate(date.Year, date.Month, day, toPastDirection);
        }
    }
}