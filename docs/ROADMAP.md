# Roadmap da versão 2.0

Levar a Calculadora de Dias Úteis para WinUI: bonita, acessível, moderna e distribuída pela Microsoft Store.

É um projeto de baixa prioridade, tocado aos poucos. Por isso o plano está dividido em fases pequenas, e cada uma termina com algo que compila e funciona; dá para parar em qualquer fase sem deixar o projeto quebrado.

Atualizado em 25/10/2026. Fase 0 concluída; na fase 1, o aplicativo está pronto e faltam as verificações que pedem alguém diante do computador.

## Onde estamos

- A linha 1.x (WinForms, .NET Framework 4.8) termina na 1.2.1. Ela fica no `master` até a 2.0 ficar pronta, recebendo apenas correção que não puder esperar.
- O trabalho da 2.0 acontece neste branch, `winui`, aberto a quem quiser acompanhar.
- **Ponto de parada (25/10/2026):** o aplicativo WinUI faz as contas da 1.2 e passa nos testes de interface no Windows Sandbox. Para fechar a fase 1 falta o teste de teclado com a sessão do Windows desbloqueada, a verificação com o Accessibility Insights e o Narrador, além das pendências listadas na [situação da fase 1](#fase-1-aplicativo-winui-com-as-funções-da-12).

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
| Idiomas | Português, espanhol e inglês | Português primeiro; espanhol e inglês na fase 5, com a interface preparada para tradução desde a fase 1. |

## Fases

### Fase 0: núcleo e testes

Ainda sem interface nova. O objetivo é proteger a lógica antes de mexer em qualquer coisa.

- Criar a estrutura `src/` e `tests/`, com solução nova.
- Mover o conteúdo de `Code/` para a biblioteca `Elekto.Calendars` (.NET 10), sem mudar comportamento, e tornar públicos os tipos que a interface usa. O exportador fica de fora: ele depende do EPPlus e será refeito na Fase 3.
- Testes de referência: gerar com a 1.2 milhares de casos (pares de datas, prazos e todas as combinações de opções, nos quatro calendários) e exigir resultado idêntico da biblioteca nova.
- Teste de regras dos calendários: o verificador usado na 1.2, que compara os feriados esperados por regra com os arquivos, vira teste automatizado e acusa arquivo desatualizado.
- Integração contínua no GitHub Actions: compilar e rodar os testes a cada push.

**Pronto quando** a biblioteca passa em todos os testes de referência na integração contínua.

**Situação:** concluída em 25/10/2026. A biblioteca em .NET 10 reproduz a 1.2 nos 32.065 casos de referência, os calendários batem com as regras de cada praça, e os testes rodam no GitHub Actions a cada push. Comportamentos da 1.2 que a fase revelou:

- **Corrigido:** a 1.2 descarta em silêncio o feriado que tiver descrição no arquivo, embora o formato documentado a permita.
- **Corrigido:** num calendário com um só feriado, somar dias úteis até exatamente o último dia do calendário deixava o programa em laço infinito. Nos calendários da Elekto isso não acontece, mas aconteceria nos criados pelo usuário na fase 2.
- **A decidir:** a partir do último dia do calendário, um prazo negativo de mais de 5 dias úteis lança exceção, enquanto um de até 5 devolve a data. O resultado depende de qual dos dois algoritmos internos do `AddWorkDays` é usado.

### Fase 1: aplicativo WinUI com as funções da 1.2

- Projeto WinUI empacotado, criado a partir do template oficial.
- Uma janela com as funções da 1.2: escolher o calendário; contar dias úteis e corridos entre duas datas, no critério financeiro ou de dias cheios; calcular a data final de um prazo em dias úteis ou corridos, com ajuste.
- Resultado recalculado ao mudar qualquer campo, sem botão de calcular.
- Controles nativos (`CalendarDatePicker`, `NumberBox`, `RadioButtons`, `ComboBox`), fundo Mica e tema claro, escuro ou de alto contraste seguindo o Windows.
- `AutomationProperties.Name` e `AutomationId` em todos os controles, ordem de tabulação revisada e atalhos de teclado.
- Página "Sobre" com versão, licença e link para o site.
- Todos os textos em arquivos de recursos (`.resw`), e datas e números no formato da cultura do Windows, para que a tradução da fase 5 não exija mudança de código.
- Ícone e identidade visual feitos a partir do logotipo da Elekto em vetor (`assets/logo-elekto.svg`), com as cores da marca: verde `#556652` e laranja `#F79646`.
- Testes de interface com `winapp ui`, rodando no Windows Sandbox.

**Pronto quando** a 2.0 faz tudo o que a 1.2 faz, menos editar calendários e exportar, e passa nos testes de interface e na verificação do Accessibility Insights.

**Situação:** aplicativo pronto em 25/10/2026, em `src/CalculadoraDiasUteis`, criado com o template `winui-mvvm` do WinApp CLI (Windows App SDK 2.5, CommunityToolkit.Mvvm). Os testes de interface (`tests/UiTests`) cobrem 19 cenários: os padrões, a escolha de datas no calendário, os dois critérios de contagem, prazos em dias úteis (inclusive negativos) e corridos com os três ajustes, o aviso de prazo além do calendário, a troca de calendário, a página "Sobre", a licença no navegador, os `AutomationId` e as escolhas lembradas ao reabrir. Os resultados esperados saem da própria biblioteca (`Expected.cs`), para as datas que o teste escolhe. No Sandbox passam 18; o de teclado (F1 e Esc) só roda com a sessão do Windows desbloqueada. Os temas claro, escuro e de alto contraste e a janela estreita foram conferidos por captura de tela. A integração contínua compila também o aplicativo.

Decisões tomadas na fase:

- **Windows 11 no manifesto**, para x64 e ARM64. Não há pacote para x86, que o Windows 11 não tem.
- **Datas e números no idioma que o Windows escolhe para o aplicativo**, o mesmo dos textos e dos seletores de data. Por enquanto é sempre o português; num Windows em inglês, seguir só o formato regional deixaria a data final como 10/27/2027 ao lado de seletores em 27/10/2027. Na fase 5, com os três idiomas, o formato acompanha o idioma escolhido.
- **Cor de destaque é o verde da marca**, com tons calculados para manter contraste de pelo menos 4,5:1 nos temas claro e escuro; o laranja fica no ícone e no logotipo. No alto contraste valem as cores do Windows.
- **As escolhas lembradas são as da 1.2:** calendário, critério de contagem, tipo de prazo, ajuste e prazo. As datas começam sempre em hoje e daqui a um ano, como lá.
- **Novidade pequena:** a data final mostra o dia da semana e avisa quando cai em feriado ou dia não útil, o que só acontece em dias corridos sem ajuste.
- **A licença vai no pacote**, no original e na tradução, e abre no programa padrão do Windows para cada tipo de arquivo.
- **Ficam para a fase 2** o link "Definições de Calendários..." e o parâmetro `--calendars` da 1.2, que dependem de calendários fora do pacote.
- **O editor é o VS Code**, com a configuração em `.vscode`: o F5 compila e usa a extensão WinApp, ainda em prévia, para abrir o app empacotado e anexar o depurador. A versão atual dela só aceita a pasta compilada; quando aceitar o projeto, a tarefa de compilação do `launch.json` deixa de ser necessária. A solução passou para o formato `.slnx`.

Falta, para fechar a fase:

- Usar o aplicativo instalado no Windows do dia a dia, fora do Sandbox, inclusive o botão "Ler a licença": o Sandbox não tem programa associado a `.txt`, então ele só foi testado lá com a tradução, em HTML.
- Rodar os testes de interface com a sessão do Windows desbloqueada, para o teste de teclado (F1 e Esc).
- Conferir que a dica do botão "Sobre" mostra a tecla F1. O "(F1)" escrito à mão saiu do texto porque o WinUI acrescenta a tecla sozinho, mas isso só se vê passando o mouse, o que não deu para testar com a sessão bloqueada.
- Verificar com o Accessibility Insights e percorrer a tela com o Narrador.
- Revisar os textos da página "Sobre", adaptados da 1.2 (o original mandava baixar a aplicação do site da Elekto).
- Decidir o logotipo no tema escuro: em verde sobre fundo escuro, ele fica legível, mas apagado.

Fica para depois, sem impedir o fim da fase:

- O comportamento do `AddWorkDays` na borda do calendário, que a fase 0 deixou [a decidir](#fase-0-núcleo-e-testes). Precisa ser resolvido até a fase 2, quando os calendários do usuário podem terminar em qualquer data.

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
- Identidade do pacote: o `Package.appxmanifest` usa por enquanto `Elekto.CalculadoraDiasUteis` e o publicador provisório `CN=Elekto`, que a Store substitui pelos da conta quando o app for associado a ela.
- Testar o pacote de publicação em Release, que é compactado (*trimmed*) ao contrário do usado no desenvolvimento. A compilação em Release já avisa (IL2104) que dois assemblies do Windows SDK, da própria Microsoft, não são totalmente seguros para essa compactação.
- O que se distribui é o MSIX assinado (`winapp pack`). Publicar numa pasta (`dotnet publish`, ou o Publicar do VS Code) não serve: fora do pacote, o app não encontra o Windows App SDK e fecha ao abrir.
- Envio, certificação e publicação.
- Feriado novo passa a ser versão nova do app, que a Store entrega sozinha a quem o tem instalado.
- Merge deste branch no `master`, removendo o projeto WinForms. A 1.2 continua disponível nos Releases do GitHub.

### Fase 5: espanhol, inglês e calendários dos vizinhos

- Interface traduzida para espanhol e inglês, escolhida pelo idioma do Windows; o português continua sendo o idioma padrão.
- Página da Store e capturas de tela nos três idiomas.
- Calendários novos com os dias sem expediente bancário da Argentina (Buenos Aires), do Paraguai (Assunção) e do Uruguai (Montevidéu), segundo o banco central de cada país, de 2000 em diante e projetados até onde as regras permitirem. Nomes provisórios: `ar-BA`, `py-ASU` e `uy-MVD`.
- Teste de regras para os três, como os que já existem para os calendários atuais.
- As descrições dos feriados ficam no idioma do país: português para o Brasil, espanhol para os vizinhos, inglês para Nova Iorque e Londres.

Esses calendários dão mais trabalho que os atuais. A Argentina define por decreto, a cada ano, os feriados "ponte" (*días no laborables con fines turísticos*) e muda alguns feriados de dia; Uruguai e Paraguai também transferem certos feriados para a segunda-feira, por lei. Parte de cada ano só se conhece quando o governo publica, então a projeção para o futuro cobre apenas o que as regras fixas permitem.

**Pronto quando** o app troca de idioma junto com o Windows, sem texto em português sobrando, e os três calendários novos passam no teste de regras.

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

A Elekto revisa os calendários todo ano em 1º de dezembro, com uma segunda passada em 15 de dezembro para o que tiver saído depois. Em cada revisão, os calendários são atualizados com o que já se sabe e projetados para os anos seguintes:

- br-BC e br-SP: conferir o calendário da B3 e se houve lei criando ou mudando feriado nacional;
- us-NYC: conferir o calendário de feriados da NYSE, publicado com três anos de antecedência;
- gb-LND: conferir os feriados bancários da Inglaterra e do País de Gales no gov.uk;
- ar-BA, py-ASU e uy-MVD, a partir da fase 5: conferir o decreto anual de feriados da Argentina e o calendário bancário publicado pelo banco central de cada um dos três países;
- rodar o teste de regras e publicar versão nova se algo mudou.

## Decisões em aberto

- **Conta na Store.** Em nome da Elekto (conta de empresa, gratuita desde maio de 2026, com verificação da empresa) ou de pessoa física. Para propaganda, o que interessa é o nome Elekto na página do app.
- **Calendários dos vizinhos: bancário ou bolsa.** A revisão anual deles fica com a Elekto, junto com a dos demais. A proposta é o expediente bancário de cada banco central, que é o que vale para prazos. Os calendários das bolsas (BYMA, BVPASA e BVM) podem vir depois, como o br-SP veio ao lado do br-BC.
- **Escopo do br-SP.** A 1.2 o alinhou à B3. Quem precisar dos feriados da cidade de São Paulo (25/01 e 9/07) poderá criar um calendário derivado na Fase 2, ou a Elekto pode enviar um calendário da cidade pronto.
- **Contribuições externas.** O README oferece licenciamento alternativo ao GPLv3. Para isso continuar possível, a Elekto precisa deter os direitos de todo o código, o que pede um acordo de contribuição (CLA) de quem enviar pull request.
- **Política de privacidade.** O texto do README serve, mas a Store pede uma URL própria, no site da Elekto ou no GitHub Pages.
