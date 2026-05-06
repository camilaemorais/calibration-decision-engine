# Calibration Decision Engine

Sistema que recebe uma estimativa de reparo de veículo e decide quais calibrações de sensores ADAS são necessárias, usando um pipeline de regras.

## Como rodar

```bash
dotnet run --project src/CalibrationDecisionEngine.Api
```

A API sobe em `http://localhost:5000`.

```bash
dotnet test
```

## Testando o endpoint

```bash
curl -X POST http://localhost:5000/vehicle/evaluate \
  -H "Content-Type: application/json" \
  -d '{
    "vin": "1HGCM82633A123456",
    "headers": ["Front Bumper", "Windshield", "Front Radar"],
    "lines": [
      { "description": "Replace front camera", "operation": "RPL" },
      { "description": "Calibrate front radar", "operation": "CAL" },
      { "description": "R&I windshield", "operation": "RI" },
      { "description": "Replace front bumper", "operation": "RPL" }
    ]
  }'
```

## Nome do projeto

O nome `CalibrationDecisionEngine` foi uma escolha própria. O enunciado sugeria `MiniRulesPipeline` mas preferi um nome que refletisse melhor o problema do domínio (decisão de calibração) do que a implementação técnica (pipeline de regras).

## Estrutura

Separei em três projetos:

- **Pipeline**: as interfaces e o builder genérico, sem saber nada de veículo ou calibração
- **Domain**: os steps, modelos e regras
- **Api**: controller e configuração da aplicação

Separar o Pipeline do Domain foi uma decisão intencional: assim o pipeline pode ser reutilizado em outros contextos sem trazer dependências do domínio junto.

## Decisões que tomei

**Por que `ConcurrentBag` no contexto?**
O `MatchRulesStep` processa as regras em paralelo. Se eu usasse uma lista normal, múltiplas tasks escrevendo ao mesmo tempo causariam problema. O `ConcurrentBag` resolve isso sem precisar gerenciar lock manualmente. A ordem de inserção não importa aqui porque as calibrações são identificadas pelo nome.

**Por que a regra do airbag é aplicada antes dos `excludes`?**
A regra "Bumper Sensor Calibration só aparece se tiver linha de airbag" precisa rodar antes de processar os `excludes`. Caso contrário, o Bumper excluiria a Windshield mesmo sendo depois descartado, o que resultaria em um output errado.

**Por que o caminho do `rules.json` é passado como parâmetro?**
Para não criar dependência do projeto Domain em coisas do ASP.NET Core. Quem sabe o caminho é o `Program.cs`, e ele passa via parâmetro. Isso também facilita os testes.

## Pacotes adicionais

- `FluentAssertions`: deixa os erros de teste mais legíveis
- `Microsoft.AspNetCore.Mvc.Testing`: permite rodar testes E2E sem subir um servidor de verdade

## O que ficou de fora

Não implementei nenhum dos bônus por falta de tempo. O B3 (logs estruturados por step) seria o mais fácil de encaixar porque o pipeline já usa `ILoggerFactory`.

## O que faria diferente

Integraria Swagger para documentar e testar o endpoint de forma visual, sem precisar montar o curl na mão. Com mais tempo ainda, faria um frontend básico para conseguir submeter estimativas pela interface e ver o resultado das calibrações de forma mais clara.

## Trade-offs

Usei `Task.Run` no `MatchRulesStep` para garantir paralelismo real no ThreadPool. Para poucas regras fixas o overhead pode não compensar, mas escala bem conforme o número de regras cresce.

O pipeline está registrado como Singleton porque ele é stateless, todo o estado fica no `VehicleContext` que é criado por request. O risco é que um step com estado introduzido no futuro causaria problema difícil de rastrear.


## Teste Manual da API

`test-api.http` está incluído apenas para conveniência do desenvolvedor, permitindo o teste manual da API a partir do cliente REST da IDE. Ele não era um requisito e não é utilizado pelo conjunto de testes automatizados. O caminho oficial de verificação continua sendo `dotnet test`.

**Curiosidade extra:** foram criados os 5 testes pedidos e 8 testes extras. O número 13 foi proposital.
