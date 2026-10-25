# Uruguai, Banco Central: regras, exceções e fontes do calendário uy-MVD

Arquivo de datas: `src/Elekto.Calendars/Calendars/0070.calendar.uy-MVD.txt`, com 465 datas de 01/01/2000 a 31/12/2027: 366 em dia útil e 99 em fim de semana. UTF-8 com BOM e CRLF, como os calendários do repositório. Descrições em espanhol, no padrão dos calendários do BCU. Ficaram fora 18 datas duvidosas, listadas mais abaixo.

## O que conta como dia sem expediente bancário

O BCU não publica uma lista anual de "días inhábiles bancarios". O critério sai destas normas e atos:

1. **RNRCSF, art. 28** (antes art. 31): as dependências de bancos com conta corrente nas capitais departamentais devem atender de segunda a sexta, no mínimo quatro horas por dia, "con excepción de los días feriados y del 31 de diciembre". A exceção vale para todos os feriados, *laborables* ou *no laborables*: na prática os bancos fecham em todos eles, e o BCU também não opera (a série de cotações, abaixo, confirma isso ano a ano).
2. **Comunicação BCU 2009/053**: segunda, terça e quarta da Semana de Turismo são hábeis "a los efectos bancocentralistas"; na quinta o BCU não realiza operações. A sexta é feriado. As comunicações anuais do Sistema Central de Liquidación (2010 a 2014) repetem: sistema habilitado de segunda a quarta, indisponível na quinta e na sexta.
3. **Resolução do BCU de 23/12/2003** (Circular 1.888 e Comunicação 2007/269): no 31 de dezembro o BCU não atende ao público nem realiza operações, para o fechamento de balanço, e o Sistema Central de Liquidación fica indisponível; os bancos ficam dispensados do horário mínimo.
4. **Fechamentos extraordinários** declarados por decreto (feriado bancário de 2002) ou documentados pelo BCU (dias de greve em que o BCU suspendeu a liquidação e a câmara de compensação).

Ficam de fora, por serem dias hábeis: 24 de dezembro (o BCU autoriza todo ano os bancos a reduzir o horário mínimo a duas horas, e o próprio BCU atende das 9h às 11h), os dias 23 e 30 de dezembro quando 24 e 31 caem no fim de semana (comunicações 2011/216 e 2022/219: horário normal), e segunda a quarta da Semana de Turismo.

## Como conferi

- **Série diária de cotações do BCU** (dólar interbancário "DLS. USA BILLETE", código 2225; cruzada com 2224, 1000 e 2222, que dão as mesmas lacunas), de 03/01/2000 a 23/10/2026, baixada do serviço da página oficial de cotações. Dia útil sem cotação é dia em que o BCU e o mercado interbancário não funcionaram. Todas as datas do arquivo em dia útil até 23/10/2026 batem com as lacunas da série, com duas exceções explicadas: o 31 de dezembro (o BCU publica a cotação de fechamento do ano mesmo sem operar; publica até em sábado e domingo, como em 31/12/2000, 2005, 2006, 2011, 2016 e 2017, e publicou em 31/12/2020, quando a Comunicação 2020/269 diz que não haveria operações) e o feriado bancário de 2002 (a cotação fica congelada em 25,025/25,125 de 29/07 a 02/08, isto é, sem mercado). As únicas lacunas que ficaram sem data no arquivo são as nove de 2008 a 2012 listadas como duvidosas.
- **Calendários de feriados publicados pelo BCU** na página antiga `autoriza/sgoioi/feriadosAAAA.html`, recuperados do Wayback Machine, para 2002 a 2010, e a **tabela interna `diasferiados.js`** do BCU (dias sem operação de 1994 a 2003, versão de fev/2004).
- **Comunicações do Área de Sistema de Pagos** sobre a atividade do Sistema Central de Liquidación e da Cámara Compensadora (Turismo, 24 e 31 de dezembro, dias de greve).
- **Leis e decretos** no IMPO, e as **comunicações e circulares** do BCU citadas.

Para repetir a conferência, `tools/conferir-series.ps1 uy-MVD` baixa a mesma série (moeda 2225) e lista os dias úteis sem cotação que não estão no arquivo e os feriados do arquivo com cotação; sem parâmetros, confere o ano corrente. De 2000 a 23/10/2026 aparecem só as nove greves duvidosas, os quatro dias do feriado bancário de 2002 (cotação congelada) e os 31 de dezembro com cotação de fechamento.

### Grau de verificação por período

| Período | Como foi montado | Confiança |
|---|---|---|
| 2000 a 2008 | Data a data contra a série do BCU, a tabela `diasferiados.js` (2000 a 2003) e os calendários do BCU (2002 a 2008). Leis e decretos para as exceções. | Alta. A dúvida real é a quinta-feira de Turismo, que ficou fora, e duas greves de 2008. |
| 2009 a 2026 (até 23/10/2026) | Data a data contra a série do BCU; calendários do BCU de 2009 e 2010; comunicações de Turismo (2009/053; 2010 a 2014; notícia do BCU de 2025), de 31/12 (2007 a 2025) e de greves (2015 a 2019). | Alta. Sete lacunas de 2010 a 2012 ficaram como duvidosas. |
| 24/10/2026 a 31/12/2027 | Por regra (02/11/2026, 25/12/2026, 31/12/2026 e todo 2027). Nem o BCU nem o IMPO publicam calendário futuro. | Alta para os feriados de lei; média-alta para 31/12/2026 e 31/12/2027, que dependem de o BCU repetir a comunicação anual (o faz desde 2003). |
| Fins de semana | Por regra (a série não distingue fim de semana). | Alta; não muda nenhuma conta. |

## Regras por período

Feriados fixos (observados no dia em que caem, inclusive fim de semana):

| Data | Descrição no arquivo | Anos | Base |
|---|---|---|---|
| 1/1 | Año Nuevo | todos | Ley 6.997 (1919); Ley 16.805, art. 2 |
| 6/1 | Día de Reyes | todos | idem |
| Páscoa −48 e −47 (segunda e terça) | Carnaval | todos | Ley 6.997, art. 3; Ley 16.805, art. 2 |
| Páscoa −3 (quinta) | Semana de Turismo | 2009 em diante | Comunicação BCU 2009/053 |
| Páscoa −2 (sexta) | Semana de Turismo | todos | Ley 6.997, art. 4; Ley 16.805, art. 2 |
| 1/5 | Día de los Trabajadores | todos | Ley 16.805, art. 2 |
| 19/6 | Natalicio de Artigas | 2002 em diante | Ley 17.414 (08/11/2001) |
| 18/7 | Jura de la Constitución | todos | Ley 16.805, art. 2 |
| 25/8 | Declaratoria de la Independencia | todos | idem |
| 2/11 | Día de los Difuntos | 2002 em diante | Ley 17.414 |
| 25/12 | Navidad | todos | Ley 16.805, art. 2 |
| 31/12 | Cierre bancario de fin de año | 2003 em diante | Resolução BCU 23/12/2003; Circular 1.888; RNRCSF art. 28 |
| 1/3 dos anos de posse presidencial (múltiplos de 5) | Transmisión del mando presidencial | 2000, 2005, 2010, 2015, 2020, 2025; próximo 2030 | Ley 7.556 (1923); Decreto 120/985 |

O 31/12 entra também quando cai em fim de semana, para a regra ficar simples; não muda nenhuma conta.

Feriados trasladáveis (Ley 16.805, art. 1): se caem em sábado, domingo ou segunda, ficam no dia; em terça ou quarta, passam para a segunda anterior; em quinta ou sexta, para a segunda seguinte. No arquivo, o dia trasladado leva o sufixo " (trasladado)".

| Data | Descrição | Anos em que se traslada |
|---|---|---|
| 19/4 | Desembarco de los 33 Orientales | todos |
| 18/5 | Batalla de las Piedras | todos, exceto 2011 (Ley 18.748: observado na quarta 18/05/2011) |
| 12/10 | Día de la Raza | todos |
| 19/6 | Natalicio de Artigas | só 2000 e 2001 (Ley 16.805 original) |
| 2/11 | Día de los Difuntos | só 2000 e 2001; em 2001 o traslado para 05/11 valeu porque a Ley 17.414 é de 08/11/2001 |

Observações sobre os traslados:

- Quando o 19 de abril cai na sexta da Semana de Turismo (2019, e de novo em 2030), ele vai para a segunda seguinte: em 2019 o BCU não operou em 22/04. Quando a segunda anterior é a segunda de Turismo (2000, 2011) ou a segunda depois da Páscoa (2006, 2017, 2022), o feriado cai nela, e a série do BCU confirma.
- A posse presidencial de 1º de março não segue o traslado: em 2000 (quarta) e 2005 (terça) o BCU fechou no próprio dia 1º, e não na segunda anterior. A Ley 7.556 vincula o feriado ao ato da posse.

## Exceções ano a ano

Feriados e fechamentos avulsos:

| Data | Descrição | Fonte |
|---|---|---|
| 23/09/2000 (sábado) | Sesquicentenario del fallecimiento de Artigas | Ley 17.262 (feriado nacional no laborable) |
| 30/07/2002 | Feriado bancario | Decreto 288/002 |
| 31/07, 01/08 e 02/08/2002 | Feriado bancario | Decreto 290/002 (prorroga por 72 horas; o BCU podia levantá-lo em parte e devia propor um funcionamento mínimo nos dias 1º e 2 para pagar salários e aposentadorias). Os bancos reabriram na segunda 05/08 |
| 10/09/2004 (sexta) | Centenario del fallecimiento de Aparicio Saravia | Ley 17.798 (feriado nacional no laborable) e Ley 17.824 |
| 18/05/2011 (quarta) | Batalla de las Piedras, sem traslado | Ley 18.748 (bicentenário) |

Sobre 01 e 02/08/2002: o decreto manteve o feriado bancário nesses dias, e a série do BCU segue congelada; o funcionamento parcial para pagamento de salários não muda a natureza de feriado bancário. Por isso entram no arquivo. A tabela interna do BCU não traz esses quatro dias, provavelmente porque foram decididos às pressas e a tabela não foi revista; o decreto prevalece.

Dias de greve sem operação no BCU, confirmados por documento do BCU (entram no arquivo):

| Data | Descrição no arquivo | Documento do BCU | Causa |
|---|---|---|---|
| 08/06/2000 | Paro general (sin operaciones en el BCU) | tabela `diasferiados.js` | 1º paro general do PIT-CNT contra o governo Batlle (Montevideo Portal) |
| 16/11/2000 | Sin operaciones en el BCU | tabela `diasferiados.js` | não identificada |
| 06/12/2000 | Paro general (sin operaciones en el BCU) | tabela `diasferiados.js` | 2º paro general do PIT-CNT (Montevideo Portal) |
| 11/07/2001 | Sin operaciones en el BCU | tabela `diasferiados.js` | não identificada |
| 25/07/2001 | Paro general (sin operaciones en el BCU) | tabela `diasferiados.js` | 3º paro general, 24 horas (Montevideo Portal) |
| 24/05/2002 | Paro general (sin operaciones en el BCU) | tabela `diasferiados.js` | paro general parcial; os bancários pararam o dia todo (Montevideo Portal) |
| 12/06/2002 | Paro general (sin operaciones en el BCU) | tabela `diasferiados.js` | 4º paro general, 24 horas, com adesão da AEBU; "los bancos no abrirán" (Montevideo Portal; La Jornada/Reuters) |
| 07/08/2002 | Paro general (sin operaciones en el BCU) | tabela `diasferiados.js` | paro general de 14 horas, que atingiu sobretudo os bancos (Emol/Reuters) |
| 19/12/2002 | Sin operaciones en el BCU | tabela `diasferiados.js` | não identificada |
| 17/06/2003 | Sin operaciones en el BCU | tabela `diasferiados.js` | não identificada |
| 28/08/2003 | Paro general (sin operaciones en el BCU) | tabela `diasferiados.js` | paro general de 24 horas (LatinNews; Página/12) |
| 06/08/2015 | Paro general (sin operaciones en el BCU) | Comunicação 2015/139 | paro general de 24 horas (LatinNews) |
| 14/07/2016 | Paro general (sin operaciones en el BCU) | Comunicação 2016/152 | paro general de 24 horas (IndustriALL) |
| 14/09/2016 | Sin operaciones en el BCU | Comunicação 2016/191 | não identificada |
| 20/07/2017 | Paro general (sin operaciones en el BCU) | Comunicação 2017/128 | paro general de 24 horas contra a Rendición de Cuentas (Crisis24) |
| 22/08/2018 | Paro general (sin operaciones en el BCU) | Comunicação 2018/159 | paro general nacional (Crisis24) |
| 25/06/2019 | Paro general (sin operaciones en el BCU) | Comunicação 2019/154 | paro general de 24 horas (La Izquierda Diario) |

As comunicações de 2015 a 2019 dizem que o Sistema Central de Liquidación "no estará disponible" e que o BCU suspende as sessões da Cámara Compensadora de Documentos naquela data. Nos dias de "causa não identificada" o BCU não operou, mas não achei notícia que diga se as agências bancárias abriram; como nenhuma liquidação interbancária nem compensação de cheques aconteceu, tratei-os como dias sem expediente bancário. Quem preferir o critério estrito "feriado por norma" pode tirar todas as linhas com "BCU" na descrição, exceto o "Cierre bancario de fin de año": elas são exatamente essas 17 datas.

## Datas duvidosas (fora do arquivo)

São 18: nove quintas-feiras de Turismo e nove dias de greve sem documento do BCU.

### Quinta-feira da Semana de Turismo, 2000 a 2008

20/04/2000, 12/04/2001, 28/03/2002, 17/04/2003, 08/04/2004, 24/03/2005, 13/04/2006, 05/04/2007 e 20/03/2008.

Motivo da dúvida: nesses anos o BCU publicou cotação na quinta (o mercado interbancário funcionou), e a tabela interna de dias sem operação do BCU não traz essas quintas. Os calendários de feriados do BCU, porém, mudaram de versão: as primeiras versões de 2002, 2003 e 2004 listavam a quinta como feriado, com nota de que o BCU funcionaria das 8h50 às 11h30; as versões corrigidas de 2003 (captura de 2004) e de 2004 (captura de maio/2004) e as de 2005 a 2008 listam só a sexta. A de 2002 nunca foi corrigida. Não achei circular, comunicação nem notícia da época dizendo se os bancos abriram ao público nessas quintas. A Comunicação 2009/053, de abril de 2009, é a primeira a dizer que o BCU não opera na quinta, o que sugere mudança de regime em 2009. Pelo critério "o BCU operou", essas datas são hábeis e o arquivo já está certo; pelo critério "a maioria dos bancos fechou", seria preciso uma fonte da AEBU ou da imprensa de 2000 a 2008.

### Greves de 2008 a 2012 sem documento do BCU

| Data | O que se sabe |
|---|---|
| 20/08/2008 (qua) | Sem cotação no BCU. Paro general de 24 horas, o primeiro contra o governo Vázquez; Página/12 diz que "permanecieron cerrados los bancos". É a mais provável de entrar, mas não achei comunicação do BCU nem da AEBU. |
| 03/09/2008 (qua) | Sem cotação no BCU; causa não encontrada. |
| 07/10/2010 (qui) | Sem cotação no BCU; causa não encontrada. |
| 04/11/2010 (qui) | Sem cotação no BCU; causa não encontrada. |
| 15/12/2010 (qua) | Sem cotação no BCU; causa não encontrada. |
| 16/08/2011 (ter) | Sem cotação no BCU; houve conflito da AEBU com a banca oficial entre set/2011 e jan/2012, sem confirmação desta data. |
| 06/12/2011 (ter) | Sem cotação no BCU; cai no mesmo conflito da banca oficial, sem confirmação. |
| 22/12/2011 (qui) | Sem cotação no BCU; assembleia geral do sindicato nas instituições financeiras públicas (AEBU banca oficial); aviso da Clearstream sobre atrasos na liquidação em pesos (hoje fora do ar). |
| 23/10/2012 (ter) | Sem cotação no BCU; causa não encontrada. |

Nenhuma delas tem comunicação do Área de Sistema de Pagos na lista de comunicações do BCU, que nesses anos só publicava as de Turismo e de fim de ano. A busca pela causa ficou incompleta por limite de buscas na web; vale retomar pelas cronologias do OSAL/CLACSO e pelo arquivo da AEBU.

### 31 de dezembro de 2001 e de 2002

Não são duvidosos, mas registro a decisão: entram como dias hábeis, porque a dispensa do 31 de dezembro só existe desde a resolução de 23/12/2003, e a tabela interna do BCU (que traz 31/12/2003) não traz 31/12/2001 nem 31/12/2002.

## Projeção para 2028 em diante

Gerar, para cada ano A:

1. Fixos: 1/1, 6/1, 1/5, 19/6, 18/7, 25/8, 2/11, 25/12 (Año Nuevo, Día de Reyes, Día de los Trabajadores, Natalicio de Artigas, Jura de la Constitución, Declaratoria de la Independencia, Día de los Difuntos, Navidad).
2. Pela Páscoa (P): P−48 e P−47 (Carnaval), P−3 e P−2 (Semana de Turismo).
3. Trasladáveis: 19/4, 18/5 e 12/10 pela regra da Ley 16.805 (terça ou quarta para a segunda anterior; quinta ou sexta para a segunda seguinte).
4. 31/12: Cierre bancario de fin de año.
5. Se A é múltiplo de 5 (2030, 2035, ...): 1/3, Transmisión del mando presidencial, no próprio dia.

O teste `CalendarioSegueAsRegrasDaPraca`, em `tests/Elekto.Calendars.Tests/CalendarRulesTests.cs`, implementa essas regras, com as exceções de 2000 a 2019, e confere o arquivo contra elas, data a data e descrição a descrição. Exemplo de 2028, dias úteis: 06/01 (qui), 28/02 e 29/02 (Carnaval), 13/04 e 14/04 (Turismo), 17/04 (Desembarco, trasladado), 01/05, 22/05 (Piedras, trasladado), 19/06, 18/07, 25/08, 16/10 (Raza, trasladado), 02/11, 25/12. Em 2030, 01/03 é sexta (posse) e o 19/04 cai na sexta de Turismo, indo para 22/04.

Riscos para os anos projetados:

- Leis avulsas que declaram feriado (como as de 2000 e 2004) ou que suspendem um traslado (como a de 2011). Candidatos óbvios são aniversários redondos de datas pátrias.
- O Decreto-Ley 14.378 faz do dia do censo nacional um feriado pago, quando o Poder Ejecutivo fixa um dia único de recenseamento. Entre 2000 e 2026 a série do BCU não tem lacuna que se explique por censo, mas um censo futuro pode trazer um.
- O 31 de dezembro e a quinta de Turismo dependem de atos do BCU que se repetem desde 2003 e 2009, mas podem mudar sem lei.
- Greves gerais não se projetam. Quando o BCU suspender o Sistema Central de Liquidación por greve, ele publica uma comunicação do Área de Sistema de Pagos com o título "Actividad [dia]"; foi assim de 2015 a 2019.
- A Semana de Turismo segue a Páscoa gregoriana; o algoritmo de Meeus do teste do repositório serve.

## Fontes

Banco Central del Uruguay, normas e comunicações gerais:

- RNRCSF, texto vigente (art. 28): https://www.bcu.gub.uy/Acerca-de-BCU/Normativa/Documents/Reordenamiento%20de%20la%20Recopilaci%C3%B3n/Sistema%20Financiero/RNRCSF.pdf
- Circular 1.888, de 24/12/2003 (inclui o 31/12 na exceção do horário mínimo): https://www.bcu.gub.uy/Circulares/seggci1888.pdf
- Circular 1.855, de 11/04/2003 (redação anterior, sem o 31/12): https://www.bcu.gub.uy/Circulares/seggci1855.pdf
- Comunicação 2007/269 (31/12 sem operações, conforme a resolução de 23/12/2003): https://www.bcu.gub.uy/Comunicados/seggco07269.pdf
- Comunicações anuais de 24 e 31 de dezembro (horário dos bancos e do BCU): 2007/276 https://www.bcu.gub.uy/Comunicados/seggco07276.pdf ; 2008/217 https://www.bcu.gub.uy/Comunicados/seggco08217.pdf ; 2009/176 https://www.bcu.gub.uy/Comunicados/seggco09176.pdf ; 2010/241 https://www.bcu.gub.uy/Comunicados/seggco10241.pdf ; 2012/225 https://www.bcu.gub.uy/Comunicados/seggco12225.pdf ; 2013/213 https://www.bcu.gub.uy/Comunicados/seggco13213.pdf ; 2014/204 https://www.bcu.gub.uy/Comunicados/seggco14204.pdf ; 2015/238 https://www.bcu.gub.uy/Comunicados/seggco15238.pdf ; 2018/254 https://www.bcu.gub.uy/Comunicados/seggco18254.pdf ; 2019/287 https://www.bcu.gub.uy/Comunicados/seggco19287.pdf ; 2020/269 https://www.bcu.gub.uy/Comunicados/seggco20269.pdf ; 2021/271 https://www.bcu.gub.uy/Comunicados/seggco21271.pdf ; 2024/231 https://www.bcu.gub.uy/Comunicados/seggco24231.pdf ; 2025/188 https://www.bcu.gub.uy/Comunicados/seggco25188.pdf
- Comunicações de 23 e 30 de dezembro com horário normal: 2011/216 https://www.bcu.gub.uy/Comunicados/seggco11216.pdf ; 2022/219 https://www.bcu.gub.uy/Comunicados/seggco22219.pdf
- Comunicação 2009/053 (Semana de Turismo): https://www.bcu.gub.uy/Comunicados/seggco09053.pdf
- Notícia do BCU sobre o atendimento na Semana de Turismo de 2025 (quinta só das 9h às 11h, sem cotação nesse dia): https://www.bcu.gub.uy/Comunicaciones/Paginas/Detalle-Noticia.aspx?noticia=418

Banco Central del Uruguay, Área de Sistema de Pagos (Sistema Central de Liquidación e Cámara Compensadora):

- Semana de Turismo, sistema indisponível na quinta e na sexta: 2010/053 https://www.bcu.gub.uy/Comunicados/seggco10053.pdf ; 2011/055 https://www.bcu.gub.uy/Comunicados/seggco11055.pdf ; 2012/049 https://www.bcu.gub.uy/Comunicados/seggco12049.pdf ; 2013/038 https://www.bcu.gub.uy/Comunicados/seggco13038.pdf ; 2014/038 https://www.bcu.gub.uy/Comunicados/seggco14038.pdf
- 24 e 31 de dezembro, sistema indisponível no dia 31: 2009/168 https://www.bcu.gub.uy/Comunicados/seggco09168.pdf ; 2010/251 https://www.bcu.gub.uy/Comunicados/seggco10251.pdf ; 2012/233 https://www.bcu.gub.uy/Comunicados/seggco12233.pdf ; 2013/216 https://www.bcu.gub.uy/Comunicados/seggco13216.pdf ; 2014/212 https://www.bcu.gub.uy/Comunicados/seggco14212.pdf ; 2015/241 https://www.bcu.gub.uy/Comunicados/seggco15241.pdf ; 2018/262 https://www.bcu.gub.uy/Comunicados/seggco18262.pdf ; 2019/297 https://www.bcu.gub.uy/Comunicados/seggco19297.pdf ; 2020/271 https://www.bcu.gub.uy/Comunicados/seggco20271.pdf ; 2021/273 https://www.bcu.gub.uy/Comunicados/seggco21273.pdf ; 2024/257 https://www.bcu.gub.uy/Comunicados/seggco24257.pdf ; 2025/179 https://www.bcu.gub.uy/Comunicados/seggco25179.pdf
- Dias de greve: 2015/139 https://www.bcu.gub.uy/Comunicados/seggco15139.pdf ; 2016/152 https://www.bcu.gub.uy/Comunicados/seggco16152.pdf ; 2016/191 https://www.bcu.gub.uy/Comunicados/seggco16191.pdf ; 2017/128 https://www.bcu.gub.uy/Comunicados/seggco17128.pdf ; 2018/159 https://www.bcu.gub.uy/Comunicados/seggco18159.docx.pdf ; 2019/154 https://www.bcu.gub.uy/Comunicados/seggco19154.pdf

Banco Central del Uruguay, dados e páginas antigas:

- Série de cotações: página https://www.bcu.gub.uy/Estadisticas-e-Indicadores/Paginas/Cotizaciones.aspx ; serviço usado pela página (POST JSON, operação `getcotizaciones`, moeda 2225, grupo 2): https://www.bcu.gub.uy/_layouts/15/BCU.Cotizaciones/handler/CotizacionesHandler.ashx?op=getcotizaciones
- Calendários de feriados do BCU (Wayback Machine): 2002 https://web.archive.org/web/20031216235806/http://www.bcu.gub.uy:80/autoriza/sgoioi/feriados2002.html ; 2003, versão inicial https://web.archive.org/web/20021029004843/http://www.bcu.gub.uy:80/autoriza/sgoioi/feriados2003.html e corrigida https://web.archive.org/web/20071228004033/http://www.bcu.gub.uy:80/autoriza/sgoioi/feriados2003.html ; 2004, versão inicial https://web.archive.org/web/20031020044115/http://www.bcu.gub.uy:80/autoriza/sgoioi/feriados2004.html e final https://web.archive.org/web/20041228175425/http://www.bcu.gub.uy:80/autoriza/sgoioi/feriados2004.html ; 2005 https://web.archive.org/web/20041228180529/http://www.bcu.gub.uy:80/autoriza/sgoioi/feriados2005.html ; 2006 https://web.archive.org/web/20071228001423/http://www.bcu.gub.uy:80/autoriza/sgoioi/feriados2006.html ; 2007 https://web.archive.org/web/20080923003838/http://www.bcu.gub.uy:80/autoriza/sgoioi/feriados2007.html ; 2008 https://web.archive.org/web/20081013085125/http://www.bcu.gub.uy:80/autoriza/sgoioi/feriados2008.html ; 2009 https://web.archive.org/web/20090929005620/http://www.bcu.gub.uy:80/autoriza/sgoioi/feriados2009.html ; 2010 https://web.archive.org/web/20100330021001/http://www.bcu.gub.uy:80/autoriza/sgoioi/feriados2010.html
- Tabela interna de dias sem operação do BCU, 1994 a 2004 (Wayback Machine): https://web.archive.org/web/20040212235326/http://www.bcu.gub.uy:80/autoriza/opinme/diasferiados.js

IMPO (leis e decretos):

- Ley 6.997 (1919), feriados, Carnaval e Semana de Turismo: https://www.impo.com.uy/bases/leyes/6997-1919
- Ley 7.556 (1923), 1º de março da posse presidencial: https://www.impo.com.uy/bases/leyes/7556-1923
- Decreto 120/985, feriado pago do 1º de março da posse: https://www.impo.com.uy/bases/decretos/120-1985
- Ley 16.805 (1996), regime de traslado: https://www.impo.com.uy/bases/leyes/16805-1996 ; texto original do art. 2: https://www.impo.com.uy/bases/leyes-originales/16805-1996/2
- Ley 17.414 (2001), 19/6 e 2/11 passam a fixos: https://www.impo.com.uy/bases/leyes/17414-2001
- Ley 18.748 (2011), 18/05/2011 sem traslado: https://www.impo.com.uy/bases/leyes/18748-2011
- Ley 17.262 (2000), 23/09/2000: https://www.impo.com.uy/bases/leyes/17262-2000
- Ley 17.798 e Ley 17.824 (2004), 10/09/2004: https://www.impo.com.uy/bases/leyes/17798-2004 ; https://www.impo.com.uy/bases/leyes/17824-2004
- Decreto 288/002, feriado bancário de 30/07/2002: https://www.impo.com.uy/bases/decretos/288-2002
- Decreto 290/002, prorrogação por 72 horas: https://www.impo.com.uy/bases/decretos/290-2002

Conferência com terceiros:

- Montevideo Portal, Semana de Turismo de 2011 (BCU fechado na sexta, quinta das 9h às 11h; BROU só com uma agência na quinta): http://www.montevideo.com.uy/auc.aspx?135914%2C3%2C1036=
- El Observador, 27/03/2024 (Turismo de 2024: Santander, BBVA e Scotiabank fechados na quinta e na sexta; BROU só com a agência do Montevideo Shopping na quinta, das 10h às 12h, e nenhuma na sexta): https://www.elobservador.com.uy/nota/bancos-abiertos-en-semana-de-turismo-como-funcionaran-brou-y-los-privados-el-jueves-y-viernes-santo-2024327132129
- El Observador, 20/12/2021 (BCU e BROU sem expediente em 31/12/2021): https://www.elobservador.com.uy/nota/transporte-shopping-y-oficinas-publicas-asi-funcionaran-en-nochebuena-navidad-y-ano-nuevo-2021122019023
- HSBC Uruguay, página de feriados do banco em 2024, com 31/12 "Fin de Año" (Wayback Machine): https://web.archive.org/web/20251207004650/https://www.hsbc.com.uy/feriados-uruguay/
- Bolsa de Valores de Montevideo, calendário de 2026 (bate com o arquivo; a bolsa não lista o 31/12): https://www.bvm.com.uy/html/calendarioDeFeriados

Greves (causa dos dias sem operação):

- Montevideo Portal, cronologia dos paros generales de 2000 a 2002: https://www.montevideo.com.uy/Noticias/EL-PITCNT-REALIZA-UN-PARO-GENERAL-Y-SU-ACTO-SERA-EN-SAN-PANCRACIO-uc61 ; paro de 12/06/2002: https://www.montevideo.com.uy/Noticias/BATLLE-ENFRENTA-HOY-SU-CUARTO-PARO-GENERAL-uc62
- La Jornada/Reuters, 13/06/2002: https://www.jornada.com.mx/2002/06/13/032n2mun.php
- Emol/Reuters, 07/08/2002: https://www.emol.com/noticias/economia/2002/08/07/91840/uruguayos-paralizan-en-protesta-por-medidas-del-gobierno.html
- LatinNews, paro de agosto de 2003: https://www.latinnews.com/component/k2/item/24449-uruguay--main-union-to-go-on-strike.html
- Página/12, 21/08/2008 (paro de 20/08/2008, bancos fechados; cita o de agosto de 2003): https://www.pagina12.com.ar/diario/elmundo/4-110071-2008-08-21.html
- LatinNews, paro de 06/08/2015: http://www.latinnews.com/index.php?option=com_legacyk2&view=item&id=65960
- IndustriALL, paro de 14/07/2016: http://www.industriall-union.org/massive-numbers-of-uruguayans-join-strike-for-better-pay
- Crisis24, paros de 20/07/2017 e 22/08/2018 (links hoje redirecionam): https://crisis24.garda.com/fr/product/217376 ; https://crisis24.garda.com/fr/product/257696
- La Izquierda Diario, paro de 25/06/2019: https://www.izquierdadiario.es/Contundente-paro-general-en-Uruguay
- Contexto do conflito da banca oficial em 2011: https://www.180.com.uy/articulo/21320_Paro-en-los-bancos ; https://ladiaria.com.uy/politica/articulo/2012/1/revancha-de-reyes/
