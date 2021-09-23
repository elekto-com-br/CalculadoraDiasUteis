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
    /// Enumera os métodos de ajuste a serem aplicados quando do calculo de prazo 
    /// e alguma das datas terminais não é dia útil
    /// </summary>
    internal enum DeltaTerminalDayAdjust
    {
        /// <summary>
        /// Retorna valores similares ao Excel com NetWorkDays, inclui primeiro e ultimo se úteis.
        /// </summary>
        Full,

        /// <summary>
        /// Padrão financeiro, inclui primeiro (se util), e exclui ultimo dia;
        /// Retorna valores em acordo com a circular do Banco Central 2.456/1994 (28 de Julho de 1994), Artigo quarto, que diz:
        /// 'PARA  EFEITO DA  APLICAÇÃO DO CRITÉRIO PRO-RATA DIA  ÚTIL PREVISTO NOS ARTS. 2o E 3o, 
        /// A CONTAGEM DO NÚMERO DE DIAS ÚTEIS ENTRE DUAS DATAS INCLUIRÁ A PRIMEIRA E EXCLUIRÁ A ÚLTIMA.'
        /// </summary>
        /// <see ref="http://www5.bcb.gov.br/normativos/detalhamentocorreio.asp?N=094140457&amp;C=2456&amp;ASS=CIRCULAR+2.456"/>
        Financial,

        /// <summary>
        /// Caso a data final não seja dia útil, leva para o dia útil anterior
        /// </summary>
        EndOnPrevWork,

        /// <summary>
        /// Caso a data final não seja dia útil, leva para o dia útil seguinte
        /// </summary>
        EndOnNextWork,

        /// <summary>
        /// Caso a data inicial não seja dia útil, leva para o dia útil anterior
        /// </summary>
        StartOnPrevWork,

        /// <summary>
        /// Caso a data inicial não seja dia útil, leva para o dia útil seguinte
        /// </summary>
        StartOnNextWork,

        /// <summary>
        /// Caso a data inicial e/ou final não seja dia útil, leva para o dia útil anterior
        /// </summary>
        StartAndEndOnPrev,

        /// <summary>
        /// Caso a data inicial e/ou final não seja dia útil, leva para o dia útil seguinte
        /// </summary>
        StartAndEndOnNext,

        /// <summary>
        /// Caso a data inicial e/ou final não seja dia útil, leva a data inicial para o util anterior
        /// e o a data final para o util posterior
        /// </summary>
        StartAndEndExpanding,

        /// <summary>
        /// Caso a data inicial e/ou final não seja dia útil, leva a data inicial para o util posterior
        /// e o a data final para o util anterior
        /// </summary>
        StartAndEndCollapsing,

        /// <summary>
        /// Não adianta e nem atrasa em caso de dia não util
        /// </summary>
        None
    }
}