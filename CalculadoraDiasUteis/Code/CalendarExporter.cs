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
using System.Globalization;
using System.IO;
using System.Text;
using OfficeOpenXml;

namespace Elekto.Code
{
    /// <summary>
    /// Classe para exportar calendários
    /// </summary>
    internal static class CalendarExporter
    {
        public enum SaveFormat
        {
            Xlsx,
            Txt,
            Csv
        }

        internal static void Export(ICalendar calendar, SaveFormat saveFormat, string file)
        {
            switch (saveFormat)
            {
                case SaveFormat.Csv:
                    ExportCsv(calendar, file);
                    break;
                case SaveFormat.Txt:
                    ExportTxt(calendar, file);
                    break;
                case SaveFormat.Xlsx:
                    ExportExcel(calendar, file);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(saveFormat), saveFormat, @"Formato de exportação não suportado.");
            }
        }

        private static void ExportExcel(ICalendar calendar, string file)
        {
            File.Delete(file);

            var fi = new FileInfo(file);
            using var package = new ExcelPackage(fi);
            var sheet = package.Workbook.Worksheets.Add("feriados");

            sheet.SetValue(1, 1, calendar.Description);
            sheet.SetValue(2, 1, calendar.Name);
            sheet.SetValue(4, 1, "Feriados");

            var row = 5;
            foreach (var h in calendar.Holidays)
            {
                sheet.SetValue(row, 1, h);
                sheet.Cells[row, 1].Style.Numberformat.Format = "dd/mm/yyyy";
                ++row;
            }

            package.Save();
        }

        private static void ExportTxt(ICalendar calendar, string file)
        {
            var sb = new StringBuilder();

            sb.AppendLine(calendar.Description ?? "Calendário sem descrição");
            sb.AppendLine($"-- {calendar.Name ?? @"sem_nome"}");
            sb.AppendLine($"-- exportado às {DateTime.UtcNow:O} por {Environment.UserName} da máquina {Environment.MachineName}");
            
            sb.AppendLine();
            foreach (var h in calendar.Holidays)
            {
                sb.AppendLine($"{h:yyyyMMdd}");
            }
            sb.AppendLine();
            sb.AppendLine("-- Fim da Lista Exportada");

            File.WriteAllText(file, sb.ToString(), Encoding.UTF8);
        }

        private static void ExportCsv(ICalendar calendar, string file)
        {
            var sb = new StringBuilder();

            sb.AppendLine(calendar.Description ?? "Calendário sem descrição");
            sb.AppendLine(calendar.Name ?? "sem_nome");
            sb.AppendLine("feriados");

            foreach (var h in calendar.Holidays)
            {
                sb.AppendLine($"{h:d}");
            }
            
            File.WriteAllText(file, sb.ToString(), Encoding.UTF8);
        }

        public static void Export(ICalendar calendar, SaveFormat saveFormat, string file, DateTime ini, DateTime end)
        {
            if (ini > end)
            {
                // invertidos
                (ini, end) = (end, ini);
            }

            var dates = new List<(DateTime date, bool isWorkDay)>();
            for (var d = ini; d <= end; d = d.AddDays(1))
            {
                dates.Add((d, calendar.IsWorkday(d)));
            }

            switch(saveFormat)
            {
                case SaveFormat.Csv:
                ExportCsv(calendar, file, dates);
                break;
                case SaveFormat.Txt:
                ExportTxt(calendar, file, dates);
                break;
                case SaveFormat.Xlsx:
                ExportExcel(calendar, file, dates);
                break;
                default:
                throw new ArgumentOutOfRangeException(nameof(saveFormat), saveFormat, @"Formato de exportação não suportado.");
            }
        }

        private static void ExportExcel(ICalendar calendar, string file, List<(DateTime date, bool isWorkDay)> dates)
        {
            File.Delete(file);

            var fi = new FileInfo(file);
            using var package = new ExcelPackage(fi);
            var sheet = package.Workbook.Worksheets.Add("dias");

            sheet.SetValue(1, 1, calendar.Description);
            sheet.SetValue(2, 1, calendar.Name);
            sheet.SetValue(4, 1, "Data");
            sheet.SetValue(4, 2, "É dia útil");

            var row = 5;
            foreach (var (date, isWorkDay) in dates)
            {
                sheet.SetValue(row, 1, date);
                sheet.Cells[row, 1].Style.Numberformat.Format = "dd/mm/yyyy";
                sheet.SetValue(row, 2, isWorkDay ? 1 : 0);
                ++row;
            }

            package.Save();
        }

        private static void ExportTxt(ICalendar calendar, string file, List<(DateTime date, bool isWorkDay)> dates)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"{calendar.Name ?? "sem_nome"};{calendar.Description ?? "sem descrição"}");
            sb.AppendLine("Data;É dia útil");

            foreach (var (date, isWorkDay) in dates)
            {
                sb.AppendLine($"{date:yyyy-MM-dd};{(isWorkDay ? "1" : "0")}");
            }
            
            File.WriteAllText(file, sb.ToString(), Encoding.UTF8);
        }

        private static void ExportCsv(ICalendar calendar, string file, List<(DateTime date, bool isWorkDay)> dates)
        {
            var sb = new StringBuilder();

            var separator = CultureInfo.CurrentCulture.TextInfo.ListSeparator;

            sb.AppendLine($"{calendar.Name ?? "sem_nome"}{separator}{calendar.Description ?? "sem descrição"}");
            sb.AppendLine($"Data{separator}É dia útil");

            foreach (var (date, isWorkDay) in dates)
            {
                sb.AppendLine($"{date:d}{separator}{(isWorkDay ? "1" : "0")}");
            }

            File.WriteAllText(file, sb.ToString(), Encoding.UTF8);
        }
    }
}