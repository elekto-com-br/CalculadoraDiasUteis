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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Elekto.Tools.GoldenMaster
{
    /// <summary>
    ///     Gera o arquivo de referência dos testes chamando, por reflexão, o núcleo de cálculo do executável da versão 1.2.
    /// </summary>
    /// <remarks>
    ///     Uso: GoldenMaster &lt;pasta com CalculadoraDiasUteis.exe e Calendars&gt; &lt;arquivo de saída .csv.gz&gt;
    /// </remarks>
    internal static class Program
    {
        private const int Seed = 20261025;
        private const int N = 150;

        private static int Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.Error.WriteLine("Uso: GoldenMaster <pasta com CalculadoraDiasUteis.exe e Calendars> <saída .csv.gz>");
                return 2;
            }

            var exe = Path.Combine(args[0], "CalculadoraDiasUteis.exe");
            var legacy = new Legacy(exe, Path.Combine(args[0], "Calendars"));

            using var file = File.Create(args[1]);
            using var gzip = new GZipStream(file, CompressionLevel.Optimal);
            using var writer = new StreamWriter(gzip, new UTF8Encoding(false)) { NewLine = "\n" };

            writer.WriteLine($"# Gerado a partir de CalculadoraDiasUteis.exe {legacy.Version}, sha-256 {Sha256(exe)}, semente {Seed}.");
            writer.WriteLine("# Colunas: calendário;operação;arg1;arg2;arg3;resultado. Exceções aparecem como !Tipo.");
            writer.WriteLine($"provider;EnumCalendarNames;;;;{string.Join(" ", legacy.CalendarNames)}");
            writer.WriteLine($"provider;DefaultCalendarName;;;;{legacy.DefaultCalendarName}");

            var calendars = legacy.CalendarNames.Select(n => (Name: n, Calendar: legacy.GetCalendar(n))).ToList();
            calendars.Add(("br-BC+us-NYC", legacy.Union(legacy.GetCalendar("br-BC"), legacy.GetCalendar("us-NYC"))));

            var rows = 0;
            for (var i = 0; i < calendars.Count; i++)
            {
                var (name, calendar) = calendars[i];
                foreach (var (op, callArgs) in Cases(new Random(Seed + i), legacy, calendar))
                {
                    var result = legacy.Call(calendar, op, callArgs);
                    var cells = callArgs.Select(Format).Concat(Enumerable.Repeat(string.Empty, 3)).Take(3);
                    writer.WriteLine($"{name};{op};{string.Join(";", cells)};{result}");
                    rows++;
                }
            }

            Console.WriteLine($"{rows} casos gravados em {args[1]}.");
            return 0;
        }

        private static IEnumerable<(string Op, object[] Args)> Cases(Random r, Legacy legacy, object calendar)
        {
            foreach (var property in new[] { "Name", "Description", "MinDate", "MaxDate", "MinWorkDate", "MaxWorkDate", "Holidays", "NonWorkWeekDays" })
            {
                yield return (property, Array.Empty<object>());
            }

            var min = (DateTime)legacy.Get(calendar, "MinDate");
            var max = (DateTime)legacy.Get(calendar, "MaxDate");
            var holidays = ((IEnumerable<DateTime>)legacy.Get(calendar, "Holidays")).ToArray();

            // Datas: perto de feriados, perto dos limites (inclusive fora deles) e espalhadas pelo calendário.
            DateTime D()
            {
                var p = r.NextDouble();
                if (p < 0.4) return holidays[r.Next(holidays.Length)].AddDays(r.Next(-3, 4));
                if (p < 0.5) return (r.Next(2) == 0 ? min : max).AddDays(r.Next(-5, 6));
                return min.AddDays(r.Next(-30, (int)(max - min).TotalDays + 31));
            }

            // Prazos curtos (onde AddWorkDays usa outro algoritmo), médios e longos, positivos e negativos.
            int T()
            {
                var p = r.NextDouble();
                if (p < 0.4) return r.Next(-12, 13);
                if (p < 0.8) return r.Next(-600, 601);
                return r.Next(-25000, 25001);
            }

            DateTime Other(DateTime a)
            {
                var p = r.NextDouble();
                if (p < 0.4) return a.AddDays(r.Next(-15, 16));
                if (p < 0.8) return a.AddDays(r.Next(-1200, 1201));
                return D();
            }

            var finalAdjusts = legacy.EnumValues("FinalDateAdjust");
            var deltaAdjusts = legacy.EnumValues("DeltaTerminalDayAdjust");
            var periodTypes = legacy.EnumValues("PeriodType");

            for (var i = 0; i < 2 * N; i++) yield return ("IsWorkday", new object[] { D() });
            for (var i = 0; i < N; i++) yield return ("IsProperHoliday", new object[] { D() });

            foreach (var op in new[] { "GetNextWorkday", "GetNextOrSameWorkday", "GetPrevWorkday", "GetPrevOrSameWorkday" })
            {
                for (var i = 0; i < N; i++) yield return (op, new object[] { D() });
            }

            foreach (var adjust in finalAdjusts)
            {
                for (var i = 0; i < N; i++) yield return ("AdjustFinalDate", new[] { D(), adjust });
                for (var i = 0; i < 2 * N; i++) yield return ("AddWorkDays", new[] { D(), T(), adjust });
                for (var i = 0; i < N; i++) yield return ("AddActualDays", new[] { D(), T(), adjust });
            }

            foreach (var periodType in periodTypes)
            {
                for (var i = 0; i < N / 2; i++) yield return ("AddDays", new[] { D(), T(), periodType });
                for (var i = 0; i < N / 3; i++)
                {
                    var a = D();
                    yield return ("GetDeltaDays", new[] { a, Other(a), periodType });
                }
            }

            foreach (var adjust in deltaAdjusts)
            {
                for (var i = 0; i < N; i++)
                {
                    var a = D();
                    yield return ("GetDeltaWorkDays", new[] { a, Other(a), adjust });
                }

                for (var i = 0; i < N / 2; i++)
                {
                    var a = D();
                    yield return ("GetDeltaActualDays", new[] { a, Other(a), adjust });
                }

                for (var i = 0; i < N / 5; i++)
                {
                    var a = D();
                    yield return ("GetWorkDates", new[] { a, a.AddDays(r.Next(-10, 46)), adjust });
                }
            }

            for (var i = 0; i < N / 3; i++)
            {
                var a = D();
                yield return ("GetNonWorkDates", new object[] { a, a.AddDays(r.Next(-60, 61)) });
            }

            foreach (var op in new[] { "GetActualMonthHead", "GetActualMonthTail", "GetWorkingMonthHead", "GetWorkingMonthTail" })
            {
                for (var i = 0; i < N / 2; i++) yield return (op, new object[] { D(), r.Next(-24, 25) });
            }

            foreach (var op in new[] { "GetNthWorkingDay", "GetNthToLastWorkingDay" })
            {
                for (var i = 0; i < N / 2; i++) yield return (op, new object[] { D(), r.Next(-24, 25), r.Next(-1, 26) });
            }
        }

        internal static string Format(object value)
        {
            switch (value)
            {
                case null:
                    return string.Empty;
                case DateTime d:
                    return d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                case bool b:
                    return b ? "true" : "false";
                case int i:
                    return i.ToString(CultureInfo.InvariantCulture);
                case string s:
                    return s;
                case IEnumerable<DateTime> dates:
                    return string.Join(" ", dates.Select(d => d.ToString("yyyyMMdd", CultureInfo.InvariantCulture)));
                case IEnumerable items:
                    return string.Join(" ", items.Cast<object>().Select(o => o.ToString()));
                default:
                    return value.ToString();
            }
        }

        private static string Sha256(string path)
        {
            using var sha = SHA256.Create();
            using var stream = File.OpenRead(path);
            return string.Concat(sha.ComputeHash(stream).Select(b => b.ToString("x2")));
        }

        /// <summary>
        ///     O núcleo da 1.2 é todo internal, então tudo passa por reflexão.
        /// </summary>
        private sealed class Legacy
        {
            private readonly Assembly _assembly;
            private readonly object _provider;
            private readonly object _server;

            public Legacy(string exe, string calendarsPath)
            {
                _assembly = Assembly.LoadFrom(exe);
                _provider = Activator.CreateInstance(Type("TextsInFolderCalendarInfoProvider"), calendarsPath);
                _server = Activator.CreateInstance(Type("CalendarServer"), _provider);
            }

            public Version Version => _assembly.GetName().Version;

            public IReadOnlyList<string> CalendarNames =>
                ((IEnumerable<string>)Invoke(_provider, "EnumCalendarNames")).ToList();

            public string DefaultCalendarName => (string)Get(_provider, "DefaultCalendarName");

            public object GetCalendar(string name)
            {
                var method = _server.GetType().GetMethod("GetCalendar", new[] { typeof(string) });
                return method!.Invoke(_server, new object[] { name });
            }

            public object Union(object a, object b) => Invoke(a, "UnionWith", b);

            public object[] EnumValues(string name) => Enum.GetValues(Type(name)).Cast<object>().ToArray();

            public object Get(object target, string property) =>
                target.GetType().GetProperty(property, BindingFlags.Public | BindingFlags.Instance)!.GetValue(target);

            /// <summary>
            ///     Chama a operação e devolve o resultado formatado, ou "!Tipo" se ela lançar exceção.
            /// </summary>
            public string Call(object calendar, string op, object[] args)
            {
                try
                {
                    var value = args.Length == 0 ? Get(calendar, op) : Invoke(calendar, op, args);
                    // Materializa enumerações preguiçosas dentro do try, para capturar exceções delas.
                    return Format(value is IEnumerable<DateTime> dates ? dates.ToList() : value);
                }
                catch (TargetInvocationException e) when (e.InnerException != null)
                {
                    return "!" + e.InnerException.GetType().Name;
                }
                catch (Exception e)
                {
                    return "!" + e.GetType().Name;
                }
            }

            private Type Type(string name) => _assembly.GetType("Elekto.Code." + name, true);

            private static object Invoke(object target, string method, params object[] args) =>
                target.GetType().GetMethod(method, BindingFlags.Public | BindingFlags.Instance)!.Invoke(target, args);
        }
    }
}
