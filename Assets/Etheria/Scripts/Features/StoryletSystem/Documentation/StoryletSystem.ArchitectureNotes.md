# StoryletSystem Architecture Notes

Этот документ хранит **expanded reasoning** по `StoryletSystem`.

Он нужен, чтобы не потерять длинные рассуждения, tradeoff analysis и промежуточные архитектурные выводы, которые больше не должны перегружать основной документ.

Важно:
- это companion doc
- это не главный нормативный документ
- source of truth по текущей архитектурной позиции остаётся `Agents.md`

Здесь полезно хранить:
- historical reasoning
- expanded tradeoff analysis
- детальные thought experiments
- длинные пояснения, которые помогают понять происхождение принятых решений

## Context and Original Reasoning

Исходная проблема возникла не из-за того, что current matcher плохо работает сам по себе.

Наоборот, current system уже попадает в важный и полезный класс решений:
- relation-aware matching
- partial feasibility pruning
- локальное сохранение дефицитных сущностей
- fit/scarcity/compatibility thinking

То есть стартовая инженерная мысль была верной:
- внутри одного storylet-а задача действительно похожа на `CSP`
- relation constraints резко усложняют локальный assignment
- наивный выбор “лучшего кандидата” быстро упирается в тупики

Проблема начинается не внутри storylet-а, а уровнем выше:
- когда storylet-ы начинают менять мир
- когда порядок storylet-ов становится meaningful
- когда один шаг меняет пространство последующих шагов

Именно тогда задача перестаёт быть только `matching` и становится `planning over state transitions`.

## Tradeoff Analysis

### Matcher vs Planner

Полезное разделение такое:

- matcher отвечает на вопрос:
  - “можно ли на данном `WorldState` инстанцировать конкретный storylet?”
- planner отвечает на вопрос:
  - “в каком порядке и через какие world transitions лучше применять storylet-ы?”

Это концептуально разные задачи.

Если смешать их в одном слое, возникают риски:
- matcher превращается в god object
- становится трудно объяснять логику
- world mutation и strategic ordering переплетаются с local assignment logic

### Greedy vs Beam Search

`Greedy` полезен как локальный и transitional инструмент:
- его легко внедрить
- он полезен как первая интеграция после появления `WorldState + Effects`
- он даёт быстрый smoke-path

Но как только:
- storylet открывает или закрывает следующие ветки
- последствия шага важнее локального выигрыша
- нужно сравнивать несколько хороших будущих миров

top-level greedy становится слишком коротким по горизонту.

`Beam search` лучше подходит этому проекту, потому что:
- сохраняет bounded cost
- умеет смотреть на несколько шагов вперёд
- хорошо сочетается с designer-friendly scoring
- даёт понятные branch traces

Именно поэтому проектный вывод сейчас такой:
- `beam search` не просто хороший option
- а target architectural requirement для top-level planner

### Immutable vs Copy-on-Write

Логически `WorldState` должен быть immutable:
- planner мыслит snapshot-ами
- одна ветка не должна портить другую
- эффекты должны симулироваться без скрытого backtracking

Но технически не обязательно делать “полную тупую копию всего мира на каждый шаг”.

Более практичный компромисс:
- semantic immutability
- implementation with structural sharing, diffing or copy-on-write

То есть для reasoning важно одно:
- `Apply` возвращает новый logical state

А способ внутренней оптимизации — уже инженерная деталь реализации.

### Local Lookahead vs Planner Search

Старый local lookahead полезен как эвристический предшественник planner-thinking:
- он уже пытается сохранить optionality
- он уже штрафует неудачное расходование сущностей

Но local lookahead и planner search — не одно и то же.

Разница такая:
- local lookahead оценивает последствия выбора в пределах локальной эвристики
- planner search сравнивает реальные последовательности state transitions

То есть старый lookahead не бесполезен.

Он просто перестаёт быть главным механизмом top-level выбора.

## Expanded Scoring Thoughts

Полезно держать scoring как минимум в трёх слоях:

### `Instantiation quality`

Что storylet получает на текущем шаге:
- fit ролей
- relation compatibility
- scarcity preservation

### `Transition value`

Что storylet делает как narrative action:
- priority
- драматичность
- magnitude of world change
- payoff / escalation value

### `Future potential`

Что новый мир обещает потом:
- сколько ещё открыто meaningful steps
- сколько веток не заперто
- не превратился ли мир в тупик или шумный повтор

Позже сюда естественно добавляется:
- `anti-repetition`
- branch diversity bias
- salience-aware penalties

Главная мысль:
- score не должен сводиться к одному “наиболее удобному локальному storylet-у”
- score должен выражать качество перехода и качество resulting world

## Detailed WorldState and Effect Discussion

`WorldState` нужен потому, что после появления effects недостаточно оперировать только:
- списком `Entity`
- списком `Relation`
- временным free pool

Planner должен видеть единый объект мира, в котором живут:
- сущности
- их теги
- атрибуты
- relations
- relation tags
- сущности, появившиеся или исчезнувшие после предыдущих шагов

`Effects` должны быть декларативными по двум причинам:

1. Simulation
- planner должен уметь примерить storylet без скрытых side effects

2. Explainability
- после шага должно быть видно, какие именно world changes были произведены

Особенно важна разница между:
- `entity tag`
- `relation tag`
- source-bound or derived effect

Практическая мысль отсюда такая:
- если эффект должен исчезнуть вместе с relation-источником, его опасно хранить как “вечный” `entity tag`
- если состояние принадлежит самой сущности независимо от внешнего источника, `entity tag` подходит лучше
- если эффект лучше вычислять из текущего состояния мира, derived representation может быть чище explicit storage

## Archived Examples and Explorations

Ниже — краткий архив того, какие исследовательские темы уже поднимались при развитии фичи.

### Какие storylet-ы лучше брать в smoke-world

Полезным оказался сценарий маленького frontier settlement, потому что он позволяет в одном test world проверить:
- role matching
- relation constraints
- state transitions
- unlock/block future storylets
- entity spawn/despawn
- variation between branches

Этот материал теперь нормализован в:
- `StoryletSystem.SmokeScenarioSpec.md`

### Почему порядок storylet-ов должен реально менять мир

Если один storylet не влияет на доступность других, planner architecture теряет часть смысла.

Хороший smoke-world должен давать случаи, где:
- сначала defensive setup -> raid ослаблен
- сначала raid -> открываются casualty and rescue consequences

Именно такие различающиеся ветки делают beam search не декоративным, а содержательным.

### Почему repeatability и salience нельзя оставлять на потом

Без них даже technically correct planner может создавать:
- шум
- повторы
- чрезмерное доминирование одного типа storylet-ов

Поэтому повторяемость и surfacing policy должны быть частью доменной модели, а не только UI-layer concern.

## Historical Notes

Какие позиции считаются уже принятыми:
- current matcher полезен и сохраняется
- `WorldState` обязателен
- `Effects` должны быть декларативными
- top-level planner обязан быть `beam-search planner`
- `greedy replan` допустим только как transitional step

Какие позиции считаются superseded:
- бесконечное наращивание static matcher-а как главной архитектуры
- top-level greedy как final solution
- хранение всех смыслов мира только через текущий список entity/relation без явного `WorldState`

Какие позиции остаются открытыми на уровне реализации, но не на уровне направления:
- точная структура `WorldState`
- точный wire format trace-ов
- точные scoring weights
- конкретная оптимизация immutable snapshot representation

Иначе говоря:
- направление уже принято
- часть реализации ещё должна уточняться

Но общая архитектурная рамка больше не является открытым вопросом.
