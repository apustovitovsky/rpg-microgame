Сейчас у нас готова цепочка:

```text
WorldCharacterSetup
→ CharacterWorldStateService
→ LocationId
→ WorldLocationRegistry
→ NpcSpawner
→ NPC instance
```

Следующий важный шаг: сделать эту цепочку не только стартовой, но и реактивной.

## Рекомендованный порядок

1. **Проверить текущий spawn в Play Mode**
   - появились все пять NPC;
   - нет дубликатов;
   - работают AI, имена, таргетинг и диалоги.

2. **Отделить каталог персонажей от начального состояния**

Сейчас `NpcSpawner` ищет prefab через `WorldCharacterSetupSO`. Это смешивает:

```text
CharacterCatalog
→ какие персонажи существуют и какие у них prefab

WorldCharacterSetup
→ где они находятся в новой игре
```

Нужен отдельный `CharacterCatalogSO`, содержащий все `CharacterDefinitionSO`.

3. **Добавить runtime presence NPC**

Текущий `NpcSpawner` работает только один раз. После:

```csharp
TryMove("hakon", "city_marketplace_03");
```

Хакон физически не переместится.

Нужен сервис вроде `CharacterWorldPresenter`, который:

- хранит `CharacterId → GameObject`;
- слушает `CharacterChanged`;
- создаёт персонажа, если его location есть в текущей сцене;
- перемещает уже созданного персонажа;
- удаляет его, если `IsAlive == false` или location отсутствует в сцене.

4. **Подключить world-state к Yarn**

Например:

```yarn
<<move_character "hakon" "city_marketplace_03">>
<<set_character_alive "bandit_01" false>>

<<if character_is_alive("hakon")>>
```

Так диалоги смогут менять размещение персонажей без прямых ссылок на GameObject.

5. **Добавить сохранение**

DTO:

```text
QuestStates
WorldFacts
CharacterStates
```

Для персонажа:

```text
CharacterId
LocationId
IsAlive
```

6. **После этого делать патрули и расписание**

```text
PatrolRoute
→ список LocationId

Schedule
→ время + LocationId
```

## Что делать прямо сейчас

Я бы начал с **`CharacterCatalogSO`**. Это небольшой рефактор, который устранит последнее смешение initial state и definition catalog. Затем безопасно превратим `NpcSpawner` в реактивный runtime-сервис.

Да. По текущему состоянию проекта я бы не стал советовать “переписать всё”. У тебя уже выбран правильный общий вектор: **данные кампании отдельно, runtime-сервисы отдельно, Yarn-команды как мост между диалогами и состоянием мира, VContainer для сборки зависимостей**. Это хороший фундамент для Gothic-like системы.

Но сейчас есть несколько глобальных архитектурных решений, которые лучше зафиксировать до того, как проект разрастётся.

## 1. Главная идея: мир должен жить не в сцене, а в Campaign Runtime State

Для игры “как Gothic” ключевое решение такое:

**NPC, квесты, факты мира, отношения, главы, расписания и смерть персонажей должны быть частью единого состояния кампании, а не набором объектов на сцене.**

Сцена должна быть только визуализацией текущего состояния.

У тебя уже есть зародыш этого подхода: `CharacterWorldStateService` хранит `CharacterId`, `LocationId`, `IsAlive`, умеет `TryMove`, `TrySetAlive` и кидает `CharacterChanged` при изменениях. Это правильное ядро для NPC-состояния. ([GitHub][1])

Я бы формализовал это так:

```text
CampaignRuntimeState
  WorldFacts
  QuestStates
  CharacterStates
  ChapterState
  FactionRelations
  DiscoveredInfo
  TimeState
```

А поверх него:

```text
Scene / GameObjects / NPC Prefabs / UI
= presentation layer
```

То есть если диалог говорит:

```text
<<move_character "hakon" "city_marketplace_03">>
<<set_character_alive "bandit_01" false>>
```

это должно менять **модель мира**, а физическое перемещение или despawn NPC должно происходить реактивно через presenter/spawner. У тебя уже есть Yarn-обвязка для `move_character`, `set_character_alive`, `character_is_alive`, `character_location`, что подтверждает правильный вектор. ([GitHub][2])

## 2. Раздели “кто существует” и “где он сейчас находится”

Это, на мой взгляд, самый важный следующий рефактор.

Сейчас даже в твоём `readme.md` уже зафиксирована проблема: `NpcSpawner` ищет prefab через `WorldCharacterSetupSO`, из-за чего смешиваются две разные сущности — каталог персонажей и начальное состояние мира. Там же предлагается `CharacterCatalogSO`, содержащий все `CharacterDefinitionSO`. ([GitHub][3])

Я полностью согласен с этим направлением.

Должно быть так:

```text
CharacterDefinitionSO
  Id
  DisplayName
  Prefab
  Faction
  DialogueEntry
  DefaultBehaviorProfile
  Voice/Profile/Icon/etc

CharacterCatalogSO
  List<CharacterDefinitionSO>

WorldCharacterSetupSO
  CharacterId
  InitialLocationId
  InitialAliveState
  InitialScheduleId
```

`CharacterDefinitionSO` отвечает на вопрос:

> Кто такой Hakon как игровой персонаж?

`WorldCharacterSetupSO` отвечает на вопрос:

> Где Hakon находится в новой игре / в первой главе / в этом сценарии?

А `CharacterWorldStateService` отвечает:

> Где Hakon находится сейчас в конкретном прохождении?

Это критично для Gothic-like структуры, потому что один и тот же NPC может быть: в лагере в первой главе, у ворот во второй, мёртв в третьей, временно скрыт после квеста, перемещён в тюрьму после диалога.

## 3. NPC не должны быть “поставлены в сцену” как источник истины

Для такого типа RPG я бы запретил себе думать так:

> NPC стоит в сцене, значит он существует там.

Правильнее:

> NPC существует в `CharacterWorldState`; если его `LocationId` относится к текущей сцене, presenter создаёт или обновляет его GameObject.

То есть тебе нужен слой:

```text
CharacterWorldPresenter / NpcPresencePresenter
```

Он делает примерно это:

```text
on scene loaded:
  получить текущие WorldLocationAnchor в сцене
  пройти по CharacterWorldStateService.States
  если NPC alive и LocationId есть в сцене:
      spawn prefab из CharacterCatalog
      поставить в anchor
  иначе:
      не создавать

on CharacterChanged(characterId):
  если NPC должен быть в текущей сцене:
      spawn / move / update
  если NPC больше не должен быть в текущей сцене:
      despawn
```

Это ровно то, что уже написано в твоём `readme.md`: сервис должен хранить `CharacterId → GameObject`, слушать `CharacterChanged`, создавать, перемещать или удалять NPC в зависимости от `LocationId` и `IsAlive`. ([GitHub][3])

Глобальное правило: **квесты и диалоги никогда не должны напрямую двигать GameObject NPC**. Они должны менять state.

## 4. Квестовая система сейчас нормальная для MVP, но её надо не усложнять раньше времени

Текущий `QuestDefinitionSO` у тебя очень простой: `Id` и массив `QuestStageDefinition[]`. Это хорошо для старта. ([GitHub][4])

`QuestService` хранит runtime-состояние квестов в словаре: status, stage, journal entries; умеет `TryStart`, `TrySetStage`, `TryAddJournalEntry`, `TryComplete`, `TryFail`; и публикует `QuestChanged`. ([GitHub][5])

Это хорошая база. Я бы **не** делал сейчас сложные quest graph, node editor, dependency graph, automatic objective resolver и прочее. Для Gothic-like лучше оставить квесты как **ручной сценарный state machine**, управляемый диалогами, триггерами, интеракциями и world facts.

То есть квест — это не “самостоятельный мозг”. Квест — это запись в состоянии кампании:

```text
questId: "missing_apprentice"
status: Active
stage: 20
journalEntries: [...]
```

А логика переходов может жить в:

```text
Yarn commands
QuestStageInteractable
World trigger
Combat/death event
Item pickup event
Custom quest script, если нужно
```

Это ближе к Gothic: квесты часто продвигаются через диалоги, убийство конкретного NPC, наличие предмета, членство во фракции, главу, репутацию, знание факта.

## 5. Добавь WorldFacts как отдельную систему, не пихай всё в квесты

Сейчас есть риск, что `QuestService` начнёт превращаться в свалку глобального состояния. Этого лучше избежать.

Примеры фактов, которые не являются квестами:

```text
player_joined_old_camp = true
gate_guard_bribed = true
hakon_knows_about_betrayal = true
chapter = 2
player_has_permission_to_enter_castle = true
```

Для них нужен отдельный сервис:

```csharp
public interface IWorldFactService
{
    bool Has(string factId);
    void Set(string factId, bool value = true);
    int GetInt(string factId);
    void SetInt(string factId, int value);
    event Action<string> FactChanged;
}
```

И Yarn-команды:

```text
<<set_fact "gate_guard_bribed">>
<<clear_fact "npc_hakon_angry">>

<<if fact("player_joined_old_camp")>>
<<if int_fact("chapter") >= 2>>
```

Почему это важно: в Gothic-like игре очень много условий диалога не являются квестовыми стадиями. Если всё выражать через `quest_stage`, система быстро станет хрупкой.

## 6. Диалоги должны быть тонким orchestration layer, а не местом всей логики

У тебя уже есть `QuestCommandHandler`, который регистрирует команды и функции в Yarn: `start_quest`, `set_quest_stage`, `add_quest_log`, `complete_quest`, `fail_quest`, а также проверки `quest_is_active`, `quest_is_completed`, `quest_stage` и т.д. ([GitHub][6])

Это правильный подход.

Но я бы ввёл правило:

**Yarn может вызывать команды домена, но не должен знать Unity-объекты.**

Хорошо:

```text
<<start_quest "join_camp">>
<<set_quest_stage "join_camp" 20>>
<<move_character "hakon" "tavern_night">>
<<set_fact "player_insulted_gomez">>
```

Плохо:

```text
<<teleport_gameobject "HakonPrefab(Clone)" 12 0 44>>
<<disable_component "GuardAI">>
<<set_animator_bool "Hakon" "Angry" true>>
```

Yarn должен работать с `questId`, `characterId`, `locationId`, `factId`, `factionId`, но не с GameObject, Transform, Animator, Scene object reference.

## 7. LocationId должен стать центральной абстракцией мира

Для Gothic-like системы `LocationId` — не просто точка спавна. Это абстракция “где находится персонаж в мире”.

Я бы разделил:

```text
WorldLocationDefinition
  LocationId
  SceneId
  SemanticType: Camp, Tavern, Gate, Mine, PatrolPoint, Bed, WorkPlace
  Optional Tags

WorldLocationAnchor : MonoBehaviour
  LocationId
  Transform
```

В состоянии NPC хранится только:

```text
characterId = "hakon"
locationId = "old_camp_blacksmith_day"
```

А сцена сама регистрирует anchors:

```text
WorldLocationRegistry
  locationId -> Transform
```

Так ты сможешь делать:

```text
<<move_character "hakon" "city_marketplace_03">>
```

без знания, в какой сцене сейчас игрок. Если игрок не в этой сцене — GameObject не нужен. Когда игрок придёт туда позже, NPC появится в правильной точке.

## 8. Расписание NPC делай после reactive presence, не раньше

Не делай расписание, пока NPC ещё спавнятся одноразово. Сначала:

```text
CharacterWorldStateService
CharacterCatalogSO
WorldLocationRegistry
CharacterWorldPresenter
Save/Load CharacterStates
```

Только потом:

```text
NpcScheduleService
```

Архитектурно расписание должно быть не “NPC сам ходит по расписанию”, а сервис, который вычисляет желаемое состояние:

```text
Schedule says:
  08:00 -> smithy_workplace
  20:00 -> tavern_table
  23:00 -> home_bed
```

И применяет:

```text
CharacterWorldStateService.TryMove("hakon", calculatedLocationId)
```

То есть schedule — это ещё один источник изменений world-state, такой же как диалог или квест.

## 9. Для Gothic-like лучше использовать не “универсальный AI”, а layered behavior

Я бы не делал NPC как полностью автономных агентов. В Gothic-like игре важнее предсказуемость и сценарность.

Хорошая модель:

```text
CharacterWorldState
  где NPC должен быть

PresencePresenter
  существует ли NPC физически в текущей сцене

NpcBrain
  что он делает прямо сейчас, если заспавнен

ScheduleService
  куда он должен переместиться по времени

DialogueSystem
  что он говорит и какие world-state изменения вызывает
```

Внутри `NpcBrain`:

```text
Priority 100: Combat
Priority 80: Dialogue lock
Priority 60: Scripted scene / cutscene
Priority 40: Travel to scheduled location
Priority 20: Ambient work / idle / patrol
```

Не пытайся делать один AI, который сам “понимает” квесты. Пусть квесты и диалоги меняют state, а AI только исполняет локальное поведение.

## 10. Сохранение надо проектировать сейчас, даже если реализуешь позже

Твой `readme.md` уже правильно перечисляет будущий save DTO: `QuestStates`, `WorldFacts`, `CharacterStates`, а для персонажа — `CharacterId`, `LocationId`, `IsAlive`. ([GitHub][3])

Я бы сделал save-модель такой:

```csharp
[Serializable]
public sealed class CampaignSaveData
{
    public int Version;
    public string ChapterId;
    public WorldTimeSaveData Time;
    public List<QuestSaveData> Quests;
    public List<CharacterSaveData> Characters;
    public List<WorldFactSaveData> Facts;
}
```

Важно: **не сохраняй ScriptableObject references, GameObject references, scene references**. Только стабильные ID и простые значения.

Сохранять:

```text
questId
questStatus
questStage
journalEntryIds или journalEntryTexts
characterId
locationId
isAlive
factId/value
time
chapter
```

Не сохранять:

```text
QuestDefinitionSO reference
CharacterDefinitionSO reference
Transform
GameObject
Animator state как главный источник истины
```

## 11. Квестовые стадии лучше сделать смысловыми, но хранить числом можно

Числовые стадии — нормальная практика:

```text
0 inactive
10 got_task
20 found_clue
30 confronted_npc
100 completed
```

Но в данных и tooling лучше иметь label:

```text
QuestStageDefinition
  Value: 20
  Key: "found_clue"
  JournalText
```

Сейчас `QuestDefinitionSO.ContainsStage(int stage)` просто проверяет наличие стадии в массиве `Stages`, что нормально для валидации. ([GitHub][4])

Но для удобства контента я бы позже добавил:

```text
StageId / DebugName
JournalEntryId
ObjectiveTextId
```

При этом runtime всё ещё может хранить `int Stage`. Не усложняй до полноценного graph editor.

## 12. Рекомендуемая целевая структура систем

Я бы мысленно разложил проект так:

```text
Game layer:
  Character
  Inventory
  Combat
  Interaction
  Targeting
  Movement

Campaign layer:
  Quests
  Dialogue
  WorldFacts
  CharacterWorldState
  Time
  Factions
  Reputation
  Chapters

Presentation layer:
  NpcPresencePresenter
  QuestJournalUI
  DialogueUI
  WorldLocationAnchors
  SceneInstallers

Data layer:
  CharacterCatalogSO
  QuestCatalogSO
  DialogueEntryCatalogSO
  WorldLocationCatalogSO
  CampaignInitialStateSO
```

Текущее разделение `Scripts/Core`, `Scripts/Features`, `Scripts/Game`, `Campaign`, `Characters`, `Resources`, `Scenes` уже в целом подходит. В `Scripts/Features/Campaign` у тебя уже есть `Quests`, `SO`, `UI`, `World`, `DialogueService`, `NpcDialogueInteractable`; это хорошая зона для доменных campaign-систем. ([GitHub][3])

## Мой конкретный roadmap для твоего проекта

Я бы двигался в таком порядке:

1. **Ввести `CharacterCatalogSO`**
   Убрать зависимость спавнера от `WorldCharacterSetupSO` как источника prefab. `WorldCharacterSetupSO` должен быть только initial state.

2. **Сделать `CharacterWorldPresenter` / `NpcPresencePresenter`**
   Он должен слушать `CharacterChanged`, создавать, перемещать и удалять NPC в текущей сцене.

3. **Добавить `WorldFactService`**
   Не смешивать квестовые стадии и глобальные факты мира.

4. **Сделать save/load DTO для QuestStates + CharacterStates + WorldFacts**
   Даже если UI сохранения будет позже, сами сервисы должны уметь экспортировать/импортировать состояние.

5. **Ввести `WorldLocationDefinition` и строгую систему `LocationId`**
   Чтобы NPC можно было перемещать не только внутри текущей сцены.

6. **После этого делать расписания NPC**
   Schedule должен менять `CharacterWorldState`, а не двигать GameObject напрямую.

7. **Потом — фракции, репутация, crime/witness система**
   Это уже следующий слой, когда базовая state-machine кампании стабильна.

Главный вывод: у тебя уже правильная основа. Я бы сейчас не расширял квесты в “суперсистему”, а наоборот укрепил границы: **definitions отдельно, initial setup отдельно, runtime state отдельно, scene presentation отдельно, Yarn только как командный мост**. Для Gothic-like RPG это гораздо важнее, чем ранний node editor или универсальный AI.

