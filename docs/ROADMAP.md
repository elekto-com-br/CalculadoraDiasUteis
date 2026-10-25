# Roadmap da versão 2.0

Levar a Calculadora de Dias Úteis para WinUI: bonita, acessível, moderna e distribuída pela Microsoft Store.

É um projeto de baixa prioridade, tocado aos poucos. Por isso o plano está dividido em fases pequenas, e cada uma termina com algo que compila e funciona; dá para parar em qualquer fase sem deixar o projeto quebrado.

Atualizado em 25/10/2026. Fases 0 a 3 concluídas; faltam a revisão de acessibilidade e a fase 4, a da Store.

## Onde estamos

- A linha 1.x (WinForms, .NET Framework 4.8) termina na 1.2.1. Ela fica no `master` até a 2.0 ficar pronta, recebendo apenas correção que não puder esperar.
- O trabalho da 2.0 acontece neste branch, `winui`, aberto a quem quiser acompanhar.
- **Ponto de parada (25/10/2026):** o aplicativo faz as contas da 1.2, edita calendários (cria derivados de um ou mais outros, inclui, remove e descreve feriados, desfaz alterações, exporta e importa, inclusive os arquivos da 1.x) e leva as datas de um período e os feriados de um calendário para o Excel, copiando ou em `.xlsx` e `.csv`. As fases 1, 2 e 3 estão marcadas com os tags `v2.0.0-alpha.1`, `alpha.2` e `alpha.3`. Falta o que só uma pessoa faz: a [revisão de acessibilidade](#revisão-de-acessibilidade) e a conta na Store, da fase 4.
- **Sem release público até a Store.** Cada fase termina com um tag (`v2.0.0-alpha.1`, `alpha.2`...), para marcar o ponto no histórico, mas sem pacote para baixar: a 2.0.0 sai pela Microsoft Store, na fase 4.
- **A revisão humana de acessibilidade é uma só**, com todas as telas prontas, [antes da Store](#revisão-de-acessibilidade). Até lá, o que pode ser feito sem uma pessoa diante do computador (os testes de interface, os nomes e `AutomationId`, o teclado, os temas por captura de tela) acompanha cada tela nova.

## Princípios

- **O cálculo não muda sem prova.** Toda mudança no núcleo passa por testes que comparam com os resultados da 1.2.
- **Acessível desde o começo.** Narrador, navegação só por teclado, alto contraste e escala de texto entram na primeira tela, não no fim.
- **Privacidade igual à de hoje.** Sem telemetria, sem conta, sem internet; as únicas saídas são os links da página "Sobre", para o site da Elekto, o código-fonte no GitHub e as políticas de privacidade, que abrem no navegador.
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
- **Corrigido na fase 2:** a partir do último dia do calendário, um prazo negativo de mais de 5 dias úteis lançava exceção, enquanto um de até 5 devolvia a data; o resultado dependia de qual dos dois algoritmos internos do `AddWorkDays` era usado. Agora funciona sempre, com o algoritmo rápido dando o mesmo resultado do lento. Os 27 casos da referência da 1.2 que gravaram a exceção ficaram documentados no teste.

### Fase 1: aplicativo WinUI com as funções da 1.2

- Projeto WinUI empacotado, criado a partir do template oficial.
- Uma janela com as funções da 1.2: escolher o calendário; contar dias úteis e corridos entre duas datas, no critério financeiro ou de dias cheios; calcular a data final de um prazo em dias úteis ou corridos, com ajuste.
- Resultado recalculado ao mudar qualquer campo, sem botão de calcular.
- Controles nativos (`CalendarDatePicker`, `NumberBox`, `RadioButtons`, `ComboBox`), fundo Mica e tema claro, escuro ou de alto contraste seguindo o Windows.
- `AutomationProperties.Name` e `AutomationId` em todos os controles, ordem de tabulação revisada e atalhos de teclado.
- Página "Sobre" com versão, licença, privacidade e links para o site da Elekto e o código-fonte.
- Todos os textos em arquivos de recursos (`.resw`), e datas e números no formato da cultura do Windows, para que a tradução da fase 5 não exija mudança de código.
- Ícone e identidade visual feitos a partir do logotipo da Elekto em vetor (`assets/logo-elekto.svg`), com as cores da marca: verde `#556652` e laranja `#F79646`.
- Testes de interface com `winapp ui`, rodando no Windows Sandbox.

**Pronto quando** a 2.0 faz tudo o que a 1.2 faz, menos editar calendários e exportar, e passa nos testes de interface.

**Situação:** concluída em 25/10/2026 e marcada com o tag `v2.0.0-alpha.1`. O aplicativo está em `src/CalculadoraDiasUteis`, criado com o template `winui-mvvm` do WinApp CLI (Windows App SDK 2.5, CommunityToolkit.Mvvm). Os testes de interface (`tests/UiTests`) cobrem 21 cenários: os padrões, a escolha de datas no calendário, os dois critérios de contagem, prazos em dias úteis (inclusive negativos) e corridos com os três ajustes, o aviso de prazo além do calendário, a troca de calendário, a página "Sobre" pelo botão e pelo teclado (F1 e Esc), seus links, a dica do botão "Sobre", a licença no navegador, o tamanho mínimo da janela, os `AutomationId` e as escolhas lembradas ao reabrir. Os resultados esperados saem da própria biblioteca (`Expected.cs`), para as datas que o teste escolhe. No Sandbox passam todos; os de teclado e mouse precisam da sessão do Windows desbloqueada e da janela da Sandbox conectada (`wsb connect`, se ela tiver caído). Fora do Sandbox, o app foi usado no Windows do dia a dia, e o "Ler a licença" abre o texto no editor padrão. Os temas claro, escuro e de alto contraste e a janela estreita foram conferidos por captura de tela. A integração contínua compila também o aplicativo.

Decisões tomadas na fase:

- **Windows 11 no manifesto**, para x64 e ARM64. Não há pacote para x86, que o Windows 11 não tem.
- **Datas e números no idioma que o Windows escolhe para o aplicativo**, o mesmo dos textos e dos seletores de data. Por enquanto é sempre o português; num Windows em inglês, seguir só o formato regional deixaria a data final como 10/27/2027 ao lado de seletores em 27/10/2027. Na fase 5, com os três idiomas, o formato acompanha o idioma escolhido.
- **Cor de destaque é o verde da marca**, com tons calculados para manter contraste de pelo menos 4,5:1 nos temas claro e escuro; o laranja fica no ícone e no logotipo. No alto contraste valem as cores do Windows.
- **As escolhas lembradas são as da 1.2:** calendário, critério de contagem, tipo de prazo, ajuste e prazo. As datas começam sempre em hoje e daqui a um ano, como lá.
- **Novidade pequena:** a data final mostra o dia da semana e avisa quando cai em feriado ou dia não útil, o que só acontece em dias corridos sem ajuste.
- **A dica do botão "Sobre" traz o "(F1)" no próprio texto.** O WinUI só acrescenta a tecla do atalho à dica padrão; quando o botão tem dica própria, como aqui, ela fica de fora.
- **A licença vai no pacote**, no original e na tradução, e abre no programa padrão do Windows para cada tipo de arquivo.
- **Links da página "Sobre":** o logotipo leva ao site da Elekto, e há um link para o código-fonte no GitHub. A privacidade aponta a política da Elekto, para o site dela, e a declaração do GitHub, pela qual a Elekto não responde. Os links em texto são sublinhados, porque o verde escuro do destaque fica perto da cor do texto comum.
- **No tema escuro, o logotipo usa o verde clareado até 7:1** (`#9DAF99`): o mesmo matiz e croma do `#556652` no espaço OKLCH, só com a luminosidade aumentada, o que mantém a identidade suave da marca. No alto contraste, o logotipo todo vai na cor de texto do Windows. Para trocar as cores por tema, ele é desenhado em XAML (`ElektoLogo.xaml`) com os traços de `assets/logo-elekto.svg`.
- **A janela tem tamanho mínimo, 500 × 400**, o menor em que os controles cabem com os cartões um embaixo do outro; a altura que faltar rola. Não há máximo: maximizar é direito do usuário, e o conteúdo é que para de crescer (1040 de largura na página principal, 720 na "Sobre").
- **Ficaram para a fase 2** o link "Definições de Calendários..." e o parâmetro `--calendars` da 1.2, que dependem de calendários fora do pacote. Lá, o link virou o botão "Editar calendários", e o parâmetro deu lugar à importação.
- **O editor é o VS Code**, com a configuração em `.vscode`: o F5 compila e usa a extensão WinApp, ainda em prévia, para abrir o app empacotado e anexar o depurador. A versão atual dela só aceita a pasta compilada; quando aceitar o projeto, a tarefa de compilação do `launch.json` deixa de ser necessária. A solução passou para o formato `.slnx`.

A verificação com o Accessibility Insights e o Narrador, que estava aqui, passou para a [revisão de acessibilidade](#revisão-de-acessibilidade) única, antes da Store.

### Fase 2: calendários editáveis

- **Modelo.** Os calendários enviados pela Elekto vêm dentro do pacote, somente leitura, no formato texto de hoje. As alterações do usuário ficam guardadas à parte, no banco local.
- **Calendário derivado.** O usuário cria um calendário a partir de outro, por exemplo "Fórum da Lapa" como o br-BC mais os feriados locais, e ele continua recebendo as atualizações que a Elekto fizer no original. A base pode ser mais de uma, e o derivado é a união delas: "br-BC + us-NYC" serve à liquidação de câmbio, que só acontece em dia útil nos dois países. Uma base pode ser outro derivado.
- **Operações.** Criar calendários próprios, vazios ou derivados; renomear e apagar; incluir feriado; editar a descrição; remover feriado, inclusive de um calendário original, como exceção local.
- **Descrição dos feriados.** Cada feriado pode ter uma descrição, opcional. Os calendários originais passam a vir com a descrição de cada feriado (Carnaval, Tiradentes, Juneteenth...).
- **Restaurar.** Desfazer todas as alterações de um calendário original, ou de todos de uma vez.
- **Atualizações.** Quando uma versão nova do app traz feriados oficiais novos, as alterações do usuário continuam valendo por cima deles.
- **Importar** os arquivos texto da 1.x, para aproveitar os de quem usava a pasta `Calendars` ou o parâmetro `--calendars`.
- **Exportar** num arquivo completo, com a herança: as bases, os feriados incluídos e removidos e as descrições. Reimportado na 2.0, o calendário volta como estava e continua recebendo as atualizações do original. O arquivo não precisa ser lido pela 1.x; a lista achatada, só com as datas, fica para a exportação em CSV e a cópia para o Excel, na fase 3. Exportar também protege o usuário: desinstalar um app MSIX apaga a pasta local dele.
- Tela de feriados por ano, com o dia da semana e a origem de cada data (original, incluída ou removida pelo usuário).
- **Muitos calendários.** Com 34 calendários, a lista de escolha abre por cima da janela, mostra uns 13 e rola, e digitar as primeiras letras com a lista em foco leva ao item. Para dezenas está bem; se os calendários próprios passarem disso, avaliar busca por texto ou agrupar por país, e ordenar pelo nome em vez do número do arquivo.

**Pronto quando** dá para criar um calendário derivado, editá-lo, fechar e reabrir o app, receber uma atualização do original e restaurar, tudo coberto por testes.

**Situação:** concluída em 25/10/2026 e marcada com o tag `v2.0.0-alpha.2`. O modelo, o banco e a importação e exportação ficam no projeto `CalculadoraDiasUteis.Core`, com 35 testes que rodam na integração contínua, inclusive o de receber uma versão nova do original com as alterações do usuário valendo por cima. No aplicativo, o botão "Editar calendários" abre a página de calendários no calendário escolhido. Os testes de interface ganharam 17 cenários: criar um derivado, incluir um feriado local e vê-lo valer nas contas da página principal, remover e restaurar um herdado, trocar a descrição, recusar um nome repetido, renomear, abrir uma exceção num calendário da Elekto e desfazê-la, exportar, excluir pedindo confirmação, importar de volta com a herança, importar um arquivo da 1.x, os atalhos da lista, os `AutomationId`, a janela mínima e reabrir o app com tudo guardado. No Sandbox passam todos, inclusive os de teclado, que precisam da sessão do Windows desbloqueada.

Decisões tomadas na fase:

- **O período de um derivado é o trecho em que todas as bases valem.** Fora dele, os feriados de alguma base não são conhecidos, e a conta sairia errada sem aviso. Um calendário próprio sem base vale do primeiro ao último feriado, como na 1.x. Um feriado incluído fora do período é recusado.
- **Cada calendário do usuário tem um código, tirado do nome na criação** ("Fórum da Lapa" vira `forum-da-lapa`), que não muda ao renomear e é o que as bases e a exportação usam. Os nomes não se repetem, nem com os da Elekto.
- **Só as alterações ficam guardadas**, num arquivo SQLite (`calendarios.db`) na pasta local do app: os calendários do usuário, as bases e, por data, o feriado incluído, o removido e a descrição trocada. Uma alteração que deixa de se aplicar, porque a base mudou, fica guardada sem efeito; "Desfazer alterações" apaga as de um calendário da Elekto.
- **Um calendário que é base de outro não pode ser excluído**; a mensagem diz quais dependem dele. Excluir pede confirmação; remover um feriado não, porque o herdado pode ser restaurado e o incluído é só incluir de novo.
- **A exportação é um JSON completo**, descrito em [formato-da-exportacao.md](formato-da-exportacao.md): a herança de cada calendário e a lista de feriados em vigor. Exportar um calendário leva as bases dele que são do usuário e as da Elekto que ele alterou.
- **A importação mostra antes o que vai fazer** com cada calendário e deixa desmarcar. De um arquivo exportado, um calendário com o mesmo código substitui o existente, as alterações nos da Elekto se juntam às que o usuário tem, e um calendário cuja base não existe vem com os feriados todos, sem a herança. De um arquivo da 1.x, o calendário vira um calendário próprio sem base; se ele tem o código e os mesmos feriados de um da Elekto, é uma cópia antiga dele e fica de fora, desmarcado.
- **Descrições dos feriados da Elekto** no idioma da praça, com os nomes oficiais. O teste de regras confere, de 2000 em diante, que cada feriado tem a descrição da regra que o explica. No br-BC, antes de 2000, também os feriados bancários dos planos Collor, o lançamento do Real e as eleições de 1994 e 1996; só o 12 de outubro de 1950 a 1979, antes de ser o dia de Nossa Senhora Aparecida, ficou sem descrição.
- **A data final que cai num feriado diz qual é**, como "terça-feira, feriado (Tiradentes)".
- **Os atalhos Delete e F2 valem só com o foco na lista de feriados.** No WinUI, um atalho sem dono vale na janela inteira, e o Delete apagaria o feriado selecionado com o foco em outro controle.
- **No Sandbox, o nome escrito por automação na caixa "Salvar como" do Windows não chega a ela.** Os testes de exportar e importar usam o nome sugerido, na pasta Documentos, e só rodam no Sandbox, para não mexer nos arquivos de quem os roda.

### Fase 3: exportação e integração

- Copiar para a área de transferência, de modo que a lista cole no Excel como datas e não como texto. É o que a maioria faz no dia a dia.
- Exportar `.xlsx` e `.csv`, tanto as datas de um período quanto os feriados de um calendário.
- Exportar `.ics` (iCalendar), para levar os feriados ao Outlook ou ao Google Agenda: em discussão, só entra se houver um caso de uso convincente.

**Situação:** concluída em 25/10/2026 e marcada com o tag `v2.0.0-alpha.3`. Na página principal, "Copiar as datas" e "Exportar as datas" levam todas as datas do período; na de calendários, "Copiar o ano" leva os feriados do ano mostrado e "Exportar planilha", todos os do calendário. As tabelas e os formatos ficam no `CalculadoraDiasUteis.Core`, com testes, e os testes de interface ganharam três cenários, que conferem a área de transferência e os arquivos dentro do Sandbox. A planilha e o HTML da cópia foram abertos no Excel, com as datas lidas como datas.

Decisões tomadas na fase:

- **As colunas.** As datas de um período: data, dia da semana, "É dia útil" (1 ou 0, como na 1.x, para somar na planilha), dias úteis desde o início e o feriado, se houver. Os feriados: data, dia da semana e descrição. As tabelas não levam linhas de título, para colar e filtrar direto; o nome do calendário vai no nome do arquivo.
- **A cópia vai em dois formatos.** Em HTML, com as datas em ISO (`2026-12-25`) marcadas como data curta, que o Excel cola como datas em qualquer idioma; e em texto separado por tabulações, para os programas que só aceitam texto.
- **O CSV segue o formato regional do Windows**, com as personalizações do usuário, e não o idioma do aplicativo: é com ele que o Excel separa as colunas e lê as datas. Num Windows com vírgula como separador, um CSV com ponto e vírgula abria tudo numa coluna só. O arquivo é UTF-8 com a marca no começo, sem a qual o Excel lê os acentos errado.
- **O `.xlsx` é gravado com o Open XML SDK**, da Microsoft, de licença MIT, e não com o ClosedXML previsto: é um pacote só, preparado para a compactação do pacote Release, e o ClosedXML traria mais seis. A planilha sai com as datas como datas, no formato de data curta de quem a abrir, o cabeçalho em negrito e fixo no alto e o filtro ligado.
- **O `.ics` fica de fora por enquanto**: o Outlook e o Google já têm os feriados nacionais. O caso que poderia convencer é o de um calendário derivado com feriados locais, que não existe em nenhuma agenda pronta; volta se alguém pedir.

### Revisão de acessibilidade

Uma passada só, quando todas as telas estiverem prontas: o Accessibility Insights (verificação automática e ordem do Tab) e o Narrador em cada tela, por uma pessoa diante do computador e com som. Pelo acesso remoto o som do Narrador não chega, e não há como um agente ouvi-lo. O roteiro, em forma de lista para marcar, está em [ACESSIBILIDADE.md](ACESSIBILIDADE.md) e cresce a cada tela nova. O que falhar é corrigido antes do envio à Store.

### Fase 4: Microsoft Store

- Conta de desenvolvedor no Partner Center (gratuita) e reserva do nome do app. A reserva pode ser feita a qualquer momento, antes mesmo de existir o app.
- Material da loja: descrição, capturas de tela, ícones, classificação etária (questionário IARC) e URL da política de privacidade.
- Identidade do pacote: o `Package.appxmanifest` usa por enquanto `Elekto.CalculadoraDiasUteis` e o publicador provisório `CN=Elekto`, que a Store substitui pelos da conta quando o app for associado a ela.
- Testar o pacote de publicação em Release, que é compactado (*trimmed*) ao contrário do usado no desenvolvimento. A compilação em Release já avisa (IL2104) que dois assemblies do Windows SDK, da própria Microsoft, não são totalmente seguros para essa compactação. **Feito em 25/10/2026:** o MSIX em Release fechava ao abrir, porque o C#/WinRT montava por reflexão a ponte das coleções entregues aos controles (a lista de calendários, a de feriados) e a compactação removia o que ele usaria. Com o gerador do C#/WinRT completo no projeto (`CsWinRTAotOptimizerEnabled` e `AllowUnsafeBlocks`), as pontes são geradas na compilação, e o pacote passa em todos os testes de interface. O script deles aceita `-Package`, para rodar o mesmo roteiro contra um MSIX instalado no Sandbox.
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
- O EPPlus 4.5.3.3 sai. É a última versão LGPL, de 2020, sem manutenção, e as versões novas exigem licença comercial. No lugar entrou o Open XML SDK, da Microsoft, de licença MIT (ver a [fase 3](#fase-3-exportação-e-integração)).
- O `.ics`, que levaria os feriados para a agenda, ficou para quando alguém pedir.

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
