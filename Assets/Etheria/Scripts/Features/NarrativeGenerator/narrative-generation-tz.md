# ТЗ: Процедурная генерация истории и квестовых зацепок

## Назначение

Документ фиксирует требования к системе генерации предыстории региона, локальных конфликтов и квестовых зацепок для активной части проекта на базе `Assets/Etheria`.

Система нужна для:

- генерации связной предыстории;
- формирования текущего состояния региона;
- записи причинно-следственной истории;
- подготовки квестовых поводов поверх сгенерированного мира.

## Цель

Построить data-driven систему, которая:

- хранит состояние региона в `WorldState`;
- генерирует историю через события;
- возвращает финальный `WorldState` и плоский `EventLog`;
- строит поверх `EventLog` граф `HistoryGraph`;
- дает на выходе 2-5 осмысленных квестовых поводов.

## Что не входит в первый этап

- полный квестовый контент и диалоги;
- детальная симуляция большого мира;
- именованные NPC на каждый шаг истории;
- строгий календарь и поминутное время;
- обязательный narrative-слой.

## Базовая терминология

- `Entity` — базовая сущность мира;
- `WorldState` — агрегированное состояние региона;
- `Tag` — качественная метка сущности, например `farm`, `abandoned`, `bandit`, `unsafe`;
- `TagMask` — bitmask-представление набора тегов сущности в simulation core;
- `Attribute` — постоянные числовые поля сущности, например `Gold`, `Food`, `Count`, `SoilQuality`, `Population`;
- `Modifier` — эффект, который меняет числовую динамику сущности по тикам и/или влияет на вычисляемые значения при проверке событий; может быть постоянным или иметь ограниченную длительность;
- `Relation` — отдельная plain-data запись о направленной связи между двумя сущностями мира;
- `Event` — шаблон возможного события мира с условиями активации, оценкой и последствиями;
- `Outcome` — блок результата события, в котором описаны все возможные варианты исхода и их изменения мира;
- `EventRecord` — плоская запись о произошедшем инстансе события в ходе симуляции;
- `HistoryGraph` — граф причинно-следственных связей, который постпроцессом строится поверх `EventRecord[]`;
- `DAG` — форма представления `HistoryGraph` без циклов между уже произошедшими событиями.

## Модель генерации

Первая версия использует обычный `daily tick`.

Один день — это один шаг симуляции:

1. По `WorldState_N` применяется daily-динамика уже существующих `Modifier`.
2. После этого формируется состояние мира текущего тика для event-фазы.
3. `Matcher` находит кандидатов на события.
4. `Relation`-этап по связям мира собирает полную комбинацию участников события.
5. `Scorer` считает `ActivationScore` только для кандидатов, успешно прошедших relation-resolve.
6. Ready считаются только события, где `ActivationScore >= ActivationThreshold`.
7. `Selector` строит `captureList`, разрешает конфликты и собирает `commitSet`.
8. Для выбранных событий разрешается `Outcome` и применяются изменения выбранного варианта.
9. В `EventLog` записываются `EventRecord`.
10. На выходе получается `WorldState_N+1`.

## WorldSimulationRunner

За orchestration одного simulation tick отвечает отдельный runner верхнего уровня:

- `WorldSimulationRunner`.

`WorldSimulationRunner` не является частью low-level simulation core. Его задача — запускать фазы тика в правильном порядке, передавать `WorldState` между фазами и собирать итоговый результат шага симуляции.

Ответственности `WorldSimulationRunner`:

- принять `WorldState_N`;
- запустить modifier daily-phase;
- пересобрать relation snapshot текущего тика;
- запустить `Matcher`;
- запустить relation-этап;
- запустить `Scorer`;
- запустить `Selector`;
- разрешить `Outcome` выбранных событий;
- записать `EventLog`;
- вернуть `WorldState_N+1`.

Минимальная концептуальная схема:

```text
WorldSimulationRunner
    -> Modifier daily phase
    -> RebuildRelationStore
    -> Matcher
    -> Relation
    -> Scorer
    -> Selector
    -> Outcome
    -> EventLog
    -> WorldState_N+1
```

Требования к runner-слою:

- runner оркестрирует фазы, но не подменяет собой их внутреннюю логику;
- runner не должен содержать narrative-оформление, текстовую генерацию или designer authoring;
- runner не должен переносить orchestration в `MonoBehaviour`;
- runner должен работать как plain C# orchestration service.

Архитектурное правило для Etheria:

- `WorldSimulationRunner` регистрируется и запускается через `VContainer`;
- orchestration находится в сервисе или entry point;
- execution core остается отделенным от `MonoBehaviour`, `ScriptableObject` authoring и narrative post-process слоя.

Практический интерфейс первого этапа может быть выражен как:

```text
RunTick(WorldState currentState) -> WorldState nextState
RunSimulation(WorldState initialState, int dayCount) -> final WorldState + EventLog
```

Где:

- `RunTick` — минимальная операция одного дня симуляции;
- `RunSimulation` — внешний цикл по дням для генерации всей предыстории региона.

## Семантика Modifier-фазы

Правила:

- любая фоновая ежедневная динамика выражается через `Modifier`;
- `upkeep`, `decay`, `income`, `consumption` — это не отдельные механики, а частные случаи daily-эффектов `Modifier`;
- `AttributeDelta` меняет `BaseAttribute` в daily-фазе;
- `AttributeBonus` не меняет `BaseAttribute`, а участвует в расчете `EffectiveAttribute`, пока модификатор жив;
- `TagAddMask` и `TagRemoveMask` модификатора участвуют в формировании `EffectiveTagMask` текущего тика;
- для каждой сущности `AttributeDelta` ее модификаторов, активных на начало тика, сначала агрегируются в отдельный буфер, а затем применяются к `BaseAttribute` этой сущности одной пачкой;
- для каждой сущности `TagAddMask` ее модификаторов, активных на начало тика, агрегируются в отдельный tag overlay;
- для каждой сущности `TagRemoveMask` ее модификаторов, активных на начало тика, агрегируются в отдельный tag overlay;
- daily-эффект модификатора срабатывает только если модификатор уже существовал в начале тика;
- модификатор, созданный событием в текущем тике, начинает влиять только со следующего тика;
- `Duration < 0` означает постоянный эффект без ограничения по времени;
- `Duration >= 0` означает число будущих daily-фаз, в которых модификатор еще участвует.

Порядок внутри тика:

1. Для каждой сущности берутся только те модификаторы, которые были активны у этой сущности в начале тика.
2. `AttributeDelta` этих модификаторов собирается в отдельный буфер изменений атрибутов этой сущности.
3. `TagAddMask` и `TagRemoveMask` этих модификаторов собираются в отдельный tag overlay этой сущности.
4. Буфер изменений одной записью применяется к `BaseAttribute` этой сущности.
5. На основе базового `TagMask` этой сущности и ее агрегированного tag overlay формируется `EffectiveTagMask`.
6. Затем у этих модификаторов уменьшается `Duration`.
7. Если `Duration == 0`, модификатор удаляется до event-фазы.
8. После этого `Matcher`, `Relation` и `Scorer` работают с уже подготовленным состоянием этой сущности для текущего тика.

Следствие:

- если модификатор удален в текущем тике, он уже не участвует в `Matcher` и `Scorer`;
- если модификатор давал `AttributeBonus`, при удалении ничего отдельно "откатывать" не нужно: бонус просто перестает входить в `EffectiveAttribute`;
- если модификатор добавлял или скрывал теги, при удалении ничего отдельно "откатывать" не нужно: его вклад просто перестает входить в `EffectiveTagMask`;
- порядок модификаторов внутри тика не должен влиять на результат daily-фазы.
- `Matcher`, `Relation` и `Scorer` читают одно и то же состояние мира текущего тика, уже после modifier daily-фазы.

## Raw-атрибуты

Правила:

- `BaseAttribute` хранит сырые данные симуляции и не обязан заранее clamp-иться в "удобный" диапазон;
- отрицательные или выходящие за привычные рамки значения допустимы, если они осмысленны для мира, например дефицит или долг;
- глобальный clamp после `aggregate apply` не обязателен;
- безопасная интерпретация `raw` делается в месте использования: в `ActivationFactor`, `Normalization` или конкретных relation-проверках;
- если конкретный расчет не должен видеть отрицательные значения, он clamp-ит или преобразует `raw` локально.

## Мир первого прототипа

Базовые сущности и их базовые теги:

- `World` — `world`;
- `Village` — `village`;
- `Farm` — `farm`;
- `HunterCamp` — `huntercamp`;
- `BanditHideout` — `bandithideout`;

Базовые параметры:

- `WildlifeActivity`;
- `BanditActivity`;
- `Gold`;
- `FoodProduction`;
- `Prosperity`;
- `Population`;
- `Control`;
- `Trust`;
- `Threat`;
- при необходимости дополнительные числовые pressure- и tension-параметры.

Примеры `Modifier` первого этапа:

- `RaidPreparation`;
- `RaidInProgress`;
- `RecentRaidMemory`;
- `DamagedFields`;
- `MilitiaProtection`.

## Теги сущностей

Каждая `Entity` хранит набор тегов как основное качественное описание своего состояния и роли в мире.

Теги одновременно используются:

- как основной качественный слой состояния сущности;
- как быстрый поисковый индекс для `Matcher`;
- как способ data-driven описывать шаблоны событий без строковой логики в execution core.

Примеры тегов первого этапа:

- `farm`;
- `village`;
- `settlement`;
- `city`;
- `hunter`;
- `bandit`;
- `hideout`;
- `remote-area`;
- `abandoned`;
- `destroyed`;
- `unsafe`;
- `prosperous`.

Правила модели:

- каждая сущность имеет `TagMask`;
- каждый тег получает стабильный `TagId` внутри общего `TagCatalog`;
- `TagId` не переиспользуется после назначения;
- runtime-представление тегов должно быть выражено в Burst-friendly plain-data форме;
- базовая операция проверки тегов должна сводиться к побитовым операциям, а не к перебору строк или managed-коллекций;
- качественное состояние сущности выражается через текущую комбинацию тегов;
- `Attribute` и `Modifier` остаются отдельными слоями симуляции и не заменяются тегами;
- `Matcher` и event-логика текущего тика читают не только базовые теги сущности, но итоговый `EffectiveTagMask`, сформированный с учетом активных модификаторов;
- семантически корректные комбинации тегов задаются дизайном событий и их `Outcome`.

Требование к представлению:

- в authoring-слое сущности и шаблоны событий хранят теги в asset-конфигурации;
- до входа в Burst execution layer для каждой сущности создается пустая runtime `TagMask`, после чего в ней выставляются биты по тегам из authoring-данных;
- внутри одного simulation run и внутри Burst execution layer расположение битов в `TagMask` остается фиксированным;
- в simulation execution core теги хранятся не строками и не списками строк;
- теги должны быть представлены как битовая маска фиксированного или заранее определенного размера;
- предпочтительное представление первого этапа — набор `ulong`-слов, например `TagMask` на `64`, `128`, `256` или другом заранее выбранном лимите тегов;
- операция `HasTag` должна сводиться к вычислению `wordIndex`, `bitMask` и одному побитовому `AND`.

Минимальная концептуальная схема:

```text
TagCatalog
    TagId -> semantic meaning

Entity
    EntityId
    TagMask
    Attributes
    Modifiers
```

Где:

- `TagMask` в сущности хранит базовые теги;
- `EffectiveTagMask` для текущего тика формируется поверх базового `TagMask` с учетом `TagAddMask` и `TagRemoveMask` всех модификаторов, активных в начале тика;
- итоговая формула первого этапа:

```text
EffectiveTagMask =
    (TagMask | AggregatedTagAddMask)
    & ~AggregatedTagRemoveMask
```

Событие должно уметь задавать теговый запрос к участникам:

```text
TagQuery
    TagId[] AllOfTags
    TagId[] AnyOfTags
    TagId[] NoneOfTags
```

`TagQuery` является универсальной структурой системы.

Один и тот же `TagQuery` используется:

- в `Event.TagQuery` для поиска инициатора в `Matcher`;
- в relation-этапе как `RelationTagQuery` для фильтрации `RelationRecord`;
- в relation-этапе как `TargetEntityTagQuery` для фильтрации целевой сущности;
- в любых других местах системы, где нужен declarative tag filter без отдельного специального query-типа.

Семантика:

- `AllOfTags` — все перечисленные теги обязаны присутствовать;
- `AnyOfTags` — должна присутствовать хотя бы одна из перечисленных меток;
- `NoneOfTags` — ни одна из перечисленных меток не должна присутствовать.
- один `TagQuery` может одновременно использовать `AllOfTags`, `AnyOfTags` и `NoneOfTags`;
- итоговый результат `TagQuery` вычисляется как логическое `AND` между проверками `AllOfTags`, `AnyOfTags` и `NoneOfTags`;
- повторяющиеся теги внутри одного списка допустимы и не меняют семантику запроса;
- полностью пустой `TagQuery`, где одновременно пусты `AllOfTags`, `AnyOfTags` и `NoneOfTags`, считается бессмысленным и не матчится ни с одной сущностью.

Authoring-правило:

- `TagQuery` собирается дизайнером в `ScriptableObject`-конфигурации;
- authoring-слой хранит query в удобной для дизайнера asset-форме;
- до входа в simulation execution core designer data компилируются в runtime plain-data представление;
- внутри Burst execution layer `TagQuery` не должен зависеть от строк, managed-коллекций или `UnityEngine.Object`.

Пример:

```text
BanditsRaidAbandonedFarm
    TagQuery
        AllOfTags:  farm, abandoned
        AnyOfTags:  none
        NoneOfTags: destroyed
```

Это означает, что `Matcher` должен очень быстро уметь ответить, подходит ли сущность под шаблон `farm + abandoned`, без строковой логики и без сложных проверок на первом проходе.

## Matcher и теговый отбор

`Matcher` — это первый и самый дешевый этап отбора кандидатов.

В первом этапе он делает следующее:

1. взять событие и его `TagQuery` инициатора;
2. быстро найти сущности, подходящие по `EffectiveTagMask` инициатора;
3. собрать по тегам массив всех потенциальных инициаторов события;
4. передать каждого такого инициатора в `Relation` и `Scorer`.

Правила:

- `Matcher` в первую очередь работает по тегам;
- `Matcher` не должен начинать с дорогих вычислений `Factors`;
- теговый матчинг должен быть дешевле, чем relation-resolve, и существенно дешевле, чем `Scorer`;
- если сущность не проходит по `EffectiveTagMask`, она не должна попадать в дальнейший расчет события;
- `Matcher` читает состояние мира после modifier daily-фазы, как и остальные этапы текущего тика;
- `Matcher` проверяет `TagQuery` по текущему `EffectiveTagMask` сущности;
- `Matcher` собирает все сущности, удовлетворяющие `TagQuery`;
- если по `TagQuery` не найдено ни одной сущности, матчинг этого запроса считается неуспешным.
- `TagQuery` описывает только инициатора события;
- дополнительные участники события определяются и валидируются на relation-этапе;
- `Matcher` не обязан собирать полную многосубъектную комбинацию события.

Минимальная схема:

```text
Event
    EventId Id
    TagQuery TagQuery
    RelationNode[] Relations
    ActivationFactor[] Factors
    float ActivationThreshold
    int Priority
    Outcome Outcome
```

Где:

- `TagQuery` — набор теговых условий для поиска потенциальных инициаторов события в `Matcher`;
- `Relations` — relation-цепочка, которая строит и валидирует набор участников события;
- `Factors` — набор числовых факторов для расчета `ActivationScore` только для уже найденных кандидатов.
- все query в событии используют один и тот же универсальный тип `TagQuery`, задаваемый в authoring через `ScriptableObject`.

## Relations и разрешение участников

`Relation` — это отдельный этап после `Matcher`, который превращает инициатора события в полную валидную комбинацию участников для `Scorer`, `captureList` и `Outcome`.

Задача relation-этапа:

- пройти по графу связей мира, начиная от инициатора;
- при необходимости пройти несколько последовательных шагов по связям;
- на каждом шаге отфильтровать подходящие связи по тегам связи;
- на каждом шаге отфильтровать подходящие целевые сущности по тегам сущности;
- заполнить runtime-контекст участников события;
- вернуть `false`, если хотя бы один шаг relation-цепочки не может быть выполнен.

`Relation` не считает важность события и не участвует в нормализации score. Его задача — доказать, что для текущего инициатора в мире существует допустимая комбинация сущностей и связей.

### Relation как слой `WorldState`

Сущности не хранят прямые ссылки друг на друга внутри `Entity`.

Связи мира лежат отдельно как часть `WorldState`.

Минимальная форма записи:

```text
RelationRecord
    EntityId Source
    EntityId Target
    TagMask RelationTagMask
```

Смысл:

- `Source` — сущность, из которой исходит связь;
- `Target` — сущность, в которую приходит связь;
- `RelationTagMask` — качественное описание самой связи.

Примеры relation-тегов первого этапа:

- `threatens`;
- `protects`;
- `belongs-to`;
- `depends-on`;
- `supplies`;
- `hunts-in`;
- `controls`.

Связь — это отдельный факт мира, а не часть `TagMask` сущности.

Пример:

```text
BanditHideout_01 -> Farm_03
RelationTagMask: threatens
```

Это означает не то, что `Farm_03` имеет тег `threatens`, а то, что между двумя сущностями существует конкретное отношение типа `threatens`.

### Runtime-представление relation-индекса

Для быстрого доступа к связям текущей сущности relation-слой должен иметь индексы по сущностям.

Предпочтительное представление первого этапа:

```text
NativeArray<RelationRecord> Relations
NativeParallelMultiHashMap<int, int> OutgoingRelationsByEntity
NativeParallelMultiHashMap<int, int> IncomingRelationsByEntity
```

Где:

- ключ `OutgoingRelationsByEntity` — `EntityId`;
- значение `OutgoingRelationsByEntity` — `RelationId` или индекс в `Relations`;
- ключ `IncomingRelationsByEntity` — `EntityId`;
- значение `IncomingRelationsByEntity` — `RelationId` или индекс в `Relations`.

Это дает:

- быстрый доступ ко всем исходящим связям сущности;
- быстрый доступ ко всем входящим связям сущности;
- возможность делать relation traversal без полного скана всего `RelationRecord[]`.

### RelationNode

Каждый шаг relation-цепочки описывается отдельным `RelationNode`.

Минимальная структура:

```text
RelationNode
    byte SourceSlot
    RelationDirection Direction
    TagQuery RelationTagQuery
    TagQuery TargetEntityTagQuery
    byte BindTargetSlot
```

Где:

- `SourceSlot` — слот уже найденной сущности, от которой начинается текущий шаг;
- `Direction` — направление прохода по связи: `Outgoing` или `Incoming`;
- `RelationTagQuery` — фильтр по тегам связи, использующий ту же структуру `TagQuery`, что и `Event.TagQuery`;
- `TargetEntityTagQuery` — фильтр по тегам целевой сущности, использующий ту же структуру `TagQuery`, что и `Matcher`;
- `BindTargetSlot` — слот, в который записывается найденная целевая сущность.

`RelationNode` одновременно:

- выбирает направление обхода;
- фильтрует relation-записи;
- фильтрует сущность на выходе шага;
- привязывает найденную сущность в runtime-контекст.

### RelationContext

Relation-этап должен заполнять отдельный runtime-контекст участников события.

Минимальная форма:

```text
RelationContext
    Slot[0..N]
```

Правила:

- слот `0` зарезервирован под инициатора;
- слоты `1..N` используются как временные binding slots конкретного события;
- смысл слотов задается в authoring-данных события, а не в C# enum;
- `Scorer`, `captureList` и `Outcome` читают уже готовый `RelationContext`.

Пример локальной схемы слотов события:

```text
Slot 0 -> Initiator
Slot 1 -> RaidTargetFarm
Slot 2 -> OwnerVillage
```

### Семантика relation-цепочки

`Relations` внутри `Event` выполняются строго по порядку.

Каждый `RelationNode`:

1. берет сущность из `SourceSlot`;
2. читает все подходящие relation-записи по направлению `Direction`;
3. фильтрует их по `RelationTagQuery`;
4. берет сущность на другом конце relation;
5. фильтрует ее по `TargetEntityTagQuery`;
6. при успехе записывает найденную сущность в `BindTargetSlot`;
7. при неуспехе останавливает relation-этап и возвращает `false`.

Первый этап использует практичное правило:

- relation-этапу достаточно найти первую валидную полную комбинацию участников;
- relation-этап не обязан перечислять все возможные пути;
- найденная комбинация фиксируется в `RelationContext` и передается дальше в `Scorer`.

Пример relation-цепочки:

```text
BanditsRaidFarm
    TagQuery: bandithideout
    Relations:
        1. SourceSlot: 0
           Direction: Outgoing
           RelationTagQuery: threatens
           TargetEntityTagQuery: farm AND NOT destroyed
           BindTargetSlot: 1

        2. SourceSlot: 1
           Direction: Outgoing
           RelationTagQuery: belongs-to
           TargetEntityTagQuery: village AND NOT destroyed
           BindTargetSlot: 2
```

После выполнения relation-этапа событие получает:

- `Slot 0` — логово бандитов;
- `Slot 1` — подходящую ферму;
- `Slot 2` — деревню-владельца фермы.

### Event и relation-этап

Каждый `Event` должен поддерживать:

- `Id`;
- `TagQuery`;
- `Relations`;
- `Factors`;
- `ActivationThreshold`;
- `Priority`;
- `Outcome`.

Минимальная структура:

```text
Event
    EventId Id
    TagQuery TagQuery
    RelationNode[] Relations
    ActivationFactor[] Factors
    float ActivationThreshold
    int Priority
    Outcome Outcome
```

Правила relation-этапа:

- relation-этап выполняется после modifier daily-phase;
- relation-этап выполняется после `Matcher`, но до `Scorer`;
- relation-этап не считает `ActivationScore`;
- relation-этап только строит допустимую комбинацию участников;
- если relation-этап не смог собрать полную цепочку, событие дальше не участвует в scoring.

## Основная предметная логика

Первый прототип строится на следующей динамике:

- высокая активность зверей вредит фермам;
- охотники приходят в опасные зоны со зверьем;
- охотники помогают против зверей, но создают богатую цель;
- бандиты тяготеют к богатым и плохо защищенным точкам;
- бандиты снижают безопасность;
- низкая безопасность и накопленная угроза двигают фермы по цепочке тегов `active -> declining -> abandoned` или `destroyed`;
- последствия событий должны открывать новые события.

## Минимальный набор событий

- охотники основали лагерь;
- охотники сократили зверье;
- звери разоряют фермы;
- фермеры наняли охотников;
- бандиты основали логово;
- бандиты ограбили ферму;
- бандиты напали на охотников;
- охотники выследили бандитов;
- бандиты ушли глубже в лес;
- ферма пришла в упадок;
- фермеры покинули ферму.

Этого достаточно для базовой живой динамики и квестовых хуков.

## Структура Event

Каждый `Event` должен поддерживать:

- `Id`;
- `TagQuery`;
- `Relations`;
- `Factors`;
- `ActivationThreshold`;
- `Priority`;
- `Outcome`.

Примерная структура:

```text
Event
    EventId Id
    TagQuery TagQuery
    RelationNode[] Relations
    ActivationFactor[] Factors
    float ActivationThreshold
    int Priority
    Outcome Outcome
```

Минимальная структура фактора:

```text
ActivationFactor
    FactorId Id
    FactorSource Source
    float Weight
    bool Penalty
    bool Inverse
    NormalizationType Normalization
    float ParamA
    float ParamB
```

Где:

- `Normalization` задает тип кривой нормализации;
- `ParamA` и `ParamB` задают ее параметры;
- `factorScore` считается уже из `raw`, `Normalization`, `ParamA` и `ParamB`.

## Outcome

`Outcome` разрешается только после того, как событие:

- успешно прошло relation-этап;
- набрало `ActivationScore >= ActivationThreshold`;
- выиграло конфликт по `captureList`.

Задача `Outcome`:

- посчитать обстоятельства уже выбранного события;
- определить конкретный результат события;
- выбрать один вариант исхода и применить его изменения мира.

Это не отдельный этап matching или scoring. Это разрешение уже выбранного события.

Практическое правило:

- если у события нет развилки, `Outcome` может содержать один вариант;
- если у события есть несколько возможных результатов, они описываются внутри одного `Outcome`;
- для первого этапа допускается простой `if / else` на основе рассчитанных величин.

Пример:

```text
BanditsRaidFarm
    Outcome
        if attackPower - defensePower >= 40
            -> FullSuccess
        else if attackPower - defensePower >= 10
            -> PartialSuccess
        else
            -> Repelled
```

## Нормализация и скоринг

Для всех числовых факторов события используется одна и та же формула.

Базовые правила:

- для каждого фактора считается `factorScore` через выбранный `Normalization`;
- `factorScore` всегда в диапазоне `0..1`;
- если `Inverse == true`, берется `1 - factorScore`;
- итоговый `ActivationScore` проверяется общим `ActivationThreshold`;
- булевы условия не входят в формулу и только допускают событие к оценке;
- вычисления должны быть пригодны для Burst-friendly математики.

Поддерживаемые режимы:

```text
Normalization
    SoftSaturate
    Saturate
    Smoothstep
```

Где:

- `SoftSaturate` — наша utility-нормализация;
- `Saturate` и `Smoothstep` — режимы на базе `Unity.Mathematics.math`.

Параметры:

- `SoftSaturate`: `ParamA = halfValue`, `ParamB` не используется;
- `Saturate`: `ParamA = maxValue`, `ParamB` не используется;
- `Smoothstep`: `ParamA = start`, `ParamB = end`.

Практические правила:

```text
if Normalization == SoftSaturate
    factorScore = raw / (raw + ParamA)

if Normalization == Saturate
    factorScore = math.saturate(raw / ParamA)

if Normalization == Smoothstep
    factorScore = math.smoothstep(ParamA, ParamB, raw)

if Inverse
    factorScore = 1 - factorScore

ActivationScore = Σ(factorScore * weight * penaltySign) / Σ(weight)
```

Где:

- `penaltySign = -1`, если `Penalty == true`;
- `penaltySign = +1`, если `Penalty == false`.

Смысл:

- обычный фактор поднимает итоговый score;
- `Penalty` понижает его, но сам по себе не запрещает событие;
- `Inverse` позволяет перевернуть смысл фактора без отдельной формулы;
- `SoftSaturate` хорошо подходит для накопительного pressure;
- `Saturate` подходит для простого линейного порога с жёстким потолком `0..1`;
- `Smoothstep` подходит для мягкого threshold в диапазоне `start..end`;
- `SoftSaturate` не упирается в жесткий потолок слишком рано.

## Алгоритм выбора события

### Candidate phase

1. `Matcher` по `TagQuery` и `EffectiveTagMask` находит кандидатов на события.
2. Теговый отбор является первым и самым дешевым фильтром.
3. `Matcher` возвращает массив потенциальных инициаторов события.
4. `Relation`-этап проходит по связям мира, собирает вторичные сущности и валидирует полную комбинацию участников.
5. События, для которых relation-цепочка не может быть собрана, дальше не участвуют.

### Scoring phase

1. Для каждого допущенного события `Scorer` считает `ActivationScore`.
2. Событие считается `ready`, если `ActivationScore >= ActivationThreshold`.
3. `ActivationScore` отвечает только за то, достаточно ли событие назрело для активации для конкретного инициатора и найденной комбинации участников.

### Commit phase

1. Для каждого `ready` события строится `captureList`.
2. `Ready` события сортируются по `Priority` по убыванию.
3. Если `Priority` равен, используется детерминированный tie-break, например по `EventId`.
4. В коммит greedily попадают только события без пересечения по `captureList` с уже выбранными.
5. События с пересечением пропускаются.
6. Для каждого выбранного события разрешается `Outcome`.
7. Применяются изменения выбранного варианта `Outcome`.

Каждое событие в score-модели обычно сочетает:

- главную причину;
- ресурс доступности;
- penalty.

Для каждого фактора сначала берется `raw`, затем через его `Normalization` считается `FactorScore`.

Общий принцип:

```text
raw
    -> Normalization
    -> FactorScore
```

Итог события:

```text
ActivationScore =
    Σ(FactorScore * Weight * PenaltySign)
    /
    Σ(Weight)
```

Где:

- `PenaltySign = +1`, если `Penalty == false`;
- `PenaltySign = -1`, если `Penalty == true`.

Смысл:

- bonus-факторы показывают, почему событие нужно;
- availability-факторы показывают, можно ли его удобно выполнить;
- penalty не запрещает событие, а только снижает его привлекательность.

Финальная схема:

```text
Modifier daily phase
    -> Matcher
    -> Relation
    -> ActivationScore
    -> ActivationThreshold
    -> captureList
    -> Priority
    -> greedy commit
    -> Outcome
```

## Конфликты и commit

Упрощенные правила первого этапа:

- если у двух событий есть хотя бы одна общая сущность, они конфликтуют;
- если пересечения по сущностям нет, события совместимы;
- совместимость проверяется только по `captureList`;
- роли события не участвуют в алгоритме конфликта напрямую.

Минимальная структура:

```text
EventIntent
    EventId EventId
    float ActivationScore
    int Priority
    EntityId[] captureList
```

Правило формирования `captureList`:

- `captureList` — это массив `EntityId[]`, который событие передает в conflict resolver;
- `captureList` содержит только сущности, которые уже существуют на момент event matching;
- в него входят все сущности, которые реально участвуют в commit этого события;
- если событие использует три стороны, в массив попадают все три `EntityId`;
- если сущность используется только как источник данных для расчета и событие ее не меняет и не расходует, ее `EntityId` в `captureList` не нужен;
- relation отдельно не lock-ается;
- сущность, созданная через `CreateEntity` в текущем тике, не входит в `captureList` и начнет участвовать в matching только со следующего тика.

Следствие:

- для lock-механики не нужно определять, кто из участников является `Target`, `Provider` или `Source`;
- если событию для собственной логики нужны роли, оно может хранить их отдельно, но conflict resolver использует только `captureList`.

Алгоритм коммита:

```text
readyEvents = sortDescByPriority(allReadyEvents)

commitSet = []
lockedEntities = {}

for event in readyEvents:
    if intersects(event.captureList, lockedEntities):
        continue

    commitSet.add(event)
    lockedEntities.addAll(event.captureList)
```

Это жадный `greedy set packing` по сущностям. Победитель конфликта определяется не по score, а по `Priority`, который задается дизайнером.

Короткий пример:

```text
HireHunters = 0.377
HireMilitia = 0.397
```

Если оба события прошли `ActivationThreshold = 0.35`, они оба считаются ready.

Если они оба используют `Farm_01`, вместе их коммитить нельзя. В greedy-проходе сначала берется событие с большим `Priority`, а второе пропускается как конфликтующее.

## Порог выбора

Минимальное практичное правило:

```text
readyEvents = events where ActivationScore >= ActivationThreshold

if readyEvents.Count == 0
    -> nothing / fallback

if readyEvents.Count == 1
    -> run best

if readyEvents.Count >= 2
    -> sort by Priority desc
    -> greedily take only events with no entity intersection
```

Практический смысл:

- общий `ActivationThreshold` делает порог активации единым между событиями;
- relation-этап отсекает события, для которых в мире нет допустимой цепочки участников;
- конфликт решается только по `Priority` и пересечению сущностей.

Практичное правило активации:

```text
if Relation == true
AND ActivationScore >= ActivationThreshold
    -> event is ready
```

## Команды Outcome

`Outcome` хранит варианты результата события. Каждый вариант содержит набор мгновенных команд изменения мира.

Базовый набор команд первого этапа:

- `AddTag`;
- `RemoveTag`;
- `AddModifier`;
- `RemoveModifier`;
- `ChangeAttribute`;
- `CreateEntity`;
- `DestroyEntity`;
- `AddRelation`.

`AddTag` и `RemoveTag` применяются в фазе event commit так же, как остальные команды изменения мира.

Их семантика:

- они не влияют на `Matcher`, `Relation` и `Scorer` текущего тика;
- они не меняют уже сформированный world snapshot текущего тика;
- они начинают влиять на теговый матчинг только в следующем тике.

`AddModifier` и `RemoveModifier` применяются в фазе event commit. Их эффекты начинают учитываться только со следующего тика.

Минимальная схема данных:

```text
Modifier
    int Duration
    Attribute[] AttributeDelta
    Attribute[] AttributeBonus
    TagMask TagAddMask
    TagMask TagRemoveMask
```

```text
Attribute
    AttributeId Id
    float Value
```

Последствия события должны открывать новые события не отдельным follow-up-механизмом, а обычным изменением `WorldState`, `TagMask`, `Attribute` и `Modifier` к следующему тику.

## Масштабы событий

Система должна поддерживать:

- `Local`;
- `Regional`;
- `Factional`;
- `Global` или `Kingdom`.

Глобальные события в первом этапе не обязаны создавать конкретных NPC. Их задача — менять агрегированное состояние мира и открывать нижестоящие события.

## История и выходные данные

Симуляционный слой должен возвращать:

- финальный `WorldState`;
- плоский `EventLog` из `EventRecord[]`.

`EventRecord` должен быть простым и Burst-friendly:

```text
EventRecord
    EventId EventId
    int Day
    EventTypeId EventType
    OutcomeTypeId OutcomeType
    EntityId ActorA
    EntityId ActorB
    EntityId Target
    ZoneId LocationOrZone
    EventId CauseEventA
    EventId CauseEventB
    EventId CauseEventC
```

`HistoryGraph` строится отдельным постпроцессом поверх `EventLog`.

Требование:

- финальный граф истории должен быть `DAG`;
- симуляция не должна пытаться строить "красивый граф" напрямую;
- в симуляции пишется только плоский лог с `cause`-ссылками;
- `DAG` собирается потом из `EventRecord[]`.

Пример:

```text
Node 88: появилось логово бандитов
Node 96: бандиты начали рейд
Node 104: рейд завершился успехом

88 -> 96 -> 104
```

## Narrative-слой

Narrative-слой не обязателен в первой версии, но архитектурно должен быть возможен.

Его задача:

- превращать `EventRecord` в текст;
- связывать историю с NPC;
- формировать слухи и квестовые поводы.

Принцип разделения:

```text
Simulation layer:
    Event -> Outcome -> EventRecord

Narrative layer:
    интерпретирует записи и оформляет их как историю и квестовые зацепки
```

## Экономика первого этапа

Нужна только опорная модель:

- `Food` показывает выживание;
- `Gold` показывает способность платить;
- фермы производят еду;
- поселения покупают или теряют доступ к еде;
- защита, охотники и ополчение требуют ресурсов;
- бандиты и звери должны бить по `Food`, `Gold`, `Safety`, `Fear`.

Этого достаточно, чтобы угрозы и реакции имели смысл.

## Технические требования

Система должна быть data-driven:

- шаблоны событий лежат в конфигурации;
- условия и веса настраиваются из данных;
- варианты `Outcome` описываются декларативно;
- должно быть понятно, почему событие было валидно и почему было выбрано.

Жесткое требование:

- исполняемое ядро симуляции должно быть выражено в Burst Compile friendly форме;
- если логика не может быть выражена в Burst Compile friendly форме, она не должна находиться внутри simulation execution core.

Для текущей архитектуры Etheria предпочтительны:

- `ScriptableObject` для конфигурации;
- plain C# сервисы для матчинга, скоринга, селекции и записи истории;
- явная регистрация через `VContainer`;
- orchestration в сервисах и entry points, а не в тяжелых `MonoBehaviour`.

## Burst + Jobs

`Burst Compile friendly` означает не просто запрет отдельных типов, а требование к форме исполнения.

Исполняемое ядро симуляции должно быть организовано так, чтобы его вычислительные проходы можно было компилировать Burst-компилятором и выполнять как Burst-compatible jobs или эквивалентные Burst-compatible проходы.

Требования:

- simulation execution core должно опираться на plain-data структуры, числовые идентификаторы, циклы и Burst-compatible математику;
- `EventLog` должен состоять из плоских записей без строк, классов и ссылок на Unity-объекты;
- вычисления факторов, весов, порогов, daily-обновления и commit-логика должны быть совместимы с Burst-friendly математикой;
- внутри simulation execution core нельзя завязываться на managed-объекты, строки, `UnityEngine.Object` и другую не Burst-friendly runtime-логику;
- `ScriptableObject`, authoring-конфигурация, narrative, текст и постобработка должны быть вынесены за пределы simulation execution core.

Практический принцип:

```text
Authoring/config layer
    -> ScriptableObject
    -> designer data

Burst/jobs execution layer
    -> WorldState
    -> EventLog

Post-process layer
    -> DAG / HistoryGraph
    -> text
    -> quest hooks
```
