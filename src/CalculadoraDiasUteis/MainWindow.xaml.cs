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
using System.Runtime.InteropServices;
using CalculadoraDiasUteis.Services;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.Graphics;

namespace CalculadoraDiasUteis;

/// <summary>
///     A janela do aplicativo: a barra de título e o quadro onde as páginas são mostradas.
/// </summary>
public sealed partial class MainWindow : Window
{
    // Tamanho inicial, em pixels independentes de resolução, para os dois cartões caberem lado a lado
    private const int InitialWidth = 960;
    private const int InitialHeight = 600;

    public MainWindow()
    {
        InitializeComponent();

        Title = Strings.Get("AppDisplayName");
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        AppWindow.SetIcon("Assets/AppIcon.ico");

        // AppWindow.Resize usa pixels físicos
        var scale = GetDpiForWindow(Win32Interop.GetWindowFromWindowId(AppWindow.Id)) / 96.0;
        AppWindow.Resize(new SizeInt32((int)(InitialWidth * scale), (int)(InitialHeight * scale)));

        RootFrame.Navigate(typeof(MainPage));
    }

    [DllImport("user32.dll")]
    private static extern uint GetDpiForWindow(IntPtr hWnd);

    private void AppTitleBar_BackRequested(TitleBar sender, object args)
    {
        GoBack();
    }

    private void BackAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        args.Handled = GoBack();
    }

    private bool GoBack()
    {
        if (!RootFrame.CanGoBack)
        {
            return false;
        }

        RootFrame.GoBack();
        return true;
    }
}
