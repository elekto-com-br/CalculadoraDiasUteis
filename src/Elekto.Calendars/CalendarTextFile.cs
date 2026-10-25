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
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Elekto.Calendars
{
    /// <summary>
    ///     O formato texto dos calendários: a descrição na primeira linha e, nas seguintes, um feriado por linha no
    ///     formato yyyyMMdd, seguido, se houver, de ponto e vírgula, tabulação ou vírgula e da descrição do feriado.
    /// </summary>
    /// <remarks>
    ///     Linhas em branco, comentários (começando com "--" ou '/') e o que não for data são ignorados, como na 1.x.
    /// </remarks>
    public static class CalendarTextFile
    {
        private const string FileNamePrefix = "0000.calendar.";

        private static readonly char[] Separators = { ';', '\t', ',' };

        /// <summary>
        ///     O nome do calendário a partir do nome do arquivo, na convenção '0000.calendar.CÓDIGO.txt'
        /// </summary>
        /// <returns>O código, ou nulo se o nome do arquivo não seguir a convenção</returns>
        public static string NameFromFileName(string filePath)
        {
            var name = Path.GetFileNameWithoutExtension(filePath);
            if (name == null || name.Length <= FileNamePrefix.Length ||
                !name.Substring(4, ".calendar.".Length).Equals(".calendar.", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return name.Substring(FileNamePrefix.Length);
        }

        /// <summary>
        ///     Lê um calendário inteiro
        /// </summary>
        /// <param name="reader">O texto, já decodificado</param>
        /// <param name="name">O nome (código) a dar ao calendário</param>
        public static CalendarInfo Read(TextReader reader, string name)
        {
            var description = reader.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(description))
            {
                throw new FormatException("A primeira linha do calendário deve ter a descrição dele.");
            }

            var holidays = new HashSet<DateTime>();
            var descriptions = new Dictionary<DateTime, string>();
            while (reader.ReadLine() is { } line)
            {
                if (!TryParseHoliday(line, out var date, out var holidayDescription) || !holidays.Add(date))
                {
                    continue;
                }

                if (holidayDescription != null)
                {
                    descriptions[date] = holidayDescription;
                }
            }

            return CalendarInfo.New(name, holidays, description, holidayDescriptions: descriptions);
        }

        /// <summary>
        ///     Lê um calendário de um arquivo em UTF-8
        /// </summary>
        public static CalendarInfo Read(string filePath, string name)
        {
            using var reader = new StreamReader(filePath, Encoding.UTF8, true);
            return Read(reader, name);
        }

        /// <summary>
        ///     Interpreta uma linha de feriado
        /// </summary>
        /// <param name="line">A linha</param>
        /// <param name="date">A data, sem informação de tipo</param>
        /// <param name="description">A descrição, ou nulo se não houver</param>
        /// <returns>Verdadeiro se a linha tem um feriado</returns>
        public static bool TryParseHoliday(string line, out DateTime date, out string description)
        {
            date = default;
            description = null;

            line = line?.Trim();
            if (string.IsNullOrEmpty(line))
            {
                return false;
            }

            // como na 1.x, separadores no começo da linha são pulados e a data é o primeiro campo
            var start = 0;
            while (start < line.Length && Array.IndexOf(Separators, line[start]) >= 0)
            {
                start++;
            }

            var end = line.IndexOfAny(Separators, start);
            var text = (end < 0 ? line.Substring(start) : line.Substring(start, end - start)).Trim();
            if (text.Length == 0 || text[0] == '/' || text[0] == '-' ||
                !DateTime.TryParseExact(text, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            {
                return false;
            }

            date = DateTime.SpecifyKind(date.Date, DateTimeKind.Unspecified);
            if (end >= 0)
            {
                description = line.Substring(end + 1).TrimStart(Separators).Trim();
                if (description.Length == 0)
                {
                    description = null;
                }
            }

            return true;
        }
    }
}
