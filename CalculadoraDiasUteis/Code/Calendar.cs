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
using System.Linq;
using System.Runtime.CompilerServices;

namespace Elekto.Code
{
    /// <summary>
    ///     Classe que representa um redoma
    /// </summary>
    internal class Calendar : ICalendar
    {
        private readonly HashSet<DateTime> _hashHolidays = new();

        private readonly HashSet<DayOfWeek> _nonWorkWeekDays = new() { DayOfWeek.Saturday, DayOfWeek.Sunday };

        /// <summary>
        ///     vetor de 0s e 1s onde 0 são feriados/fim-de-semana, e 1 são dias de trabalho
        /// </summary>
        private int[] _days;

        /// <summary>
        ///     vetor de prazos de dias uteis, incrementais
        /// </summary>
        private int[] _period;

        /// <summary>
        ///     Tamanho da redoma
        /// </summary>
        private int _size;

        /// <summary>
        ///     Constrói um calendário com somente sábados e domingos
        /// </summary>
        public Calendar()
            : this(Array.Empty<DateTime>())
        {
        }

        /// <summary>
        ///     Constrói um ICalendar
        /// </summary>
        /// <param name="holidays">Lista com os feriados</param>
        /// <param name="businessCenterCode">código do lugar, opcional</param>
        /// <param name="description">descrição, opcional</param>
        /// <param name="nonWorkWeekDays">Os dias da semana não-uteis, opcional. Se nulo sábado e domingo serão não-uteis.</param>
        public Calendar(IEnumerable<DateTime> holidays, string businessCenterCode = null,
            string description = null, IEnumerable<DayOfWeek> nonWorkWeekDays = null)
        {
            Initialize(businessCenterCode, description, holidays, nonWorkWeekDays);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Calendar" /> class.
        /// </summary>
        /// <param name="calendarInfoProvider">The holiday provider.</param>
        public Calendar(ICalendarInfoProvider calendarInfoProvider)
        {
            var ci = calendarInfoProvider.GetCalendarInfo(calendarInfoProvider.DefaultCalendarName);
            Initialize(ci.Name, ci.Description, ci.Holidays, ci.NonWorkDays);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Calendar" /> class.
        /// </summary>
        /// <param name="calendarInfo">The calendar info.</param>
        public Calendar(CalendarInfo calendarInfo)
        {
            Initialize(calendarInfo.Name, calendarInfo.Description, calendarInfo.Holidays, calendarInfo.NonWorkDays);
        }

        private void AdjustTerminalDates(DeltaTerminalDayAdjust adjust, ref DateTime ini, ref DateTime end)
        {
            switch (adjust)
            {
                case DeltaTerminalDayAdjust.EndOnNextWork:
                    end = GetNextOrSameWorkday(end);
                    break;
                case DeltaTerminalDayAdjust.EndOnPrevWork:
                    end = GetPrevOrSameWorkday(end);
                    break;
                case DeltaTerminalDayAdjust.StartAndEndCollapsing:
                    ini = GetNextOrSameWorkday(ini);
                    end = GetPrevOrSameWorkday(end);
                    break;
                case DeltaTerminalDayAdjust.StartAndEndExpanding:
                    ini = GetPrevOrSameWorkday(ini);
                    end = GetNextOrSameWorkday(end);
                    break;
                case DeltaTerminalDayAdjust.StartAndEndOnNext:
                    ini = GetNextOrSameWorkday(ini);
                    end = GetNextOrSameWorkday(end);
                    break;
                case DeltaTerminalDayAdjust.StartAndEndOnPrev:
                    ini = GetPrevOrSameWorkday(ini);
                    end = GetPrevOrSameWorkday(end);
                    break;
                case DeltaTerminalDayAdjust.StartOnNextWork:
                    ini = GetNextOrSameWorkday(ini);
                    break;
                case DeltaTerminalDayAdjust.StartOnPrevWork:
                    ini = GetPrevOrSameWorkday(ini);
                    break;
            }
        }

        private bool IsWorkDatesZeroOrAlmostZeroSized(DateTime ini, DateTime end, ICollection<DateTime> workDates)
        {
            if (ini == end && IsWorkday(ini))
            {
                workDates.Add(ini);
                return true;
            }

            if (ini == end && !IsWorkday(ini))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Inicializa a Redoma
        /// </summary>
        /// <param name="businessCenterCode">The business center code.</param>
        /// <param name="description">The description.</param>
        /// <param name="holidays">The holidays.</param>
        /// <param name="nonWorkWeekDays"> </param>
        private void Initialize(string businessCenterCode, string description, IEnumerable<DateTime> holidays,
            IEnumerable<DayOfWeek> nonWorkWeekDays)
        {
            Name = businessCenterCode ?? string.Empty;
            Description = description ?? Name;

            if (nonWorkWeekDays != null)
            {
                _nonWorkWeekDays.Clear();
                _nonWorkWeekDays.UnionWith(nonWorkWeekDays);
                if (_nonWorkWeekDays.Count >= 7)
                {
                    throw new ArgumentException(@"Um mundo só de dias não úteis não será funcional.", nameof(nonWorkWeekDays));
                }
            }

            holidays ??= Array.Empty<DateTime>();

            _hashHolidays.UnionWith(holidays.Select(d => d.Date.RemoveKind()));

            if (_hashHolidays.Count == 0)
            {
                MinDate = DateExtensions.Today.AddYears(-25);
                MaxDate = DateExtensions.Today.AddYears(+75);
            }
            else if (_hashHolidays.Count == 1)
            {
                MinDate = _hashHolidays.First().AddYears(-25);
                MaxDate = _hashHolidays.First().AddYears(+75);
            }
            else
            {
                Debug.Assert(_hashHolidays.Count > 1);
                MinDate = _hashHolidays.Min();
                MaxDate = _hashHolidays.Max();
            }

            // dimensiona os vetores de calculo rápido
            var deltaMax = MaxDate - MinDate;
            _size = deltaMax.Days + 1;

            _days = new int[_size];
            _period = new int[_size];

            // preenche o vetor de trabalho
            var curDate = MinDate;
            for (var i = 0; i < _size; ++i)
            {
                if (_nonWorkWeekDays.Contains(curDate.DayOfWeek) || _hashHolidays.Contains(curDate))
                {
                    _days[i] = 0;
                }
                else
                {
                    _days[i] = 1;
                }

                curDate = curDate.AddDays(1);
            }

            // preenche o vetor de prazos acumulados
            _period[0] = _days[0];
            for (var i = 1; i < _size; ++i)
            {
                _period[i] = _period[i - 1] + _days[i];
            }

            if (_period[_period.Length - 1] == 0)
            {
                throw new ArgumentException(
                    $@"O calendário '{businessCenterCode}' contém em seus limites [{MinDate:yyyy-MM-dd}; {MaxDate:yyyy-MM-dd}] apenas dias não úteis! Não é possível computar num calendário assim.",
                    nameof(holidays));
            }

            MaxWorkDate = GetPrevOrSameWorkday(MaxDate);
            MinWorkDate = GetNextOrSameWorkday(MinDate);
        }

        /// <summary>
        ///     Adiciona dias uteis a uma data, mas usa um método lento, e robusto
        /// </summary>
        /// <param name="date">data inicial</param>
        /// <param name="workDays">dias a acrescentar (ou subtrair)</param>
        /// <param name="option">Método de ajuste da data final</param>
        /// <returns>uma data útil</returns>
        private DateTime AddWorkDaysOneByOne(DateTime date, int workDays, FinalDateAdjust option = FinalDateAdjust.Following)
        {
            date = date.Date.RemoveKind();
            if (workDays == 0)
            {
                return date;
            }

            if (workDays > 0)
            {
                for (var i = 0; i < workDays; ++i)
                {
                    date = GetNextWorkday(date);
                }
            }
            else
            {
                for (var i = 0; i > workDays; --i)
                {
                    date = GetPrevWorkday(date);
                }
            }

            return AdjustFinalDate(date, option);
        }

        public override string ToString()
        {
            return Description ?? Name ?? string.Empty;
        }

        /// <summary>
        ///     Nome do calendário
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        ///     Descrição para este ICalendar
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        ///     Maior dia util que o calendário alcança
        /// </summary>
        public DateTime MaxWorkDate { get; private set; }

        /// <summary>
        ///     Data mínima alcançada
        /// </summary>
        public DateTime MinDate { get; private set; }

        /// <summary>
        ///     Menor data útil que o calendário alcança
        /// </summary>
        public DateTime MinWorkDate { get; private set; }

        /// <summary>
        ///     Os feriados todos
        /// </summary>
        public IEnumerable<DateTime> Holidays
        {
            get { return _hashHolidays.OrderBy(h => h); }
        }

        /// <summary>
        ///     Os dias não uteis da semana (normalmente apenas sábado e domingo)
        /// </summary>
        public IEnumerable<DayOfWeek> NonWorkWeekDays => _nonWorkWeekDays;

        /// <summary>
        ///     Data máxima alcançada
        /// </summary>
        /// <value>The max date.</value>
        public DateTime MaxDate { get; private set; }


        /// <summary>
        ///     Retorna o n-ésimo dia util do mes, contado relativamente a data de referencia
        /// </summary>
        /// <param name="referenceDate">The reference date.</param>
        /// <param name="monthsAhead">The months ahead.</param>
        /// <param name="n">The day.</param>
        /// <returns></returns>
        public DateTime GetNthWorkingDay(DateTime referenceDate, int monthsAhead, int n)
        {
            var date = GetWorkingMonthHead(referenceDate, monthsAhead);
            if (n <= 1)
            {
                return date;
            }

            var candidateDate = AddWorkDays(date, n - 1);
            return date.GetSerialMonth() != candidateDate.GetSerialMonth()
                ? GetWorkingMonthTail(referenceDate, monthsAhead)
                : candidateDate;
        }

        /// <summary>
        ///     Gets the Nth to last working day.
        /// </summary>
        /// <param name="referenceDate">The reference date.</param>
        /// <param name="monthsAhead">The months ahead.</param>
        /// <param name="n">The n.</param>
        /// <returns></returns>
        public DateTime GetNthToLastWorkingDay(DateTime referenceDate, int monthsAhead, int n)
        {
            var date = GetWorkingMonthTail(referenceDate, monthsAhead);
            if (n <= 1)
            {
                return date;
            }

            var candidateDate = AddWorkDays(date, -n + 1);
            return date.GetSerialMonth() != candidateDate.GetSerialMonth()
                ? GetWorkingMonthHead(referenceDate, monthsAhead)
                : candidateDate;
        }

        /// <summary>
        ///     Retorna se é um dia de trabalho
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns>
        ///     <c>true</c> if the specified date is workday; otherwise, <c>false</c>.
        /// </returns>
        public bool IsWorkday(DateTime date)
        {
            if (date < MinDate)
            {
                throw new ArgumentOutOfRangeException(nameof(date), date, $@"Data anterior a mínima aceitável {MinDate:yyyy-MM-dd} no calendário '{Name}'.");
            }

            if (date > MaxDate)
            {
                throw new ArgumentOutOfRangeException(nameof(date), date, $@"Data posterior a máxima aceitável {MaxDate:yyyy-MM-dd} no calendário '{Name}'.");
            }

            date = date.NormalizeDate();
            return !_nonWorkWeekDays.Contains(date.DayOfWeek) && !_hashHolidays.Contains(date);
        }

        /// <summary>
        ///     Testa se o dia está marcado como feriado
        /// </summary>
        /// <param name="date">A data</param>
        /// <returns>
        ///     verdadeiro se a data for um feriado propriamente marcado.
        /// </returns>
        public bool IsProperHoliday(DateTime date)
        {
            return _hashHolidays.Contains(date.NormalizeDate());
        }

        /// <summary>
        ///     Retorna os dias Não-Uteis entre as datas (inclusive as pontas)
        /// </summary>
        /// <param name="minDate"></param>
        /// <param name="maxDate"></param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public IEnumerable<DateTime> GetNonWorkDates(DateTime minDate, DateTime maxDate)
        {
            minDate = minDate.NormalizeDate();
            maxDate = maxDate.NormalizeDate();

            if (minDate < MinDate)
            {
                throw new ArgumentOutOfRangeException(nameof(minDate), minDate, $@"A data mínima no calendário '{Name}' é {MinDate:yyyy-MM-dd}");
            }

            if (minDate > MaxDate)
            {
                throw new ArgumentOutOfRangeException(nameof(minDate), minDate, $@"A data máxima no calendário '{Name}' é {MaxDate:yyyy-MM-dd}");
            }

            if (maxDate < MinDate)
            {
                throw new ArgumentOutOfRangeException(nameof(maxDate), maxDate, $@"A data mínima no calendário '{Name}' é {MinDate:yyyy-MM-dd}");
            }

            if (maxDate > MaxDate)
            {
                throw new ArgumentOutOfRangeException(nameof(maxDate), maxDate, $@"A data máxima no calendário '{Name}' é {MaxDate:yyyy-MM-dd}");
            }

            if (minDate > maxDate)
            {
                return GetNonWorkDates(maxDate, minDate).Reverse();
            }

            var list = new List<DateTime>();
            var curDate = minDate;
            while (curDate <= maxDate)
            {
                if (!IsWorkday(curDate))
                {
                    list.Add(curDate);
                }

                curDate = curDate.AddDays(1).NormalizeDate();
            }

            return list;
        }

        /// <summary>
        ///     Devolve um novo calendário que é a união (em termos de feriados e dias não uteis) entre esta instância e a outra
        ///     fornecida
        /// </summary>
        /// <param name="other">O outro calendário</param>
        /// <returns>
        ///     A união entre os calendário
        /// </returns>
        public ICalendar UnionWith(ICalendar other)
        {
            var name = Name + "+" + other.Name;
            var description = Description + " & " + other.Description;

            var nonWorkWeekDays = new HashSet<DayOfWeek>(NonWorkWeekDays);
            nonWorkWeekDays.UnionWith(other.NonWorkWeekDays);

            var holidays = new HashSet<DateTime>(Holidays);
            holidays.UnionWith(other.Holidays);

            return new Calendar(holidays, name, description, nonWorkWeekDays);
        }

        /// <summary>
        ///     Retorna o próximo dia útil
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        public DateTime GetNextWorkday(DateTime date)
        {
            date = date.Date.RemoveKind();

            if (date < MinDate)
            {
                throw new ArgumentOutOfRangeException(nameof(date), date, $@"Data anterior a mínima aceitável {MinDate:yyyy-MM-dd} no calendário '{Name}'.");
            }

            if (date > MaxDate)
            {
                throw new ArgumentOutOfRangeException(nameof(date), date, $@"Data posterior a máxima aceitável {MaxDate:yyyy-MM-dd} no calendário '{Name}'.");
            }

            date = date.AddDays(1);
            while (IsWorkday(date) == false)
            {
                date = date.AddDays(1);
            }

            return date;
        }


        /// <summary>
        ///     Numero de dias úteis entre <paramref name="ini" /> e <paramref name="end" /> utilizando
        ///     <paramref name="periodType" />
        /// </summary>
        public int GetDeltaDays(DateTime ini, DateTime end, PeriodType periodType)
        {
            switch (periodType)
            {
                case PeriodType.WorkDays:
                    return GetDeltaWorkDays(ini, end);
                case PeriodType.ActualDays:
                    return GetDeltaActualDays(ini, end);
                default:
                    throw new ArgumentOutOfRangeException(nameof(periodType));
            }
        }

        /// <summary>
        ///     Retorna o mesmo dia, caso seja útil, do contrário retorna o próximo útil
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        public DateTime GetNextOrSameWorkday(DateTime date)
        {
            date = date.Date.RemoveKind();

            return IsWorkday(date) ? date : GetNextWorkday(date);
        }


        /// <summary>
        ///     Retorna o dia útil anterior
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        public DateTime GetPrevWorkday(DateTime date)
        {
            date = date.Date.RemoveKind();

            if (date < MinDate)
            {
                throw new ArgumentOutOfRangeException(nameof(date), date, $"Data anterior a mínima aceitável {MinDate:yyyy-MM-dd} no calendário '{Name}'.");
            }

            if (date > MaxDate)
            {
                throw new ArgumentOutOfRangeException(nameof(date), date, $"Data posterior a máxima aceitável {MaxDate:yyyy-MM-dd} no calendário '{Name}'.");
            }

            date = date.AddDays(-1);
            while (IsWorkday(date) == false)
            {
                date = date.AddDays(-1);
            }

            return date;
        }


        /// <summary>
        ///     Retorna o mesmo dia, caso seja útil, do contrário retorna o dia útil anterior
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        public DateTime GetPrevOrSameWorkday(DateTime date)
        {
            date = date.Date.RemoveKind();

            return IsWorkday(date) ? date : GetPrevWorkday(date);
        }

        /// <summary>
        ///     Ajusta a data final de um prazo
        /// </summary>
        /// <param name="date">a data a ser ajustada</param>
        /// <param name="option">um dos métodos a ser usado no ajuste</param>
        /// <returns>a data ajustada</returns>
        public DateTime AdjustFinalDate(DateTime date, FinalDateAdjust option)
        {
            date = date.Date.RemoveKind();
            switch (option)
            {
                case FinalDateAdjust.Following:
                    date = GetNextOrSameWorkday(date);
                    break;
                case FinalDateAdjust.ModifiedFollowing:
                {
                    var month = date.Month;
                    date = GetNextOrSameWorkday(date);
                    if (date.Month != month)
                    {
                        date = GetPrevWorkday(date);
                    }
                }
                    break;
            }

            return date;
        }


        /// <summary>
        ///     Adiciona (ou subtrai) dias uteis a data passada
        /// </summary>
        /// <param name="date">data base</param>
        /// <param name="workDays">número de dias úteis a soma ou subtrair (se negativo)</param>
        /// <param name="finalDateAdjust">Ajuste da data final</param>
        /// <returns></returns>
        public DateTime AddWorkDays(DateTime date, int workDays,
            FinalDateAdjust finalDateAdjust = FinalDateAdjust.Following)
        {
            date = date.Date.RemoveKind();
            if (workDays == 0)
            {
                return date;
            }

            const int limitBetweenMethods = 5;

            // método tosco
            if (Math.Abs(workDays) <= limitBetweenMethods)
            {
                return AddWorkDaysOneByOne(date, workDays);
            }

            if (workDays < 0)
            {
                date = GetNextOrSameWorkday(date);
            }

            var initialIndex = (int)(date - MinDate).TotalDays;
            if (initialIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(date), date,
                    $@"A data inicial é anterior a mínima aceitável de {MinDate:yyyy-MM-dd} para o calendário '{Name}'.");
            }

            if (initialIndex >= _period.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(date), date,
                    $@"A data inicial ultrapassou a máxima possível de {MaxDate:yyyy-MM-dd} para o calendário '{Name}'.");
            }

            var initialPeriod = _period[initialIndex];
            var finalPeriod = initialPeriod + workDays;

            if (finalPeriod < _period[0])
            {
                throw new ArgumentOutOfRangeException(nameof(workDays), workDays,
                    $@"A data inicial é anterior a mínima aceitável de {MinDate:yyyy-MM-dd} para o calendário '{Name}'.");
            }

            if (finalPeriod > _period[_period.Length - 1])
            {
                throw new ArgumentOutOfRangeException(nameof(workDays), workDays,
                    $@"A data final ultrapassou a máxima possível de {MaxDate:yyyy-MM-dd} para o calendário '{Name}'.");
            }


            // localiza o índice que contem o prazo final
            var minSearch = 0;
            var maxSearch = _period.Length - 1;
            var mid = maxSearch / 2;
            do
            {
                if (_period[mid] == finalPeriod)
                {
                    break;
                }

                if (_period[mid] > finalPeriod)
                {
                    maxSearch = mid;
                }

                if (_period[mid] < finalPeriod)
                {
                    minSearch = mid;
                }

                mid = (minSearch + maxSearch) / 2;

                if (minSearch == maxSearch)
                {
                    break;
                }
            } while (true);

            var res = MinDate.AddDays(mid);
            res = GetPrevOrSameWorkday(res);
            if (workDays < 0)
            {
                res = GetPrevOrSameWorkday(res);
            }

            return AdjustFinalDate(res, finalDateAdjust);
        }


        /// <summary>
        ///     Adiciona <paramref name="days" /> em <paramref name="date" /> utilizando <paramref name="periodType" />
        /// </summary>
        public DateTime AddDays(DateTime date, int days, PeriodType periodType)
        {
            switch (periodType)
            {
                case PeriodType.WorkDays:
                    return AddWorkDays(date, days);
                case PeriodType.ActualDays:
                    return AddActualDays(date, days);
                default:
                    throw new ArgumentOutOfRangeException(nameof(periodType));
            }
        }

        /// <summary>
        ///     Adiciona dias calendário, com ajuste da data final
        /// </summary>
        /// <param name="date">data</param>
        /// <param name="days">dias calendário a adicionar (ou subtrair se negativo)</param>
        /// <param name="finalDateAdjust">Método de ajuste da data final</param>
        /// <returns></returns>
        public DateTime AddActualDays(DateTime date, int days, FinalDateAdjust finalDateAdjust = FinalDateAdjust.None)
        {
            date = date.Date.RemoveKind();
            date = date.AddDays(days);
            return AdjustFinalDate(date, finalDateAdjust);
        }


        /// <summary>
        ///     Calcula o numero de dias corridos entre as duas datas, fazendo ajustes nas datas terminais,
        ///     se necessário
        /// </summary>
        /// <param name="ini">data inicial</param>
        /// <param name="end">data final</param>
        /// <param name="adjust">Um método de ajuste para as datas terminais.</param>
        /// <returns>
        ///     o número de dias corridos entre as datas
        /// </returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public int GetDeltaActualDays(DateTime ini, DateTime end,
            DeltaTerminalDayAdjust adjust = DeltaTerminalDayAdjust.Financial)
        {
            ini = ini.Date.RemoveKind();
            end = end.Date.RemoveKind();

            // Curto Circuitado?
            if (ini == end)
            {
                if (adjust == DeltaTerminalDayAdjust.Full)
                {
                    return 1;
                }

                return 0;
            }

            // invertido?
            if (ini > end)
            {
                return -GetDeltaActualDays(end, ini, adjust);
            }

            // Aplica o ajuste aos extremos
            AdjustTerminalDates(adjust, ref ini, ref end);

            // Curto circuitado novamente?
            if (ini == end)
            {
                return 0;
            }

            // faz o calculo real
            var timeSpan = end - ini;
            var delta = timeSpan.Days;

            if (adjust == DeltaTerminalDayAdjust.Full)
            {
                ++delta;
            }

            if (adjust == DeltaTerminalDayAdjust.Financial)
            {
                // a adaptação para dias corridos já esta correta.
            }

            return delta;
        }


        /// <summary>
        ///     Calcula o numero de dias uteis entre as duas datas, fazendo ajustes nas datas terminais,
        ///     se necessário
        /// </summary>
        /// <param name="ini">data inicial</param>
        /// <param name="end">data final</param>
        /// <param name="adjust">Um método de ajuste para as datas terminais</param>
        /// <returns>o número de dias uteis entre as datas</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public int GetDeltaWorkDays(DateTime ini, DateTime end,
            DeltaTerminalDayAdjust adjust = DeltaTerminalDayAdjust.Financial)
        {
            ini = ini.Date.RemoveKind();
            end = end.Date.RemoveKind();

            // Curto Circuitado?
            if (ini == end)
            {
                if (adjust == DeltaTerminalDayAdjust.Full && IsWorkday(ini))
                {
                    return 1;
                }

                return 0;
            }

            // invertido?
            if (ini > end)
            {
                return -GetDeltaWorkDays(end, ini, adjust);
            }

            // Aplica o ajuste aos extremos
            AdjustTerminalDates(adjust, ref ini, ref end);

            // Curto circuitado novamente?
            if (ini == end)
            {
                if (adjust == DeltaTerminalDayAdjust.Full && IsWorkday(ini))
                {
                    return 1;
                }

                return 0;
            }

            // valida limites
            if (ini > MaxDate)
            {
                var ex =
                    new ArgumentOutOfRangeException(nameof(ini), ini,
                        $@"Data inicial posterior a máxima aceitável {MaxDate:yyyy-MM-dd} no calendário '{Name}'.");
                throw ex;
            }

            if (end > MaxDate)
            {
                var ex =
                    new ArgumentOutOfRangeException(nameof(end), end,
                        $@"Data final posterior a máxima aceitável {MaxDate:yyyy-MM-dd} no calendário '{Name}'.");
                throw ex;
            }

            if (ini < MinDate)
            {
                var ex =
                    new ArgumentOutOfRangeException(nameof(ini), ini,
                        $@"Data inicial anterior a mínima aceitável {MinDate:yyyy-MM-dd} no calendário '{Name}'.");
                throw ex;
            }

            if (end < MinDate)
            {
                var ex =
                    new ArgumentOutOfRangeException(nameof(end), end,
                        $@"Data final anterior a mínima aceitável {MinDate:yyyy-MM-dd} no calendário '{Name}'.");
                throw ex;
            }


            // indices
            var timeSpan = ini - MinDate;
            var initialIndex = timeSpan.Days;

            timeSpan = end - MinDate;
            var finalIndex = timeSpan.Days;

            var delta = _period[finalIndex] - _period[initialIndex];

            var iniWorkday = IsWorkday(ini);
            var endWorkday = IsWorkday(end);

            if (adjust == DeltaTerminalDayAdjust.Full)
            {
                if (iniWorkday)
                {
                    ++delta;
                }
            }
            else
            {
                if (delta == 0 && iniWorkday && !endWorkday)
                {
                    ++delta;
                }
                else if (!iniWorkday && endWorkday)
                {
                    --delta;
                }
                else if (iniWorkday && !endWorkday)
                {
                    ++delta;
                }
            }

            return delta;
        }

        /// <summary>
        ///     Retorna um array com datas uteis
        /// </summary>
        /// <param name="ini">dia inicial</param>
        /// <param name="end">dia final</param>
        /// <param name="deltaTerminalDayAdjust">método de ajuste</param>
        /// <returns>O Array requerido</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public IEnumerable<DateTime> GetWorkDates(DateTime ini, DateTime end,
            DeltaTerminalDayAdjust deltaTerminalDayAdjust =
                DeltaTerminalDayAdjust.Financial)
        {
            var workDates = new List<DateTime>();

            ini = ini.NormalizeDate();
            end = end.NormalizeDate();

            // curto circuito?
            if (IsWorkDatesZeroOrAlmostZeroSized(ini, end, workDates))
            {
                return workDates;
            }

            // invertido?
            if (ini > end)
            {
                var normalWorkDates = GetWorkDates(end, ini, deltaTerminalDayAdjust);
                return normalWorkDates.Reverse();
            }

            // Aplica o ajuste aos extremos
            AdjustTerminalDates(deltaTerminalDayAdjust, ref ini, ref end);

            // tornou-se curto circuito?
            if (IsWorkDatesZeroOrAlmostZeroSized(ini, end, workDates))
            {
                return workDates;
            }

            // tornou-se invertido?
            if (ini > end)
            {
                var normalWorkDates = GetWorkDates(end, ini, deltaTerminalDayAdjust);
                return normalWorkDates.Reverse();
            }

            var curDate = GetNextOrSameWorkday(ini);
            while (curDate <= end)
            {
                workDates.Add(curDate);
                curDate = GetNextWorkday(curDate);
            }

            return workDates;
        }

        /// <summary>
        ///     Retorna o primeiro dia util do mês
        /// </summary>
        /// <param name="referenceDate">Data de referência</param>
        /// <param name="monthsAhead">Meses a adicionar (ou subtrair) da data de referencia.</param>
        /// <returns>O primeiro dia util do mês</returns>
        public DateTime GetWorkingMonthHead(DateTime referenceDate, int monthsAhead)
        {
            var date = GetActualMonthHead(referenceDate, monthsAhead);
            date = GetNextOrSameWorkday(date);
            return date;
        }

        /// <summary>
        ///     Retorna o primeiro dia do mês
        /// </summary>
        /// <param name="referenceDate">Data de referência</param>
        /// <param name="monthsAhead">Meses a adicionar (ou subtrair) da data de referencia.</param>
        /// <returns>O primeiro dia do mês</returns>
        public DateTime GetActualMonthHead(DateTime referenceDate, int monthsAhead)
        {
            var date = referenceDate.Date.RemoveKind();
            date = date.AddMonths(monthsAhead);
            date = new DateTime(date.Year, date.Month, 1, 0, 0, 0, DateTimeKind.Unspecified);
            return date;
        }

        /// <summary>
        ///     Retorna o ultimo dia do mês
        /// </summary>
        /// <param name="referenceDate">Data de referencia</param>
        /// <param name="monthsAhead">Meses a adicionar (ou subtrair) da data de referencia.</param>
        /// <returns>O ultimo dia do mês</returns>
        public DateTime GetActualMonthTail(DateTime referenceDate, int monthsAhead)
        {
            var date = referenceDate.Date.RemoveKind();
            date = date.AddMonths(monthsAhead);
            date = date.ChangeDay(31);
            return date;
        }

        /// <summary>
        ///     Retorna o último dia util do mês
        /// </summary>
        /// <param name="referenceDate">Data de referência</param>
        /// <param name="monthsAhead">Meses a adicionar (ou subtrair) da data de referencia.</param>
        /// <returns>O último dia util do mês</returns>
        public DateTime GetWorkingMonthTail(DateTime referenceDate, int monthsAhead)
        {
            var date = GetActualMonthTail(referenceDate, monthsAhead);
            date = GetPrevOrSameWorkday(date);
            return date;
        }

        
    }
}