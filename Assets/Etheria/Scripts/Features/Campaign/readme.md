Ты рассуждаешь правильно. Для Gothic-подобной RPG нужно разделить **персонажа**, **его положение в мире**, **диалоги** и **квесты**.

**Главное разделение**
```text
NPC Definition
Кто этот персонаж вообще

World Character State
Где он сейчас и что с ним произошло

Spawn Point
Физическое место на карте

Dialogue
Что он может сказать при текущем состоянии мира

Quest State
Что произошло в конкретной сюжетной линии
```

### NPC не должен владеть квестом

На NPC не должно быть списка:

```text
Quests Given:
- clear_bandits
- find_sword
```

NPC знает только:

- свой постоянный `CharacterId`;
- внешний вид;
- поведение;
- корневой диалог.

А уже Yarn знает сюжетные связи:

```yarn
<<if quest_stage("clear_bandits") == 0>>
    // предложить квест
<<elseif quest_stage("clear_bandits") == 30>>
    // принять отчёт
<<endif>>
```

То есть NPC не выдаёт квест напрямую. Он запускает диалог, а диалог читает состояние кампании.

---

## Постоянная личность NPC

Нужен отдельный стабильный ID:

```text
hakon
guard_captain
blacksmith_harad
```

Он не должен зависеть от:

- имени GameObject;
- позиции;
- Unity instance ID;
- конкретного созданного prefab instance.

Текущий GUID внутри `TargetCandidate` относится скорее к таргетингу. Для сохранений лучше позже вернуть отдельную сущность вроде:

```csharp
CharacterIdentity
{
    CharacterId
}
```

## Definition и State

### `NpcDefinitionSO`

Неизменяемые authoring-данные:

```text
Id: hakon
Visual: CharacterVisual_Grunt
Dialogue Node: Hakon
Default Behaviour: Civilian
```

### `WorldCharacterState`

Runtime-данные:

```text
CharacterId: hakon
LocationId: city_gate
SpawnPointId: gate_guard_post
IsAlive: true
RoutineState: Working
```

Именно `WorldCharacterState` сохраняется, а не `GameObject`.

---

## Spawn points

В перспективе на сцене лучше размещать не готовых NPC, а точки:

```text
NpcSpawnPoint
├── Id: gate_guard_post
├── Default Character: hakon
└── Transform
```

При загрузке сцены:

```text
WorldState
→ определяет, кто должен находиться здесь
→ NpcSpawner находит точку
→ создаёт общий NPC prefab
→ применяет NpcDefinitionSO
→ восстанавливает runtime-состояние
```

Для начала точка может содержать `Default Character`. Это задаёт начальное состояние новой игры. После загрузки сохранения главным становится `WorldState`.

## Перемещение NPC

NPC не переносится между сценами как вечный GameObject. Переносится его состояние:

```text
Hakon.LocationId = "tavern"
Hakon.SpawnPointId = "tavern_table_02"
```

Если нужная сцена загружена, spawner переставляет или пересоздаёт Хакона. Если не загружена, меняются только данные.

Позже распорядок дня может выбирать точки:

```text
08:00 → forge_work
18:00 → tavern_table
22:00 → home_bed
```

## Структура сохранения

```text
SaveGame
├── PlayerState
├── QuestStates
├── WorldFacts
├── CharacterStates
├── DoorStates
├── ChestStates
└── PickupStates
```

Примеры:

```text
QuestState:
clear_bandits, Completed, stage 100

CharacterState:
hakon, alive, location city_gate

WorldFact:
old_mine_open = true

ChestState:
chest_city_04, opened = true
```

Все сохраняемые объекты мира требуют постоянного ID.

## Важный момент с локализацией

Сейчас журнал хранит готовый текст. Для прототипа нормально, но в сохранении лучше хранить ключ:

```text
clear_bandits.started
clear_bandits.target_cleared
clear_bandits.completed
```

Не стоит сохранять русскую строку. Иначе при смене языка старые записи останутся русскими.

Yarn должен предоставлять локализуемый текст, а `QuestState` хранить идентификаторы записей.

## Что делать дальше

Правильная последовательность:

1. Завершить текущий квестовый вертикальный срез.
2. Добавить отдельный постоянный `CharacterIdentity`.
3. Создать `NpcDefinitionSO`.
4. Создать `NpcSpawnPoint`.
5. Сделать простой `WorldStateService` только в памяти.
6. Перенести одного Хакона со сцены на создание через spawn point.
7. Затем добавить сохранение DTO в файл.
8. Только после этого делать расписания и перемещение между точками.

Следующий разумный шаг: **`CharacterIdentity + NpcDefinitionSO`**, пока без спавнера и сохранения. Это заложит правильную идентичность NPC, не заставляя сразу строить всю систему мира.