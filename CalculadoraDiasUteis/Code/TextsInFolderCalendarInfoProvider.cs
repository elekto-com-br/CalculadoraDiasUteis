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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Elekto.Code
{
    /// <summary>
    ///     Fornece feriados e locais a partir de um diretório contendo arquivos texto, cada um para um local
    /// </summary>
    internal class TextsInFolderCalendarInfoProvider : ICalendarInfoProvider
    {
        private readonly Dictionary<string, Holidays> _cache = new();
        private readonly string _path;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TextsInFolderCalendarInfoProvider" /> class.
        /// </summary>
        /// <param name="path">O caminho, sem a barra final, para o diretório que contem os arquivos chamados '????.calendar.{code}.txt</param>
        public TextsInFolderCalendarInfoProvider(string path)
        {
            if (!Directory.Exists(path))
            {
                throw new ArgumentException($@"Diretório '{path} não existe.", nameof(path));
            }

            _path = path;

            Initialize();
        }

        /// <summary>
        ///     Gets the default business center code.
        /// </summary>
        /// <value>The default business center code.</value>
        public string DefaultCalendarName { get; private set; }

        /// <summary>
        ///     Retorna a lista de lugares cujos feriados são conhecidos
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> EnumCalendarNames()
        {
            return _cache.Keys;
        }

        /// <summary>
        ///     Retorna a informação mínima necessária para a construção de um calendário
        /// </summary>
        /// <param name="name">The business center code.</param>
        /// <returns></returns>
        public CalendarInfo GetCalendarInfo(string name)
        {
            return _cache.TryGetValue(name, out var h) ? h.GetCalendarInfo() : null;
        }

        /// <summary>
        ///     Enumera todos os calendários disponíveis
        /// </summary>
        public IEnumerable<CalendarInfo> All()
        {
            return EnumCalendarNames().Select(GetCalendarInfo);
        }

        private void Initialize()
        {
            _cache.Clear();
            DefaultCalendarName = string.Empty;

            foreach (var filePath in Directory.EnumerateFiles(_path, "????.calendar.*.txt").OrderBy(s => s.ToLowerInvariant()))
            {
                var holidays = new Holidays(filePath);
                if (string.IsNullOrWhiteSpace(DefaultCalendarName))
                {
                    DefaultCalendarName = holidays.Name;
                }

                _cache[holidays.Name] = holidays;
            }
        }


        private class Holidays
        {
            private readonly string _filePath;

            private CalendarInfo _info;

            public Holidays(string filePath)
            {
                if (!File.Exists(filePath))
                {
                    throw new ArgumentException($@"Arquivo '{filePath}' não existe.", nameof(filePath));
                }

                Name = Path.GetFileNameWithoutExtension(filePath);
                Debug.Assert(!string.IsNullOrWhiteSpace(Name));
                if (Name.Length < "0000.calendar.".Length)
                {
                    throw new ArgumentException(
                        $@"Arquivo '{filePath}' deveria ter o nome na convenção '0000.calendar.CÓDIGO.txt'.",
                        nameof(filePath));
                }

                Name = Name.Substring("0000.calendar.".Length);

                Description = File.ReadLines(filePath, Encoding.UTF8).FirstOrDefault();
                if (string.IsNullOrWhiteSpace(Description))
                {
                    throw new ArgumentException(
                        $@"Arquivo '{filePath}' deve ter na primeira linha a descrição do local.",
                        nameof(filePath));
                }

                _filePath = filePath;
            }

            public string Name { get; }

            private string Description { get; }


            private IEnumerable<DateTime> GetHolidays()
            {
                if (!File.Exists(_filePath))
                {
                    throw new InvalidOperationException($"Arquivo '{_filePath}' não existe.");
                }

                var allLines = File.ReadAllLines(_filePath, Encoding.UTF8);

                var l = new HashSet<DateTime>();
                for (var i = 1; i < allLines.Length; i++)
                {
                    var line = allLines[i].Trim();
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    var fields = line.Split(new[] { ';', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (fields.Length <= 0)
                    {
                        continue;
                    }
                    var s = fields[0].Trim();
                    if (string.IsNullOrWhiteSpace(s))
                    {
                        continue;
                    }

                    if (s[0] == ';' || s[0] == '/' || s[0] == '-')
                    {
                        // comentários
                        continue;
                    }

                    if (!DateTime.TryParseExact(line, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
                    {
                        continue;
                    }

                    d = DateTime.SpecifyKind(d.Date, DateTimeKind.Utc);
                    l.Add(d);
                }

                return l;
            }

            public CalendarInfo GetCalendarInfo()
            {
                if (_info != null)
                {
                    return _info;
                }

                _info = CalendarInfo.New(Name, GetHolidays(), Description);
                return _info;
            }
        }
    }
}