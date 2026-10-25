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
using System.ComponentModel;
using CalculadoraDiasUteis.Services;
using CalculadoraDiasUteis.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;
using Windows.Globalization.NumberFormatting;

namespace CalculadoraDiasUteis;

/// <summary>
///     A página da calculadora.
/// </summary>
public sealed partial class MainPage : Page
{
    public MainPage()
    {
        InitializeComponent();

        // O prazo é inteiro, com separador de milhar no formato do idioma
        TermNumberBox.NumberFormatter = new DecimalFormatter
        {
            FractionDigits = 0,
            IsGrouped = true,
            NumberRounder = new IncrementNumberRounder
            {
                Increment = 1,
                RoundingAlgorithm = RoundingAlgorithm.RoundHalfAwayFromZero
            }
        };

        ViewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    public MainPageViewModel ViewModel { get; } = new();

    public static bool Not(bool value) => !value;

    public static Visibility CollapsedIf(bool value) => value ? Visibility.Collapsed : Visibility.Visible;

    public static string Join(string value, string caption) =>
        string.IsNullOrEmpty(value) ? string.Empty : Strings.Format("ValueWithCaption", value, caption);

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Os resultados mudam sem que o foco vá até eles; o leitor de tela é avisado do resultado novo
        switch (e.PropertyName)
        {
            case nameof(MainPageViewModel.PeriodDescription):
                Announce(WorkDaysText, ViewModel.PeriodDescription, "PeriodResult");
                break;
            case nameof(MainPageViewModel.FinalDateDescription):
                Announce(FinalDateText, ViewModel.FinalDateDescription, "TermResult");
                break;
        }
    }

    private static void Announce(UIElement element, string text, string activityId)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        var peer = FrameworkElementAutomationPeer.FromElement(element) ??
                   FrameworkElementAutomationPeer.CreatePeerForElement(element);
        peer?.RaiseNotificationEvent(AutomationNotificationKind.ActionCompleted,
            AutomationNotificationProcessing.MostRecent, text, activityId);
    }

    private void AboutButton_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(AboutPage));
    }
}
