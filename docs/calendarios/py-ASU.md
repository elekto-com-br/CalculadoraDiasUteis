# Paraguai, Banco Central: regras do calendário py-ASU

Calendário dos dias em que os bancos não abrem ao público em Assunção, segundo o Banco Central del Paraguay (BCP), de 01/01/2000 a 31/12/2027. O arquivo é `src/Elekto.Calendars/Calendars/0060.calendar.py-ASU.txt`: primeira linha com o nome, depois `yyyyMMdd;Descrição`, em UTF-8 com BOM e fim de linha CRLF, como os calendários da aplicação. As descrições estão em espanhol.

Números: 363 datas, 288 em dias úteis e 75 em sábado ou domingo. Ficaram de fora 8 datas duvidosas, listadas mais abaixo.

Pesquisado em 25/10/2026. Os sites oficiais (bacn.gov.py, presidencia.gov.py, gacetaoficial.gov.py e bcp.gov.py) estavam fora do ar, bloqueavam acesso automatizado ou recusavam conexão daqui. Por isso as leis foram lidas no Justia Paraguay (transcrição dos textos oficiais), na Gaceta Oficial em PDF e em cópias do Internet Archive; os decretos, em PDFs da Presidência guardados no Internet Archive, em PDFs hospedados pela imprensa, na Agencia IP (agência oficial de notícias do governo) e em jornais que citam o número do decreto.

## O que entra e o que não entra

Entram:

1. Os feriados nacionais, na data efetiva de cada ano (depois do traslado decretado, quando houve). A data original de um feriado trasladado foi dia útil e não aparece.
2. Jueves Santo e Viernes Santo (Páscoa menos 3 e menos 2 dias).
3. Os feriados nacionais extraordinários criados por lei ou decreto (censo, bicentenário, visita do Papa, feriados adicionais da Ley 7.544/2025).
4. O feriado bancário do último dia útil de dezembro (Decreto 9.131 de 23/12/1949, citado todo ano pelo BCP). Nesse dia os bancos abrem as ventanillas só das 8h45 às 10h45, e só para vencimentos daquela data; o SPI (pagamentos instantâneos até G. 5.000.000), os caixas automáticos e o internet banking funcionam. Para prazos é dia não útil: o próprio BCP o chama de "feriado bancario".
5. Feriados que caem em sábado ou domingo, com a descrição.

Não entram:

- Asuetos. São folgas só da administração pública (Quarta-feira Santa, tardes de 24 e 31 de dezembro, jogos da seleção, Dia D contra a dengue em 29/01/2007, meio dia contra a dengue em 19/04/2011, 10/09/2009 pela classificação para a Copa de 2010, 14/06/2010 e 29/06/2010 à tarde etc.). Os bancos privados abrem. A Ley 7.544/2025, art. 7, diz expressamente que asuetos "serán considerados días hábiles".
- Meio expediente bancário. Em 24/12 os bancos abrem (em alguns anos até o meio-dia: 24 e 30/12/2008 por anúncio da Asociación de Bancos del Paraguay; 24 e 28/12/2018 no BBVA). A série diária do BCP de 2000 a 2007 tem cotação em todo 24/12 que caiu em dia útil.
- Feriado bancário próprio além do de fim de ano. Não há "Día del Bancario" no Paraguai (a data de 6 de novembro é argentina).
- Feriados municipais. O de Assunção (15 de agosto, fundação da cidade) já é nacional.

Regras de descrição: o feriado trasladado leva " (trasladado)" no fim; o 14 e o 15 de maio levam ambos "Día de la Independencia Nacional", como no art. 1 da Ley 7.544/2025; o 15 de agosto leva sempre "Día de la Fundación de Asunción", inclusive nos anos de posse presidencial (2003, 2008, 2013, 2018, 2023).

## Regras por período

### Feriados fixos

| Data | Descrição | Desde | Até | Base legal |
|---|---|---|---|---|
| 1/1 | Año Nuevo | antes de 2000 | vigente | Ley 8/1990; Ley 7.544/2025 |
| Páscoa menos 3 | Jueves Santo | antes de 2000 | vigente | idem |
| Páscoa menos 2 | Viernes Santo | antes de 2000 | vigente | idem |
| 14/5 | Día de la Independencia Nacional | 2012 | vigente | Ley 4.531/2011 (promulgada em novembro de 2011); Ley 7.544/2025 |
| 15/5 | Día de la Independencia Nacional | antes de 2000 | vigente | Ley 8/1990; Ley 7.544/2025 |
| 8/12 | Día de la Virgen de Caacupé | antes de 2000 | vigente | idem |
| 25/12 | Navidad | antes de 2000 | vigente | idem |
| 1/5 | Día de los Trabajadores | antes de 2000 | vigente | fixo desde 03/09/2025 (Ley 7.544); antes era trasladável (Ley 1.723/2001) |
| 15/8 | Día de la Fundación de Asunción | antes de 2000 | vigente | fixo desde 03/09/2025 (Ley 7.544); antes era trasladável |

O 14 de maio não foi feriado de 1990 a 2011 (a Ley 8/1990 o tirou). Em 2011, o 14/5 (sábado) e o 16/5 (segunda) foram feriados únicos do Bicentenário, pela Ley 4.316/2011.

### Feriados trasladáveis

| Data legal | Descrição | Desde | Trasladável |
|---|---|---|---|
| 1/3 | Día de los Héroes de la Patria | antes de 2000 | de 29/06/2001 em diante |
| 1/5 | Día de los Trabajadores | antes de 2000 | de 29/06/2001 a 02/09/2025 |
| 12/6 | Día de la Paz del Chaco | antes de 2000 | de 29/06/2001 em diante |
| 15/8 | Día de la Fundación de Asunción | antes de 2000 | de 29/06/2001 a 02/09/2025 |
| 29/9 | Día de la Batalla de Boquerón | 1995 (Ley 715/1995, por cinco anos); permanente pela Ley 1.601 de 28/09/2000 | de 29/06/2001 em diante |
| 20/6 | Día de la Jura de la Constitución Nacional | 2026 (Ley 7.544/2025) | sim |

O 29/09/2000 foi feriado: a Ley 1.601 foi promulgada na véspera, quando o prazo de cinco anos da Ley 715/1995 vencia; a série do BCP não tem cotação nesse dia.

Como o traslado funciona:

- De 29/06/2001 a 02/09/2025, Ley 1.723/2001: o Executivo pode, por decreto anual, levar ao "día lunes" qualquer feriado das Leis 8/1990 e 1.601/2000, menos 1/1, 15/5, 8/12, 25/12 e a Semana Santa; o decreto deveria sair seis meses antes do ano. Na prática: quase nenhum traslado até 2012 (só 2002), traslados anuais de 2013 a 2026, nenhum em 2020 (pandemia). Houve traslados para a segunda anterior e para a seguinte, e também de domingo para a segunda (1/5/2022, 29/9/2024).
- Desde 03/09/2025, Ley 7.544/2025 (Gaceta Oficial nº 200): só 1/3, 12/6, 20/6 e 29/9 são móveis, e só para "los días lunes, anterior o siguiente al feriado que se traslada" (art. 3). O decreto não tem prazo mínimo de antecedência. O art. 4 permite até três feriados nacionais adicionais por ano, por decreto, "con los mismos efectos que los demás Feriados Nacionales".

### Feriado bancário

Último dia útil de dezembro (seg a sex que não seja feriado), todos os anos (Decreto 9.131/1949, art. 1: "Declárase feriado bancario el último día hábil del mes de diciembre de cada año"). Cai em 31/12, ou em 30/12 ou 29/12 quando o 31 é sábado ou domingo.

## Projeção de 2028 em diante

Datas certas pela lei vigente (Ley 7.544/2025 e Decreto 9.131/1949):

- 1/1, Jueves Santo, Viernes Santo, 1/5, 14/5, 15/5, 15/8, 8/12 e 25/12, sempre na data;
- feriado bancário no último dia útil de dezembro.

Datas que dependem de decreto de cada ano:

- 1/3, 12/6, 20/6 e 29/9: ficam na data legal, salvo decreto que os leve à segunda anterior ou seguinte. Não há regra automática; a prática de 2013 a 2026 mostra os dois sentidos e também feriados de fim de semana levados para segunda (1/3/2026, 20/6/2026). Em geral: terça e quarta vão para a segunda anterior, quinta para a seguinte, sexta e fim de semana às vezes ficam e às vezes vão para a segunda seguinte. Projete na data legal e atualize quando sair o decreto do ano.
- Até três feriados adicionais por ano (art. 4 da Ley 7.544/2025), imprevisíveis: 2025 teve dois e 2026 teve um até agora.

Isso vale também para 2027, que está no arquivo só por regra: nenhum decreto de 2027 tinha saído até 25/10/2026. O 1/3/2027 cai numa segunda; o 12/6 (sábado), o 20/6 (domingo) e o 29/9 (quarta) ainda podem ser trasladados. O 29/09/2027 está no arquivo porque é a data da lei hoje; é a data com mais chance de mudar.

Também em 2026 ainda podem sair até dois feriados adicionais em novembro ou dezembro.

## Exceções ano a ano

### Traslados (data legal para data efetiva)

| Ano | Feriado | Data legal | Data efetiva | Instrumento | Fonte |
|---|---|---|---|---|---|
| 2002 | Héroes | sex 01/03 | seg 04/03 | decreto não localizado; série diária do BCP sem cotação em 04/03 e com cotação em 01/03 | https://web.archive.org/web/20110507045750/http://www.bcp.gov.py/doma/Mercadolibre/2002.htm |
| 2013 | Héroes | sex 01/03 | seg 04/03 | Decreto 9.393 de 02/08/2012 | https://web.archive.org/web/20130919003447/http://www.presidencia.gov.py/uploads/pdf/presidencia-3895fd546d1ac1a89fa42197d213c3075077b1093742057392547a9a87c20412.pdf ; https://www.ultimahora.com/feriados-este-ano-se-trasladan-dos-fechas-al-dia-lunes-n603160 |
| 2013 | Paz del Chaco | qua 12/06 | seg 10/06 | Decreto 9.393/2012 | idem; https://www.abc.com.py/nacionales/dia-de-la-paz-del-chaco-sera-feriado-el-lunes-10-558317.html |
| 2014 | Paz del Chaco | qui 12/06 | seg 16/06 | Decreto 280 de 13/09/2013 | https://web.archive.org/web/20160306035108/http://www.presidencia.gov.py/archivos/documentos/presidencia-58782acae7e54b1746294c50d041a01ba7478e7af125ee0af104776c88dfbba8.pdf |
| 2015 | Boquerón | ter 29/09 | seg 28/09 | Decreto 2.755 | https://www.ip.gov.py/ip/2015/09/23/feriado-del-29-de-setiembre-se-adelanta-un-dia/ |
| 2016 | Héroes | ter 01/03 | seg 29/02 | Decreto 4.508/2015 | https://www.ip.gov.py/ip/2016/02/23/ejecutivo-adelanta-feriad-a-este-lunes-29/ |
| 2016 | Boquerón | qui 29/09 | seg 03/10 | Decreto 4.508/2015 | https://www.ip.gov.py/ip/2016/09/28/feriado-del-29-de-setiembre-se-traslada-al-lunes-3-de-octubre/ |
| 2017 | Héroes | qua 01/03 | seg 27/02 | Decreto 6.292 de 17/11/2016 | https://www.ultimahora.com/establecen-traslados-feriados-el-2017-n1042098 |
| 2017 | Boquerón | sex 29/09 | seg 02/10 | Decreto 6.292/2016 | idem; https://www.ip.gov.py/ip/2017/09/28/turismo-interno-brinda-atractivas-propuestas-para-el-fin-de-semana-largo/ |
| 2018 | Héroes | qui 01/03 | seg 26/02 | Decreto 8.108 de 20/11/2017 | https://www.ip.gov.py/ip/2017/11/21/ejecutivo-decreta-traslado-de-dias-feriados-en-el-2018/ |
| 2018 | Paz del Chaco | ter 12/06 | seg 11/06 | Decreto 8.108/2017 | idem |
| 2019 | Héroes | sex 01/03 | seg 04/03 | Decreto 913/2018 | https://www.ip.gov.py/ip/2018/12/19/ejecutivo-dispone-el-traslado-de-dos-feriados-en-el-2019/ |
| 2019 | Paz del Chaco | qua 12/06 | seg 17/06 | Decreto 913/2018 | idem |
| 2021 | Boquerón | qua 29/09 | seg 27/09 | Decreto 5.958/2021 | https://www.ultimahora.com/ejecutivo-oficializa-traslado-del-feriado-del-29-setiembre-n2961154.html |
| 2022 | Héroes | ter 01/03 | seg 28/02 | Decreto 6.618/2022 | https://www.lanacion.com.py/pais/2022/02/04/traslado-de-feriados-dia-de-los-heroes-al-28-de-febrero-y-batalla-de-boqueron-al-3-de-octubre/ |
| 2022 | Trabajadores | dom 01/05 | seg 02/05 | Decreto 6.979 de 26/04/2022 | https://impuestospy.com/impuestos/decreto-n-6-979-2022/ |
| 2022 | Boquerón | qui 29/09 | seg 03/10 | Decreto 6.618/2022 | https://www.lanacion.com.py/pais/2022/09/29/feriado-del-29-de-setiembre-se-traslado-para-el-proximo-lunes/ |
| 2023 | Héroes | qua 01/03 | seg 27/02 | Decreto 7.915/2022 | https://www.ultimahora.com/ejecutivo-traslada-gran-anticipacion-feriado-del-1-marzo-n3026589 |
| 2024 | Paz del Chaco | qua 12/06 | seg 10/06 | Decreto 390 de 25/09/2023 | https://www.abc.com.py/politica/2023/12/26/ejecutivo-dispuso-que-el-12-de-junio-sea-el-unico-feriado-movil-del-2024/ ; https://www.bcra.gob.ar/event/dia-de-la-paz-del-chaco-paraguay-2/ |
| 2024 | Boquerón | dom 29/09 | seg 30/09 | decreto de 11/06/2024 (número não achado) | https://www.ultimahora.com/ejecutivo-traslada-feriado-del-domingo-29-de-setiembre-al-lunes-30 |
| 2025 | Héroes | sáb 01/03 | seg 03/03 | Decreto 3.047/2024 | https://www.lanacion.com.py/politica/2024/12/18/ejecutivo-establecio-el-traslado-de-dos-feriados-en-el-2025-para-incentivar-el-turismo |
| 2025 | Paz del Chaco | qui 12/06 | seg 16/06 | Decreto 3.047/2024 | idem; https://www.lanacion.com.py/pais/2025/06/11/paz-del-chaco-recuerdan-que-el-feriado-de-manana-pasa-para-lunes-16-de-junio/ |
| 2026 | Héroes | dom 01/03 | seg 02/03 | decreto de fevereiro de 2026 (número não confirmado) | https://www.hoy.com.py/nacionales/2026/02/17/los-proximos-feriados-moviles-ademas-del-1-de-marzo |
| 2026 | Jura de la Constitución | sáb 20/06 | seg 22/06 | Decreto 6.215 de 09/06/2026 (PDF lido) | https://grupovierci.brightspotcdn.com/39/cd/808fd5cf4b2a91e60a5b066dcbd0/decreto-6215-traslado-feriado-20-al-22-de-junio.pdf |
| 2026 | Boquerón | ter 29/09 | seg 28/09 | Decreto 6.601/2026 | https://www.hoy.com.py/nacionales/2026/08/21/feriado-del-29-de-septiembre-se-adelanta-al-lunes-28 |

Anos sem traslado: 2000, 2001, 2003 a 2012 e 2020. Casos que poderiam confundir:

- 2001: um decreto de julho levava o 15/8 (quarta) ao lunes 20/8; o Decreto 14.078 de 24/07/2001 o revogou depois do protesto da Igreja. A série do BCP confirma 15/8 fechado e 20/8 aberto. https://www.abc.com.py/edicion-impresa/politica/gobierno-deja-sin-feriado-del-15-de-agosto-a-la-iglesia-1540671.html
- 2003: Decreto 20.973/2003 manteve o 1/5 na quinta. 2004: Decreto 3.390/2004 manteve o 29/9 na quarta. 2007: o 1/3 ficou na quinta. Resumo do Q++ Studio, feito na época: https://web.archive.org/web/20160229110917/http://www.qppstudio.net/national-news-digests/paraguay-public-holidays.htm
- 2008 a 2012: a Ley 1.723 não foi usada. Em 2008 a Feprinco, em 2010 a Câmara de Deputados e em 2012 a Senatur cobraram do governo que a aplicasse; o Decreto 9.393/2012 (para 2013) é descrito como a primeira aplicação.
- 2017: o Decreto 6.292 levava também o 15/8 (terça) à segunda 14/8, mas o Decreto 6.382 (novembro de 2016) desfez esse traslado; o 15/8/2017 ficou na terça. A biblioteca python-holidays erra aqui. https://www.ip.gov.py/ip/2016/11/28/ejecutivo-deja-sin-efecto-traslado-del-feriado-del-15-de-agosto/ ; https://www.ultimahora.com/dejan-efecto-traslado-feriado-la-fundacion-asuncion-n1043480
- 2020: o Decreto 2.757 (outubro de 2019) levava o 12/6 a 15/6 e o 29/9 a 28/9; por causa da pandemia, o Decreto 3.668 (05/06/2020) manteve o 12/6 na sexta e o Decreto 4.040 (setembro de 2020) manteve o 29/9 na terça. https://www.ip.gov.py/ip/2020/06/05/ejecutivo-mantiene-feriado-por-la-paz-del-chaco-en-el-viernes-12-de-junio/ ; https://www.ip.gov.py/ip/2020/09/10/feriado-por-la-batalla-de-boqueron-se-mantiene-el-dia-martes/
- 2021: o 1/5 (sábado), o 12/6 (sábado) e o 15/8 (domingo) ficaram na data. Não há notícia de traslado deles; só achei a do 29/9. Se algum tivesse ido para a segunda, faltaria uma data, mas nada indica isso.
- 2022: o 12/6 (domingo) ficou no domingo. https://www.abc.com.py/nacionales/2022/06/08/se-trasladara-el-feriado-por-la-paz-del-chaco-al-lunes-13/

### Feriados extraordinários

| Data | Descrição no arquivo | Instrumento | Fonte |
|---|---|---|---|
| 28/08/2002 (qua) | Censo Nacional de Población y Viviendas | Ley 1.731/2001, art. 1 ("declárase feriado ese día en todo el territorio nacional"); série do BCP sem cotação | https://paraguay.justia.com/nacionales/leyes/ley-1731-jul-27-2001/gdoc/ |
| 14/05/2011 (sáb) e 16/05/2011 (seg) | Bicentenario de la Independencia Nacional | Ley 4.316/2011, promulgada em 10/05/2011, obrigatória também para o setor privado | https://www.ultimahora.com/lugo-promulgo-los-feriados-del-14-y-16-las-fiestas-patrias-n427529 ; https://web.archive.org/web/2011/http://www.abc.com.py:80/nota/los-feriados-se-cumplen-advierten/ |
| 10/07/2015 (sex) e 11/07/2015 (sáb) | Visita del Papa Francisco | lei sancionada em 27/05/2015 e promulgada (número não achado) | https://www.ip.gov.py/ip/2015/05/27/diputados-sancionan-feriados-durante-visita-del-papa/ ; https://www.ip.gov.py/ip/2015/07/03/aduanas-no-verificara-cargas-con-destino-a-argentina-por-visita-papal/ |
| 09/11/2022 (qua) | Censo Nacional de Población y Viviendas | Ley 6.987/2022 ("declárase feriado ese día en todo el territorio nacional, por única vez") | https://www.bacn.gov.py/leyes-paraguayas/10794/ley-n-6987-dispone-la-fecha-de-realizaci-n-del-censo-nacional-de-poblaci-n-y-viviendas-del-a-o-2022 ; https://www.pj.gov.py/notas/23110-se-suspenden-plazos-procesales-y-registrales-del-miercoles-9-de-noviembre |
| 05/09/2025 (sex) | Clasificación de la Selección Paraguaya al Mundial 2026 | decreto pelo art. 4 da Ley 7.544/2025 (número 4.522 só em fonte secundária) | https://www.pj.gov.py/notas/28594-feriado-nacional-el-poder-judicial-mantendra-servicios-esenciales-y-remates-fijados ; https://www.lanacion.com.py/politica/2025/09/04/es-oficial-todo-paraguay-celebra-el-feriado-nacional-tras-la-clasificacion-de-la-albirroja/ |
| 26/12/2025 (sex) | Feriado adicional de Navidad | Decreto 5.040 de 03/12/2025, art. 4 da Ley 7.544 (PDF lido) | https://grupovierci.brightspotcdn.com/23/0b/b887cf2b40b0ad71ec3245a1e6c5/decreto5040.PDF |
| 30/06/2026 (ter) | Victoria de la Selección Paraguaya en el Mundial 2026 | Decreto 6.280/2026 (vitória sobre a Alemanha) | https://www.pj.gov.py/notas/30118-poder-judicial-se-adhiere-al-feriado-nacional-del-martes-30-de-junio ; https://www.minutouno.com/deportes/que-dice-el-decreto-6280-del-presidente-las-razones-del-feriado-nacional-paraguay-n6294208 |

Nos feriados extraordinários de 2022 a 2026 não achei comunicado do BCP; o fechamento dos bancos decorre de serem feriados nacionais (para os do art. 4, a lei diz que têm "los mismos efectos que los demás Feriados Nacionales"). O Poder Judicial suspendeu ou trasladou para o dia útil seguinte os prazos que venciam em 09/11/2022, 05/09/2025 (Resolución 1.489/2025) e 30/06/2026.

### Feriado bancário de fim de ano

| Ano | Data | Conferência |
|---|---|---|
| 2000 a 2007 | 29/12/2000, 31/12/2001, 31/12/2002, 31/12/2003, 31/12/2004, 30/12/2005, 29/12/2006, 31/12/2007 | série diária do BCP sem cotação em cada um; 2005 também por anúncio do BCP (Q++) |
| 2008 a 2014 | 31/12, salvo 30/12/2011 | tabelas do Q++ da época; 2011 por anúncio do BCP de 29/12/2011 |
| 2015 a 2017, 2019, 2020 | 31/12/2015, 30/12/2016, 29/12/2017, 31/12/2019, 31/12/2020 | só pela regra |
| 2018 | 31/12 | aviso do BBVA Paraguay: https://www.bbva.com/es/bbva-paraguay-fija-horario-de-atencion-por-fiestas-y-atendera-24-horas-por-canales-digitales/ |
| 2021 | 31/12 | BCP via La Nación: https://www.lanacion.com.py/negocios/2021/12/30/las-transferencias-financieras-podran-procesarse-hasta-hoy-y-regresaran-el-lunes/ |
| 2022 | 30/12 | ABC: https://www.abc.com.py/economia/2022/12/26/gestiones-bancarias-hasta-el-jueves-29/ ; lista SML do BCRA: https://www.bcra.gob.ar/sml-feriados-2022/ |
| 2023 | 29/12 | BCP via Última Hora: https://www.ultimahora.com/bcp-recuerda-el-feriado-bancario ; BCRA: https://www.bcra.gob.ar/event/feriado-bancario-paraguay/ |
| 2024 | 31/12 | BCP via La Nación: https://www.lanacion.com.py/negocios/2024/12/26/el-31-de-diciembre-es-feriado-bancario-recuerda-el-bcp/ |
| 2025 | 31/12 | BCP via Última Hora: https://www.ultimahora.com/feriado-bancario-transferencias-por-sipap-seran-normales |
| 2026 | 31/12 | regra; lista SML 2026 do BCRA já traz "Feriado Bancario, Paraguay" em 31/12/2026: https://www.bcra.gob.ar/sml-feriados-2026/ |
| 2027 | 31/12 | só pela regra |

## Datas duvidosas (fora do arquivo)

| Data | Dia | Motivo da dúvida |
|---|---|---|
| 19/12/2000 | ter | Sem cotação na série diária do BCP (com cotação em 18 e 21/12). Nenhuma lei, decreto ou notícia explica. Pode ser asueto que só fechou o BCP, falha de registro ou fechamento real. |
| 20/12/2000 | qua | Idem. |
| 14/08/2001 | ter | Sem cotação na série do BCP, véspera do 15/8, no mês da briga sobre o traslado do 15/8 (Decreto 14.078). Não achei asueto nem feriado. |
| 11/06/2002 | ter | Sem cotação na série do BCP, véspera do 12/6, em plena crise bancária de 2002. Não achei norma nem feriado bancário. |
| 08/04/2005 | sex | Sem cotação na série do BCP. Dia do funeral de João Paulo II, com luto nacional declarado; não achei se houve feriado ou só asueto. |
| 06/04/2020 | seg | Covid: o BCP anunciou em 03/04/2020 que na Segunda e na Terça Santa os bancos não atenderiam presencialmente, só pelos canais digitais, com o Sipap funcionando. Não houve feriado bancário declarado; em março o BCP dissera que as medidas sanitárias "no implican feriado bancario". https://www.ultimahora.com/bancos-no-atenderan-al-publico-manera-presencial-este-lunes-n2878436 ; https://www.ferrere.com/en/news/paraguay-opciones-para-el-empleador-de-cara-las-medidas-sanitarias-adoptadas-por-el-gobierno-nacional/ |
| 07/04/2020 | ter | Idem. |
| 08/04/2020 | qua | Idem, e nesse dia matriz, filiais e agências ficaram fechadas (era também asueto do setor público). Mesmo assim não houve declaração de feriado bancário; não sei como os vencimentos foram tratados. |

Os dias úteis sem cotação nas planilhas do BCP de 17/12/2004 e 23/05/2005 foram descartados: a cotação está lançada no fim de semana vizinho, erro de digitação da planilha. Também foram descartados 02 e 03/08/2004 (luto pelo incêndio do Ycuá Bolaños, com cotação) e 14/08/2013 (a Presidência descartou feriado na véspera da posse de Cartes).

## O que foi conferido data a data e o que foi montado por regra

| Período | Como foi conferido | Confiança |
|---|---|---|
| 2000 a 2007 | Data a data numa fonte oficial: a série diária do BCP "Cotizaciones del dólar en el mercado fluctuante" (Mesa de Cambios, uma planilha por ano, no Internet Archive). Todo feriado legal em dia útil e todo último dia útil de dezembro estão sem cotação; nenhum feriado de fim de semana gerou segunda sem cotação. A exceção é o traslado de 2002 (04/03), visto só na série: sexta 01/03 com cotação, segunda 04/03 sem; o decreto não foi achado. | Alta; média para alta em 04/03/2002 |
| 2008 a 2012 | Montado pela regra (lei sem traslado). A falta de traslado foi conferida nas tabelas do Q++ Studio gravadas na época (terceiros) e em notícias que dizem que a Ley 1.723 não era usada. Os feriados do Bicentenário de 2011 vêm da lei, confirmada pela imprensa. | Média para alta |
| 2013 e 2014 | Decretos de traslado lidos nos PDFs da Presidência (Internet Archive), confirmados por imprensa e comunicados da Presidência. | Alta |
| 2015 a 2020 | Cada traslado confirmado pela Agencia IP (oficial), com o número do decreto, e pela imprensa. Os textos dos decretos não foram lidos. Feriado bancário só pela regra, menos 2018. | Alta |
| 2021 a 2025 | Imprensa que cita o decreto, o PDF do Decreto 5.040/2025, notas do Poder Judicial e as listas SML do BCRA (2022 a 2025). Feriado bancário confirmado em cada ano por comunicado do BCP divulgado pela imprensa. | Alta; média para alta nos sábados e domingos de 2021 sem traslado e no fechamento bancário dos extraordinários, inferido da lei |
| 2026 | Decreto 6.215 lido em PDF; os outros decretos por imprensa e Poder Judicial; lista SML do BCRA. Podem sair ainda até dois feriados adicionais. | Alta até outubro; provisória para novembro e dezembro |
| 2027 | Só pela regra da Ley 7.544/2025 e do Decreto 9.131/1949; nenhum decreto do ano. | Provisória nas datas móveis e sujeita a feriados adicionais |

## Para o teste de regras

A regra de cada ano, no estilo de `CalendarRulesTests`, fica assim:

1. Fixos: 1/1 "Año Nuevo"; Páscoa menos 3 "Jueves Santo"; Páscoa menos 2 "Viernes Santo"; 14/5 (de 2012 em diante) e 15/5 "Día de la Independencia Nacional"; 8/12 "Día de la Virgen de Caacupé"; 25/12 "Navidad".
2. Móveis na data legal, salvo a tabela de traslados acima (25 traslados, descrição com " (trasladado)"): 1/3 "Día de los Héroes de la Patria"; 1/5 "Día de los Trabajadores"; 12/6 "Día de la Paz del Chaco"; 20/6 "Día de la Jura de la Constitución Nacional" (de 2026 em diante); 15/8 "Día de la Fundación de Asunción"; 29/9 "Día de la Batalla de Boquerón".
3. Extraordinários: as 9 datas da tabela de extraordinários, com a descrição do arquivo.
4. Feriado bancário: último dia de segunda a sexta de dezembro que não seja feriado, "Feriado bancario (último día hábil del año)".

Nenhuma data do período tem duas regras ao mesmo tempo.

O teste `CalendarioSegueAsRegrasDaPraca`, em `tests/Elekto.Calendars.Tests/CalendarRulesTests.cs`, aplica exatamente essas regras e tabelas e confere o arquivo `src/Elekto.Calendars/Calendars/0060.calendar.py-ASU.txt` contra elas, data a data e descrição a descrição.

## Fontes

Leis:

- Ley 8/1990 (lista básica): https://paraguay.justia.com/nacionales/leyes/ley-8-jun-6-1990/gdoc/ ; cópia da BACN: https://web.archive.org/web/20250427173904/https://www.bacn.gov.py/leyes-paraguayas/2358/ley-n-8--por-la-cual-se-determinan-los-feriados-de-la-republica
- Ley 715/1995 (29/9 por cinco anos): https://paraguay.justia.com/nacionales/leyes/ley-715-sep-27-1995/gdoc/
- Ley 1.601/2000 (29/9 permanente): https://paraguay.justia.com/nacionales/leyes/ley-1601-nov-28-2000/gdoc/
- Ley 1.723/2001 (traslado para segunda-feira): https://paraguay.justia.com/nacionales/leyes/ley-1723-jun-29-2001/gdoc/
- Ley 1.731/2001 (censo de 2002): https://paraguay.justia.com/nacionales/leyes/ley-1731-jul-27-2001/gdoc/
- Ley 4.531/2011 (14 de maio): https://web.archive.org/web/20250420210317/https://www.bacn.gov.py/leyes-paraguayas/3831/ley-n-4531-restablece-el-dia-14-de-mayo-de-cada-ano-como-feriado-nacional ; https://www.abc.com.py/nacionales/2025/05/12/el-feriado-perdido-y-recuperado-de-la-independencia-nacional/
- Ley 6.987/2022 (censo de 2022): https://www.bacn.gov.py/leyes-paraguayas/10794/ley-n-6987-dispone-la-fecha-de-realizaci-n-del-censo-nacional-de-poblaci-n-y-viviendas-del-a-o-2022
- Ley 7.544/2025, Gaceta Oficial nº 200 de 03/09/2025 (PDF lido): https://grupovierci.brightspotcdn.com/51/1e/94457a28442c9bc15eb7a0e9dd03/ley-7544-feriados-fijos-y-moviles-2025.pdf ; BACN: https://www.bacn.gov.py/leyes-paraguayas/12908/ley-n-7544-2025

Feriado bancário (Decreto 9.131/1949): ver a tabela do feriado bancário; resumo do BCP em https://www.lanacion.com.py/negocios/2025/12/29/el-bcp-confirma-feriado-bancario-este-miercoles-31-pero-spi-operara-con-normalidad/ ; bancos abertos em 24/12: https://www.bloomberglinea.com/latinoamerica/asueto-24-y-31-de-diciembre-en-paraguay-bancos-supermercados-y-mas/

Série diária oficial do BCP, 2000 a 2007 (Internet Archive; para os dados crus, acrescente `id_` ao carimbo):

- 2000: https://web.archive.org/web/20050428163748/http://www.bcp.gov.py:80/doma/Mercadolibre/2000.htm
- 2001: https://web.archive.org/web/20110507045744/http://www.bcp.gov.py/doma/Mercadolibre/2001.htm
- 2002: https://web.archive.org/web/20110507045750/http://www.bcp.gov.py/doma/Mercadolibre/2002.htm
- 2003: https://web.archive.org/web/20110507045755/http://www.bcp.gov.py/doma/Mercadolibre/2003.htm
- 2004: https://web.archive.org/web/20110507050441/http://www.bcp.gov.py/doma/Mercadolibre/2004.htm
- 2005: https://web.archive.org/web/20110507050446/http://www.bcp.gov.py/doma/Mercadolibre/2005.htm
- 2006: https://web.archive.org/web/20110509025923/http://www.bcp.gov.py/doma/Mercadolibre/2006.htm
- 2007: https://web.archive.org/web/20080513105805/http://www.bcp.gov.py/doma/Mercadolibre/2007_archivos/sheet001.htm

Tabelas de terceiros usadas só para conferir:

- Q++ Studio, tabelas anuais de 2008 a 2014 e resumo de notícias do Paraguai, gravados no Internet Archive: https://web.archive.org/web/20081220165256/http://www.qppstudio.net/publicholidays2008/paraguay.htm (troque o ano no caminho) e https://web.archive.org/web/20160229110917/http://www.qppstudio.net/national-news-digests/paraguay-public-holidays.htm
- Listas SML do BCRA (Banco Central da Argentina), 2022 a 2026: https://www.bcra.gob.ar/sml-feriados-2022/ (troque o ano). Atenção: misturam feriados dos Estados Unidos rotulados "Paraguay" e saem no começo do ano, sem os decretos posteriores.
- python-holidays, módulo do Paraguai: https://github.com/vacanza/holidays/blob/dev/holidays/countries/paraguay.py . Bate com os traslados de 2013 a 2025, menos em 14/08/2017 (o traslado foi desfeito). Diverge também nos asuetos que ela trata como feriado (29/01/2007, 10/09/2009, 14/06/2010, 19/04/2011, 14/08/2013), no 02/05/2022 (que ela trata como asueto, mas foi traslado do 1/5 pelo Decreto 6.979) e no que ela não tem: o traslado de 2002, os extraordinários de 2002, 2022, 2025 e 2026 e os traslados de 2026 além do 22/06.

