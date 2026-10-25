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
using CalculadoraDiasUteis.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.System;

namespace CalculadoraDiasUteis;

/// <summary>
///     Versão, licença, privacidade e os links para o site da Elekto e o código-fonte.
/// </summary>
public sealed partial class AboutPage : Page
{
    public AboutPage()
    {
        InitializeComponent();
    }

    /// <summary>
    ///     A versão do pacote instalado.
    /// </summary>
    public string Version
    {
        get
        {
            var version = Package.Current.Id.Version;
            return Strings.Format("AboutVersion", $"{version.Major}.{version.Minor}.{version.Build}");
        }
    }

    private async void LicenseButton_Click(object sender, RoutedEventArgs e)
    {
        await OpenPackagedFileAsync("ms-appx:///Licenses/license.txt");
    }

    private async void LicenseTranslationButton_Click(object sender, RoutedEventArgs e)
    {
        await OpenPackagedFileAsync("ms-appx:///Licenses/licenca.html");
    }

    /// <summary>
    ///     Abre um arquivo do pacote no aplicativo que o Windows associa ao tipo dele.
    /// </summary>
    private async Task OpenPackagedFileAsync(string uri)
    {
        OpenErrorInfoBar.IsOpen = false;
        try
        {
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(uri));
            if (await Launcher.LaunchFileAsync(file))
            {
                return;
            }

            OpenErrorInfoBar.Message = Strings.Format("ErrorOpenFile", file.Path);
        }
        catch (Exception ex)
        {
            OpenErrorInfoBar.Message = ex.Message;
        }

        OpenErrorInfoBar.IsOpen = true;
    }
}
