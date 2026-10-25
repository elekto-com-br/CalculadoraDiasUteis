# Verificação de acessibilidade

Roteiro da [revisão de acessibilidade](ROADMAP.md#revisão-de-acessibilidade), feita uma vez só, com todas as telas prontas, antes do envio à Store. Precisa de uma pessoa diante do computador, com som: o Narrador fala, e pelo acesso remoto o som não chega. Cada tela nova acrescenta aqui os seus itens. Os testes automáticos (`tests/UiTests`) já cobrem os nomes e `AutomationId` dos controles, o teclado e o tamanho da janela; o que fica aqui é o que só se confere ouvindo e olhando.

Marque cada item ao terminar. O que falhar, anote embaixo do item, com um print se ajudar, para ser corrigido depois.

## Preparação

- [ ] Instalar o Accessibility Insights for Windows, baixado em <https://accessibilityinsights.io/downloads/> (ele não está no winget).
- [ ] Abrir a calculadora pelo F5 no VS Code, ou pelo menu Iniciar depois de um `winapp run src/CalculadoraDiasUteis`.

## Accessibility Insights

Verificações automáticas (FastPass):

- [ ] No Accessibility Insights, escolher **FastPass**, com **What to select** em **Entire app**.
- [ ] Voltar à calculadora com Alt+Tab e apertar **Shift+F8**. As falhas aparecem marcadas sobre uma captura da tela; clicando em cada uma, a aba **How to fix** diz o que corrigir.
- [ ] Repetir na página "Sobre", aberta com F1.

Ordem do Tab:

- [ ] Voltar à lista de testes com **Ctrl+T**, abrir **Tab Stops** e começar a gravar com **Shift+F7**.
- [ ] Percorrer a tela com Tab, Shift+Tab e as setas. O Accessibility Insights numera cada parada.
- [ ] Conferir que todos os controles são alcançados, que a ordem segue a leitura (calendário, "Sobre", cartão "Dias entre duas datas", cartão "Data final de um prazo"), que o foco está sempre visível e que ele nunca fica preso num controle.
- [ ] Repetir na página "Sobre".

## Narrador

- [ ] Ligar com **Ctrl+Win+Enter** (o mesmo atalho desliga). A tecla do Narrador é o **Caps Lock**.
- [ ] Percorrer com Tab: cada controle é anunciado com o nome, o tipo (botão, caixa de combinação, botão de opção) e o estado (selecionado, desabilitado).
- [ ] Trocar uma data, o prazo e uma opção: o resultado novo é anunciado sozinho, sem mover o foco, por exemplo "249 dias úteis e 365 dias corridos" ou "Data final: quarta-feira, 27 de outubro de 2027".
- [ ] Digitar um prazo além do calendário (100000): o aviso é lido.
- [ ] Com **Caps Lock+setas**, ler os títulos e textos que não recebem foco.
- [ ] Na página "Sobre", os links são anunciados como link, e o logotipo como "Site da Elekto".
- [ ] Atalhos úteis: **Caps Lock+Enter** aciona o item, **Caps Lock+F1** lista os comandos e **Ctrl+Caps Lock+F12** esconde a tela, deixando só o que o Narrador enxerga.

## Resultado

- [ ] Passar o resultado ao roadmap: se nada falhou, a fase 1 está fechada; se falhou, abrir as correções.
