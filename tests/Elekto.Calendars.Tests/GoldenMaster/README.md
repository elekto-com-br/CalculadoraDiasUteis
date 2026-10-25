# Referência da versão 1.2

`golden-1.2.csv.gz` guarda 32.065 chamadas ao núcleo de cálculo, com o resultado de cada uma, gravadas a partir do executável publicado no [release v1.2](https://github.com/elekto-com-br/CalculadoraDiasUteis/releases/tag/v1.2): o arquivo `CalculadoraDiasUteis.1_2_0_0.zip`, de sha-256 `f28e30cc34a0c65e99f071114e1394949ef542a9836222a26c410187e4c5b9f7`. A pasta `calendars-1.2` é a cópia dos calendários desse mesmo zip.

O teste `GoldenMasterTests` refaz cada chamada na biblioteca nova e exige o mesmo resultado, inclusive a mesma exceção. Os calendários ficam congelados aqui para que uma atualização de feriados em `src` não quebre este teste: ele compara o código, não os dados.

## O que é coberto

Para cada calendário (br-BC, br-SP, us-NYC, gb-LND e a união de br-BC com us-NYC), todas as operações públicas de `ICalendar`, com todas as opções de ajuste: propriedades e limites, dia útil, próximo e anterior dia útil, ajuste de data final, soma de dias úteis e corridos, contagem de dias, listas de datas úteis e não úteis, início e fim de mês, e n-ésimo dia útil do mês. As datas são sorteadas perto de feriados, perto dos limites do calendário (inclusive fora deles) e espalhadas pelo período; os prazos vão de curtos a dezenas de milhares de dias, positivos e negativos.

## Formato

Texto UTF-8 compactado com gzip, uma chamada por linha, campos separados por ponto e vírgula:

```
calendário;operação;arg1;arg2;arg3;resultado
br-BC;AddWorkDays;2026-11-19;3;Following;2026-11-25
```

Datas no formato `yyyy-MM-dd` (nas listas, `yyyyMMdd` separadas por espaço), opções pelo nome do enum e exceções como `!Tipo`. As linhas que começam com `#` são comentários.

## Como regerar

Só faz sentido se o gerador mudar (mais casos ou outra operação): o comportamento de referência é sempre o da 1.2.

```powershell
gh release download v1.2 -R elekto-com-br/CalculadoraDiasUteis -p *.zip -D $env:TEMP\v12
Expand-Archive $env:TEMP\v12\CalculadoraDiasUteis.1_2_0_0.zip $env:TEMP\v12\app
dotnet run --project tools/GoldenMaster -c Release -- $env:TEMP\v12\app tests/Elekto.Calendars.Tests/GoldenMaster/golden-1.2.csv.gz
```

O gerador roda em .NET Framework 4.8, como a 1.2, e usa semente fixa: a mesma versão do gerador produz sempre os mesmos casos.
