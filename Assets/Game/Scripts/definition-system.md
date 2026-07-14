По умолчанию каждый instance я бы делал **обычным `sealed` C#-объектом**, содержащим только:

```text
InstanceId
Definition
```

И удобный делегирующий `TryGetFragment<T>()`.

## Минимальные instances

```csharp
public sealed class EntityInstance
{
    public EntityInstance(
        Guid instanceId,
        EntityDefinition definition)
    {
        if (instanceId == Guid.Empty)
            throw new ArgumentException(
                "Instance id cannot be empty.",
                nameof(instanceId));

        InstanceId = instanceId;
        Definition = definition
            ?? throw new ArgumentNullException(nameof(definition));
    }

    public Guid InstanceId { get; }

    public EntityDefinition Definition { get; }

    public bool TryGetFragment<TFragment>(
        out TFragment fragment)
        where TFragment : EntityFragment
    {
        return Definition.TryGetFragment(out fragment);
    }
}
```

Аналогично:

```csharp
public sealed class PickupInstance
{
    public Guid InstanceId { get; }
    public PickupDefinition Definition { get; }

    public bool TryGetFragment<TFragment>(
        out TFragment fragment)
        where TFragment : PickupFragment
    {
        return Definition.TryGetFragment(out fragment);
    }
}
```

```csharp
public sealed class ItemInstance
{
    public Guid InstanceId { get; }
    public ItemDefinition Definition { get; }

    public bool TryGetFragment<TFragment>(
        out TFragment fragment)
        where TFragment : ItemFragment
    {
        return Definition.TryGetFragment(out fragment);
    }
}
```

## Чего в instance по умолчанию быть не должно

В `EntityInstance` и `PickupInstance` я бы не добавлял:

```text
GameObject
Transform
LifetimeScope
Inventory
Health
Navigation
CommandReceiver
список capabilities
словарь произвольного runtime-state
```

`GameObject` хранится техническим spawn registry. Inventory, здоровье, диалог и остальные состояния принадлежат соответствующим runtime-компонентам и сервисам.

Иначе instance постепенно снова станет god-object.

## Аналогия с Lyra

`ULyraInventoryItemInstance` хранит ссылку на item definition и собственное изменяемое состояние в форме `StatTags`. Поиск fragment он делегирует своей definition. При этом stack count находится не в item instance, а в отдельной inventory entry. ([GitHub][1])

При создании inventory manager:

1. создаёт item instance;
2. устанавливает ему definition;
3. вызывает `OnInstanceCreated` у fragments;
4. устанавливает количество в inventory entry. ([GitHub][2])

Например, `SetStats` fragment копирует начальные значения из authoring definition в mutable stat state item instance. ([GitHub][3])

Твой аналог:

```text
Definition
    immutable authoring fragments

Instance
    identity + definition
    собственное mutable state только при необходимости

Owner/component
    остальное runtime-state
```

## Особенность `ItemInstance`

У `ItemInstance` чаще появится собственное mutable state:

```text
durability
charges
random modifiers
quality
custom name
item stat values
```

Но не нужно добавлять эти поля заранее.

Начни с:

```csharp
public sealed class ItemInstance
{
    public Guid InstanceId { get; }
    public ItemDefinition Definition { get; }
}
```

Первое реальное изменяемое состояние добавит либо конкретное поле, либо специализированный контейнер:

```csharp
public sealed class ItemInstance
{
    private readonly Dictionary<StatId, int> _stats;
}
```

Не стоит сразу создавать универсальный `Dictionary<string, object>`.



# Что именно брать из Lyra


От Lyra стоит взять структуру:

```text
definition
    immutable configuration

instance
    ссылка на definition
    изменяемые данные конкретного экземпляра

container entry
    instance + count

manager/factory
    создаёт instance
    применяет initial fragment data
```


## Финальная рекомендация

```text
EntityInstance:
    Guid InstanceId
    EntityDefinition Definition

PickupInstance:
    Guid InstanceId
    PickupDefinition Definition

ItemInstance:
    Guid InstanceId
    ItemDefinition Definition
    только реально необходимое mutable item-state
```

То есть instance остаётся минимальной runtime-идентичностью, а не одновременно объектом мира, registry entry, контейнером компонентов.

```text
Fragment
    immutable authoring-данные capability внутри Definition

Endpoint
    prefab-компонент
    получает Instance
    достаёт нужный Fragment
    создаёт runtime capability
    реализует IRegistryBindingSource<T>

RegistryBindingSource<T>
    предоставляет готовую пару:
    InstanceId + runtime value

RegistryBinding<T>
    VContainer lifecycle-объект
    на Initialize добавляет value в Registry<T>
    на Dispose удаляет его
```

На примере inventory:

```text
InventoryFragment
    capacity + initial items

InventoryOwner
    EntityInstance
    → TryGet<InventoryFragment>()
    → создаёт Inventory
    → IRegistryBindingSource<IInventory>

RegistryBinding<IInventory>
    InventoryOwner.InstanceId + Inventory
    → Registry<IInventory>
```

Итоговое разделение:

```text
Definition/Fragment
    authoring

Endpoint
    authoring → runtime

RegistryBinding
    runtime → global lookup
```

`Registry<T>` при этом остаётся простым хранилищем `InstanceId → T` и ничего не знает ни о prefab, ни о fragments, ни о VContainer.


### UPDATE

Технически ничего не мешает. Можно сделать так:

```csharp
public abstract class AssetDefinition<TInstance, TFragment>
    : ScriptableObject
    where TInstance : class
    where TFragment : class
{
    [SerializeReference]
    private List<TFragment> _fragments = new();

    public bool TryGetFragment<TConcrete>(
        out TConcrete fragment)
        where TConcrete : TFragment
    {
        foreach (var current in _fragments)
        {
            if (current is TConcrete typed)
            {
                fragment = typed;
                return true;
            }
        }

        fragment = default;
        return false;
    }

    public abstract TInstance CreateInstance(Guid instanceId);
}
```

И дальше:

```csharp
public sealed class ActorDefinition
    : AssetDefinition<ActorInstance, ActorFragment>
{
    public override ActorInstance CreateInstance(Guid instanceId)
    {
        return new ActorInstance(instanceId, this);
    }
}
```

```csharp
public sealed class PickupDefinition
    : AssetDefinition<PickupInstance, PickupFragment>
{
    public override PickupInstance CreateInstance(Guid instanceId)
    {
        return new PickupInstance(instanceId, this);
    }
}
```

```csharp
public sealed class ItemDefinition
    : AssetDefinition<ItemInstance, ItemFragment>
{
    public override ItemInstance CreateInstance(Guid instanceId)
    {
        return new ItemInstance(instanceId, this);
    }
}
```

Это вполне нормальный вариант.

## Почему я предложил промежуточный тип

Потому что тогда различаются две концепции:

```text
AssetDefinition<TInstance>
    любой authoring asset, создающий instance

FragmentedAssetDefinition<TInstance, TFragment>
    конкретно definition с fragment-контейнером
```

То есть промежуточная база полезна, если потенциально существуют definitions без fragments:

```text
QuestDefinition
LocationDefinition
DialogueDefinition
AbilitySetDefinition
```

Тогда они могут наследоваться от:

```csharp
AssetDefinition<QuestInstance>
```

и не обязаны придумывать бессмысленный `QuestFragment`.

Если же ты зафиксировал правило:

> Все наследники `AssetDefinition` обязательно являются fragment-based definitions,

то промежуточный класс не нужен.

## Главный минус двух generic-параметров

Сигнатуры становятся немного тяжелее:

```csharp
AssetDefinition<ActorInstance, ActorFragment>
```

вместо:

```csharp
AssetDefinition<ActorInstance>
```

И любой общий код, которому не важны fragments, уже должен учитывать два generic-параметра.

Например, было:

```csharp
public void Register<TInstance>(
    AssetDefinition<TInstance> definition)
```

станет:

```csharp
public void Register<TInstance, TFragment>(
    AssetDefinition<TInstance, TFragment> definition)
    where TFragment : class
```

Хотя часто это можно обойти негeneric-базой.

## Наиболее удобная иерархия

Я бы сделал негeneric-корень:

```csharp
public abstract class AssetDefinition : ScriptableObject
{
    [field: SerializeField]
    public string Id { get; private set; }

    [field: SerializeField]
    public string DisplayName { get; private set; }
}
```

А затем fragment-based generic-базу:

```csharp
public abstract class AssetDefinition<TInstance, TFragment>
    : AssetDefinition
    where TInstance : class
    where TFragment : class
{
    [SerializeReference]
    private List<TFragment> _fragments = new();

    public abstract TInstance CreateInstance(Guid instanceId);

    public bool TryGetFragment<TConcrete>(
        out TConcrete fragment)
        where TConcrete : TFragment
    {
        foreach (var current in _fragments)
        {
            if (current is TConcrete typed)
            {
                fragment = typed;
                return true;
            }
        }

        fragment = default;
        return false;
    }
}
```

Тогда общий catalog может хранить:

```csharp
IReadOnlyList<AssetDefinition>
```

а специализированный код получает конкретную definition.

## Рекомендация

Если сейчас у тебя только:

```text
ActorDefinition
PickupDefinition
ItemDefinition
```

и все три гарантированно используют fragments, смело помещай fragments прямо в:

```csharp
AssetDefinition<TInstance, TFragment>
```

`FragmentedAssetDefinition` не даёт технического преимущества. Он полезен только как семантическое разделение на fragment-based и обычные definitions.

В твоей текущей модели я бы выбрал более короткую иерархию:

```text
AssetDefinition
    Id, DisplayName

AssetDefinition<TInstance, TFragment>
    CreateInstance
    fragments
    TryGetFragment

ActorDefinition
PickupDefinition
ItemDefinition
```
Да. Оставляешь fragments обычными `[Serializable]` классами и добавляешь один generic drawer плюс три пустых регистратора для доменных базовых типов.

Файл положи в папку `Editor`.

```csharp
#if UNITY_EDITOR

using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    public abstract class ManagedReferenceFragmentDrawer<TFragment>
        : PropertyDrawer
        where TFragment : class
    {
        private static readonly Type[] FragmentTypes =
            TypeCache.GetTypesDerivedFrom<TFragment>()
                .Where(type =>
                    !type.IsAbstract &&
                    !type.IsGenericType)
                .OrderBy(type => type.Name)
                .ToArray();

        private static readonly string[] FragmentNames =
            new[] { "None" }
                .Concat(FragmentTypes.Select(type =>
                    ObjectNames.NicifyVariableName(type.Name)))
                .ToArray();

        public override void OnGUI(
            Rect position,
            SerializedProperty property,
            GUIContent label)
        {
            EditorGUI.BeginProperty(
                position,
                label,
                property);

            DrawHeader(
                position,
                property,
                label);

            if (property.isExpanded &&
                property.managedReferenceValue != null)
            {
                DrawChildren(position, property);
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(
            SerializedProperty property,
            GUIContent label)
        {
            var height = EditorGUIUtility.singleLineHeight;

            if (!property.isExpanded ||
                property.managedReferenceValue == null)
            {
                return height;
            }

            var child = property.Copy();
            var end = child.GetEndProperty();
            var enterChildren = true;

            while (child.NextVisible(enterChildren) &&
                   !SerializedProperty.EqualContents(child, end))
            {
                height +=
                    EditorGUIUtility.standardVerticalSpacing +
                    EditorGUI.GetPropertyHeight(
                        child,
                        includeChildren: true);

                enterChildren = false;
            }

            return height;
        }

        private static void DrawHeader(
            Rect position,
            SerializedProperty property,
            GUIContent label)
        {
            var line = new Rect(
                position.x,
                position.y,
                position.width,
                EditorGUIUtility.singleLineHeight);

            var labelRect = new Rect(
                line.x,
                line.y,
                EditorGUIUtility.labelWidth,
                line.height);

            var popupRect = new Rect(
                labelRect.xMax,
                line.y,
                line.width - labelRect.width,
                line.height);

            if (property.managedReferenceValue != null)
            {
                property.isExpanded = EditorGUI.Foldout(
                    labelRect,
                    property.isExpanded,
                    label,
                    toggleOnLabelClick: true);
            }
            else
            {
                EditorGUI.LabelField(labelRect, label);
            }

            var currentType =
                property.managedReferenceValue?.GetType();

            var currentIndex = currentType == null
                ? 0
                : Array.IndexOf(FragmentTypes, currentType) + 1;

            EditorGUI.BeginChangeCheck();

            var selectedIndex = EditorGUI.Popup(
                popupRect,
                currentIndex,
                FragmentNames);

            if (!EditorGUI.EndChangeCheck())
                return;

            property.managedReferenceValue =
                selectedIndex == 0
                    ? null
                    : Activator.CreateInstance(
                        FragmentTypes[selectedIndex - 1]);

            property.isExpanded = selectedIndex != 0;
        }

        private static void DrawChildren(
            Rect position,
            SerializedProperty property)
        {
            var child = property.Copy();
            var end = child.GetEndProperty();
            var enterChildren = true;

            var y =
                position.y +
                EditorGUIUtility.singleLineHeight +
                EditorGUIUtility.standardVerticalSpacing;

            EditorGUI.indentLevel++;

            while (child.NextVisible(enterChildren) &&
                   !SerializedProperty.EqualContents(child, end))
            {
                var height = EditorGUI.GetPropertyHeight(
                    child,
                    includeChildren: true);

                var childRect = new Rect(
                    position.x,
                    y,
                    position.width,
                    height);

                EditorGUI.PropertyField(
                    childRect,
                    child,
                    includeChildren: true);

                y +=
                    height +
                    EditorGUIUtility.standardVerticalSpacing;

                enterChildren = false;
            }

            EditorGUI.indentLevel--;
        }
    }
}

#endif
```

Регистрация для трёх семейств:

```csharp
#if UNITY_EDITOR

using Game.Actor;
using Game.Item;
using Game.Pickup;
using UnityEditor;

namespace Game.Editor
{
    [CustomPropertyDrawer(
        typeof(ActorFragment),
        useForChildren: true)]
    public sealed class ActorFragmentDrawer
        : ManagedReferenceFragmentDrawer<ActorFragment>
    {
    }

    [CustomPropertyDrawer(
        typeof(PickupFragment),
        useForChildren: true)]
    public sealed class PickupFragmentDrawer
        : ManagedReferenceFragmentDrawer<PickupFragment>
    {
    }

    [CustomPropertyDrawer(
        typeof(ItemFragment),
        useForChildren: true)]
    public sealed class ItemFragmentDrawer
        : ManagedReferenceFragmentDrawer<ItemFragment>
    {
    }
}

#endif
```

Runtime-классы остаются простыми:

```csharp
[Serializable]
public abstract class ActorFragment
{
}

[Serializable]
public sealed class InventoryFragment : ActorFragment
{
    [SerializeField, Min(0)]
    private int _capacity = 20;

    public int Capacity => _capacity;
}
```

В definition:

```csharp
[SerializeReference]
private List<ActorFragment> _fragments = new();
```

В Inspector каждый элемент будет выглядеть примерно так:

```text
Element 0    [Inventory Fragment ▼]
    Capacity    20
```

Popup содержит `None` и все конкретные наследники соответствующего fragment-типа. `TypeCache.GetTypesDerivedFrom<T>()` используется для поиска наследников, а `managedReferenceValue` — для создания и назначения выбранного объекта. `useForChildren: true` применяет drawer к наследникам базового fragment-класса. ([docs.unity3d.com][1])

[1]: https://docs.unity3d.com/6000.3/Documentation/ScriptReference/CustomPropertyDrawer-ctor.html "Unity - Scripting API: CustomPropertyDrawer.CustomPropertyDrawer"

### UPDATE 2

По аналогии с Lyra это **не обычный `string`**, а `FGameplayTag` — типизированный идентификатор зарегистрированного иерархического имени вроде:

```text
Item.Stat.Ammo
Item.Stat.MagazineSize
Item.Stat.Charges
```

Внутри Lyra контейнер фактически хранит соответствие:

```text
GameplayTag → int
```

Gameplay Tags основаны на строковых именах, но сами теги валидируются через центральный словарь и представлены отдельным типом `FGameplayTag`, а не произвольной строкой. ([Epic Games Developers][1])

Для тебя лучший вариант:

```csharp
private readonly Dictionary<GameplayTag, int> _stats = new();
```

если в твоём GAS уже есть собственный `GameplayTag`.

Если tag-системы пока нет, простой локальный аналог:

```csharp
using System;
using UnityEngine;

namespace Game.Item
{
    [Serializable]
    public struct ItemStat : IEquatable<ItemStat>
    {
        [SerializeField]
        private string _value;

        public ItemStat(string value)
        {
            _value = string.IsNullOrWhiteSpace(value)
                ? throw new ArgumentException(
                    "Item stat cannot be empty.",
                    nameof(value))
                : value;
        }

        public string Value => _value;

        public bool Equals(ItemStat other) =>
            _value == other._value;

        public override bool Equals(object obj) =>
            obj is ItemStat other && Equals(other);

        public override int GetHashCode() =>
            _value?.GetHashCode() ?? 0;

        public override string ToString() => _value;
    }
}
```

Использование:

```csharp
private readonly Dictionary<ItemStat, int> _stats = new();
```

```csharp
public static class ItemStats
{
    public static readonly ItemStat Ammo =
        new("Item.Stat.Ammo");

    public static readonly ItemStat MagazineSize =
        new("Item.Stat.MagazineSize");

    public static readonly ItemStat Charges =
        new("Item.Stat.Charges");
}
```

Но если у тебя уже есть или планируется общая Gameplay Tag system, отдельный `ItemStat` не нужен:

```csharp
private readonly Dictionary<GameplayTag, int> _statStacks = new();
```

Это будет ближе всего к Lyra. `ItemStat` в предыдущем примере был лишь условным типом ключа, а не обязательной отдельной сущностью.

[1]: https://dev.epicgames.com/documentation/en-us/unreal-engine/gameplay-tags?application_version=4.27&utm_source=chatgpt.com "Gameplay Tags | Unreal Engine 4.27 Documentation"

Да. Сделай `ItemStat` простым immutable `ScriptableObject`, а runtime-значения храни в `ItemInstance`.

```csharp
using UnityEngine;

namespace Game.Item
{
    [CreateAssetMenu(
        fileName = "ItemStat",
        menuName = "Game/Item/Stat")]
    public sealed class ItemStat : ScriptableObject
    {
        [field: SerializeField]
        public string Id { get; private set; }

        [field: SerializeField]
        public string DisplayName { get; private set; }
    }
}
```

Создаёшь assets:

```text
ItemStats/
    CurrentAmmo.asset
    MaxAmmo.asset
    Durability.asset
    Charges.asset
```

Например:

```text
CurrentAmmo:
    Id = item.stat.ammo.current
    DisplayName = Current Ammo
```

## Runtime-стеки в `ItemInstance`

```csharp
using System;
using System.Collections.Generic;

namespace Game.Item
{
    public sealed class ItemInstance
    {
        private readonly Dictionary<ItemStat, int> _statStacks = new();

        public ItemInstance(
            Guid instanceId,
            ItemDefinition definition)
        {
            if (instanceId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Item instance id cannot be empty.",
                    nameof(instanceId));
            }

            InstanceId = instanceId;

            Definition = definition ??
                throw new ArgumentNullException(nameof(definition));
        }

        public Guid InstanceId { get; }

        public ItemDefinition Definition { get; }

        public bool TryGetFragment<TFragment>(
            out TFragment fragment)
            where TFragment : ItemFragment
        {
            return Definition.TryGetFragment(out fragment);
        }

        public int GetStatStack(ItemStat stat)
        {
            ArgumentNullException.ThrowIfNull(stat);

            return _statStacks.GetValueOrDefault(stat);
        }

        public void SetStatStack(ItemStat stat, int value)
        {
            ArgumentNullException.ThrowIfNull(stat);

            if (value == 0)
            {
                _statStacks.Remove(stat);
                return;
            }

            _statStacks[stat] = value;
        }

        public void AddStatStack(ItemStat stat, int amount)
        {
            ArgumentNullException.ThrowIfNull(stat);

            SetStatStack(
                stat,
                GetStatStack(stat) + amount);
        }

        public bool RemoveStatStack(ItemStat stat, int amount)
        {
            ArgumentNullException.ThrowIfNull(stat);

            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount));

            var current = GetStatStack(stat);

            if (current < amount)
                return false;

            SetStatStack(stat, current - amount);
            return true;
        }
    }
}
```

`ScriptableObject` здесь используется только как типизированный ключ:

```text
ItemStat asset → идентификатор характеристики
int            → значение конкретного ItemInstance
```

Сам `ItemStat` никогда не изменяется во время игры.

## Начальные значения во fragment

```csharp
using System;
using UnityEngine;

namespace Game.Item
{
    [Serializable]
    public struct InitialItemStat
    {
        [field: SerializeField]
        public ItemStat Stat { get; private set; }

        [field: SerializeField]
        public int Value { get; private set; }
    }
}
```

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Item
{
    [Serializable]
    public sealed class InitialStatsFragment : ItemFragment
    {
        [SerializeField]
        private InitialItemStat[] _stats =
            Array.Empty<InitialItemStat>();

        public IReadOnlyList<InitialItemStat> Stats => _stats;
    }
}
```

При создании instance:

```csharp
public override ItemInstance CreateInstance(Guid instanceId)
{
    var instance = new ItemInstance(instanceId, this);

    if (TryGetFragment<InitialStatsFragment>(out var fragment))
    {
        foreach (var stat in fragment.Stats)
        {
            instance.SetStatStack(
                stat.Stat,
                stat.Value);
        }
    }

    return instance;
}
```

## Важное различие

```text
InventoryEntry.Count
    количество одинаковых предметов:
    20 стрел, 5 зелий

ItemInstance stat stack
    состояние конкретного предмета:
    7 патронов, 80 прочности, 3 заряда
```

Для сохранений записывай `ItemStat.Id`, а не ссылку на `ScriptableObject`. В runtime словаре ссылка на SO как ключ — нормальный и простой вариант.

Технически слово **stack** используется в обоих случаях, но смысл разный:

```text
InventoryEntry.StackCount
    сколько одинаковых предметов лежит в inventory

ItemInstance.StatTags[CurrentAmmo]
    текущее число патронов у конкретного оружия

ItemInstance.StatTags[MaxAmmo]
    вместимость/максимальное число патронов этого оружия
```

Например:

```text
InventoryEntry:
    RifleInstance
    StackCount = 1

RifleInstance:
    CurrentAmmo = 18
    MaxAmmo = 30
```

`CurrentAmmo` — это действительно количество чего-либо, но **не количество экземпляров предмета Rifle**. В Lyra stat stack — просто универсальная пара `GameplayTag → int`, а inventory stack — агрегирование одинаковых предметов в одной записи. ([Epic Games Developers][1])

Для твоего проекта:

```csharp
InventoryEntry.Count       // 20 стрел или 5 зелий
ItemInstance.StatStacks    // 18 патронов, 80 прочности, 3 заряда
```

То есть механически оба являются счётчиками, но принадлежат разным уровням модели.

[1]: https://dev.epicgames.com/documentation/unreal-engine/lyra-inventory-and-equipment-in-unreal-engine?utm_source=chatgpt.com "Lyra Inventory and Equipment in Unreal Engine"
