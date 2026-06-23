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