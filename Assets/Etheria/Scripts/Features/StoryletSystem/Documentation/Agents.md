# StoryletSystem: architectural position and target direction

Этот документ объясняет, **почему** `StoryletSystem` должен быть устроен именно так, а не иначе.

Он не является:
- полным engineering playbook
- authoring guide для narrative content
- debugging specification
- smoke-test scenario spec

Его задача уже и жёстче:
- зафиксировать проблему текущей системы
- зафиксировать принятый target-state
- объяснить, почему именно этот target-state выбран
- показать migration direction без разрастания в reasoning dump

## Documentation Map

- `Agents.md`
  - читать для архитектурной позиции, target-state и project-level решений
- `StoryletSystem.Playbook.md`
  - читать для инженерных правил, canonical model, `WorldState`, `WorldEffect`, scoring и anti-patterns
- `StoryletSystem.AuthoringGuide.md`
  - читать для authoring storylets, repeatability, salience, naming и narrative pacing
- `StoryletSystem.DebuggingAndTelemetry.md`
  - читать для explainability, traces, rejection reasons и observability
- `StoryletSystem.SmokeScenarioSpec.md`
  - читать для smoke-world, expected branches и verification coverage
- `StoryletSystem.ArchitectureNotes.md`
  - читать для expanded reasoning, tradeoff analysis и historical notes

## Problem Statement

Текущая система уже неплохо решает одну важную подзадачу:
- есть статичный снимок мира
- есть набор сущностей и relations
- нужно relation-aware назначить сущности на роли storylet-а
- затем greedily выбрать storylet из общего пула

Для этой задачи текущий matcher полезен и во многом удачен.

Но после появления `Effects` задача перестаёт быть только задачей `matching`.

С этого момента каждый storylet:
- не только читает мир как вход
- но и создаёт новый мир на выходе

Поэтому важен уже не только ответ на вопрос:
- “какой storylet можно инстанцировать сейчас?”

Но и ответы на вопросы:
- какой storylet лучше применить первым
- какие future branches он откроет или закроет
- какой `WorldState` получится после шага
- как изменится множество последующих storylet-ов

Именно здесь current system начинает упираться в свой потолок: она хорошо решает `static relation-aware assignment`, но не является хорошей моделью `state-transition planning`.

## Architectural Position

Для этого проекта канонический target-state такой:

- `WorldState` — source of truth для planner-а
- storylet — это `Preconditions + Role Binding + Effects + Repeatability + Salience`
- текущий relation-aware matcher остаётся локальным solver-ом одного шага
- `Effects` должны быть декларативными и пригодными для simulation
- верхнеуровневый planner обязан быть `beam-search planner`

Это означает, что система должна мыслить не “пачкой storylet-ов на одном снимке мира”, а так:

`instantiate -> score -> apply -> replan`

Ключевой сдвиг здесь не в усложнении локального matcher-а, а в подъёме задачи на уровень выше:
- из `static assignment problem`
- в `state transition planner`

## Why This, Not That

### Почему не расширять matcher до бесконечности

`GreedyStoryletAssignmentBuilder` и связанные relation-aware проверки хороши как local instantiation solver.

Они не должны становиться местом, где:
- мутируется мир
- строится долгосрочный порядок storylet-ов
- прячется логика будущих world transitions

Иначе получится тяжёлый god object, который одновременно:
- матчит роли
- принимает стратегические решения
- меняет runtime state

Это плохое разделение ответственности и плохая explainability.

### Почему не оставаться на top-level greedy

Как только effects реально меняют доступность следующих storylet-ов, простой top-level greedy становится слишком локальным.

Он может хорошо выбирать следующий шаг, но плохо выбирать цепочку.

Для этого проекта это недостаточно, потому что мы уже приняли, что:
- порядок storylet-ов важен
- один шаг может ослабить, усилить или заблокировать следующие
- качество результата определяется не только ближайшим ходом, но и future potential мира

Поэтому `greedy replan` допустим только как transitional implementation.

### Почему не идти сразу в тяжёлые solver-подходы

Для проекта сейчас не нужен самый общий и тяжёлый класс решений вроде:
- полного перебора
- CP/SAT-style solver infrastructure
- сложного planner stack с высокой ценой объяснимости

Причина не в том, что они “плохие”, а в том, что они хуже подходят текущим приоритетам:
- controllable runtime cost
- designer-friendly scoring
- explainable branch comparison
- постепенная миграция от уже существующего relation-aware matcher-а

Для этих целей `beam search` даёт нужный баланс между:
- глубиной lookahead
- ограниченной стоимостью
- понятной трассировкой решений

## What Stays Useful

Важно не переписывать систему так, будто текущий код ошибочен целиком.

Из current system стоит сохранить:
- relation-aware matching
- partial validation
- role ordering по ограниченности
- entity-role fit evaluation
- final validation of assignments

То есть current matcher не выбрасывается.

Он переосмысляется:
- не как final top-level algorithm
- а как local instantiation component внутри planner architecture

## Migration Direction

Практический путь внедрения такой:

1. Ввести явный `WorldState`.
2. Ввести декларативный `EffectBatch`.
3. Превратить текущий matcher в instantiation component для одного storylet-а на одном `WorldState`.
4. Ввести отдельный `Apply(candidate, worldState) -> nextWorldState`.
5. Поднять top-level flow до `greedy replan` как временного integration step.
6. Обязательно заменить его на `beam-search planner` как final target implementation.

Ключевое правило миграции:
- временный `greedy replan` нужен для поэтапного внедрения
- но не считается достаточной конечной архитектурой для проекта

## Non-Negotiable Rules

- matcher не мутирует мир
- planner не содержит storylet-specific hardcode
- один storylet = один `state transition`
- `Effects` должны быть декларативными и пригодными для simulation
- `WorldState` является planner-facing source of truth
- top-level planner обязан быть `beam-search planner`
- `greedy replan` допустим только как transitional step

## Reading Guidance

Идите в соседние документы, если нужен другой уровень детализации:

- если нужен нормативный engineering guidance
  - читайте `StoryletSystem.Playbook.md`
- если нужен content authoring guidance
  - читайте `StoryletSystem.AuthoringGuide.md`
- если нужна explainability и telemetry discipline
  - читайте `StoryletSystem.DebuggingAndTelemetry.md`
- если нужен сценарий проверки ветвления, effects и beam search
  - читайте `StoryletSystem.SmokeScenarioSpec.md`
- если нужен полный expanded reasoning и historical tradeoff archive
  - читайте `StoryletSystem.ArchitectureNotes.md`

## Short Conclusion

Если свести всё к одной позиции:

> `StoryletSystem` не должен эволюционировать в ещё более сложный static matcher. Он должен эволюционировать в planner over world states, где текущий relation-aware matcher остаётся локальным solver-ом инстанцирования, `Effects` являются декларативными, а top-level выбор реализуется через обязательный `beam-search planner`.
