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

namespace Elekto.Calendars
{
    /// <summary>
    ///     Informação mínima sobre um calendário, necessária para a construção de um.
    /// </summary>
    public class CalendarInfo
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
        ///     A descrição de cada feriado que tiver uma, como "Carnaval" ou "Tiradentes"
        /// </summary>
        public IReadOnlyDictionary<DateTime, string> HolidayDescriptions { get; private set; } =
            new Dictionary<DateTime, string>();

        /// <summary>
        ///     Primeiro dia do período em que o calendário vale. Se nulo, o período vai do primeiro ao último feriado.
        /// </summary>
        public DateTime? MinDate { get; private set; }

        /// <summary>
        ///     Último dia do período em que o calendário vale. Se nulo, o período vai do primeiro ao último feriado.
        /// </summary>
        public DateTime? MaxDate { get; private set; }

        /// <summary>
        /// Cria um novo com opções padrão
        /// </summary>
        /// <param name="name">O código</param>
        /// <param name="holidays">Os feriados</param>
        /// <param name="description">The description.</param>
        /// <param name="nonWorkDays">Os dias não uteis da semana. Se nulo sábado e domingo serão os não-uteis.</param>
        /// <param name="holidayDescriptions">A descrição dos feriados que tiverem uma, opcional.</param>
        /// <param name="minDate">Primeiro dia do período em que o calendário vale, opcional, sempre junto de <paramref name="maxDate"/>.</param>
        /// <param name="maxDate">Último dia do período em que o calendário vale, opcional, sempre junto de <paramref name="minDate"/>.</param>
        /// <returns></returns>
        public static CalendarInfo New(string name, IEnumerable<DateTime> holidays, string description = null,
                                       IEnumerable<DayOfWeek> nonWorkDays = null,
                                       IEnumerable<KeyValuePair<DateTime, string>> holidayDescriptions = null,
                                       DateTime? minDate = null, DateTime? maxDate = null)
        {
            nonWorkDays ??= new[] { DayOfWeek.Saturday, DayOfWeek.Sunday };

            if (minDate.HasValue != maxDate.HasValue)
            {
                throw new ArgumentException(@"O período do calendário precisa do primeiro e do último dia.", nameof(minDate));
            }

            if (minDate > maxDate)
            {
                throw new ArgumentException(@"O primeiro dia do período é posterior ao último.", nameof(minDate));
            }

            var descriptions = new Dictionary<DateTime, string>();
            foreach (var pair in holidayDescriptions.Normalize())
            {
                if (!string.IsNullOrWhiteSpace(pair.Value))
                {
                    descriptions[pair.Key.NormalizeDate()] = pair.Value.Trim();
                }
            }

            return new CalendarInfo
            {
                Holidays = holidays.Normalize().ToArray(),
                NonWorkDays = nonWorkDays.Normalize().ToArray(),
                Name = name,
                Description = description,
                HolidayDescriptions = descriptions,
                MinDate = minDate?.NormalizeDate(),
                MaxDate = maxDate?.NormalizeDate()
            };
        }

        public override string ToString()
        {
            return Description ?? Name ?? string.Empty;
        }
    }
}