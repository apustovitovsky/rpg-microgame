# StoryletSystem Debugging and Telemetry

Этот документ описывает минимальные требования к explainability и observability для `StoryletSystem`.

Он ориентирован на future implementation:
- не привязан к конкретной UI
- не требует уже существующего инструмента
- фиксирует, на какие вопросы система обязана отвечать

## Обязательные debug questions

Система должна уметь ответить:
- почему storylet недоступен
- какая `Precondition` не прошла
- какой `Role Binding` был выбран
- какой `Effect` изменил доступность следующего storylet-а
- почему planner выбрал одну ветку, а не другую

Если хотя бы на один из этих вопросов нельзя ответить без чтения внутреннего кода, explainability недостаточна.

## Rejection reasons

Недоступность storylet-а должна быть объяснима через конкретную причину.

Минимальные категории причин:
- missing `entity tag`
- missing `attribute threshold`
- missing `relation`
- invalid `role binding`
- repeatability lock
- salience deprioritization
- planner pruning

Желательно различать:
- `not valid`
- `valid but not selected`
- `selected but later pruned`

## Minimum runtime trace

Для каждого шага planner-а полезен минимальный trace record:

- `current world snapshot id`
- `candidate storylets`
- `best assignment`
- `score breakdown`
- `applied effects`
- `next world snapshot id`

### Что желательно логировать внутри `candidate storylets`

Для каждого кандидата:
- `storylet id`
- valid / invalid
- если invalid: короткий rejection reason
- если valid: краткая сводка assignment
- локальный score
- selection status

### Что желательно логировать внутри `best assignment`

- storylet id
- выбранные role bindings
- relation constraints, которые сыграли ключевую роль
- объяснение, почему выбран именно этот assignment, если есть альтернативы

### Что желательно логировать внутри `score breakdown`

Минимальные компоненты:
- `instantiation quality`
- `transition value`
- `future potential`
- `anti-repetition`

Цель:
- score должен быть объяснимым по частям, а не только одним финальным числом

## Diagnostics for planner search

Так как target planner для проекта должен быть `beam-search planner`, trace обязан объяснять структуру beam search.

Минимальные поля:
- `depth`
- `beam width`
- branch pruning reason
- repeated-state collapse
- surviving branches at each depth
- winning branch score summary

Желательно фиксировать:
- сколько веток было расширено
- сколько веток было отброшено по score
- сколько веток было схлопнуто как эквивалентные состояния
- какие ветки пережили каждый beam cut
- какой branch ranking был у победившей ветки на каждом depth

## Author-facing diagnostics

Автору контента полезны объяснения на более человеческом уровне.

Система должна уметь показать, что storylet:
- unavailable because of missing tag
- unavailable because of missing attribute
- unavailable because of missing relation
- unavailable because of repeatability lock
- deprioritized because of salience
- deprioritized because of anti-repetition

Важно:
- автору не нужен внутренний код ошибки
- автору нужен понятный narrative reason и технический hint

## Telemetry and metrics

Минимальные рекомендованные метрики:
- `storylet activation count`
- average candidate count per state
- most common rejection reasons
- branch diversity indicator
- repetition indicator

### Зачем нужна `storylet activation count`

Позволяет увидеть:
- storylet-ы, которые никогда не достигаются
- storylet-ы, которые доминируют слишком часто
- storylet-ы, которые появляются заметно чаще или реже ожиданий

### Зачем нужна `average candidate count per state`

Позволяет увидеть:
- переузкие миры
- слишком шумные миры
- ветки, где planner почти не имеет выбора

### Зачем нужны `most common rejection reasons`

Позволяет понять:
- слишком ли суровы preconditions
- не переусложнены ли relations
- не душит ли систему repeatability policy

### Зачем нужны `branch diversity` и `repetition indicators`

Они помогают заметить:
- скучные однотипные последовательности
- чрезмерную локальную оптимизацию
- недостаток narrative variety

## Smoke-test recommendations

Для первых тестов нужны маленькие handcrafted worlds.

Рекомендуемые сценарии:
- `chain-unlock scenario`
- `conflict-over-resource scenario`
- `repeatability sanity check`
- `relation evolution scenario`

### `chain-unlock scenario`

Проверяет:
- один storylet открывает другой
- trace показывает, какой именно effect это сделал

### `conflict-over-resource scenario`

Проверяет:
- два storylet-а конкурируют за редкий actor pool
- planner может объяснить, почему выбрал один из них

### `repeatability sanity check`

Проверяет:
- `once ever`
- `once per run`
- cooldown
- anti-repetition policy

### `relation evolution scenario`

Проверяет:
- relation tags меняются по шагам
- новая relation меняет eligibility будущих storylet-ов

## Logging format guidance

Формат может быть любым, если он:
- последователен
- человекочитаем
- пригоден для последующего анализа

Минимальное требование:
- у каждой записи должен быть стабильный идентификатор шага или snapshot-а

Желательное требование:
- trace должен быть пригоден и для инженера, и для narrative author-а
- trace должен явно показывать beam-specific decisions, а не только итоговый выбранный путь

## Good vs bad diagnostics

Плохо:
- `Storylet failed`
- `No candidate`
- `Planner rejected branch`

Хорошо:
- `storylet.banish_the_raider` недоступен: нет relation `Enemy` между `role.guard` и `role.raider`
- ветка отброшена на depth 2: низкий `future potential`, после шага остаётся только 1 repeatable ambient candidate
- storylet valid, но не выбран: проиграл по `anti-repetition` и `transition value`

## Minimum Acceptance Questions

После внедрения diagnostics инженер должен уметь ответить:
- почему storylet не сработал
- почему storylet сработал именно с этим assignment
- какой effect создал новый narrative opportunity
- почему planner предпочёл одну ветку другой
- какие причины отказа встречаются чаще всего
