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
using Microsoft.UI.Windowing;
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

    // Menor tamanho em que os controles ainda cabem, com os cartões um embaixo do outro; a altura que faltar rola
    private const int MinimumWidth = 500;
    private const int MinimumHeight = 400;

    private double _scale;

    public MainWindow()
    {
        InitializeComponent();

        Title = Strings.Get("AppDisplayName");
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        AppWindow.SetIcon("Assets/AppIcon.ico");

        // AppWindow.Resize e os limites do OverlappedPresenter usam pixels físicos
        _scale = GetScale();
        AppWindow.Resize(new SizeInt32((int)(InitialWidth * _scale), (int)(InitialHeight * _scale)));
        ApplyMinimumSize();
        AppWindow.Changed += AppWindow_Changed;

        RootFrame.Navigate(typeof(MainPage));
    }

    [DllImport("user32.dll")]
    private static extern uint GetDpiForWindow(IntPtr hWnd);

    private double GetScale()
    {
        return GetDpiForWindow(Win32Interop.GetWindowFromWindowId(AppWindow.Id)) / 96.0;
    }

    private void ApplyMinimumSize()
    {
        if (AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.PreferredMinimumWidth = (int)(MinimumWidth * _scale);
            presenter.PreferredMinimumHeight = (int)(MinimumHeight * _scale);
        }
    }

    // Num monitor com outra escala, o mínimo em pixels físicos precisa ser refeito
    private void AppWindow_Changed(AppWindow sender, AppWindowChangedEventArgs args)
    {
        if (!args.DidPositionChange && !args.DidSizeChange)
        {
            return;
        }

        var scale = GetScale();
        if (scale != _scale)
        {
            _scale = scale;
            ApplyMinimumSize();
        }
    }

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
