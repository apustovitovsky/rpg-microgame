Остались такие хвосты, по приоритету.

**1. Старый CharacterWorldPresenter / ICharacterInstanceRegistry**
Это самый явный дубль новой NPC-системы.

Сейчас живы:
```text
Assets/Etheria/Scripts/Game/Character/ICharacterInstanceRegistry.cs
Assets/Etheria/Scripts/Features/Character/WorldState/CharacterWorldPresenter.cs
```

`CharacterWorldPresenter` реализует старый registry инстансов и спавнит через `CharacterSpawner`, а у нас уже есть:
```text
INpcAgentRegistry
NpcSpawner
NpcTravelService
```

Но удалять прямо сейчас нельзя, пока не проверить, не он ли всё еще спавнит NPC из world state. Если NPC уже создаются через `NpcSpawner`, это следующий кандидат на выпиливание.

**2. CharacterSpawner / CharacterCatalogSO / CharacterDefinitionSO**
Похоже на старую character-spawn модель:

```text
Assets/Etheria/Scripts/Features/Character/Spawning/CharacterSpawner.cs
Assets/Etheria/Scripts/Features/Character/SO/CharacterCatalogSO.cs
Assets/Etheria/Scripts/Features/Character/SO/CharacterDefinitionSO.cs
```

Но `WorldCharacterStateInstallerSO` всё еще ссылается на `CharacterCatalogSO`, а `WorldCharacterInitialState` на `CharacterDefinitionSO`. Это надо отдельно мигрировать на `NpcCatalogSO/NpcDefinitionSO`, если хотим убрать дубль.

**3. CharacterIdentity / ICharacterIdentity**
Все еще используется:
```text
CharacterLabelPresenter
NpcDialogueInteractable
```

Но для новых NPC у нас уже есть `NpcAgent.NpcId`. Это потенциальный следующий рефактор: UI/dialogue должны брать идентичность из NPC/target identity, а не из `CharacterIdentity`.

**4. NpcDialogueInteractable**
Старый Campaign interactable еще жив:
```text
Assets/Etheria/Scripts/Features/Campaign/Dialogue/NpcDialogueInteractable.cs
```

Но новые NPC взаимодействуют через:
```text
NpcAgent : IInteractable
NpcInteractionService
```

Если на prefab-ах/сцене `NpcDialogueInteractable` уже не используется, его можно удалить после поиска по prefab/scene.

**5. ActorFactory / ActorNameGenerator**
Они зарегистрированы в `CharacterSystemInstallerSO` и используются `SyntyWorldEntryPoint`.

```text
Assets/Etheria/Scripts/Features/Character/Factories/ActorFactory.cs
Assets/Etheria/Scripts/Features/Character/Helpers/ActorNameGenerator.cs
```

Это старый “actor” хвост, но трогать аккуратно, потому что player/synty entrypoint может еще зависеть.

**6. Stale YAML identifier**
Остался:
```text
Etheria.Features.Actor::Etheria.Features.Character.NpcAnimationController
```

Но ты уже нашел, что это реально `NpcCharacterAnimationController`, так что это не удалять.

Я бы следующим шагом чистил **NpcDialogueInteractable**, потому что он проще и явно дублирует `NpcAgent : IInteractable`. Нужно проверить prefab/scene ссылки на него.