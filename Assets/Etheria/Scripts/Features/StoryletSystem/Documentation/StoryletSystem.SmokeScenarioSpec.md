# StoryletSystem Smoke Scenario Spec

Этот документ описывает целевой smoke-сценарий для проверки работоспособности `StoryletSystem`.

Он ориентирован не на “один match result”, а на пошаговую симуляцию с trace, world transitions и planner branching.

Для этого проекта smoke-сценарий должен проверять не только matching, но и обязательную top-level стратегию `beam search`.

## Цель сценария

Один связный сценарий должен покрывать:
- `WorldState` как source of truth
- relation-aware role binding
- декларативные `Effects`
- unlock/block storylet-ов после state transitions
- `beam search` как target planner
- repeatability и salience как факторы выбора
- explainability через trace и rejection reasons

## Сценарий

`small frontier settlement`

Смысл:
- есть маленькое поселение на границе
- несколько NPC уже связаны отношениями доверия, долга и вражды
- первые storylet-ы поднимают тревогу и перестраивают мир
- дальше порядок шагов меняет качество обороны, число потерь и набор доступных последствий

Это хороший сценарий, потому что в нём естественно проверяются:
- role matching
- relation requirements
- world effects
- изменение доступности следующих storylet-ов
- создание и удаление сущностей
- изменение тегов и атрибутов
- порядок применения storylet-ов
- branch comparison в `beam search`

## Стартовый мир

### Сущности

- `entity.village_elder`
- `entity.young_knight`
- `entity.bandit_scout`
- `entity.temple_priest`
- `entity.wandering_mage`
- `entity.militia_guard`
- `entity.refugee_child`
- `entity.settlement_square`

### Примеры тегов

- `tag.noble`
- `tag.warrior`
- `tag.bandit`
- `tag.priest`
- `tag.arcane`
- `tag.settler`
- `tag.child`
- `tag.wounded`
- `tag.outlaw`
- `tag.missing`
- `tag.alerted`
- `tag.mobilized`
- `tag.leading_defense`
- `tag.exhausted`
- `tag.rescued`

### Атрибуты

- `attribute.wealth`
- `attribute.fear`
- `attribute.loyalty`
- `attribute.health`
- `attribute.ward_strength`

### Стартовые отношения

- `young_knight -> village_elder [tag.vassal]`
- `village_elder -> young_knight [tag.trust]`
- `bandit_scout -> militia_guard [tag.enemy]`
- `temple_priest -> refugee_child [tag.caretaker]`
- `wandering_mage -> temple_priest [tag.trust]`

### Repeatability and salience assumptions

Для smoke-версии полезно считать:
- `border_warning` — `once per run`
- `call_to_arms` — `once per run`
- `arcane_ritual` — `once per run`
- `bandit_raid` — `repeatable until state changes`, но с сильным salience pressure после `alerted`
- `heal_the_wounded` — `repeatable with cooldown`
- `search_for_missing_child` — `once per missing incident`
- `banish_the_raider` — `once per outlaw entity`

## Набор storylets

### 1. `storylet.border_warning`

Роли:
- `role.scout_reporter` -> `tag.bandit`
- `role.authority` -> `tag.noble | tag.settler`

Эффекты:
- authority получает `tag.alerted`
- у authority растёт `attribute.fear`
- у `entity.settlement_square` растёт global tension marker

Покрывает:
- базовый assignment
- entity tag add
- attribute modify
- unlock defensive branch

### 2. `storylet.call_to_arms`

Требует:
- authority должен иметь `tag.alerted`

Роли:
- `role.commander` -> `tag.noble | tag.warrior`
- `role.guard` -> `tag.warrior`

Relation requirement:
- `role.guard` должен иметь `tag.trust` или `tag.vassal` к `role.commander`

Эффекты:
- guard получает `tag.mobilized`
- commander получает `tag.leading_defense`
- relation `guard -> commander` получает `tag.sworn_to`

Покрывает:
- relation-aware matching
- relation tag add
- unlock военной ветки

### 3. `storylet.arcane_ritual`

Роли:
- `role.mage` -> `tag.arcane`
- `role.acolyte` -> `tag.priest`

Relation requirement:
- между ними есть `tag.trust` или как минимум нет `tag.enemy`

Эффекты:
- mage получает `tag.exhausted`
- создаётся `entity.ward_manifestation`
- `attribute.ward_strength` мира повышается

Покрывает:
- relation check
- entity creation
- защитную альтернативу военной ветке

### 4. `storylet.bandit_raid`

Требует:
- в мире есть `tag.alerted`
- `attribute.ward_strength` недостаточен или `entity.ward_manifestation` отсутствует

Роли:
- `role.raider` -> `tag.bandit`
- `role.defender` -> `tag.warrior`

Relation requirement:
- если defender уже `tag.mobilized`, он не должен быть `tag.enemy` по отношению к commander-side authority

Эффекты:
- defender может получить `tag.wounded`
- `entity.refugee_child` может получить `tag.missing`
- fear у поселения растёт
- `entity.bandit_scout` получает `tag.outlaw`

Покрывает:
- negative relation constraints
- branching after previous state changes
- tag and attribute mutations

### 5. `storylet.heal_the_wounded`

Требует:
- есть сущность с `tag.wounded`

Роли:
- `role.healer` -> `tag.priest`
- `role.patient` -> `tag.wounded`

Эффекты:
- убрать у patient `tag.wounded`
- повысить `attribute.health`
- добавить relation `patient -> healer [tag.gratitude]`

Покрывает:
- entity tag remove
- attribute modify
- relation create/update
- repeatable support action

### 6. `storylet.search_for_missing_child`

Требует:
- есть сущность с `tag.missing`

Роли:
- `role.searcher` -> `tag.warrior`
- `role.guardian` -> `tag.child`

Эффекты:
- убрать `tag.missing`
- добавить `tag.rescued`
- relation `guardian -> searcher [tag.trust]`

Покрывает:
- storylet открывается только после эффекта другого storylet-а
- tag remove/add
- relation add

### 7. `storylet.banish_the_raider`

Требует:
- raider помечен как `tag.outlaw`
- есть authority или commander

Роли:
- `role.judge` -> `tag.noble | tag.warrior`
- `role.outlaw` -> `tag.outlaw`

Эффекты:
- удалить сущность `outlaw`
- убрать связанные relation edges

Покрывает:
- entity deletion
- relation cleanup
- destructive state transition

## Почему этот сценарий хороший

Он покрывает почти всё, что нужно для v1 smoke:
- `entity tag add/remove`
- `relation tag add/remove`
- `attribute change`
- `entity spawn`
- `entity despawn`
- `storylet unlock after previous effects`
- `storylet block after previous effects`
- `relation-aware assignment`
- `negative relation constraints`
- `repeatability`
- `salience pressure`
- `beam search branch comparison`

## Обязательная форма smoke-теста

Smoke-тест должен быть пошаговым trace, а не одним итоговым `match result`.

### Что выводить на каждом шаге

1. номер шага
2. `current world snapshot id`
3. какие storylets применимы
4. какие storylets valid, но проиграли по salience или score
5. какое assignment выбрано для каждого top candidate
6. какие ветки остались в beam
7. почему часть веток была pruned
8. какие effects применены
9. каким стал мир после шага

## Минимальные ожидаемые ветки

Сценарий должен поддерживать как минимум две meaningful branches:

### Defensive branch

1. `border_warning`
2. `call_to_arms`
3. `arcane_ritual`
4. `bandit_raid` ослаблен или заблокирован

### Raid-first branch

1. `border_warning`
2. `call_to_arms`
3. `bandit_raid`
4. `heal_the_wounded`
5. `search_for_missing_child`
6. `banish_the_raider`

Важно:
- обе ветки должны быть explainable
- planner должен уметь сравнить их через `future potential` и `transition value`
- smoke не обязан всегда выбирать одну и ту же ветку, если scoring policy изменяется

## Что особенно важно проверить

### 1. Один storylet реально меняет доступность других

Это главный критерий новой системы.

### 2. Выбор порядка даёт разные миры

Например:
- сначала `arcane_ritual` -> raid слабее или откладывается
- сначала отказ от ritual -> raid сильнее и рождает more costly aftermath

### 3. Эффекты применяются не пачкой, а пошагово

После каждого шага мир должен быть новым snapshot-ом.

### 4. Relation-состояние тоже эволюционирует

Должны меняться не только сущности, но и их связи.

### 5. Repeatability не создаёт шума

Нужно видеть, что support storylets не спамятся бесконечно без cooldown или state change.

### 6. `Beam search` реально сравнивает ветки

Trace должен показывать:
- несколько surviving branches
- pruning reasons
- branch ranking
- победившую ветку

## Мой совет по объёму первой smoke-версии

Оптимально для v1:
- 6-7 storylets
- 7-8 сущностей
- 4-6 relation tags
- 2 meaningful branches
- 1 repeatable support storylet
- 1 destructive cleanup storylet

Этого достаточно, чтобы проверить и matching, и planning, и diagnostics, не раздувая сценарий до неподъёмного размера.
