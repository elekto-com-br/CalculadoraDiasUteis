# Formato da exportação

A versão 2.0 exporta os calendários do usuário num arquivo JSON, em UTF-8. O arquivo é completo: leva a herança de cada calendário, para a 2.0 remontá-lo igual ao importar, e a lista dos feriados em vigor, para quem quiser ler as datas sem conhecer a herança. Ele não foi feito para a 1.x, que continua lendo só o formato texto descrito em [formato-dos-calendarios.txt](formato-dos-calendarios.txt).

## Exemplo

```json
{
  "format": "calculadora-dias-uteis",
  "version": 1,
  "exported": "2026-10-25T14:30:00-03:00",
  "application": "2.0.0",
  "calendars": [
    {
      "code": "br-BC",
      "name": "Brasil - Banco Central",
      "kind": "original",
      "changes": [
        { "date": "2026-02-16", "change": "described", "description": "Carnaval (segunda-feira)" }
      ],
      "minDate": "1950-02-20",
      "maxDate": "2078-12-25",
      "holidays": [
        { "date": "1950-02-20", "description": "Carnaval" }
      ]
    },
    {
      "code": "forum-da-lapa",
      "name": "Fórum da Lapa",
      "kind": "custom",
      "bases": [ "br-BC" ],
      "changes": [
        { "date": "2026-10-15", "change": "added", "description": "Aniversário do fórum" },
        { "date": "2026-11-20", "change": "removed" }
      ],
      "minDate": "1950-02-20",
      "maxDate": "2078-12-25",
      "holidays": [
        { "date": "1950-02-20", "description": "Carnaval" }
      ]
    }
  ]
}
```

As listas `holidays` estão encurtadas no exemplo; no arquivo, vão completas.

## Campos

| Campo | O que é |
|---|---|
| `format` | Sempre `calculadora-dias-uteis`. |
| `version` | A versão do formato, hoje 1. Uma versão mais nova do aplicativo pode gravar uma versão maior, que esta não importa. |
| `exported` | Quando o arquivo foi gravado, com o fuso. |
| `application` | A versão do aplicativo que gravou. |
| `calendars` | Os calendários, com as bases antes de quem depende delas. |

Em cada calendário:

| Campo | O que é |
|---|---|
| `code` | O código, que identifica o calendário e não muda quando ele é renomeado. Os da Elekto usam o código do arquivo (`br-BC`, `br-SP`, `us-NYC`, `gb-LND`). |
| `name` | O nome mostrado na tela. |
| `kind` | `original`, para um calendário da Elekto com alterações do usuário, ou `custom`, para um calendário do usuário. |
| `bases` | Só nos do usuário: os códigos das bases, na ordem. O calendário é a união delas, no período em que todas valem. Sem bases, ele começa só com os fins de semana. |
| `changes` | As alterações do usuário, uma por data: `added` (feriado incluído, com descrição opcional), `removed` (feriado herdado que deixa de contar) e `described` (feriado herdado com outra descrição; sem `description`, fica sem descrição). |
| `minDate`, `maxDate` | O período em que o calendário vale. |
| `holidays` | Os feriados em vigor, com a descrição de cada um, já com as bases e as alterações. |

As datas vão no formato `yyyy-MM-dd`.

## O que vai no arquivo

"Exportar tudo" grava todos os calendários do usuário e os da Elekto que ele alterou. Exportar um calendário grava esse calendário e as bases de que ele depende: as do usuário, sempre, e as da Elekto, se ele as alterou, porque sem essas alterações o calendário não seria remontado igual. Um calendário da Elekto sem alteração não vai, porque todo aplicativo já o tem.

## Na importação

- Um calendário do usuário com o mesmo código de um que já existe substitui o existente: é o caso de importar de novo uma exportação anterior.
- Nos calendários da Elekto, as alterações do arquivo se juntam às que o usuário já tem; na mesma data, vale a do arquivo.
- Se uma base não existe nem no arquivo nem no aplicativo, o calendário vem com os feriados de `holidays`, sem a herança.
- Se o nome já é de outro calendário, o importado ganha um número no fim, como "Fórum da Lapa (2)".

Antes de gravar, o aplicativo mostra o que vai fazer com cada calendário, e o usuário pode desmarcar o que não quiser.
