# Calculadora de Dias Úteis
Aplicação para cálculos de prazos e dias finais considerando os dias úteis

Copyright © 2021 by Elekto Produtos Financeiros Ltda.

Licença [GPLv3](https://www.gnu.org/licenses/gpl-3.0.html)

## O que é?

Este é um programa Desktop Windows clássico que permite o cálculo de prazos em dias úteis, e também datas finais dado um prazo em dias úteis.

Ele usa o mesmo fonte base, e segue as mesma lógica que a calculadora de prazos que existia no [site da Elekto](https://elekto.com.br). A [documentação funcional](https://elekto.com.br/Blog/ManualDoUsuarioDaCalculadoraDePrazos) é a mesma aplicáve à versão web, até a aparência da aplicação é a mesma, respeitando-se as limitações de aplicativos Windows.

## Requisitos

Para executar a máquina deve ter o .Net Framework 4.8 instalado. É muito provável que qualquer Windows relativamente moderno já o tenha, caso não, ao executar pela primeira vez a aplicação será informada a situaçao. Se necessário baixe o Runtime do .Net Framework 4.8 no [site da Microsoft](https://dotnet.microsoft.com/download/dotnet-framework/net48).

A aplicação não requer nenhuma permissão especial, podendo ser instalada apenas com permissões de usuário, seja manualmente, copiando os arquivos em qualquer diretório ou usando o programa de setup.

## Garantias

Absolutamente nenhuma! A Elekto não é responsável por quaisquer danos, de qualquer natureza, advindos do uso correto ou não dessa aplicação. Se não concorda com estes termos, basta não usar a aplicação.

## Licença

Essa aplicação usa a licença Licença [GPLv3](https://www.gnu.org/licenses/gpl-3.0.html) o que implica, basicamente:

* Você é livre para executar e usar, mesmo para fins comerciais a aplicação.
* Se você incorporar, na forma de binário, ou na forma de fonte, essa aplicação (ou parte dela), em outro software, este também deve ser licenciado pela GPLv3.

Se houver interesse em incorporar esse código em software proprietário procure a Elekto para negociar um licenciamento alternativo.

## Privacidade

Nem a aplicação, e nem seu programa de setup, monitoram o uso, enviam logs de erros ou telemetria. Nenhum deles se comunica com a Internet, a aplicação contém um único link para o [site da Elekto](https://elekto.com.br), que abrirá no seu browser, e o link sequer rastreia a origem do acesso.

## Compilando você mesmo

A aplicação foi desenvolvida com o [Visual Studio 2019](https://visualstudio.microsoft.com/vs/) e não usa nada realmente especial. O programa de setup é o [Inno Setup](https://jrsoftware.org/isinfo.php).

## Contribuições

### Em código

A rotina de sempre: faça um fork, clone em sua máquina, faça a correção ou melhoria, suba para seu clone, faça um pull request. Caso deseje incoporar uma mudancá grande, é melhor nos contatar antes. Não nos obrigamos a aceitar contribuição alguma, mas consideraremos com carinho, obviamente.

Em termos de estilo, mantenha o que existe em código atualmente, que é basicamente o padrão do [ReSharper](https://www.jetbrains.com/resharper/).

### Novos Calendários e Edições em Calendários

A Elekto monitora ativamente apenas os feriados `br-BC` (Banco Central do Brasil) e `br-SP` (B3 em São Paulo), essenciais aos nossos negócios e sempre que houver alteração relevante nestes iremos fazer um novo release.

Mas aceitaremos contribuições de feriados de outras praças, basta mandar mensagem para suporte-csl@elekto.com.br contendo:

* os feriados (com as descrições dos não óbvios), em Excel ou arquivo texto, 
* informando o local, aplicabilidade e uma fonte de referência, 
* consiga ao menos 5 anos para o passado e 5 anos para o futuro (ainda que seja necessário alguma extrapolação). 

A idéia não é ter os feriados de todas as cidades do Brasil (ficaria horrível na interface de usuário atual), mas ao menos das principais praças comerciais, ou mesmo jurídicas. Também a nosso, critério, aceitaremos ou não a contribuição; se estiver na dúvida entre em contato antes de empenhar seu trabalho nisso.

Tenha em mente que não é necessária nossa ajuda para adicionar feriados à sua instalação em particular, bastando colocar arquivos texto, com a formatação necessária, no diretório onde os calendários ficam, [conforme instruções](https://github.com/elekto-com-br/CalculadoraDiasUteis/blob/master/CalculadoraDiasUteis/Calendars/LeiaMe.txt) que estão no mesmo diretório.
