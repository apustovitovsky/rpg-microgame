# StoryletSystem Authoring Guide

Этот документ предназначен для narrative/design authoring.

Он помогает проектировать storylet-ы так, чтобы они:
- были понятны planner-у
- хорошо комбинировались друг с другом
- не создавали случайные soft-locks
- не превращались в одноразовые хаотичные special cases

## Как мыслить storylet-ом

Хороший storylet — это:
- atomic narrative chunk
- clear prerequisites
- clear consequences
- reusable parametrized roles

Практическая формула:
- storylet существует потому, что в мире есть узнаваемая ситуация
- storylet меняет мир так, чтобы появились новые последствия
- storylet не требует уникального хардкода в planner-е ради собственного существования

## Как писать preconditions

### Когда использовать tags

Используйте `tags`, когда нужно описать:
- архетип
- принадлежность к группе
- устойчивый narrative status
- простую категориальную фильтрацию

Примеры:
- `Warrior`
- `Noble`
- `Arcane`
- `Urban`

### Когда использовать attributes

Используйте `attributes`, когда нужен:
- порог
- диапазон
- накопление
- сравнение силы/ресурса/уровня риска

Примеры:
- `Wealth >= 20`
- `Fear < 50`
- `Influence >= 3`

### Когда использовать relations

Используйте `relations`, когда storylet зависит не от сущности “самой по себе”, а от связи между сущностями.

Примеры:
- доверие
- вражда
- покровительство
- долг
- союз

Хорошее правило:
- если смысл истории меняется при замене конкретной пары акторов, это обычно relation-driven storylet

### Как избегать переузких storylets

Не делайте storylet слишком узким без причины.

Сигналы переузкости:
- он требует слишком много редких тегов сразу
- ему нужен почти один-единственный actor pairing
- он срабатывает только после слишком длинной и хрупкой цепочки внутренних флагов

Лучше:
- сделать один сильный ключевой gating condition
- оставить часть драматической специфики на effects и salience
- строить семейства похожих storylet-ов вместо одного сверхспецифичного

## Repeatability policy

Repeatability должна быть явным authoring-решением.

Базовые режимы:
- `once ever`
- `once per run`
- `repeatable with cooldown`
- `repeatable until state changes`

### Когда использовать `once ever`

Для:
- крупных reveal moments
- irreversible turning points
- одноразовых payoff scenes

### Когда использовать `once per run`

Для:
- сюжетных событий, которые не должны повторяться в одной симуляции
- событий, допустимых в новых прохождениях, но не как ambient loop

### Когда использовать `repeatable with cooldown`

Для:
- ambient beats
- recurring incidents
- world flavor events, которые не должны спамиться подряд

### Когда использовать `repeatable until state changes`

Для:
- pressure loops
- unresolved situations
- recurring prompts, которые имеют смысл, пока мир не разрешил их причину

## Salience and surfacing

Storylet может быть valid, но не должен всплывать прямо сейчас.

`Salience` регулирует:
- заметность
- приоритет
- частоту появления
- защиту от шума и повторов

Повышать salience стоит, если:
- storylet является payoff после unlock
- storylet драматически отличает текущий world state от обычного
- storylet закрывает важную tension

Понижать salience стоит, если:
- storylet недавно уже показывался
- он повторяет тот же паттерн, что и предыдущие ходы
- он технически доступен, но narratively не должен доминировать

## Narrative pacing

Storylet-система лучше работает, когда контент образует ритм:
- unlock
- escalation
- resolve
- payoff

### Практические правила pacing

- не делайте все storylet-ы одинаково “важными”
- смешивайте ambient, pressure и payoff storylet-ы
- после сильного unlock желательно иметь хотя бы один заметный consequence beat
- не создавайте слишком много storylet-ов, конкурирующих за один и тот же редкий actor pool без драматической причины

### Как избегать content dead-ends

Проверьте:
- открывает ли storylet что-то дальше
- закрывает ли он только ветку или ещё и весь интересный темп
- не уводит ли он систему в состояние, где доступны только скучные повторы

### Как избегать accidental soft-locks

Не полагайтесь на:
- один-единственный редкий storylet без fallback
- один уникальный relation path, который легко потерять случайным выбором
- цепочку, где один early action навсегда убивает весь тематический кластер

## Naming conventions

Именование должно помогать explainability.

### Storylet ids

Используйте формат вида:
- `storylet.border_warning`
- `storylet.call_to_arms`
- `storylet.heal_the_wounded`

Правила:
- одно понятное действие или ситуация
- без технических префиксов, не нужных автору
- без двусмысленных сокращений

### Role keys

Role key должен описывать narrative function:
- `role.noble`
- `role.guard`
- `role.witness`
- `role.smuggler`

Не делать:
- `role.a`
- `role.slot2`
- `role.mainOptional`

### Effect names

Имена effect-ов должны объяснять изменение:
- `AddEntityTag`
- `RemoveRelationTag`
- `SetAttribute`
- `DespawnEntity`

### Relation tag names

Relation tags лучше называть как читаемое отношение:
- `Trust`
- `Enemy`
- `Vassal`
- `SwornAlly`
- `Patronage`

## Authoring checklist

### Storylet good citizen

Хороший storylet:
- имеет ясную ситуацию, а не случайный набор флагов
- не требует planner-specific special case
- может объяснить, почему он доступен именно сейчас

### Storylet changes world in a traceable way

Проверьте:
- его последствия можно перечислить
- последствия видны в world state
- последствия можно показать в debug trace

### Storylet explains why it exists in the narrative space

Проверьте:
- он что-то открывает, эскалирует, разрешает или окрашивает
- он не дублирует уже существующий beat без новой функции
- его repeatability и salience осмыслены

## Patterns

### 1. Unlocker storylet

Назначение:
- открыть новый narrative cluster

Хорошо подходит для:
- предупреждения
- revelation
- first contact

Требование:
- после него в мире должны появиться хотя бы 1–2 meaningful consequences

### 2. Consequence storylet

Назначение:
- отреагировать на предыдущий шаг

Хорошо подходит для:
- fallout
- escalation
- collateral damage

Требование:
- должно быть понятно, на что именно это consequence

### 3. Relation-dependent storylet

Назначение:
- использовать уже существующую драму между акторами

Хорошо подходит для:
- betrayal
- loyalty
- debt
- rescue

Требование:
- relation должна быть narrative source of truth, а не случайной формальностью

### 4. Repeatable ambient storylet

Назначение:
- поддерживать ощущение живого мира

Хорошо подходит для:
- городских слухов
- мелких столкновений
- routine interactions

Требование:
- cooldown или anti-repetition policy обязательны

### 5. One-shot dramatic storylet

Назначение:
- дать сильный сюжетный поворот

Хорошо подходит для:
- sacrificial choice
- public reveal
- irreversible oath or betrayal

Требование:
- `once ever` или `once per run`
- сильные и понятные effects

## Minimum Acceptance Questions

Автор нового storylet-а должен уметь ответить:
- какие у storylet-а `Preconditions`
- какие у него `Effects`
- почему он повторяемый или неповторяемый
- почему он должен быть заметным или малозаметным
- как он помогает ритму narrative space
