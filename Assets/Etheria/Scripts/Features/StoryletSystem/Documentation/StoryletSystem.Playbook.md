# StoryletSystem Playbook

Этот документ фиксирует инженерные правила для `StoryletSystem`.

Он не описывает текущее состояние кода как уже завершённую систему. Везде ниже важно различать:
- `current system` — то, что уже есть в коде сегодня
- `target model` — куда система должна прийти
- `recommended future implementation` — практический способ двигаться к target model

## Каноническая модель

Каноническая модель storylet-а:

`Storylet = Preconditions + Role Binding + Effects + Repeatability + Salience`

Правила:
- `Preconditions` отвечают за доступность storylet-а на текущем `WorldState`
- `Role Binding` отвечает за конкретное назначение сущностей на роли
- `Effects` описывают, как storylet меняет мир после применения
- `Repeatability` отвечает за политику повторного появления и повторного исполнения
- `Salience` отвечает за приоритет показа или выбора среди уже валидных storylet-ов

Коротко:
- `valid` не означает `should surface now`
- `high priority` не означает `safe to apply repeatedly`
- `good local assignment` не означает `good future sequence`

## Current System vs Target Model

### Current system

Сегодня система уже хорошо решает одну подзадачу:
- взять статичный набор `Entity`
- проверить локальные `Tag` / `Attribute` / `Relation` ограничения
- найти relation-aware assignment внутри одного storylet-а
- greedily выбрать storylet из общего пула

Это сильная база для `instantiation`, но не полноценная модель narrative progression.

### Target model

После появления `Effects` система должна мыслить так:
- есть `WorldState`
- на нём storylet может быть инстанцирован
- инстанцированный storylet даёт `EffectBatch`
- `EffectBatch` создаёт новый `WorldState`
- следующий storylet выбирается уже на новом состоянии

То есть верхний уровень задачи превращается из `static assignment problem` в `state transition planning`.

## Правила для `WorldState`

`WorldState` — source of truth для planner-а.

Он должен содержать:
- существующие сущности
- их `entity tags`
- их атрибуты
- существующие `relations`
- `relation tags`
- любые source-of-truth данные, которые участвуют в preconditions, effects или scoring

Он не должен:
- зависеть от скрытых mutable singleton-ов
- опираться на неявные side effects из gameplay callbacks
- смешивать authoring metadata с runtime state без явной причины

### Почему state должен быть immutable или diff-friendly

Planner многократно симулирует последствия.

Поэтому:
- семантически `WorldState` должен вести себя как immutable snapshot
- технически реализация может быть copy-on-write, structurally shared или diff-based

Минимальное правило:
- `Apply(candidate, state)` не мутирует входной state
- результатом всегда считается новый logical snapshot

### Runtime world state vs authoring data

Разделение обязательно:
- authoring data описывает шаблоны storylet-ов, роли, эффекты и policy
- runtime `WorldState` описывает конкретное текущее состояние мира

Не смешивать:
- дизайнерские комментарии
- UI labels
- narrative notes
- planner-facing state

в один источник правды.

## Правила для `WorldEffect`

Предпочтительная модель — декларативные эффекты.

Правила:
- storylet не должен как базовый механизм вызывать произвольный gameplay callback для planner-simulation
- planner должен уметь симулировать effects без запуска непредсказуемой логики
- effects должны быть inspectable, replayable и debuggable

Допустимые базовые эффекты:
- добавить `entity tag`
- убрать `entity tag`
- изменить или установить attribute
- удалить attribute
- создать entity
- удалить entity
- создать relation
- удалить relation
- добавить `relation tag`
- убрать `relation tag`

### Что считается хорошим effect-моделированием

Хорошо:
- эффект можно перечислить списком
- эффект можно показать в trace
- эффект можно применить к snapshot без побочных runtime вызовов

Плохо:
- effect спрятан в произвольной ветке C# кода
- effect зависит от внешнего mutable состояния
- effect нельзя объяснить автору или инженеру после факта

## Модель исполнения

Целевая модель исполнения:

`instantiate -> score -> apply -> replan`

Правила:
- один storylet = один `state transition`
- matcher отвечает за `can instantiate?` и `what assignment?`
- effect applier отвечает за переход `state -> next state`
- planner отвечает за порядок применения storylet-ов

Инварианты:
- matcher не мутирует мир
- planner не содержит storylet-specific hardcode
- effects применяются после выбора конкретного `Storylet + Assignment`
- каждый следующий шаг рассчитывается уже на новом `WorldState`

## Выбор алгоритма

Для этого проекта верхнеуровневый planner в target architecture должен быть реализован как `beam-search planner`.

Это проектное архитектурное требование.

### Роль `single-step greedy replan`

`Single-step greedy replan` допустим только как:
- migration step
- smoke-test implementation
- временная интеграционная версия, пока не готов полный beam planner

Он не считается достаточной target-реализацией для этого проекта.

### Почему target strategy — именно `beam search`

`Beam search` обязателен, потому что:
- один storylet реально открывает или закрывает следующие
- порядок часто меняет качество финальной цепочки
- нужен bounded lookahead на несколько шагов вперёд
- нужен controllable runtime cost без полного перебора
- нужен explainable search, пригодный для designer-friendly scoring и debugging

### Что важно понимать про `beam search`

Для этого проекта:
- он не заменяет хорошую effect-модель
- он не отменяет необходимости explainability и scoring discipline
- он должен работать поверх корректных `WorldState`, `Effects` и storylet instantiation rules

Практическая рекомендация:
- сначала ввести `WorldState + Effects + greedy replan` как переходную ступень
- затем обязательно поднять top-level choice до `beam-search planner`
- после этого тюнинговать ширину beam, scoring и state deduplication

## Scoring principles

Scoring должен быть designer-friendly и explainable.

Удобно мыслить его в четырёх слоях:

### `Instantiation quality`

Оценивает, насколько удачно storylet инстанцируется сейчас:
- fit ролей
- качество relation binding
- scarcity preservation

### `Transition value`

Оценивает ценность текущего шага:
- narrative priority
- драматичность
- magnitude of world change
- payoff value

### `Future potential`

Оценивает перспективность нового состояния:
- сколько ещё storylet-ов остаётся доступно
- не зашёл ли мир в тупик
- открылись ли новые meaningful branches

### `Anti-repetition`

Ограничивает шум:
- штраф за недавний повтор того же паттерна
- штраф за однотипные шаги подряд
- штраф за слишком частое использование одной и той же пары ролей или actor types

## Decision Table: entity tag vs relation tag vs derived effect

| Хранить как | Когда использовать | Когда не использовать |
| --- | --- | --- |
| `entity tag` | Когда состояние принадлежит самой сущности и не зависит от живого внешнего источника | Когда эффект должен исчезнуть вместе с relation/source |
| `relation tag` | Когда состояние выражает связь между двумя сущностями или source-bound effect | Когда состояние должно жить независимо от связи |
| `derived/computed effect` | Когда состояние лучше вычислять из текущего мира, чем хранить явно | Когда вычисление слишком дорогое или теряет explainability |

### Практическое правило

Хранить на сущности:
- устойчивую роль или долгоживущий статус
- результат необратимого narrative change
- state, который остаётся даже после исчезновения источника

Хранить в relation:
- бафф или влияние, завязанное на источник
- доверие, вражду, клятву, долг, покровительство
- состояния вида “X влияет на Y”

Делать derived:
- эффекты, которые естественно вычисляются из текущих связей
- агрегированные narrative flags
- статусы, которые опасно дублировать в нескольких местах

## Anti-patterns

Не делать:
- world mutation внутри matcher или assignment builder
- hidden mutable state как dependency для eligibility или effects
- source-bound effects как “вечные” `entity tags` без указания источника
- storylet-specific special cases внутри общего planner-а
- смешивание authoring concerns и runtime planning logic

Признаки опасной архитектуры:
- storylet нельзя честно симулировать без Unity scene side effects
- один и тот же state хранится в двух местах без явного owner
- выбор storylet-а зависит от неявной логики вне `WorldState`

## Recommended Future Implementation

Практический порядок внедрения:
- ввести явный `WorldState`
- выделить декларативный `EffectBatch`
- превратить текущий matcher в instantiation component для одного storylet-а
- ввести `Apply(candidate, state) -> nextState`
- после этого временно поднять top-level chooser до `greedy replan`
- затем обязательно заменить его на `beam-search planner`

Главное правило:
- не переписывать всё сразу
- сохранить текущий relation-aware matcher как полезный локальный solver

## Minimum Acceptance Questions

После любого расширения `StoryletSystem` инженер должен уметь ответить:
- что в этой фиче является source of truth
- где заканчиваются `Preconditions` и начинаются `Effects`
- почему это `entity tag`, а не `relation tag`
- можно ли симулировать storylet без побочных runtime вызовов
- соответствует ли верхнеуровневый planner требованию `beam search`
