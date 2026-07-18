## Правила имплементации

1. Не изменяй код самостоятельно — все правки вношу только я.
2. За одно сообщение присылай полное содержимое не более 1–4 файлов. Где основной принцип это 1 - вероятность что у меня возникнут вопросы или я захочу внести корректировки высока, а 4 - дальнейшие шаги вполне очевидны.
3. Перед каждым файлом указывай кликабельную ссылку для открытия в VS Code, даже если файл ещё не создан.
4. Не присылай отдельные фрагменты кода — всегда показывай файл целиком.
5. Избегай лишних промежуточных изменений. Временная поломка билда допустима, если это сокращает количество ненужных шагов.
6. Старайся чистить хвосты как можно раньше.
7. В конце каждого сообщения кратко указывай что именно было получено и цель следующего шага, периодически добавляй общий процент завершенности.
8. Периодически предлагай ренейминг для сущностей, как бы между делом, обращайся к нейминг конвенциям unity.

## Naming
Use the `SO` suffix only when a ScriptableObject name would conflict with a runtime type.

## VContainer First
1. Вся разработка должна вестись нативно под VContainer.
2. При выборе архитектуры отдавать предпочтение VContainer лучшим подходам и рекомендациям.
3. Prefer `AsImplementedInterfaces()` over explicit interface registration when VContainer can infer the intended closed interfaces.
4. В VContainer не имеет значение порядок регистрации.

## C# конвенции
Зпрещено делать такие конструкции для unity objects:

```csharp
    _module = module
        ?? throw new ArgumentNullException(...); // Unity objects should not use null coalescing.
```

## Unity
1. Терминал не подключен, необходимо смотреть логи в Editor.log

## ModuleScope

```csharp
class ModuleScope;
interface IModuleInstaller;
```
Модуль — это автономный набор регистраций и компонентов, который добавляет объекту одну законченную capability: диалог, инвентарь, навигацию, взаимодействие и т. п. Компонент, являющийся IModuleInstaller не должен регистрировать себя в LifetimeScope ни в каком виде.

Если необходимо регистрировать какие-либо authoring данные, то разрешается регистрация отдельной immutable структуры:

```csharp
public readonly struct DialogueSettings(
    float InteractionRange);

builder.RegisterInstance(
    new DialogueSettings(_interactionRange));
```

## Commands

Между world instances → Game.Commands
Внутри scope одного instance → прямой вызов через DI