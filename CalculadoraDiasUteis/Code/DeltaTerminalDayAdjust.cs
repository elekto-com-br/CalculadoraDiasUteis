/*
    Calculadora de Dias �teis - Calcula Prazos e Datas Finais considerando os feriados
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
    /// Enumera os m�todos de ajuste a serem aplicados quando do calculo de prazo 
    /// e alguma das datas terminais n�o � dia �til
    /// </summary>
    internal enum DeltaTerminalDayAdjust
    {
        /// <summary>
        /// Retorna valores similares ao Excel com NetWorkDays, inclui primeiro e ultimo se �teis.
        /// </summary>
        Full,

        /// <summary>
        /// Padr�o financeiro, inclui primeiro (se util), e exclui ultimo dia;
        /// Retorna valores em acordo com a circular do Banco Central 2.456/1994 (28 de Julho de 1994), Artigo quarto, que diz:
        /// 'PARA  EFEITO DA  APLICA��O DO CRIT�RIO PRO-RATA DIA  �TIL PREVISTO NOS ARTS. 2o E 3o, 
        /// A CONTAGEM DO N�MERO DE DIAS �TEIS ENTRE DUAS DATAS INCLUIR� A PRIMEIRA E EXCLUIR� A �LTIMA.'
        /// </summary>
        /// <see ref="http://www5.bcb.gov.br/normativos/detalhamentocorreio.asp?N=094140457&amp;C=2456&amp;ASS=CIRCULAR+2.456"/>
        Financial,

        /// <summary>
        /// Caso a data final n�o seja dia �til, leva para o dia �til anterior
        /// </summary>
        EndOnPrevWork,

        /// <summary>
        /// Caso a data final n�o seja dia �til, leva para o dia �til seguinte
        /// </summary>
        EndOnNextWork,

        /// <summary>
        /// Caso a data inicial n�o seja dia �til, leva para o dia �til anterior
        /// </summary>
        StartOnPrevWork,

        /// <summary>
        /// Caso a data inicial n�o seja dia �til, leva para o dia �til seguinte
        /// </summary>
        StartOnNextWork,

        /// <summary>
        /// Caso a data inicial e/ou final n�o seja dia �til, leva para o dia �til anterior
        /// </summary>
        StartAndEndOnPrev,

        /// <summary>
        /// Caso a data inicial e/ou final n�o seja dia �til, leva para o dia �til seguinte
        /// </summary>
        StartAndEndOnNext,

        /// <summary>
        /// Caso a data inicial e/ou final n�o seja dia �til, leva a data inicial para o util anterior
        /// e o a data final para o util posterior
        /// </summary>
        StartAndEndExpanding,

        /// <summary>
        /// Caso a data inicial e/ou final n�o seja dia �til, leva a data inicial para o util posterior
        /// e o a data final para o util anterior
        /// </summary>
        StartAndEndCollapsing,

        /// <summary>
        /// N�o adianta e nem atrasa em caso de dia n�o util
        /// </summary>
        None
    }
}