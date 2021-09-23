
namespace Elekto
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.buttonCalculateTerm = new System.Windows.Forms.Button();
            this.linkSaveDates = new System.Windows.Forms.LinkLabel();
            this.label6 = new System.Windows.Forms.Label();
            this.actualDays = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.workDays = new System.Windows.Forms.TextBox();
            this.radioFullDays = new System.Windows.Forms.RadioButton();
            this.radioFinancial = new System.Windows.Forms.RadioButton();
            this.label3 = new System.Windows.Forms.Label();
            this.termEndDate = new System.Windows.Forms.DateTimePicker();
            this.label2 = new System.Windows.Forms.Label();
            this.termStartDate = new System.Windows.Forms.DateTimePicker();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.buttonCalculateFinal = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.radioWorkDays = new System.Windows.Forms.RadioButton();
            this.radioActualDays = new System.Windows.Forms.RadioButton();
            this.radioAdjustNextModified = new System.Windows.Forms.RadioButton();
            this.radioAdjustNext = new System.Windows.Forms.RadioButton();
            this.radioAdjustNone = new System.Windows.Forms.RadioButton();
            this.label12 = new System.Windows.Forms.Label();
            this.finalDate = new System.Windows.Forms.TextBox();
            this.term = new System.Windows.Forms.NumericUpDown();
            this.label9 = new System.Windows.Forms.Label();
            this.startDate = new System.Windows.Forms.DateTimePicker();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.saveDatesDialog = new System.Windows.Forms.SaveFileDialog();
            this.saveHolidaysDialog = new System.Windows.Forms.SaveFileDialog();
            this.calendars = new System.Windows.Forms.ComboBox();
            this.linkExport = new System.Windows.Forms.LinkLabel();
            this.linkConfig = new System.Windows.Forms.LinkLabel();
            this.linkAbout = new System.Windows.Forms.LinkLabel();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.term)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.buttonCalculateTerm);
            this.groupBox1.Controls.Add(this.linkSaveDates);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.actualDays);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.workDays);
            this.groupBox1.Controls.Add(this.radioFullDays);
            this.groupBox1.Controls.Add(this.radioFinancial);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.termEndDate);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.termStartDate);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(13, 40);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(395, 119);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Cálculo de Prazo";
            // 
            // buttonCalculateTerm
            // 
            this.buttonCalculateTerm.Location = new System.Drawing.Point(9, 66);
            this.buttonCalculateTerm.Name = "buttonCalculateTerm";
            this.buttonCalculateTerm.Size = new System.Drawing.Size(78, 23);
            this.buttonCalculateTerm.TabIndex = 13;
            this.buttonCalculateTerm.Text = "&Prazos";
            this.buttonCalculateTerm.UseVisualStyleBackColor = true;
            this.buttonCalculateTerm.Click += new System.EventHandler(this.ButtonCalculateTerm_Click);
            // 
            // linkSaveDates
            // 
            this.linkSaveDates.AutoSize = true;
            this.linkSaveDates.LinkColor = System.Drawing.SystemColors.MenuText;
            this.linkSaveDates.Location = new System.Drawing.Point(6, 95);
            this.linkSaveDates.Name = "linkSaveDates";
            this.linkSaveDates.Size = new System.Drawing.Size(124, 13);
            this.linkSaveDates.TabIndex = 12;
            this.linkSaveDates.TabStop = true;
            this.linkSaveDates.Text = "Salvar Datas do Período";
            this.linkSaveDates.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkSaveDates_LinkClicked);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(315, 71);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(66, 13);
            this.label6.TabIndex = 11;
            this.label6.Text = "dias corridos";
            // 
            // actualDays
            // 
            this.actualDays.Location = new System.Drawing.Point(238, 68);
            this.actualDays.Name = "actualDays";
            this.actualDays.ReadOnly = true;
            this.actualDays.Size = new System.Drawing.Size(71, 20);
            this.actualDays.TabIndex = 10;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(167, 71);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(51, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "dias úteis";
            // 
            // workDays
            // 
            this.workDays.Location = new System.Drawing.Point(93, 68);
            this.workDays.Name = "workDays";
            this.workDays.ReadOnly = true;
            this.workDays.Size = new System.Drawing.Size(71, 20);
            this.workDays.TabIndex = 8;
            // 
            // radioFullDays
            // 
            this.radioFullDays.AutoSize = true;
            this.radioFullDays.Location = new System.Drawing.Point(170, 45);
            this.radioFullDays.Name = "radioFullDays";
            this.radioFullDays.Size = new System.Drawing.Size(78, 17);
            this.radioFullDays.TabIndex = 6;
            this.radioFullDays.TabStop = true;
            this.radioFullDays.Text = "dias c&heios";
            this.radioFullDays.UseVisualStyleBackColor = true;
            this.radioFullDays.CheckedChanged += new System.EventHandler(this.RadioFullDays_CheckedChanged);
            // 
            // radioFinancial
            // 
            this.radioFinancial.AutoSize = true;
            this.radioFinancial.Location = new System.Drawing.Point(93, 45);
            this.radioFinancial.Name = "radioFinancial";
            this.radioFinancial.Size = new System.Drawing.Size(71, 17);
            this.radioFinancial.TabIndex = 5;
            this.radioFinancial.TabStop = true;
            this.radioFinancial.Text = "fi&nanceiro";
            this.radioFinancial.UseVisualStyleBackColor = true;
            this.radioFinancial.CheckedChanged += new System.EventHandler(this.RadioFinancial_CheckedChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 47);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(81, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Tipo de Cálculo";
            // 
            // termEndDate
            // 
            this.termEndDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.termEndDate.Location = new System.Drawing.Point(271, 17);
            this.termEndDate.Name = "termEndDate";
            this.termEndDate.Size = new System.Drawing.Size(110, 20);
            this.termEndDate.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(210, 23);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Data &Final";
            // 
            // termStartDate
            // 
            this.termStartDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.termStartDate.Location = new System.Drawing.Point(93, 19);
            this.termStartDate.Name = "termStartDate";
            this.termStartDate.Size = new System.Drawing.Size(110, 20);
            this.termStartDate.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(27, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Data &Inicial";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.buttonCalculateFinal);
            this.groupBox2.Controls.Add(this.panel1);
            this.groupBox2.Controls.Add(this.radioAdjustNextModified);
            this.groupBox2.Controls.Add(this.radioAdjustNext);
            this.groupBox2.Controls.Add(this.radioAdjustNone);
            this.groupBox2.Controls.Add(this.label12);
            this.groupBox2.Controls.Add(this.finalDate);
            this.groupBox2.Controls.Add(this.term);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.startDate);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Location = new System.Drawing.Point(13, 166);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(395, 123);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Cálculo de Data Final";
            // 
            // buttonCalculateFinal
            // 
            this.buttonCalculateFinal.Location = new System.Drawing.Point(9, 89);
            this.buttonCalculateFinal.Name = "buttonCalculateFinal";
            this.buttonCalculateFinal.Size = new System.Drawing.Size(78, 23);
            this.buttonCalculateFinal.TabIndex = 14;
            this.buttonCalculateFinal.Text = "D&ata Final";
            this.buttonCalculateFinal.UseVisualStyleBackColor = true;
            this.buttonCalculateFinal.Click += new System.EventHandler(this.ButtonCalculateFinal_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.radioWorkDays);
            this.panel1.Controls.Add(this.radioActualDays);
            this.panel1.Location = new System.Drawing.Point(93, 47);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(193, 20);
            this.panel1.TabIndex = 7;
            // 
            // radioWorkDays
            // 
            this.radioWorkDays.AutoSize = true;
            this.radioWorkDays.Location = new System.Drawing.Point(0, 0);
            this.radioWorkDays.Name = "radioWorkDays";
            this.radioWorkDays.Size = new System.Drawing.Size(69, 17);
            this.radioWorkDays.TabIndex = 5;
            this.radioWorkDays.TabStop = true;
            this.radioWorkDays.Text = "dias ú&teis";
            this.radioWorkDays.UseVisualStyleBackColor = true;
            this.radioWorkDays.CheckedChanged += new System.EventHandler(this.radioWorkDays_CheckedChanged);
            // 
            // radioActualDays
            // 
            this.radioActualDays.AutoSize = true;
            this.radioActualDays.Location = new System.Drawing.Point(77, 0);
            this.radioActualDays.Name = "radioActualDays";
            this.radioActualDays.Size = new System.Drawing.Size(84, 17);
            this.radioActualDays.TabIndex = 6;
            this.radioActualDays.TabStop = true;
            this.radioActualDays.Text = "dias c&orridos";
            this.radioActualDays.UseVisualStyleBackColor = true;
            this.radioActualDays.CheckedChanged += new System.EventHandler(this.RadioActualDays_CheckedChanged);
            // 
            // radioAdjustNextModified
            // 
            this.radioAdjustNextModified.AutoSize = true;
            this.radioAdjustNextModified.Location = new System.Drawing.Point(257, 68);
            this.radioAdjustNextModified.Name = "radioAdjustNextModified";
            this.radioAdjustNextModified.Size = new System.Drawing.Size(92, 17);
            this.radioAdjustNextModified.TabIndex = 10;
            this.radioAdjustNextModified.TabStop = true;
            this.radioAdjustNextModified.Text = "útil &modificado";
            this.radioAdjustNextModified.UseVisualStyleBackColor = true;
            this.radioAdjustNextModified.CheckedChanged += new System.EventHandler(this.RadioAdjustNextModified_CheckedChanged);
            // 
            // radioAdjustNext
            // 
            this.radioAdjustNext.AutoSize = true;
            this.radioAdjustNext.Location = new System.Drawing.Point(170, 68);
            this.radioAdjustNext.Name = "radioAdjustNext";
            this.radioAdjustNext.Size = new System.Drawing.Size(81, 17);
            this.radioAdjustNext.TabIndex = 9;
            this.radioAdjustNext.TabStop = true;
            this.radioAdjustNext.Text = "útil &seguinte";
            this.radioAdjustNext.UseVisualStyleBackColor = true;
            this.radioAdjustNext.CheckedChanged += new System.EventHandler(this.RadioAdjustNext_CheckedChanged);
            // 
            // radioAdjustNone
            // 
            this.radioAdjustNone.AutoSize = true;
            this.radioAdjustNone.Location = new System.Drawing.Point(93, 68);
            this.radioAdjustNone.Name = "radioAdjustNone";
            this.radioAdjustNone.Size = new System.Drawing.Size(63, 17);
            this.radioAdjustNone.TabIndex = 8;
            this.radioAdjustNone.TabStop = true;
            this.radioAdjustNone.Text = "&nenhum";
            this.radioAdjustNone.UseVisualStyleBackColor = true;
            this.radioAdjustNone.CheckedChanged += new System.EventHandler(this.RadioAdjustNone_CheckedChanged);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(48, 70);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(36, 13);
            this.label12.TabIndex = 7;
            this.label12.Text = "A&juste";
            // 
            // finalDate
            // 
            this.finalDate.Location = new System.Drawing.Point(93, 91);
            this.finalDate.Name = "finalDate";
            this.finalDate.ReadOnly = true;
            this.finalDate.Size = new System.Drawing.Size(110, 20);
            this.finalDate.TabIndex = 12;
            // 
            // term
            // 
            this.term.Location = new System.Drawing.Point(271, 19);
            this.term.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.term.Minimum = new decimal(new int[] {
            100000,
            0,
            0,
            -2147483648});
            this.term.Name = "term";
            this.term.Size = new System.Drawing.Size(110, 20);
            this.term.TabIndex = 3;
            this.term.ThousandsSeparator = true;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(226, 24);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(39, 13);
            this.label9.TabIndex = 2;
            this.label9.Text = "&Prazos";
            // 
            // startDate
            // 
            this.startDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.startDate.Location = new System.Drawing.Point(93, 19);
            this.startDate.Name = "startDate";
            this.startDate.Size = new System.Drawing.Size(110, 20);
            this.startDate.TabIndex = 1;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(14, 47);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(73, 13);
            this.label8.TabIndex = 4;
            this.label8.Text = "Tipo de Prazo";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(27, 24);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(60, 13);
            this.label7.TabIndex = 0;
            this.label7.Text = "&Data Inicial";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(13, 13);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(57, 13);
            this.label11.TabIndex = 0;
            this.label11.Text = "&Calendário";
            // 
            // saveDatesDialog
            // 
            this.saveDatesDialog.Filter = "Planilha Excel|*.xlsx|Arquivo Texto|*.txt|Arquivo CSV|*.csv";
            this.saveDatesDialog.Title = "Salvar Datas do Período";
            // 
            // saveHolidaysDialog
            // 
            this.saveHolidaysDialog.Filter = "Planilha Excel|*.xlsx|Arquivo Texto|*.txt|Arquivo CSV|*.csv";
            this.saveHolidaysDialog.Title = "Salvar feriados";
            // 
            // calendars
            // 
            this.calendars.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.calendars.FormattingEnabled = true;
            this.calendars.Location = new System.Drawing.Point(76, 10);
            this.calendars.Name = "calendars";
            this.calendars.Size = new System.Drawing.Size(280, 21);
            this.calendars.TabIndex = 1;
            this.calendars.SelectedValueChanged += new System.EventHandler(this.Calendars_SelectedValueChanged);
            // 
            // linkExport
            // 
            this.linkExport.AutoSize = true;
            this.linkExport.LinkColor = System.Drawing.SystemColors.MenuText;
            this.linkExport.Location = new System.Drawing.Point(362, 13);
            this.linkExport.Name = "linkExport";
            this.linkExport.Size = new System.Drawing.Size(46, 13);
            this.linkExport.TabIndex = 2;
            this.linkExport.TabStop = true;
            this.linkExport.Text = "Exportar";
            this.linkExport.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkExport_LinkClicked);
            // 
            // linkConfig
            // 
            this.linkConfig.AutoSize = true;
            this.linkConfig.LinkColor = System.Drawing.SystemColors.MenuText;
            this.linkConfig.Location = new System.Drawing.Point(10, 295);
            this.linkConfig.Name = "linkConfig";
            this.linkConfig.Size = new System.Drawing.Size(139, 13);
            this.linkConfig.TabIndex = 5;
            this.linkConfig.TabStop = true;
            this.linkConfig.Text = "Definições de Calendários...";
            this.linkConfig.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkConfig_LinkClicked);
            // 
            // linkAbout
            // 
            this.linkAbout.AutoSize = true;
            this.linkAbout.LinkColor = System.Drawing.SystemColors.MenuText;
            this.linkAbout.Location = new System.Drawing.Point(289, 295);
            this.linkAbout.Name = "linkAbout";
            this.linkAbout.Size = new System.Drawing.Size(119, 13);
            this.linkAbout.TabIndex = 6;
            this.linkAbout.TabStop = true;
            this.linkAbout.Text = "Sobre essa Aplicação...";
            this.linkAbout.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkAbout_LinkClicked);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(419, 320);
            this.Controls.Add(this.linkAbout);
            this.Controls.Add(this.linkConfig);
            this.Controls.Add(this.linkExport);
            this.Controls.Add(this.calendars);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "Cálculadora de Dias Úteis - Elekto";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.term)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.DateTimePicker termStartDate;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.LinkLabel linkSaveDates;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox actualDays;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox workDays;
        private System.Windows.Forms.RadioButton radioFullDays;
        private System.Windows.Forms.RadioButton radioFinancial;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.DateTimePicker termEndDate;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.NumericUpDown term;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.RadioButton radioActualDays;
        private System.Windows.Forms.DateTimePicker startDate;
        private System.Windows.Forms.RadioButton radioWorkDays;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.SaveFileDialog saveDatesDialog;
        private System.Windows.Forms.SaveFileDialog saveHolidaysDialog;
        private System.Windows.Forms.ComboBox calendars;
        private System.Windows.Forms.LinkLabel linkExport;
        private System.Windows.Forms.TextBox finalDate;
        private System.Windows.Forms.LinkLabel linkConfig;
        private System.Windows.Forms.LinkLabel linkAbout;
        private System.Windows.Forms.RadioButton radioAdjustNextModified;
        private System.Windows.Forms.RadioButton radioAdjustNext;
        private System.Windows.Forms.RadioButton radioAdjustNone;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button buttonCalculateTerm;
        private System.Windows.Forms.Button buttonCalculateFinal;
    }
}

