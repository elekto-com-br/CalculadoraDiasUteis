# Calculadora de Dias Úteis
Aplicação para cálculos de prazos e dias finais considerando os dias úteis

[![CI](https://github.com/elekto-com-br/CalculadoraDiasUteis/actions/workflows/ci.yml/badge.svg?branch=winui)](https://github.com/elekto-com-br/CalculadoraDiasUteis/actions/workflows/ci.yml)

> Este é o branch `winui`, onde a versão 2.0 está sendo desenvolvida; o andamento está no [roadmap](docs/ROADMAP.md). A versão estável, 1.2, está no [`master`](https://github.com/elekto-com-br/CalculadoraDiasUteis/tree/master) e nos [Releases](https://github.com/elekto-com-br/CalculadoraDiasUteis/releases).

Copyright © 2021-2026 by Elekto Produtos Financeiros Ltda.

Licença [GPLv3](https://www.gnu.org/licenses/gpl-3.0.html)

## O que é?

Este é um programa Desktop Windows clássico que permite o cálculo de prazos em dias úteis, e também datas finais dado um prazo em dias úteis.

Ele usa o mesmo fonte base, e segue as mesma lógica que a calculadora de prazos que existia no [site da Elekto](https://elekto.com.br). A [documentação funcional](https://elekto.com.br/Blog/ManualDoUsuarioDaCalculadoraDePrazos) é a mesma aplicável à versão web, até a aparência da aplicação é a mesma, respeitando-se as limitações de aplicativos Windows.

## Próxima versão

A versão 1.2 é a última em .NET Framework 4.8 e WinForms. A próxima geração, em WinUI, acessível e distribuída pela Microsoft Store, está sendo desenvolvida às claras no branch [`winui`](https://github.com/elekto-com-br/CalculadoraDiasUteis/tree/winui); o plano está no [roadmap](https://github.com/elekto-com-br/CalculadoraDiasUteis/blob/winui/docs/ROADMAP.md).

## Requisitos

Para executar a máquina deve ter o .Net Framework 4.8 instalado. É muito provável que qualquer Windows relativamente moderno já o tenha, caso não, ao executar pela primeira vez a aplicação será informada a situação. Se necessário baixe o Runtime do .Net Framework 4.8 no [site da Microsoft](https://dotnet.microsoft.com/download/dotnet-framework/net48).

A aplicação não requer nenhuma permissão especial, podendo ser instalada apenas com permissões de usuário, seja manualmente, copiando os arquivos em qualquer diretório ou usando o programa de setup.

## Garantias

Absolutamente nenhuma! A Elekto não é responsável por quaisquer danos, de qualquer natureza, advindos do uso correto ou não dessa aplicação. Se não concorda com estes termos, basta não usar a aplicação.

## Licença

Essa aplicação usa a licença Licença [GPLv3](https://www.gnu.org/licenses/gpl-3.0.html) o que implica, basicamente:

* Você é livre para executar e usar, mesmo para fins comerciais a aplicação.
* Se você incorporar, na forma de binário, ou na forma de fonte, essa aplicação (ou parte dela), em outro software, este também deve ser licenciado pela GPLv3.

Se houver interesse em incorporar esse código em software proprietário procure a Elekto para negociar um licenciamento alternativo.

## Privacidade

A aplicação não monitora o uso, não envia registros de erro nem telemetria e não se comunica com a Internet; o programa de setup da 1.2 também não. Os links da aplicação abrem no seu navegador, sem nada que identifique a origem do acesso. Na 1.2 há um só, para o [site da Elekto](https://elekto.com.br); na 2.0, a página "Sobre" tem também o do código-fonte, aqui no GitHub, e os das políticas de privacidade. No site da Elekto vale a [política de privacidade da Elekto](https://elekto.com.br/Info/PrivacyPolicy); no GitHub, a [declaração de privacidade do GitHub](https://docs.github.com/pt/site-policy/privacy-policies/github-general-privacy-statement), pela qual a Elekto não responde.

## Compilando você mesmo

Neste branch há o núcleo de cálculo (`src/Elekto.Calendars`), o aplicativo WinUI (`src/CalculadoraDiasUteis`) e os testes (`tests`). Para compilar tudo e rodar os testes do núcleo basta, no Windows, o [SDK do .NET 10](https://dotnet.microsoft.com/download), com qualquer editor:

```
dotnet test
```

O aplicativo é empacotado (MSIX). Para executá-lo é preciso também o [WinApp CLI](https://github.com/microsoft/winappCli) e o Modo de Desenvolvedor do Windows ligado:

```
winapp run src/CalculadoraDiasUteis
```

No VS Code, o repositório recomenda as extensões C# Dev Kit e WinApp; com elas, o F5 compila, abre e depura o app. Abrir o `.exe` direto da pasta `bin` não funciona: fora do pacote, o app não encontra o Windows App SDK e fecha na hora.

Os testes de interface usam o mesmo WinApp CLI e rodam no Windows Sandbox, sem mexer na sua área de trabalho; os de teclado e mouse precisam da sessão do Windows desbloqueada e da janela da Sandbox aberta:

```
.\tests\UiTests\ui-tests.ps1 -Target sandbox
```

A versão 1.2, em WinForms, compila a partir do `master` com o Visual Studio 2019 ou posterior.

## Contribuições

### Em código

A rotina de sempre: faça um fork, clone em sua máquina, faça a correção ou melhoria, suba para seu clone, faça um pull request. Caso deseje incorporar uma mudança grande, é melhor nos contatar antes. Não nos obrigamos a aceitar contribuição alguma, mas consideraremos com carinho, obviamente.

Em termos de estilo, mantenha o que existe em código atualmente, que é basicamente o padrão do [ReSharper](https://www.jetbrains.com/resharper/).

### Novos Calendários e Edições em Calendários

A Elekto monitora ativamente apenas os feriados `br-BC` (Banco Central do Brasil) e `br-SP` (B3 em São Paulo), essenciais aos nossos negócios e sempre que houver alteração relevante nestes iremos fazer um novo release.

Mas aceitaremos contribuições de feriados de outras praças, basta mandar mensagem para suporte-csl@elekto.com.br contendo:

* os feriados (com as descrições dos não óbvios), em Excel ou arquivo texto, 
* informando o local, aplicabilidade e uma fonte de referência, 
* consiga ao menos 5 anos para o passado e 5 anos para o futuro (ainda que seja necessário alguma extrapolação). 

A ideia não é ter os feriados de todas as cidades do Brasil (ficaria horrível na interface de usuário atual), mas ao menos das principais praças comerciais, ou mesmo jurídicas. Também, a nosso critério, aceitaremos ou não a contribuição; se estiver na dúvida entre em contato antes de empenhar seu trabalho nisso.

Tenha em mente que não é necessária nossa ajuda para adicionar feriados à sua instalação em particular, bastando colocar arquivos texto, com a formatação necessária, no diretório onde os calendários ficam, [conforme instruções](https://github.com/elekto-com-br/CalculadoraDiasUteis/blob/master/CalculadoraDiasUteis/Calendars/LeiaMe.txt) que estão no mesmo diretório.
