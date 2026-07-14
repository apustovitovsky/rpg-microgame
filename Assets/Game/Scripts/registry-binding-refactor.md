Да, **`BindingSource` можно убрать**, если он у тебя везде только передаёт:

```csharp
InstanceId
Value
```

В таком случае это лишний адаптер между DI и `RegistryBinding<T>`.

Сделал бы так:

```csharp
public sealed class RegistryBinding<T> :
    IInitializable,
    IDisposable
{
    private readonly IInstanceIdentity _identity;
    private readonly T _value;
    private readonly IRegistryWriter<T> _writer;

    private IDisposable _registration;

    public RegistryBinding(
        IInstanceIdentity identity,
        T value,
        IRegistryWriter<T> writer)
    {
        _identity = identity;
        _value = value;
        _writer = writer;
    }

    public void Initialize()
    {
        _registration = _writer.Register(
            _identity.InstanceId,
            _value);
    }

    public void Dispose()
    {
        _registration?.Dispose();
    }
}
```

Тогда capability просто регистрируется как `T`, а binding сам получает всё из scope:

```csharp
public void Install(IContainerBuilder builder)
{
    builder.Register<WorldCommandReceiver>(Lifetime.Scoped)
        .As<ICommandReceiver>();

    builder.RegisterEntryPoint<
        RegistryBinding<ICommandReceiver>>();
}
```

То есть декларация действительно становится:

```text
зарегистрировать ICommandReceiver локально
опубликовать ICommandReceiver глобально
```

Без отдельного:

```text
CommandReceiverBindingSource
IRegistryBindingSource<ICommandReceiver>
```

## Спрятать публикацию в extension

Чтобы endpoint не повторял `RegisterEntryPoint`, можно добавить:

```csharp
public static class RegistryBindingExtensions
{
    public static void Publish<T>(
        this IContainerBuilder builder)
        where T : class
    {
        builder.RegisterEntryPoint<RegistryBinding<T>>(
            Lifetime.Scoped);
    }
}
```

Использование:

```csharp
public void Install(IContainerBuilder builder)
{
    builder.Register<WorldCommandReceiver>(Lifetime.Scoped)
        .As<ICommandReceiver>();

    builder.Publish<ICommandReceiver>();
}
```

Для компонента:

```csharp
public void Install(IContainerBuilder builder)
{
    builder.RegisterComponent(this)
        .As<ITargetable>();

    builder.Publish<ITargetable>();
}
```

Это, на мой взгляд, оптимальный уровень явности.

## Можно объединить и регистрацию capability

Для plain C# сервиса:

```csharp
public static void RegisterPublished<TImplementation, TContract>(
    this IContainerBuilder builder,
    Lifetime lifetime = Lifetime.Scoped)
    where TImplementation : class, TContract
    where TContract : class
{
    builder.Register<TImplementation>(lifetime)
        .As<TContract>();

    builder.Publish<TContract>();
}
```

Тогда:

```csharp
builder.RegisterPublished<
    WorldCommandReceiver,
    ICommandReceiver>();
```

Для существующего компонента:

```csharp
public static void RegisterPublishedComponent<T>(
    this IContainerBuilder builder,
    T component)
    where T : class
{
    builder.RegisterComponent(component)
        .As<T>();

    builder.Publish<T>();
}
```

Использование:

```csharp
builder.RegisterPublishedComponent<ITargetable>(this);
```

Но я бы оставил и раздельный API:

```csharp
builder.Register(...);
builder.Publish<T>();
```

Потому что capability иногда регистрируется сложнее:

```csharp
builder.Register<IInventory>(
    resolver => CreateInventory(resolver),
    Lifetime.Scoped);

builder.Publish<IInventory>();
```

## Ограничение

Такой вариант корректен, только если в одном `PrefabScope` существует **ровно одна регистрация `T`**.

Например:

```text
один ICommandReceiver
один IInventory
один ITargetable
```

Если зарегистрировано несколько `IWorldCommandHandler`, публиковать их таким способом нельзя:

```csharp
builder.Publish<IWorldCommandHandler>();
```

VContainer не сможет однозначно определить, какое значение передавать в binding. Но handlers и не нужно публиковать: они остаются локальной коллекцией receiver.

## Когда `BindingSource` всё ещё нужен

Оставлять source имеет смысл, когда публикуется не то же значение, которое зарегистрировано в DI:

```text
локальный объект → наружу публикуется адаптер
один сервис → публикуется несколько представлений
ID берётся не из общей identity scope
значение вычисляется специально для registry
```

Например:

```csharp
IRegistryBindingSource<IDisplayNameProvider>
```

может публиковать специально созданный immutable snapshot вместо локального сервиса.

Но если таких случаев сейчас нет, не стоит строить вокруг них основной API.

## Рекомендация

Убрать:

```text
IRegistryBindingSource<T>
CommandReceiverBindingSource
InventoryBindingSource
TargetBindingSource
```

Оставить:

```text
RegistryBinding<T>
IRegistryWriter<T>
IInstanceIdentity
```

И добавить:

```csharp
builder.Publish<T>();
```

Получится простая модель:

```csharp
builder.Register<WorldCommandReceiver>(Lifetime.Scoped)
    .As<ICommandReceiver>();

builder.Publish<ICommandReceiver>();
```

То есть `RegistryBinding<T>` становится универсальным lifecycle-публикатором, а endpoint явно помечает, какой из локальных capabilities должен быть доступен глобально.
