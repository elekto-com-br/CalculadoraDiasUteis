# Argentina, Banco Central: dias sem expediente bancário do calendário ar-BA

Calendário `ar-BA`: os dias em que as entidades financeiras não abrem ao público na Cidade Autônoma de Buenos Aires,
segundo o Banco Central de la República Argentina (BCRA). O arquivo `src/Elekto.Calendars/Calendars/0050.calendar.ar-BA.txt`
tem 534 datas de 2000-01-01 a 2027-12-25 (446 delas em dia útil). As descrições estão em espanhol; quando dois feriados
caem no mesmo dia, as descrições vêm unidas por " y ".

## O que entra e o que fica de fora

Entram:

* feriados nacionais inamovíveis e trasladáveis, na data em que foram efetivamente gozados (depois dos traslados);
* Jueves Santo (día no laborable; os bancos fecham em todos os anos do período);
* os feriados ou días no laborables con fines turísticos (feriados puente); em todos os anos o BCRA os incluiu na lista
  de dias em que "las entidades financieras no funcionarán";
* Día del Bancario (6 de novembro, Convenio Colectivo 18/75, art. 50);
* os feriados bancários decretados pelo BCRA (crise de 2001 e 2002, G20 em 2018, visita do papa em 2026), o feriado
  bancário de fim de ano do Decreto 24.879/1953 (até 2000) e os feriados nacionais extraordinários (censos,
  bicentenários, 2 de setembro de 2022, visita do papa);
* os asuetos de dia inteiro em que o BCRA deu folga ao seu pessoal, parou o MEP e convidou as entidades a fazer o mesmo
  (24 e 31 de dezembro em vários anos, 10 de julho de 2009);
* feriados em sábado ou domingo, com descrição (o 6 de novembro também, quando cai em fim de semana).

Ficam de fora: os dias não laboráveis religiosos (judeus, muçulmanos, armênio), em que os bancos abrem; os asuetos de
meio dia, em que os bancos atenderam até 12 h ou 13 h; os feriados só da Província de Buenos Aires; e as datas
duvidosas da seção própria.

## Como as datas foram conferidas

1. **Séries diárias do BCRA, dia a dia, de 2000-01-03 a 2026-10-21.** A API de estatísticas do BCRA (v4.0) publica só
   em dia útil bancário: reservas internacionais (variável 1, desde 1996), tipo de câmbio de referência Com. A 3500
   (variável 5, desde 2002-03-04), BADLAR de bancos privados (variável 7) e taxa de depósitos a 30 dias (variável 12).
   Um dia de semana sem a BADLAR e sem a taxa de depósitos (e, desde março de 2002, sem a Com. A 3500) é um dia sem
   atividade bancária. Gerei o calendário pelas regras e exceções abaixo e comparei com essas lacunas: **batem em todos os dias
   úteis do período**, com uma única lacuna sem feriado, 2002-01-14, investigada e descartada (ver "Datas investigadas e
   descartadas"). Nos feriados bancários da crise, a série de reservas tem valor (o BCRA trabalhou), mas BADLAR,
   depósitos e Com. A 3500 não.
2. **Listas anuais do BCRA.** Todo fim de ano o BCRA publica uma Comunicación "C" intitulada "Feriados que observarán
   las entidades financieras durante el año ...", com os dias de semana em que "las entidades financieras no
   funcionarán". Conferi cada lista data a data contra o arquivo. As diferenças são sempre as mesmas: a lista não traz o
   Día del Bancario (vem do convênio coletivo, não da lei), nem o que foi decidido depois da publicação (traslados
   excepcionais, censos, feriados extraordinários, asuetos de dezembro), que conferi nas normas e comunicações próprias.
   As páginas "Feriados bancarios AAAA" do site do BCRA (2011 a 2026) também foram lidas, mas têm erros de digitação
   (por exemplo 2011 com San Martín em 15/8, 2018 com "28 feb" e "18 dic"); prevalecem as Comunicaciones em PDF.
3. **Normas.** Leis, decretos e resoluções no InfoLEG / argentina.gob.ar (Boletín Oficial), e o calendário oficial do
   Governo (JSON de 2025 e 2026 em argentina.gob.ar).
4. **Conferência de terceiros.** A biblioteca Python `holidays` (vacanza), módulo da Argentina, só para comparar; ela
   não é fonte de nenhuma data.

### Grau de verificação por ano

| Ano | Lista anual do BCRA conferida | Séries do BCRA | Como foi montado |
|---|---|---|---|
| 2000 | não está no site do BCRA | sim | regra + séries + normas |
| 2001 | não está no site do BCRA | sim | regra + séries + Com. "A" da crise |
| 2002 | C 33729 | sim | lista + séries + Com. "A" da crise |
| 2003 | C 35602 | sim | lista + séries |
| 2004 | C 37026 (errata C 37066) | sim | lista + séries |
| 2005 | C 40400 | sim | lista + séries |
| 2006 | C 43915 | sim | lista + séries |
| 2007 | C 47079 | sim | lista + séries |
| 2008 | C 49743 | sim | lista + séries |
| 2009 | C 52431 | sim | lista + séries |
| 2010 | C 55024 | sim | lista + séries |
| 2011 | C 57511 | sim | lista + séries |
| 2012 | C 60197 | sim | lista + séries |
| 2013 | C 62554 | sim | lista + séries |
| 2014 | C 64929 | sim | lista + séries |
| 2015 | C 67410 | sim | lista + séries |
| 2016 | C 70049 | sim | lista + séries |
| 2017 | C 74076 (substitui a C 73620, anterior aos Decretos 52/2017 e 80/2017) | sim | lista + séries |
| 2018 | C 77542 + B 11773 | sim | lista + séries |
| 2019 | C 81592 | sim | lista + séries |
| 2020 | C 85832 + C 86916 | sim | lista + séries |
| 2021 | C 88780 | sim | lista + séries |
| 2022 | C 91610 + C 92108 | sim | lista + séries |
| 2023 | C 94199 | sim | lista + séries |
| 2024 | C 97002 | sim | lista + séries |
| 2025 | C 99321 + C 100942 | sim | lista + séries |
| 2026 | C 101352 + C 102390 + A 8487 | até 21/10 | lista + séries; novembro e dezembro só pela lista e pelas normas |
| 2027 | não publicada | não | **só regra**; faltam os dias turísticos e as decisões de dezembro |

Em resumo: 2000 a 2025 conferidos data a data em fonte oficial (séries do BCRA em todos; lista anual do BCRA de 2002
em diante); 2026 conferido na lista do BCRA e nas séries até 21/10; 2027 montado por regra.

## Regras de projeção

### Feriados e dias fixos, com os anos em que cada regra vale

| Data | Descrição no arquivo | Regra e vigência |
|---|---|---|
| 1/1 | Año Nuevo | fixo em todos os anos |
| Páscoa - 48 e - 47 (segunda e terça) | Carnaval | só desde 2011 (Decreto 1584/2010; Ley 27.399). Não era feriado de 1977 a 2010 (Ley 21.329) |
| 24/3 | Día Nacional de la Memoria por la Verdad y la Justicia | desde 2006, fixo (Ley 26.085, de março de 2006; Decreto 1584/2010; Ley 27.399). O Decreto 52/2017 o tornou trasladável por alguns dias, entre o fim de janeiro e o começo de fevereiro de 2017, e o Decreto 80/2017 desfez, sem efeito prático |
| Páscoa - 3 | Jueves Santo | día no laborable em todos os anos (Ley 21.329; Decreto 1584/2010; Ley 27.399); bancos fechados em todos os anos |
| Páscoa - 2 | Viernes Santo | fixo em todos os anos |
| 2/4 | Día del Veterano y de los Caídos en la Guerra de Malvinas | feriado desde 2001 (Ley 25.370, de novembro de 2000). De 2001 a 2006 era trasladável pela regra da Ley 23.555 (2002: 1/4; 2003: 31/3; 2004: 5/4). Fixo desde a Ley 26.110 (junho de 2006). 2020: 31/3 (Decreto 297/2020, art. 7). Em 2000 o 2/4 não era feriado (a Ley 24.160 só criou o dia comemorativo) |
| 10/6 | Día de la Afirmación de los Derechos Argentinos sobre las Malvinas, Islas y Sector Antártico | até 2000, trasladável pela Ley 23.555 (em 2000 caiu num sábado e ficou). Revogado pela Ley 25.370 |
| 1/5 | Día del Trabajo | fixo |
| 25/5 | Día de la Revolución de Mayo | fixo |
| 17/6 | Paso a la Inmortalidad del General Don Martín Miguel de Güemes | desde 2016 (Ley 27.258). 2016: fixo (o Decreto 1584/2010 só movia 17/8, 12/10 e 20/11). Desde 2017: trasladável pela regra geral; numa sexta-feira fica na sexta (Decreto 789/2021, art. 1: não se traslada para cima de um inamovível, o 20/6) |
| 20/6 | Paso a la Inmortalidad del General Don Manuel Belgrano | 1995 a 2010: terceira segunda-feira de junho (Ley 24.445, art. 4). Desde 2011: fixo (Decreto 1584/2010; Decreto 80/2017; Ley 27.399) |
| 9/7 | Día de la Independencia | fixo |
| 17/8 | Paso a la Inmortalidad del General Don José de San Martín | 1995 a 2016: terceira segunda-feira de agosto (Ley 24.445, art. 4; Decreto 1584/2010, art. 2), exceto 2011 (22/8, Decreto 521/2011, por causa das primárias). Desde 2017: trasladável pela regra geral |
| 12/10 | Día de la Raza (até 2010); Día del Respeto a la Diversidad Cultural (desde 2011) | até 2007: Ley 23.555 (fim de semana fica), com 2001 em 8/10 (Ley 25.442), 2002 em 14/10 (Decreto 1932/2002) e 2003 em 13/10 (Decreto 810/2003). 2008 a 2010: Ley 26.416 (terça e quarta vão para a segunda anterior; de quinta a domingo, para a segunda seguinte). 2011 a 2016: segunda segunda-feira de outubro (Decreto 1584/2010, art. 2). Desde 2017: regra geral; 2025 em 10/10 (Resolución JGM 139/2025, pelo Decreto 614/2025) |
| 6/11 | Día del Bancario | todos os anos, fica onde cair (Convenio Colectivo 18/75, art. 50). Não consta das listas anuais do BCRA, mas as séries do BCRA não têm dado em nenhum 6/11 de semana de 2000 a 2025 |
| 20/11 | Día de la Soberanía Nacional | desde novembro de 2010 (Decreto 1584/2010). 2010 a 2016: quarta segunda-feira de novembro, exceto 2015 (27/11, Decreto 2226/2015, por causa do segundo turno). Desde 2017: regra geral (2021 e 2022 caíram no fim de semana e não se moveram) |
| 8/12 | Día de la Inmaculada Concepción de María | fixo (feriado desde a Ley 24.445, de 1995) |
| 25/12 | Navidad | fixo |
| último dia útil do ano | Feriado bancario de fin de año | feriado bancário do Decreto 24.879/1953 ("31 de diciembre o último día hábil de cada año"); vale até 2000 (29/12/2000). Abrogado pelo Decreto 972/2001 (B.O. 2/8/2001) |

### Regras de traslado, por período

* **Ley 23.555 (1988 a novembro de 2010):** feriado em terça ou quarta vai para a segunda anterior; em quinta ou sexta,
  para a segunda seguinte; em sábado ou domingo, fica. Exceções do art. 3 (fixos): Viernes Santo, 1/5, 25/5, 20/6, 9/7,
  17/8, 8/12, 25/12 e 1/1, mais o 24/3 (Ley 26.085) e o 2/4 desde a Ley 26.110. Na prática, em 2000 a 2010 moviam-se
  por ela o 10/6 (até 2000), o 2/4 (2001 a 2006) e o 12/10 (até 2007).
* **Ley 24.445 (1995 a 2010):** 20/6 e 17/8 na terceira segunda-feira do mês.
* **Ley 26.416 (outubro de 2008 a novembro de 2010):** só o 12/10, com terça e quarta para a segunda anterior e de quinta
  a domingo para a segunda seguinte.
* **Decreto 1584/2010 (novembro de 2010 a janeiro de 2017):** revogou as leis anteriores; 17/8 na terceira segunda de
  agosto, 12/10 na segunda segunda de outubro, 20/11 na quarta segunda de novembro; os demais fixos (o 17/6 entrou fixo
  em 2016). Dois feriados turísticos por ano, fixados por triênio (Decretos 1585/2010 e 1768/2013).
* **Decreto 52/2017 e Decreto 80/2017 (janeiro a outubro de 2017), depois Ley 27.399 (outubro de 2017 em diante):**
  trasladáveis 17/6, 17/8, 12/10 e 20/11; terça e quarta vão para a segunda anterior; quinta e sexta, para a segunda
  seguinte; sábado e domingo ficam. 2017 não teve feriado turístico (o Decreto 52/2017 os suprimiu). A Ley 27.399, art. 7,
  permite até três dias turísticos por ano, em segunda ou sexta, que o Executivo deve fixar "con una antelación de
  cincuenta (50) días a la finalización del año calendario" (na prática, até meados de novembro do ano anterior; o de
  2024 saiu atrasado, em 29/12/2023).
* **Decreto 789/2021, art. 1 (novembro de 2021 em diante):** o trasladável fica na data quando o traslado o poria sobre
  um inamovível. Só afeta o 17/6 numa sexta (2022, e de novo em 2033, 2039, 2044 e 2050).
* **Decreto 614/2025, art. 1 (28/8/2025 em diante):** o trasladável que cai em sábado ou domingo "podrá" ir para a sexta
  anterior ou a segunda seguinte, conforme decidir a Jefatura de Gabinete de Ministros (autoridade de aplicação). Não é
  automático: em 2025 o 12/10 (domingo) foi para a sexta 10/10 (Resolución JGM 139/2025); o 17/8/2025, domingo, ficou
  (o decreto é posterior).

### Algoritmo para 2028 em diante

Para o ano A, com a Páscoa P de A:

1. Fixos: 1/1, P - 48, P - 47, 24/3, P - 3 (Jueves Santo), P - 2, 2/4, 1/5, 25/5, 20/6, 9/7, 6/11 (Día del Bancario),
   8/12, 25/12.
2. Trasladáveis 17/6, 17/8, 12/10 e 20/11: terça e quarta para a segunda anterior; quinta e sexta para a segunda
   seguinte; sábado e domingo ficam; o 17/6 numa sexta fica na sexta.
3. Acrescentar, quando publicados: até três dias turísticos (segunda ou sexta), o eventual traslado de fim de semana
   decidido pela JGM, o asueto de 24 e 31/12 (o BCRA decide em dezembro; desde 2019 houve asueto de dia inteiro em todos
   os anos em que essas datas caíram em dia útil) e os feriados extraordinários.

Os itens do passo 3 não são previsíveis; um calendário projetado só com os passos 1 e 2 fica incompleto e deve ser
revisto a cada ano com a nova Comunicación "C" do BCRA.

## Exceções ano a ano

| Data | Descrição no arquivo | Motivo e fonte |
|---|---|---|
| 2000-12-29 | Feriado bancario de fin de año | último dia útil de 2000; Decreto 24.879/1953 (ainda vigente; abrogado em agosto de 2001). Séries sem dado |
| 2001-10-08 | Día de la Raza | 12/10 (sexta) antecipado para a segunda anterior; Ley 25.442 |
| 2001-11-17 e 18 | Censo Nacional 2001 | censo, feriado nacional pela Ley 24.254 (no 18, até 13 h); Decreto 1154/2001. Sábado e domingo |
| 2001-12-21, 24 e 26 | Feriado bancario y cambiario | Com. "A" 3409, 3410 e 3413 (com B 7073). Ficaram habilitadas só algumas operações: pagamento de aposentadorias, saques com limite, contas de serviços e, em 26/12, depósitos e transferências entre contas |
| 2002-01-07 a 10 | Feriado bancario y cambiario | Com. "A" 3421 (7 e 8), 3422 com errata C 33690 (9) e 3424 (10), com operações habilitadas |
| 2002-02-04 a 07 | Feriado bancario y cambiario | Com. "A" 3459 (4 e 5) e 3460 (6 e 7); reabertura em 8/2 pela Com. "A" 3466 |
| 2002-04-01 | Día del Veterano y de los Caídos en la Guerra de Malvinas | 2/4 (terça) trasladado para a segunda anterior (Ley 23.555). A lista C 33729, de janeiro de 2002, dava 2/4; as séries não têm dado em 1/4 e têm em 2/4, e as Com. "A" 3539 ("antes de las 9.00 horas del martes 2 de abril") e 3546 marcam prazos para 2/4, primeiro dia útil depois do feriado |
| 2002-04-22 a 26 | Feriado bancario y cambiario | Com. "A" 3571 (a partir do fechamento de 19/4), 3576 e 3577 (até 25/4) e 3580 (26/4, feriado até 12 h e depois só operações habilitadas), com B 7222. A Com. "A" 3578 manda não contar como hábeis os dias de feriado bancário |
| 2002-10-14 | Día de la Raza | 12/10 (sábado) para a segunda seguinte; Decreto 1932/2002 |
| 2003-10-13 | Día de la Raza | 12/10 (domingo) para a segunda seguinte; Decreto 810/2003 |
| 2007-12-24 e 31 | Asueto bancario | Decreto 1781/2007 (asueto da administração pública; art. 5 convida os bancos); o BCRA aderiu e o MEP não funcionou (Comunicado P 49073). Séries sem dado |
| 2009-07-10 | Asueto por la gripe A (H1N1) | Decisión Administrativa JGM 572/2009; o BCRA aderiu, sem operações nem MEP (Comunicado P 49516); as associações de bancos aderiram e a bolsa não operou (imprensa da época) |
| 2010-05-24 | Bicentenario de la Revolución de Mayo | Decreto 615/2010 |
| 2010-10-27 | Censo Nacional 2010 | Decreto 67/2010, art. 19, e Ley 24.254 (citados na Resolución conjunta 1159/2010) |
| 2010-11-22 | Día de la Soberanía Nacional | primeiro 20/11, na quarta segunda de novembro; Decreto 1584/2010 |
| 2010-12-24 e 31 | Asueto bancario | Comunicado P 49790 (asueto do BCRA e convite às entidades, a pedido da Asociación Bancaria) e Com. "B" 9985 (sem MEP); o Comunicado P 49787, de três dias antes, anunciava atendimento normal |
| 2011-08-22 | Paso a la Inmortalidad del General Don José de San Martín | Decreto 521/2011 (a regra daria 15/8) |
| 2011-12-26 e 2012-01-02 | Asueto bancario | Com. "B" 10248 ("los días 26 de diciembre de 2011 y 2 de enero de 2012 no habrá actividad"); imprensa: bancos fechados |
| 2012-02-27 | Bicentenario de la Creación y Primera Jura de la Bandera | Ley 26.721 |
| 2012-09-24 | Bicentenario de la Batalla de Tucumán | Ley 26.763 |
| 2013-01-31 | Bicentenario de la Asamblea General Constituyente de 1813 | Ley 26.840 |
| 2013-02-20 | Bicentenario de la Batalla de Salta | Ley 26.837 |
| 2013-12-24 e 31 | Asueto bancario | Comunicado P 50318 (o mesmo comunicado diz que 26/12 e 2/1/2014 foram dias normais nos bancos) |
| 2014-12-31 | Asueto bancario | Comunicado P 50488 (em 24/12/2014 o asueto foi só a partir das 13 h) |
| 2015-11-27 | Día de la Soberanía Nacional | Decreto 2226/2015 (a regra daria 23/11) |
| 2018-11-30 | Cumbre de Líderes del G20 | Decreto 967/2018 (feriado na CABA) e Com. "B" 11773 (feriado bancário em todo o país) |
| 2019-12-24 e 31 | Asueto bancario | Comunicado P 50962 |
| 2020-03-31 | Día del Veterano y de los Caídos en la Guerra de Malvinas | Decreto 297/2020, art. 7, e Com. "C" 86916 |
| 2020-12-24 e 31 | Asueto bancario | Comunicado P 50983 |
| 2021-12-24 e 31 | Asueto bancario | Comunicado P 51020 |
| 2022-05-18 | Censo Nacional 2022 | Decreto 42/2022 e Com. "C" 92108 |
| 2022-09-02 | Feriado nacional extraordinario (DNU 573/2022) | DNU 573/2022, depois do atentado contra a vice-presidente; o BCRA informou que as agências não abririam (imprensa); séries sem dado |
| 2024-12-24 e 31 | Asueto bancario | Comunicado P 51155 |
| 2025-10-10 | Día del Respeto a la Diversidad Cultural | 12/10 (domingo) para a sexta anterior; Decreto 614/2025, Resolución JGM 139/2025, Com. "C" 100942 |
| 2025-12-24 e 31 | Asueto bancario | Comunicado P 51199 |
| 2026-11-09 | Visita de Su Santidad el Papa León XIV | DNU 1103/2026, art. 2 (feriado nacional); Com. "C" 102390 |
| 2026-11-10 | Feriado bancario por la visita de Su Santidad el Papa León XIV | Com. "A" 8487 (feriado bancário em todo o país) e DNU 1103/2026, art. 3 (feriado na CABA e em Córdoba) |

Feriados turísticos (todos no arquivo): 2011: 25/3 e 9/12; 2012: 30/4 e 24/12; 2013: 1/4 e 21/6 (Decreto 1585/2010);
2014: 2/5 e 26/12; 2015: 23/3 e 7/12; 2016: 8/7 e 9/12 (Decreto 1768/2013); 2017: nenhum; 2018: 30/4, 24/12 e 31/12;
2019: 8/7, 19/8 e 14/10 (Decreto 923/2017, "días no laborables con fines turísticos"); 2020: 23/3, 10/7 e 7/12
(Decreto 717/2019); 2021: 24/5, 8/10 e 22/11 (Decreto 947/2020); 2022: 7/10, 21/11 e 9/12 (Decreto 789/2021); 2023:
26/5, 19/6 e 13/10 (Decreto 764/2022); 2024: 1/4, 21/6 e 11/10 (Decreto 106/2023); 2025: 2/5, 15/8 e 21/11 (Decreto
1027/2024, "días no laborables con fines turísticos"); 2026: 23/3, 10/7 e 7/12 (Resolución JGM 164/2025). No arquivo, a
descrição segue a norma de cada ano: "Feriado con fines turísticos" ou "Día no laborable con fines turísticos".

## Datas duvidosas (não estão no arquivo)

| Data | Motivo da dúvida |
|---|---|
| 2020-03-20, 2020-03-25, 2020-03-26, 2020-03-27 e 2020-03-30 | Isolamento obrigatório da covid-19. A Com. "A" 6942 proibiu abrir agências ao público de 20 a 31/3/2020, suspendeu a compensação de cheques e passou para 1/4 os vencimentos de financiamentos desses dias; mas não houve feriado bancário: câmbio, MEP e serviços remotos funcionaram, a Com. "C" 86916 diz que nesses dias as entidades "operarán con el alcance previsto en la Comunicación A 6942" e as séries do BCRA têm dado. Para prazos, são dias úteis com agências fechadas. (23, 24 e 31/3 já são feriados no arquivo; de 1 a 12/4 as agências seguiram fechadas ao público em geral, com atendimento a aposentados desde 3/4, mas a compensação voltou em 1/4, Com. "A" 6949.) |
| 2026-12-24 e 2026-12-31 | Asueto ainda não anunciado (o BCRA costuma anunciar em meados de dezembro). Houve asueto de dia inteiro em 2019, 2020, 2021, 2024 e 2025, os anos em que as datas caíram em dia útil desde 2019 |
| 2027-12-24 e 2027-12-31 | Idem |
| 2027-11-19 ou 2027-11-22 | O 20/11/2027 cai num sábado; pelo Decreto 614/2025 a JGM pode passá-lo para a sexta 19/11 ou para a segunda 22/11, ou deixá-lo no sábado. Ainda não decidido; o arquivo traz o 20/11 (sábado) |
| 2027, dias turísticos | Até três, em segunda ou sexta, ainda não fixados (o prazo legal é 50 dias antes do fim de 2026). Nenhuma data no arquivo |

## Datas investigadas e descartadas

| Data | Por que não está no arquivo |
|---|---|
| 2002-01-14 | BADLAR e taxa de depósitos sem dado, mas não houve feriado bancário: a última prorrogação foi até 10/1 (Com. "A" 3424); a Com. "A" 3426 prorrogou para 14/1/2002 os vencimentos de operações ativas, e o BCRA emitiu nesse dia as Com. "A" 3432, "A" 3433 e "C" 33729 |
| 2007-12-10 | asueto só a partir das 12 h (Decreto 1781/2007; Comunicado P 49053) |
| 2010-12-23 e 30, 2013-12-26, 2014-01-02, 2015-01-02 | asueto da administração pública que excluiu os bancos (Comunicados P 49787, P 50318 e P 50488) |
| 2011-12-23 e 30 | bancos abertos em horário reduzido (Com. "B" 10248) |
| 2012-12-31 | asueto a partir das 13 h (Comunicado P 50139) |
| 2014-12-24 | asueto a partir das 13 h (Comunicado P 50488) |
| 2015-12-24 e 31 | bancos abertos até 13 h (Comunicados P 50674 e P 50677) |
| 2022-12-20 | feriado nacional pela conquista da Copa (Decreto 842/2022), mas o art. 3 manteve o pessoal dos bancos trabalhando até 12 h; o BCRA confirmou atendimento até o meio-dia e as séries têm dado. Não é dia sem expediente |
| 2026-11-11 | feriado só na Província de Buenos Aires (DNU 1103/2026, art. 4); não há feriado bancário nem na CABA |
| dias não laboráveis religiosos (Rosh Hashaná, Yom Kipur, Pessach, Id al-Fitr, Id al-Adha, 24/4) | optativos para quem professa a religião; os bancos abrem (as séries têm dado) |

## Fontes

BCRA, dados e listas:

* API de estatísticas monetárias v4.0 (séries diárias usadas): `https://api.bcra.gob.ar/estadisticas/v4.0/monetarias/{1|5|7|12}?desde=AAAA-01-01&hasta=AAAA-12-31`
* Listas anuais de feriados (Comunicaciones "C"), todas em `https://www.bcra.gob.ar/archivos/Pdfs/comytexord/<número>.pdf`:
  C33729 (2002), C35602 (2003), C37026 e C37066 (2004), C40400 (2005), C43915 (2006), C47079 (2007), C49743 (2008),
  C52431 (2009), C55024 (2010), C57511 (2011), C60197 (2012), C62554 (2013), C64929 (2014), C67410 (2015), C70049 (2016), C73620 e C74076 (2017), C77542 (2018), C81592 (2019), C85832 e C86916 (2020), C88780 (2021),
  C91610 e C92108 (2022), C94199 (2023), C97002 (2024), C99321 e c100942 (2025), C101352 e C102390 (2026).
  Exemplo: https://www.bcra.gob.ar/archivos/Pdfs/comytexord/C101352.pdf
* Páginas "Feriados bancarios" do BCRA (com erros; só conferência): https://www.bcra.gob.ar/consulta-feriados-bancarios-2018/ (troque o ano, 2011 a 2026)
* Crise de 2001 e 2002: https://www.bcra.gob.ar/archivos/Pdfs/comytexord/A3409.pdf , A3410.pdf , A3413.pdf , A3416.pdf , B7073.pdf , A3421.pdf , A3422.pdf , C33690.pdf , A3424.pdf , A3426.pdf , A3459.pdf , A3460.pdf , A3466.pdf , A3539.pdf , A3546.pdf , A3571.pdf , A3576.pdf , A3577.pdf , A3578.pdf , A3580.pdf , B7222.pdf (mesmo endereço base)
* Asuetos e feriados bancários: P49053.pdf , P49073.pdf (2007), P49516.pdf (2009), P49787.pdf , P49790.pdf , B9978.pdf , B9985.pdf (2010), B10248.pdf (2011), P50139.pdf (2012), P50318.pdf (2013), P50488.pdf (2014), P50674.pdf , P50677.pdf (2015), B11773.pdf (2018), P50962.pdf (2019), p50983.pdf (2020), P51020.pdf (2021), P51155.pdf (2024), P51199.pdf (2025), A8487.pdf (2026), A6942.pdf e A6949.pdf (covid-19), no mesmo endereço base

Normas (InfoLEG / argentina.gob.ar, texto original):

* Decreto 24.879/1953 (feriado bancário do último dia útil; nota de abrogação pelo Decreto 972/2001): https://www.argentina.gob.ar/normativa/nacional/norma-175381/texto
* Ley 21.329: https://www.argentina.gob.ar/normativa/nacional/ley-21329-65453/texto
* Ley 23.555: https://www.argentina.gob.ar/normativa/nacional/ley-23555-20986/texto
* Ley 24.160: https://www.argentina.gob.ar/normativa/nacional/ley-24160-558/texto
* Ley 24.254 (dia do censo é feriado nacional): https://biblioteca.afip.gob.ar/dcp/LEY_C_024254_1993_10_13
* Ley 24.445: https://www.argentina.gob.ar/normativa/nacional/ley-24445-782/texto
* Ley 25.370: https://www.argentina.gob.ar/normativa/nacional/ley-25370-65442/texto
* Ley 25.442: https://www.argentina.gob.ar/normativa/nacional/ley-25442-67888/texto
* Decreto 1154/2001 (censo 2001): https://www.argentina.gob.ar/normativa/nacional/norma-68781/texto
* Decreto 1932/2002: https://www.argentina.gob.ar/normativa/nacional/decreto-1932-2002-78207/texto
* Decreto 810/2003: https://argentina.justia.com/federales/decretos/decreto-n-810-2003-sep-24-2003/gdoc
* Ley 26.085: https://www.argentina.gob.ar/normativa/nacional/ley-26085-114811/texto
* Ley 26.110: https://www.argentina.gob.ar/normativa/nacional/ley-26110-117507/texto
* Decreto 1781/2007: https://docs.argentina.justia.com/federales/decretos/decreto-n-1781-2007-dec-4-2007.pdf
* Ley 26.416: https://www.argentina.gob.ar/normativa/nacional/ley-26416-145231/texto
* Decreto 615/2010: https://www.argentina.gob.ar/normativa/nacional/decreto-615-2010-166825/texto
* Resolución conjunta 1159/2010 (cita o Decreto 67/2010, censo 2010): https://www.argentina.gob.ar/normativa/nacional/resolución-1159-2010-173748/texto
* Decreto 1584/2010: https://www.argentina.gob.ar/normativa/nacional/decreto-1584-2010-174389/texto
* Decreto 1585/2010: https://www.argentina.gob.ar/normativa/nacional/decreto-1585-2010-174391/texto
* Decreto 521/2011: https://www.argentina.gob.ar/normativa/nacional/decreto-521-2011-181754/texto
* Ley 26.721: https://www.argentina.gob.ar/normativa/nacional/ley-26721-191835/texto
* Ley 26.763: https://www.argentina.gob.ar/normativa/nacional/ley-26763-201915/texto
* Ley 26.837: https://www.argentina.gob.ar/normativa/nacional/ley-26837-207405/texto
* Ley 26.840: https://www.argentina.gob.ar/normativa/nacional/ley-26840-207258/texto
* Decreto 1768/2013: https://www.argentina.gob.ar/normativa/nacional/decreto-1768-2013-222021/texto
* Decreto 2226/2015: https://www.argentina.gob.ar/normativa/nacional/decreto-2226-2015-253971/texto
* Ley 27.258: https://www.argentina.gob.ar/normativa/nacional/ley-27258-262574/texto
* Decreto 52/2017: https://www.argentina.gob.ar/normativa/nacional/decreto-52-2017-271094/texto
* Decreto 80/2017: https://www.argentina.gob.ar/normativa/nacional/decreto-80-2017-271382/texto
* Ley 27.399: https://www.argentina.gob.ar/normativa/nacional/ley-27399-281835/texto
* Decreto 923/2017: https://www.argentina.gob.ar/normativa/nacional/decreto-923-2017-287145/texto
* Decreto 967/2018: https://www.argentina.gob.ar/normativa/nacional/decreto-967-2018-315818/texto
* Decreto 717/2019: https://www.argentina.gob.ar/normativa/nacional/decreto-717-2019-330204/texto
* Decreto 297/2020: https://www.argentina.gob.ar/normativa/nacional/decreto-297-2020-335741/texto
* Decreto 947/2020: https://www.argentina.gob.ar/normativa/nacional/decreto-947-2020-344620/texto
* Decreto 789/2021: https://www.argentina.gob.ar/normativa/nacional/decreto-789-2021-356678/texto
* Decreto 42/2022: https://www.argentina.gob.ar/normativa/nacional/decreto-42-2022-360018/texto
* DNU 573/2022: https://www.argentina.gob.ar/normativa/nacional/decreto-573-2022-370795/texto
* Decreto 764/2022: https://www.argentina.gob.ar/normativa/nacional/decreto-764-2022-375264/texto
* Decreto 842/2022: https://www.argentina.gob.ar/normativa/nacional/decreto-842-2022-376857/texto
* Decreto 106/2023: https://www.argentina.gob.ar/normativa/nacional/decreto-106-2023-395689/texto
* Decreto 1027/2024: https://www.argentina.gob.ar/normativa/nacional/decreto-1027-2024-406417/texto
* Decreto 614/2025: https://www.argentina.gob.ar/normativa/nacional/norma-416911/texto
* Resolución JGM 164/2025 (dias turísticos de 2026): https://www.argentina.gob.ar/normativa/nacional/resolución-164-2025-421799/texto
* DNU 1103/2026: https://www.argentina.gob.ar/normativa/nacional/norma-430580/texto
* Calendário oficial do Governo: https://www.argentina.gob.ar/interior/feriados e os dados em https://www.argentina.gob.ar/sites/default/files/holidays-2025-es.json e https://www.argentina.gob.ar/sites/default/files/holidays-2026-es.json

Imprensa da época (só onde a norma não bastava):

* 2009-07-10: https://www.econoblog.com.ar/2184/feriado-sanitario-no-habra-bancos-adelantan-pago-a-los-jubilados/ e https://www.diariodecuyo.com.ar/noticias/en-asueto-no-hay-bancos-ni-opera-la-bolsa-pero-abren-comercios-965877.html
* 2010-12-24 e 31: https://www.econoblog.com.ar/?p=9967
* 2011-12-26 e 2012-01-02: https://www.diariodecuyo.com.ar/noticias/los-bancos-estaran-cerrados-el-26-de-diciembre-y-el-2-de-enero-879203.html e https://www.econoblog.com.ar/?p=15111
* 2022-09-02: https://news.agrofy.com.ar/noticia/201173/no-abren-bancos-feriado-nacional-2-septiembre-tampoco-habra-mercados e https://blogdelcontador.com.ar/no-hay-actividad-bancaria-ni-cambiaria-por-el-feriado-nacional-decretado-para-el-viernes-2-de-septiembre
* 2022-12-20: https://www.perfil.com/noticias/economia/feriado-nacional-los-bancos-atienden-hasta-el-mediodia-y-manana-para-el-provincia.phtml
* Día del Bancario (CCT 18/75, art. 50): https://www.infobae.com/economia/2025/11/05/por-que-no-hay-bancos-manana-6-de-noviembre/ e https://labancaria.org/convenio-colectivo-18-75/
* 2027 ainda sem dias turísticos (setembro de 2026): https://calcularsueldo.com.ar/interes-general/16229/feriados-2027-calendario-oficial-y-fines-de-semana-largos.html

Conferência de terceiros: https://github.com/vacanza/holidays/blob/dev/holidays/countries/argentina.py (difere das fontes
oficiais em alguns pontos: dá o Jueves Santo só desde 2011; não tem o feriado bancário de 29/12/2000, os da crise de 2001
e 2002, o asueto de 10/7/2009, os de dezembro de 2007, 2010, 2011, 2013 e 2014, nem o 2/9/2022; e conta o 20/12/2022
como feriado, quando os bancos abriram até as 12 h. Nenhuma data do arquivo veio dela).

## Como o arquivo é conferido

O teste `CalendarioSegueAsRegrasDaPraca`, em `tests/Elekto.Calendars.Tests/CalendarRulesTests.cs`, implementa as regras e
exceções acima, escritas de novo a partir deste documento, e confere o arquivo contra elas, data a data; o teste
`FeriadoTemADescricaoDaRegra` confere as descrições. A cada ano, a nova Comunicación "C" do BCRA traz os dias turísticos e
os traslados do ano seguinte, e os comunicados de dezembro, os asuetos.
