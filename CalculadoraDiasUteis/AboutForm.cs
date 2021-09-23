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
using System.Windows.Forms;

namespace Elekto
{
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();
        }

        private void AboutForm_Load(object sender, EventArgs e)
        {
            const string text =
                "Calculadora de Dias Úteis, Copyright © 2021 by Elekto Produtos Financeiros\r\n\r\nAplicação livre para o cálculo de prazos e datas finais" +
                " considerando os dias úteis de calendários.\r\n\r\n" +
                "Essa aplicação é livre! O código fonte está disponível, e se você pagou por este aplicativo, peça seu dinheiro de volta, posto que a aplicação" +
                " pode ser baixada gratuitamente a partir de nosso site em https://elekto.com.br\r\n\r\n" +
                "Esta aplicação usa a licença GPLv3, o que implica que você é livre para usar como quiser, mas nenhuma garantia, de espécie alguma é data, " +
                "pelo uso correto ou não desse aplicativo. Nenhum prejuízo, de qualquer natureza, será atribuído a Elekto Produtos Financeiros.\r\n" +
                "A licença usada permite que você incorpore o código em seus aplicativos, desde que você o licencie pela mesma licença. " +
                "Na raiz dessa aplicação você pode ler o arquivo de licença completo no original em Inglês e numa tradução para português.\r\n\r\n" +
                "Se você tem a necessidade de incorporar essa aplicação, na forma de binário, ou fonte, em software proprietário, procure a Elekto " +
                "para negociar um licenciamento alternativo."; 
            
            textBoxAbout.Text = text;
        }

        private void LinkSite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                var sInfo = new ProcessStartInfo("https://elekto.com.br/");
                Process.Start(sInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), @"Erro não Antecipado", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
