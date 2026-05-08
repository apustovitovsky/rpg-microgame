# StoryletSystem Authoring RFC

Этот документ описывает целевую authoring-модель StoryletSystem.

Он фиксирует не исторические варианты схемы, а желаемое разделение ответственности между `Authoring`, `Runtime`, `RoleDefinitionSO`, `StoryletRoleAuthoring` и `StoryletDefinitionSO`.

## Цели

- держать authoring-модель понятной для Unity Inspector и для человека
- держать runtime-модель компактной и дешевой для массового matching и score evaluation
- не смешивать reusable role definition с storylet-local настройкой роли
- считать score через роли, назначенные в storylet, а не через безличный global bonus pool storylet-а

## Главная идея

`Storylet` хранит:

- собственные metadata tags
- список ролей
- storylet-level preconditions
- repeatability / salience / effects

Каждая роль внутри storylet является storylet-local slot-ом.

Этот slot хранит:

- ссылку на reusable `RoleDefinitionSO`
- slot-specific relation requirements
- slot-specific score bonuses

Это ключевой инвариант системы:

- бонус принадлежит не storylet-у в целом
- бонус принадлежит конкретной роли внутри storylet
- бонус вычисляется по актору, который назначен на эту роль

## Authoring vs Runtime

`Authoring` хранит editor-friendly форму:

- string tag ids
- serializable Unity types
- ScriptableObject assets
- удобные для инспектора списки и вложенные структуры

`Runtime` хранит executable форму:

- скомпилированные tag ids
- компактные query и stack checks
- runtime roles
- runtime score rules

Один и тот же смысл задается в authoring и затем компилируется в runtime.

## Инварианты модели

- `RoleDefinitionSO` описывает reusable роль, но не хранит storylet-specific бонусы
- `StoryletRoleAuthoring` описывает, как конкретная роль используется внутри конкретного storylet
- `StoryletDefinitionSO` не владеет глобальным списком `ScoreBonuses[]`
- metadata tags storylet-а не должны описываться через `TagQueryAuthoring`
- `ScoreBonusAuthoring` принадлежит role slot-у, а не storylet-у целиком
- storylet-level preconditions отвечают на вопрос "можно ли вообще рассматривать этот storylet"
- role requirements отвечают на вопрос "подходит ли актор на эту роль"
- relation requirements отвечают на вопрос "подходит ли связь между двумя назначенными ролями"
- score bonuses отвечают на вопрос "насколько хорошо конкретный назначенный актор подходит на эту роль"

## Где именно лежат проверки

Ниже важно различать четыре разных уровня проверки.

### `StoryletDefinitionSO._requirements`

Это preconditions storylet-а.

Они отвечают на вопрос:

```text
можно ли вообще рассматривать этот storylet
```

То есть это проверка не роли и не конкретного актора.
Это фильтр на storylet целиком.

Примеры:

- в мире должен быть `tag.border_tension`
- storylet не должен быть доступен при `tag.peace_time`

### `StoryletDefinitionSO._storyletTags`

Это собственные metadata tags storylet-а.

Они отвечают на вопрос:

```text
что это за storylet по смыслу и категории
```

Примеры:

- `tag.storylet.conflict`
- `tag.storylet.ambient`
- `tag.storylet.payoff`

Это не query и не requirement.
Эти теги не проверяют доступность storylet сами по себе.
Они нужны для classification, filtering, salience и policy logic.

### `StoryletDefinitionSO._roles`

Это не список проверок.
Это список role slot-ов, которые storylet хочет заполнить.

Каждый элемент `_roles` описывает:

- какую роль нужно заполнить
- по какому `RoleDefinitionSO` матчить актора
- какие relation checks применяются к этому slot-у
- какие score bonuses считаются для актора, назначенного на этот slot

Сам массив `_roles` не является tag-check.
Tag-check-и лежат внутри описания каждой роли.

### `RoleDefinitionSO`

Именно здесь лежат actor requirements роли.

Они отвечают на вопрос:

```text
подходит ли конкретный актор на эту роль
```

Примеры:

- `guard` требует `tag.actor` и `tag.guard`
- `guard` исключает `tag.dead`
- `guard` требует `reputation >= 2`

### `StoryletRoleAuthoring.RelationRequirements`

Здесь лежат relation checks между ролями.

Они отвечают на вопрос:

```text
подходит ли связь между актором этой роли и актором другой роли
```

### `StoryletRoleAuthoring.ScoreBonuses`

Здесь лежат не gating-проверки, а score checks.

Они отвечают на вопрос:

```text
насколько хорошо уже назначенный актор подходит на эту роль
```

Итоговый порядок такой:

1. `StoryletDefinitionSO._storyletTags` описывает storylet как metadata
2. `StoryletDefinitionSO._requirements` фильтрует storylet целиком
3. `RoleDefinitionSO` фильтрует акторов для каждой роли
4. `RelationRequirements` фильтрует связи между выбранными ролями
5. `ScoreBonuses` считает вклад каждой роли в итоговый score

## Authoring-сущности

### `TagCollectionAuthoring`

`TagCollectionAuthoring` хранит простой набор tag ids без семантики условия.

Он нужен для случаев, когда объект просто "имеет теги", а не "проверяет теги".

Примеры использования:

- metadata tags у storylet-а
- classification tags у authoring-asset-а

Не стоит по умолчанию использовать `TagCollectionAuthoring` внутри `TagQueryEntryAuthoring`.

Причина простая:

- `TagCollectionAuthoring` описывает plain tag ownership
- `TagQueryEntryAuthoring` описывает operand внутри условия

То есть это разные semantic roles, даже если внутри у них может быть одинаковая форма хранения `string[]`.

Пример формы:

```csharp
[Serializable]
public struct TagCollectionAuthoring
{
    [SerializeField]
    private string[] _tagIds;

    public IReadOnlyList<string> TagIds => _tagIds;
}
```

### `TagQueryAuthoring`

`TagQueryAuthoring` задает presence/absence условия через entries:

- `Required`
- `Any`
- `Excluded`

Он нужен для качественных условий вида:

- actor должен иметь `tag.actor`
- actor должен иметь `tag.knight`
- actor не должен иметь `tag.dead`

`TagQueryAuthoring` не проверяет количество стаков.
Он является именно query-типом, а не контейнером metadata tags.

Поэтому `TagQueryEntryAuthoring` лучше оставлять самостоятельным query-типом, а не оборачивать его теги в `TagCollectionAuthoring` только ради structural reuse.

### `TagStackQueryAuthoring`

`TagStackQueryAuthoring` задает количественную проверку одного stack tag.

Примеры:

- `fear < 5`
- `trust >= 3`
- `reputation == 0`

Используемые режимы сравнения:

- `Equal`
- `NotEqual`
- `More`
- `MoreOrEqual`
- `Less`
- `LessOrEqual`

### `TagRequirementAuthoring`

`TagRequirementAuthoring` объединяет оба типа условий:

- `Query`
- `Stack`

Это базовый authoring-тип для требований, который можно использовать:

- в preconditions storylet-а
- в actor requirements роли
- в relation requirements между ролями

### `ScoreBonusAuthoring`

`ScoreBonusAuthoring` задает score contribution.

Поддерживаются два режима:

- `Query`
- `Stack`

Для stack-бонусов поддерживаются два способа расчета:

- `Single`
- `PerStack`

Смысл режимов:

- `Single`: если stack больше нуля, бонус применяется один раз
- `PerStack`: бонус умножается на текущее количество стаков

### `RoleDefinitionSO`

`RoleDefinitionSO` является reusable role definition.

Он хранит:

- `RoleId`
- actor requirements
- reusable relation requirements, если они действительно являются частью общего смысла роли

Он не должен хранить:

- storylet-specific score bonuses
- storylet-local relation nuance
- storylet-local salience logic

Одна и та же reusable роль может использоваться в нескольких storylet-ах по-разному.

### `StoryletRoleAuthoring`

`StoryletRoleAuthoring` является storylet-local role slot-ом.

Он должен описывать:

- имя или key роли внутри storylet
- ссылку на `RoleDefinitionSO`
- relation requirements, специфичные для этого slot-а
- `ScoreBonuses[]`, принадлежащие этой роли

Именно здесь должна жить логика вида:

- "для `guard` loyalty дает бонус"
- "для `witness` fear снижает score"
- "для `noble` наличие `tag.wounded` повышает salience fit этой роли"

Это не reusable свойства роли вообще.
Это свойства роли внутри конкретного storylet.

### `StoryletDefinitionSO`

`StoryletDefinitionSO` является контейнером storylet-а.

Он должен хранить:

- `StoryletId`
- `StoryletTags`
- `Preconditions[]`
- `Roles[]`
- repeatability policy
- salience policy
- effects

Он не должен быть основным местом хранения role-local score bonuses.

## Целевая authoring-форма

Ниже приведена целевая схема ответственности.

```csharp
[CreateAssetMenu(
    fileName = "RoleDefinition",
    menuName = "Etheria/Features/StoryletSystem/Authoring/Role Definition")]
public sealed class RoleDefinitionSO : ScriptableObject
{
    [field: SerializeField]
    public string RoleId { get; private set; }

    [SerializeField]
    private TagRequirementAuthoring[] _actorRequirements;

    [SerializeField]
    private RelationRequirementAuthoring[] _relationRequirements;
}
```

```csharp
[Serializable]
public sealed class StoryletRoleAuthoring
{
    [field: SerializeField]
    public string Name { get; private set; }

    [field: SerializeField]
    public RoleDefinitionSO RoleDefinition { get; private set; }

    [field: SerializeField]
    public IReadOnlyList<RelationRequirementAuthoring> RelationRequirements => _relationRequirements;

    [field: SerializeField]
    public ScoreBonusAuthoring[] ScoreBonuses { get; private set; }
}
```

```csharp
[CreateAssetMenu(
    fileName = "StoryletDefinition",
    menuName = "Etheria/Features/StoryletSystem/Authoring/Storylet Definition")]
public sealed class StoryletDefinitionSO : ScriptableObject
{
    [field: SerializeField]
    public string StoryletId { get; private set; }

    [SerializeField]
    private TagCollectionAuthoring _storyletTags;

    [SerializeField]
    private TagRequirementAuthoring[] _preconditions;

    [SerializeField]
    private StoryletRoleAuthoring[] _roles;
}
```

Это не обязательно буквальная копия текущего кода.
Это целевая ownership-модель, которую документ фиксирует.

Если имя `_requirements` читается слишком общо, его следует понимать именно как storylet-level preconditions.
Смыслово более явное имя для этого поля - `_preconditions`.

## Runtime-сущности

### `TagSet`

`TagSet` хранит только факт присутствия тегов.

Он отвечает на вопросы:

- есть ли все эти теги
- есть ли хотя бы один из этих тегов
- отсутствуют ли запрещенные теги

### `TagStackSet`

`TagStackSet` хранит:

- маску присутствия тегов
- количество стаков по конкретным тегам

Главный инвариант:

- `stack > 0` означает, что тег присутствует
- отсутствие stack tag считается `0`

### `TagQuery`

`TagQuery` является runtime-формой `TagQueryAuthoring`.

Он работает только с presence/absence.

### `TagRequirement`

`TagRequirement` является runtime-формой `TagRequirementAuthoring`.

Он проверяет:

- либо `TagQuery`
- либо stack comparison

### `ScoreBonusRule`

`ScoreBonusRule` является runtime-формой `ScoreBonusAuthoring`.

Но семантически он должен применяться не к storylet-у "вообще", а к tags конкретного актора, назначенного на конкретную роль.

Иными словами:

- role slot authoring компилируется в role-local runtime bonus rules
- evaluator считает вклад этих бонусов по назначенному actor binding

## Семантика score

### Что делает precondition

Precondition отвечает на вопрос:

```text
можно ли вообще рассматривать этот storylet на текущем world state
```

Если precondition не проходит, storylet дальше не рассматривается.

### Что делает role requirement

Role requirement отвечает на вопрос:

```text
подходит ли этот актор на эту роль
```

Это gating-условие для assignment.

### Что делает relation requirement

Relation requirement отвечает на вопрос:

```text
подходит ли отношение между актором текущей роли и актором другой роли
```

Это тоже gating-условие для assignment.

### Что делает score bonus

Score bonus отвечает на вопрос:

```text
насколько хорошо уже назначенный актор подходит на эту роль
```

Он не открывает storylet.
Он не заменяет role requirement.
Он только добавляет вклад в оценку уже валидного assignment.

## Почему бонусы принадлежат роли

Если бонус живет у storylet-а глобально, теряется субъект оценки.

Становится неясно:

- по чьим тегам считается бонус
- к какой роли относится этот bonus signal
- как переиспользовать одну и ту же reusable роль в двух storylet-ах с разной score-семантикой

Если бонус живет у role slot-а, смысл ясен:

- есть назначенный актор на роль
- у этого актора есть tags и stacks
- роль считает свои бонусы по своему актору
- общий storylet score агрегирует вклады всех назначенных ролей

Это делает систему:

- проще для понимания
- лучше совместимой с role assignment
- менее хрупкой при reuse `RoleDefinitionSO`

## Минимальный пример

```text
RoleDefinition: role.guard

Actor requirements:
  Query:
    Required: tag.actor, tag.guard
    Excluded: tag.dead

  Stack:
    tag.reputation >= 2
```

```text
Storylet: storylet.border_warning

Preconditions:
  Stack:
    tag.border_tension > 0

Roles:
  authority:
    Definition: role.authority

  guard:
    Definition: role.guard
    Relation requirements:
      authority -> guard:
        Query:
          Any: tag.trust, tag.vassal

    Score bonuses:
      Query:
        Required: tag.loyal
        Bonus: +5
        Weight: 1

      Stack:
        Tag: tag.fear
        Mode: PerStack
        Bonus: -2
        Weight: 1
```

Смысл:

- storylet доступен только если в мире есть `border_tension`
- на роль `guard` можно назначить только подходящего актора
- связь `authority -> guard` тоже должна пройти
- после валидного assignment роль `guard` считает свой score contribution по своему назначенному актору

## Пример вычисления score

Пусть на роль `guard` назначен актор со state:

```text
tag.actor
tag.guard
tag.loyal
tag.reputation = 3
tag.fear = 2
```

Тогда role-local bonuses для `guard` дают:

```text
Bonus 1:
  loyal present
  raw = +5

Bonus 2:
  fear = 2
  raw = -2 * 2 = -4
```

Итоговый вклад роли:

```text
+5 + (-4) = +1
```

Storylet может затем агрегировать:

- вклад `authority`
- вклад `guard`
- вклад других ролей

Но первичный источник этих bonus signals всегда role-local.

## Правила authoring

- используйте `TagCollectionAuthoring`, когда объект просто имеет собственные metadata tags
- используйте `TagQueryAuthoring`, когда нужен именно query по наличию или отсутствию тегов
- используйте `TagStackQueryAuthoring`, когда нужен порог, диапазон или сравнение количества
- не превращайте бонусы в скрытые preconditions
- не храните storylet-local смысл в reusable `RoleDefinitionSO`
- не делайте global storylet bonus list, если бонус должен считаться по назначенному актору роли
- используйте role-local бонусы для soft preference, а requirements для hard gating

## Нейминг

В документе используется следующий словарь:

- `RoleDefinitionSO` - reusable определение роли
- `StoryletRoleAuthoring` - storylet-local role slot
- `StoryletDefinitionSO` - контейнер storylet-а
- `TagCollectionAuthoring` - простой набор собственных тегов без семантики условия
- `TagRequirementAuthoring` - универсальное tag-based требование
- `ScoreBonusAuthoring` - authoring-форма bonus rule
- `ScoreBonusRule` - runtime-форма bonus rule

Если в коде временно встречается старая схема или старые поля, этот документ следует читать как описание целевой модели, к которой приводится authoring-архитектура.
