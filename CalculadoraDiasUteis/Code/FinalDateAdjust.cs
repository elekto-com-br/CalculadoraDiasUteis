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
namespace Elekto.Code
{
    /// <summary>
    /// Métodos para calcular a data final de um prazo
    /// </summary>
    internal enum FinalDateAdjust
    {
        /// <summary>
        /// Se terminar em dia não util coloca no dia util seguinte.
        /// </summary>
        Following = 1,

        /// <summary>
        /// Se terminar em dia não util coloca no dia util seguinte, a não ser que isso faça
        /// o mês trocar, neste caso passa a ser o dia util anterior
        /// </summary>
        ModifiedFollowing = 2,

        /// <summary>
        /// Nenhum ajuste
        /// </summary>
        None = 0
    }

    
}