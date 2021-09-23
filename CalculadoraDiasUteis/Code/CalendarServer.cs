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
    ///     Constrói calendários
    /// </summary>
    /// <remarks>
    /// Mantenha como uma instância pronta para uso, de forma a maximizar o desempenho e evitar o custo de inicialização.
    /// </remarks>
    internal class CalendarServer : ICalendarServer
    {
        /// <summary>
        ///     mapa de lugares para suas redomas
        /// </summary>
        private readonly Dictionary<string, ICalendar> _mapBusinessCalendar =
            new();

        /// <summary>
        ///     Constrói o servidor
        /// </summary>
        /// <param name="defaultProvider">provedor padrão dos feriados</param>
        public CalendarServer(ICalendarInfoProvider defaultProvider)
        {
            Provider = defaultProvider;
        }

        /// <summary>
        ///     Devolve o provedor de feriados deste servidos
        /// </summary>
        /// <value>The provider.</value>
        private ICalendarInfoProvider Provider { get; }

        
        /// <summary>
        ///     enumera os códigos dos locais de feriados possíveis (com o provedor padrão)
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> EnumCalendarNames()
        {
            return Provider.EnumCalendarNames().ToArray();
        }

        /// <summary>
        ///     Retorna o ICalendar do lugar passado
        /// </summary>
        /// <param name="name">código do lugar</param>
        /// <returns>uma ICalendar!</returns>
        public ICalendar GetCalendar(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            lock (_mapBusinessCalendar)
            {
                if (_mapBusinessCalendar.TryGetValue(name, out var calendar))
                {
                    return calendar;
                }

                var ci = Provider.GetCalendarInfo(name);
                if (ci != null)
                {
                    calendar = new Calendar(ci);
                    _mapBusinessCalendar.Add(name, calendar);
                    return calendar;
                }
            }

            throw new ArgumentOutOfRangeException(nameof(name), name, @"Não existe calendário com esse nome disponível.");
        }

        /// <summary>
        ///     Gets the business calendar.
        /// </summary>
        /// <param name="holidays">The holidays.</param>
        /// <returns></returns>
        public ICalendar GetCalendar(IEnumerable<DateTime> holidays)
        {
            return new Calendar(holidays);
        }

        /// <summary>
        ///     Retorna a Redoma padrão
        /// </summary>
        /// <value>The default.</value>
        public ICalendar Default
        {
            get
            {
                var defaultCalendar = Provider.DefaultCalendarName;
                if (defaultCalendar == null)
                {
                    throw new ArgumentException(
                        "A redoma padrão não foi obtida, o usuário possuí permissão em ao menos um calendário?");
                }
                return GetCalendar(defaultCalendar);
            }
        }

      
    }
}