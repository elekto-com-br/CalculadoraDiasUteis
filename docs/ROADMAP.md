# Roadmap da versão 2.0

Levar a Calculadora de Dias Úteis para WinUI: bonita, acessível, moderna e distribuída pela Microsoft Store.

É um projeto de baixa prioridade, tocado aos poucos. Por isso o plano está dividido em fases pequenas, e cada uma termina com algo que compila e funciona; dá para parar em qualquer fase sem deixar o projeto quebrado.

Atualizado em 25/10/2026. Fase atual: 0.

## Onde estamos

- A versão 1.2 (WinForms, .NET Framework 4.8) é a última dessa linha. Ela fica no `master` até a 2.0 ficar pronta, recebendo apenas correção de feriado que não puder esperar.
- O trabalho da 2.0 acontece neste branch, `winui`, aberto a quem quiser acompanhar.

## Princípios

- **O cálculo não muda sem prova.** Toda mudança no núcleo passa por testes que comparam com os resultados da 1.2.
- **Acessível desde o começo.** Narrador, navegação só por teclado, alto contraste e escala de texto entram na primeira tela, não no fim.
- **Privacidade igual à de hoje.** Sem telemetria, sem conta, sem internet; a única saída continua sendo o link para o site da Elekto.
- **Os dados do usuário são dele.** Tudo o que ele cadastra pode ser exportado num formato aberto.

## Decisões tomadas

| Tema | Decisão | Por quê |
|---|---|---|
| Interface | WinUI (Windows App SDK 2.x) | É a plataforma nativa de longo prazo da Microsoft; o UWP não evolui mais. |
| Windows mínimo | Windows 11 | Lançado em 2021; quem ficou no Windows 10 continua com a 1.2. |
| Runtime | .NET 10 (LTS) | Suporte até novembro de 2028; a troca para a LTS seguinte é simples. |
| Distribuição | MSIX pela Microsoft Store | Cadastro gratuito, assinatura digital feita pela própria Store (some o aviso do SmartScreen) e atualização automática. |
| Ferramentas | dotnet CLI, WinApp CLI, VS Code ou Visual Studio | O Visual Studio deixa de ser obrigatório. Para o Claude Code, o plugin oficial `winui@win-dev-skills` da Microsoft. |
| Dados do usuário | SQLite (Microsoft.Data.Sqlite) | Ver [Armazenamento](#armazenamento). |
| Exportação | Mantida, com outra biblioteca e mais formatos | Ver [Exportação](#exportação). |

## Fases

### Fase 0: núcleo e testes

Ainda sem interface nova. O objetivo é proteger a lógica antes de mexer em qualquer coisa.

- Criar a estrutura `src/` e `tests/`, com solução nova.
- Mover o conteúdo de `Code/` para a biblioteca `Elekto.Calendars` (.NET 10), sem mudar comportamento, e tornar públicos os tipos que a interface usa. O exportador fica de fora: ele depende do EPPlus e será refeito na Fase 3.
- Testes de referência: gerar com a 1.2 milhares de casos (pares de datas, prazos e todas as combinações de opções, nos quatro calendários) e exigir resultado idêntico da biblioteca nova.
- Teste de regras dos calendários: o verificador usado na 1.2, que compara os feriados esperados por regra com os arquivos, vira teste automatizado e acusa arquivo desatualizado.
- Integração contínua no GitHub Actions: compilar e rodar os testes a cada push.

**Pronto quando** a biblioteca passa em todos os testes de referência na integração contínua.

### Fase 1: aplicativo WinUI com as funções da 1.2

- Projeto WinUI empacotado, criado a partir do template oficial.
- Uma janela com as funções da 1.2: escolher o calendário; contar dias úteis e corridos entre duas datas, no critério financeiro ou de dias cheios; calcular a data final de um prazo em dias úteis ou corridos, com ajuste.
- Resultado recalculado ao mudar qualquer campo, sem botão de calcular.
- Controles nativos (`CalendarDatePicker`, `NumberBox`, `RadioButtons`, `ComboBox`), fundo Mica e tema claro, escuro ou de alto contraste seguindo o Windows.
- `AutomationProperties.Name` e `AutomationId` em todos os controles, ordem de tabulação revisada e atalhos de teclado.
- Página "Sobre" com versão, licença e link para o site.
- Ícone e identidade visual feitos a partir do logotipo da Elekto em vetor.
- Testes de interface com `winapp ui`, rodando no Windows Sandbox.

**Pronto quando** a 2.0 faz tudo o que a 1.2 faz, menos editar calendários e exportar, e passa nos testes de interface e na verificação do Accessibility Insights.

### Fase 2: calendários editáveis

- **Modelo.** Os calendários enviados pela Elekto vêm dentro do pacote, somente leitura, no formato texto de hoje. As alterações do usuário ficam guardadas à parte, no banco local.
- **Calendário derivado.** O usuário cria um calendário a partir de outro, por exemplo "Fórum da Lapa" como o br-BC mais os feriados locais, e ele continua recebendo as atualizações que a Elekto fizer no original.
- **Operações.** Criar, renomear e apagar calendários próprios; incluir feriado; editar a descrição; remover feriado, inclusive de um calendário original, como exceção local.
- **Descrição dos feriados.** Cada feriado pode ter uma descrição, opcional. Os calendários originais passam a vir com a descrição de cada feriado (Carnaval, Tiradentes, Juneteenth...).
- **Restaurar.** Desfazer todas as alterações de um calendário original, ou de todos de uma vez.
- **Atualizações.** Quando uma versão nova do app traz feriados oficiais novos, as alterações do usuário continuam valendo por cima deles.
- **Importar e exportar** no formato texto da 1.x, para aproveitar os arquivos de quem usava a pasta `Calendars` ou o parâmetro `--calendars`. Isso também protege o usuário: desinstalar um app MSIX apaga a pasta local dele.
- Tela de feriados por ano, com o dia da semana e a origem de cada data (original, incluída ou removida pelo usuário).

**Pronto quando** dá para criar um calendário derivado, editá-lo, fechar e reabrir o app, receber uma atualização do original e restaurar, tudo coberto por testes.

### Fase 3: exportação e integração

- Copiar para a área de transferência, de modo que a lista cole no Excel como datas e não como texto. É o que a maioria faz no dia a dia.
- Exportar `.xlsx` e `.csv`, tanto as datas de um período quanto os feriados de um calendário.
- Exportar `.ics` (iCalendar), para levar os feriados ao Outlook ou ao Google Agenda: em discussão, só entra se houver um caso de uso convincente.

### Fase 4: Microsoft Store

- Conta de desenvolvedor no Partner Center (gratuita) e reserva do nome do app. A reserva pode ser feita a qualquer momento, antes mesmo de existir o app.
- Material da loja: descrição, capturas de tela, ícones, classificação etária (questionário IARC) e URL da política de privacidade.
- Envio, certificação e publicação.
- Feriado novo passa a ser versão nova do app, que a Store entrega sozinha a quem o tem instalado.
- Merge deste branch no `master`, removendo o projeto WinForms. A 1.2 continua disponível nos Releases do GitHub.

### Fora de escopo: versão web

Não haverá versão web. A Elekto já manteve a calculadora no próprio site e a retirou do ar por causa do uso indevido da página por terceiros.

## Armazenamento

Os calendários originais ficam no pacote, no formato texto de hoje, que é simples, aberto e fácil de revisar num diff. As alterações do usuário vão para um arquivo SQLite na pasta local do app (`ApplicationData.LocalFolder`); as preferências simples, como o último calendário e as opções marcadas, vão para as configurações locais do app.

SQLite em vez de JSON ou texto porque a gravação é atômica (o arquivo não se corrompe se o computador desligar no meio), as consultas da tela de feriados ficam simples e há espaço para crescer sem inventar formato. É o caminho que a Microsoft documenta para dados estruturados em apps Windows, e o volume aqui é pequeno, alguns milhares de linhas.

## Exportação

Exportar para o Excel ainda vale a pena: o público da calculadora (finanças, tesouraria, jurídico) trabalha em planilha. O que muda é a forma.

- Copiar e colar resolve a maior parte dos casos, sem arquivo nenhum.
- O `.xlsx` continua para quem quer um arquivo, com as datas gravadas como datas. O `.csv` fica como alternativa, mas sofre com separador e formato de data no Excel em português.
- O EPPlus 4.5.3.3 sai. É a última versão LGPL, de 2020, sem manutenção, e as versões novas exigem licença comercial. No lugar entra o ClosedXML, de licença MIT.
- O `.ics`, que levaria os feriados para a agenda, está em discussão.

## Manutenção dos calendários

Uma revisão por ano, em janeiro, quando o calendário de negociação da B3 do ano já saiu:

- br-BC e br-SP: conferir o calendário da B3 e se houve lei criando ou mudando feriado nacional;
- us-NYC: conferir o calendário de feriados da NYSE, publicado com três anos de antecedência;
- gb-LND: conferir os feriados bancários da Inglaterra e do País de Gales no gov.uk;
- rodar o teste de regras e publicar versão nova se algo mudou.

## Decisões em aberto

- **Conta na Store.** Em nome da Elekto (conta de empresa, gratuita desde maio de 2026, com verificação da empresa) ou de pessoa física. Para propaganda, o que interessa é o nome Elekto na página do app.
- **Idiomas.** Só português, ou também inglês? A sugestão é começar em português, já com os textos em arquivos de recursos para traduzir depois.
- **Escopo do br-SP.** A 1.2 o alinhou à B3. Quem precisar dos feriados da cidade de São Paulo (25/01 e 9/07) poderá criar um calendário derivado na Fase 2, ou a Elekto pode enviar um calendário da cidade pronto.
- **Contribuições externas.** O README oferece licenciamento alternativo ao GPLv3. Para isso continuar possível, a Elekto precisa deter os direitos de todo o código, o que pede um acordo de contribuição (CLA) de quem enviar pull request.
- **Política de privacidade.** O texto do README serve, mas a Store pede uma URL própria, no site da Elekto ou no GitHub Pages.
