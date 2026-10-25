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
using System.Globalization;
using Microsoft.UI.Xaml;
using Windows.Globalization;

namespace CalculadoraDiasUteis;

/// <summary>
///     O aplicativo: cria e mostra a janela principal.
/// </summary>
public partial class App : Application
{
    /// <summary>
    ///     A janela principal, para quem precisar dela (diálogos, seletores de arquivo, interoperabilidade).
    /// </summary>
    public static Window Window { get; private set; } = null!;

    /// <summary>
    ///     O formato regional do Windows, com as personalizações do usuário (separador de listas, data curta). É com ele
    ///     que o Excel lê um CSV ou um texto colado, e por isso é o dos arquivos CSV, qualquer que seja o idioma do app.
    /// </summary>
    public static CultureInfo RegionalCulture { get; private set; } = CultureInfo.CurrentCulture;

    public App()
    {
        InitializeComponent();

        // Antes de a cultura passar a ser a do idioma do aplicativo, logo abaixo
        RegionalCulture = CultureInfo.CurrentCulture;

        // Datas e números formatados pelo .NET seguem o idioma que o Windows escolheu para o aplicativo, o mesmo
        // dos textos e dos controles do WinUI; sem isso, num Windows em inglês, a data final sairia como 10/27/2027
        // ao lado de seletores de data em 27/10/2027
        var language = ApplicationLanguages.Languages.FirstOrDefault();
        if (!string.IsNullOrEmpty(language))
        {
            var culture = new CultureInfo(language);
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = culture;
            CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = culture;
        }
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        Window = new MainWindow();
        Window.Activate();
    }
}
