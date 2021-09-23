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

namespace Elekto.Code
{
    /// <summary>
    ///     Informação mínima sobre um calendário, necessária para a construção de um.
    /// </summary>
    internal class CalendarInfo
    {
        private readonly HashSet<DateTime> _holidays = new();
        private readonly HashSet<DayOfWeek> _nonWorkDays = new();

        /// <summary>
        ///     Nome do Calendário
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Descrição do Calendário
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        ///     Feriados do local
        /// </summary>
        public DateTime[] Holidays
        {
            get { return _holidays.OrderBy(d => d).ToArray(); }
            private set
            {
                _holidays.Clear();
                if (value == null)
                {
                    return;
                }
                _holidays.UnionWith(value.Select(d => d.NormalizeDate()));
            }
        }

        /// <summary>
        ///     Dias da semana definidos como não úteis
        /// </summary>
        public DayOfWeek[] NonWorkDays
        {
            get { return _nonWorkDays.OrderBy(w => w).ToArray(); }
            private set
            {
                _nonWorkDays.Clear();
                if (value == null)
                {
                    return;
                }
                _nonWorkDays.UnionWith(value);
            }
        }

        /// <summary>
        /// Cria um novo com opções padrão
        /// </summary>
        /// <param name="name">O código</param>
        /// <param name="holidays">Os feriados</param>
        /// <param name="description">The description.</param>
        /// <param name="nonWorkDays">Os dias não uteis da semana. Se nulo sábado e domingo serão os não-uteis.</param>
        /// <returns></returns>
        public static CalendarInfo New(string name, IEnumerable<DateTime> holidays, string description = null,
                                       IEnumerable<DayOfWeek> nonWorkDays = null)
        {
            nonWorkDays ??= new[] { DayOfWeek.Saturday, DayOfWeek.Sunday };

            return new CalendarInfo
            {
                Holidays = holidays.Normalize().ToArray(),
                NonWorkDays = nonWorkDays.Normalize().ToArray(),
                Name = name,
                Description = description
            };
        }

        public override string ToString()
        {
            return Description ?? Name ?? string.Empty;
        }
    }
}