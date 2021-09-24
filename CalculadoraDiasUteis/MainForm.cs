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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Elekto.Code;

namespace Elekto
{
    public partial class MainForm : Form
    {

        public MainForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Caminho para o diretório contendo calendários externos
        /// </summary>
        public string ExternalCalendarsPath { get; set; }

        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                var dataPath = ExternalCalendarsPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Calendars");
                if (!Directory.Exists(dataPath))
                {
                    throw new ApplicationException($"O diretório '{dataPath}' não existe. Você o apagou ou trocou o nome? Tente reinstalar este aplicativo.");
                }
                var calendarProvider = new TextsInFolderCalendarInfoProvider(dataPath);

                var calendarInfos = calendarProvider.All().ToList();
                if (calendarInfos.Count <= 0)
                {
                    throw new ApplicationException(
                        $"Nenhum calendário válido foi encontrado em '{dataPath}'. Você o apagou ou trocou o nome? Tente reinstalar este aplicativo.");
                }
                var calendarServer = new CalendarServer(calendarProvider);
                var calendarList = calendarInfos.Select(ci => calendarServer.GetCalendar(ci.Name)).ToList();

                calendars.DataSource = calendarList;

                // Datas Padrão
                termStartDate.Value = DateTime.Today;
                termEndDate.Value = termStartDate.Value.AddYears(1);
                startDate.Value = DateTime.Today;

                RestoreFromSettings();

                ValidateControls();
            }
            catch (ApplicationException ex)
            {
                DisableInputControls();
                MessageBox.Show(ex.Message, @"Erro na Aplicação", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                DisableInputControls();
                MessageBox.Show(ex.ToString(), @"Erro não Antecipado", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void ValidateControls()
        {
            // Limites dos calendários
            var calendar = (ICalendar)calendars.SelectedItem;

            var min = calendar.MinDate;
            if (termStartDate.Value < min) termStartDate.Value = min;
            if (termEndDate.Value < min) termStartDate.Value = min;
            if (startDate.Value < min) termStartDate.Value = min;
            termStartDate.MinDate = termEndDate.MinDate = startDate.MinDate = min;

            var max = calendar.MaxDate;
            if (termStartDate.Value > max) termStartDate.Value = max;
            if (termEndDate.Value > max) termStartDate.Value = max;
            if (startDate.Value > max) termStartDate.Value = max;
            termStartDate.MaxDate = termEndDate.MaxDate = startDate.MaxDate = max;

            // Ajuste só existe para dias corridos
            if (radioActualDays.Checked)
            {
                radioAdjustNext.Enabled = radioAdjustNextModified.Enabled = radioAdjustNone.Enabled = true;
            }
            else
            {
                radioAdjustNext.Enabled = radioAdjustNextModified.Enabled = radioAdjustNone.Enabled = false;
            }
        }

        private void DisableInputControls()
        {
            linkExport.Enabled = false;

            calendars.Enabled = termStartDate.Enabled = termEndDate.Enabled = false;
            radioFinancial.Enabled = radioFullDays.Enabled = false;
            linkSaveDates.Enabled = false;

            startDate.Enabled = term.Enabled = radioWorkDays.Enabled = radioActualDays.Enabled = false;
            radioAdjustNext.Enabled = radioAdjustNextModified.Enabled = radioAdjustNone.Enabled = false;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveSettings();
        }

        private void RestoreFromSettings()
        {
            var lastCalendar = Properties.Settings.Default.LastCalendar ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(lastCalendar))
            {
                foreach (var calendar in calendars.Items.Cast<ICalendar>())
                {
                    if (calendar.Description.Equals(lastCalendar, StringComparison.CurrentCultureIgnoreCase))
                    {
                        calendars.SelectedItem = calendar;
                    }
                }
            }

            var lastCalculationType = Properties.Settings.Default.LastCalculationType ?? string.Empty;
            if (lastCalculationType == DeltaTerminalDayAdjust.Financial.ToString())
            {
                radioFinancial.Checked = true;
            }
            else
            {
                radioFullDays.Checked = true;
            }

            term.Value = Properties.Settings.Default.LastTerm;

            var lastPeriodType = Properties.Settings.Default.LastPeriodType ?? string.Empty;
            if (lastPeriodType == PeriodType.WorkDays.ToString())
            {
                radioWorkDays.Checked = true;
            }
            else
            {
                radioActualDays.Checked = true;
            }

            var lastAdjust = Properties.Settings.Default.LastFinalDateAdjust ?? string.Empty;
            if (lastAdjust == FinalDateAdjust.Following.ToString())
            {
                radioAdjustNext.Checked = true;
            }
            else if (lastAdjust == FinalDateAdjust.ModifiedFollowing.ToString())
            {
                radioAdjustNextModified.Checked = true;
            }
            else
            {
                radioAdjustNone.Checked = true;
            }

        }

        private void SaveSettings()
        {
            try
            {
                var lastCalendar = (ICalendar)calendars.SelectedItem;
                if (!string.IsNullOrWhiteSpace(lastCalendar.Description))
                {
                    Properties.Settings.Default.LastCalendar = lastCalendar.Description;
                }

                Properties.Settings.Default.LastCalculationType = radioFinancial.Checked
                    ? DeltaTerminalDayAdjust.Financial.ToString()
                    : DeltaTerminalDayAdjust.Full.ToString();

                Properties.Settings.Default.LastTerm = (int)term.Value;

                Properties.Settings.Default.LastPeriodType = radioWorkDays.Checked
                        ? PeriodType.WorkDays.ToString()
                        : PeriodType.ActualDays.ToString();

                var adjust = FinalDateAdjust.None;
                if (radioAdjustNext.Checked)
                {
                    adjust = FinalDateAdjust.Following;
                }
                else if (radioAdjustNextModified.Checked)
                {
                    adjust = FinalDateAdjust.ModifiedFollowing;
                }
                Properties.Settings.Default.LastFinalDateAdjust = adjust.ToString();

                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), @"Erro não Antecipado", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Calendars_SelectedValueChanged(object sender, EventArgs e)
        {
            ClearAllResults();
            ValidateControls();
        }

        private void ClearAllResults()
        {
            workDays.Text = actualDays.Text = finalDate.Text = string.Empty;
        }

        private void RadioFullDays_CheckedChanged(object sender, EventArgs e)
        {
            workDays.Text = actualDays.Text = string.Empty;
        }

        private void RadioFinancial_CheckedChanged(object sender, EventArgs e)
        {
            workDays.Text = actualDays.Text = string.Empty;
        }

        private void RadioWorkDays_CheckedChanged(object sender, EventArgs e)
        {
            finalDate.Text = string.Empty;
            ValidateControls();
        }

        private void RadioActualDays_CheckedChanged(object sender, EventArgs e)
        {
            finalDate.Text = string.Empty;
            ValidateControls();
        }

        private void RadioAdjustNone_CheckedChanged(object sender, EventArgs e)
        {
            finalDate.Text = string.Empty;
        }

        private void RadioAdjustNext_CheckedChanged(object sender, EventArgs e)
        {
            finalDate.Text = string.Empty;
        }

        private void RadioAdjustNextModified_CheckedChanged(object sender, EventArgs e)
        {
            finalDate.Text = string.Empty;
        }

        private void ButtonCalculateTerm_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                var calendar = (ICalendar)calendars.SelectedItem;
                var start = termStartDate.Value;
                var end = termEndDate.Value;
                var calculationType = radioFinancial.Checked
                    ? DeltaTerminalDayAdjust.Financial
                    : DeltaTerminalDayAdjust.Full;

                var days = calendar.GetDeltaWorkDays(start, end, calculationType);
                workDays.Text = days.ToString("N0");

                days = calendar.GetDeltaActualDays(start, end, calculationType);
                actualDays.Text = days.ToString("N0");

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), @"Erro não Antecipado", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void ButtonCalculateFinal_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                var calendar = (ICalendar)calendars.SelectedItem;
                var start = startDate.Value;
                
                var termType = radioWorkDays.Checked
                    ? PeriodType.WorkDays
                    : PeriodType.ActualDays;

                var finalDateAdjust = FinalDateAdjust.None;
                if (radioAdjustNext.Checked) finalDateAdjust = FinalDateAdjust.Following;
                else if (radioAdjustNextModified.Checked) finalDateAdjust = FinalDateAdjust.ModifiedFollowing;

                var end = termType == PeriodType.WorkDays
                    ? calendar.AddWorkDays(start, (int)term.Value)
                    : calendar.AddActualDays(start, (int)term.Value, finalDateAdjust);

                finalDate.Text = end.ToString("d");

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), @"Erro não Antecipado", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void LinkExport_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                var calendar = (ICalendar)calendars.SelectedItem;

                var name = $"calendar_{calendar.Name}.xlsx";
                saveHolidaysDialog.FileName = name;
                var res = saveHolidaysDialog.ShowDialog();
                if (res != DialogResult.OK)
                {
                    return;
                }

                Cursor = Cursors.WaitCursor;

                var file = saveHolidaysDialog.FileName;
                var saveFormat = CalendarExporter.SaveFormat.Txt;
                var extension = (Path.GetExtension(file) ?? string.Empty).ToLowerInvariant();
                if (extension == ".xlsx") saveFormat = CalendarExporter.SaveFormat.Xlsx;
                else if (extension == ".csv") saveFormat = CalendarExporter.SaveFormat.Csv;

                CalendarExporter.Export(calendar, saveFormat, file);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), @"Erro não Antecipado", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void LinkSaveDates_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                var calendar = (ICalendar)calendars.SelectedItem;

                var name = $"dias_entre_{termStartDate.Value:yyyy-MM-dd}_e_{termEndDate.Value:yyyy-MM-dd}_{calendar.Name}.xlsx";
                saveDatesDialog.FileName = name;

                var res = saveDatesDialog.ShowDialog();
                if (res != DialogResult.OK)
                {
                    return;
                }

                Cursor = Cursors.WaitCursor;

                var file = saveDatesDialog.FileName;
                var saveFormat = CalendarExporter.SaveFormat.Txt;
                var extension = (Path.GetExtension(file) ?? string.Empty).ToLowerInvariant();
                if (extension == ".xlsx") saveFormat = CalendarExporter.SaveFormat.Xlsx;
                else if (extension == ".csv") saveFormat = CalendarExporter.SaveFormat.Csv;

                CalendarExporter.Export(calendar, saveFormat, file, termStartDate.Value, termEndDate.Value);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), @"Erro não Antecipado", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void LinkConfig_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                var path = ExternalCalendarsPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Calendars");
                if (!Directory.Exists(path))
                {
                    throw new ApplicationException($"O diretório contendo os calendários, '{path}', não foi localizado. Você o moveu? " +
                                                   "Reinstale a aplicação para que ele seja novamente criado.");
                }
                var readmeFile = Path.Combine(path, "LeiaMe.txt");
                if (File.Exists(readmeFile))
                {
                    path = readmeFile;
                }

                Process.Start("explorer.exe", "/select, \"" + path + "\"");
            }
            catch (ApplicationException ex)
            {
                MessageBox.Show(ex.Message, @"Não localizado!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), @"Erro não Antecipado", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LinkAbout_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var about = new AboutForm();
            about.ShowDialog();
        }
    }
}
