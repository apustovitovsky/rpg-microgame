Главная идея:

Максимизируем количество активных квестов не через полный перебор, а через порядок выбора квестов и порядок назначения акторов.

Базовый greedy-алгоритм

На каждом шаге:

Для каждого ещё не выбранного квеста проверяем, можно ли закрыть все роли свободными акторами.
Считаем для него greedy-score.
Берём лучший квест.
Назначаем акторов на его роли.
Удаляем этих акторов из свободного пула.
Повторяем, пока больше нельзя закрыть ни один квест.

Псевдо:

while (true)
{
    QuestCandidate best = default;
    bool found = false;

    foreach (var quest in remainingQuests)
    {
        if (!TryBuildAssignment(quest, freeActors, out var assignment))
            continue;

        var score = EvaluateQuestCandidate(quest, assignment, freeActors);

        if (!found || score > best.Score)
        {
            best = new QuestCandidate(quest, assignment, score);
            found = true;
        }
    }

    if (!found)
        break;

    ActivateQuest(best.Quest, best.Assignment);
    RemoveAssignedActors(freeActors, best.Assignment);
    remainingQuests.Remove(best.Quest);
}

Это не гарантирует глобальный оптимум, но хорошо масштабируется и управляется дизайнерскими весами.

Самая важная эвристика

Не выбирай квест только по “лучше подходит”. Это часто ломает покрытие.

Для максимального покрытия важнее:

закрывать квесты так, чтобы потратить как можно меньше редких акторов.

То есть роль должен закрывать не “самый лучший актор”, а часто наоборот:

самый дешёвый подходящий актор, у которого меньше полезности для других ролей.

Метрики
1. Actor versatility

Сколько ролей/квестов может закрыть актор.

actorVersatility[actor] = count of roles actor can fill

Чем выше значение, тем актор ценнее как универсальный ресурс.

Например:

Actor A может быть: Warrior, Guard, Mercenary, Bandit
Actor B может быть: Bandit only

Для роли Bandit лучше взять Actor B, потому что Actor A может пригодиться где-то ещё.

2. Role scarcity

Насколько роль редкая.

roleScarcity[role] = 1f / compatibleFreeActorsCount

Если роль могут закрыть только 2 актора, она дорогая.
Если роль могут закрыть 200 акторов, она дешёвая.

3. Quest cost

Сколько редких ресурсов потребляет квест.

Пример:

questCost = sum(roleScarcity for each role)

Чем меньше questCost, тем дешевле квест закрыть.

4. Quest efficiency

Для максимального количества квестов обычно выгодны маленькие дешёвые квесты:

efficiency = 1f / questCost

Но если просто брать самые маленькие квесты, можно навсегда оставить крупные сюжетные квесты без акторов.

Поэтому лучше:

score =
    questPriority * PriorityWeight
  + questEfficiency * EfficiencyWeight
  - questCost * CostWeight
  - assignedActorOpportunityCost * OpportunityCostWeight;
Назначение акторов внутри квеста

Внутри квеста тоже нужен greedy.

Правильный порядок:

Сначала закрывать самые редкие роли.
Для каждой роли выбирать самого дешёвого подходящего актора.
roles.SortBy(role => compatibleFreeActorsCount(role));

То есть:

сначала Priest, если Priest редкий
потом Warrior
потом Bandit, если Bandit массовый
Actor choice score

Для конкретной роли:

actorCost =
    actorVersatility * VersatilityWeight
  - actorRoleFit * FitWeight
  + actorPriorityPenalty;

Выбираем актора с минимальной стоимостью.

bestActor = compatibleActors.MinBy(actorCost);

Интуитивно:

универсальных акторов бережём;
специализированных акторов используем;
плохой fit не берём, если есть заметно лучший;
но fit не должен доминировать над покрытием, если главная цель — maximum coverage.
Общая структура
public sealed class GreedyQuestMatcher
{
    public QuestMatchResult Match(
        IReadOnlyList<Actor> actors,
        IReadOnlyList<Quest> quests)
    {
        var freeActors = new HashSet<Actor>(actors);
        var remainingQuests = new HashSet<Quest>(quests);

        var result = new QuestMatchResult();

        while (remainingQuests.Count > 0)
        {
            QuestCandidate? bestCandidate = null;

            foreach (var quest in remainingQuests)
            {
                if (!TryAssignQuestGreedy(quest, freeActors, out var assignment))
                    continue;

                var score = EvaluateQuestCandidate(quest, assignment, freeActors);

                if (bestCandidate == null || score > bestCandidate.Score)
                {
                    bestCandidate = new QuestCandidate(
                        quest,
                        assignment,
                        score);
                }
            }

            if (bestCandidate == null)
                break;

            result.Add(bestCandidate.Quest, bestCandidate.Assignment);

            foreach (var assignment in bestCandidate.Assignment)
                freeActors.Remove(assignment.Actor);

            remainingQuests.Remove(bestCandidate.Quest);
        }

        return result;
    }
}
TryAssignQuestGreedy
private bool TryAssignQuestGreedy(
    Quest quest,
    HashSet<Actor> freeActors,
    out List<RoleAssignment> assignments)
{
    assignments = new List<RoleAssignment>();

    var localUsed = new HashSet<Actor>();

    var orderedRoles = quest.Roles
        .OrderBy(role => CountCompatibleActors(role, freeActors))
        .ToList();

    foreach (var role in orderedRoles)
    {
        Actor bestActor = null;
        float bestCost = float.PositiveInfinity;

        foreach (var actor in freeActors)
        {
            if (localUsed.Contains(actor))
                continue;

            if (!CanActorFillRole(actor, role))
                continue;

            float cost = EvaluateActorCost(actor, role);

            if (cost < bestCost)
            {
                bestCost = cost;
                bestActor = actor;
            }
        }

        if (bestActor == null)
            return false;

        localUsed.Add(bestActor);
        assignments.Add(new RoleAssignment(role, bestActor));
    }

    return true;
}
EvaluateActorCost
private float EvaluateActorCost(Actor actor, Role role)
{
    float versatility = GetActorVersatility(actor);
    float fit = GetActorRoleFit(actor, role);

    return
        versatility * 1.0f
      - fit * 0.5f;
}

Если для тебя главное именно покрытие, versatility должен весить сильнее, чем fit.

Например:

return
    versatility * 10.0f
  - fit * 1.0f;
EvaluateQuestCandidate
private float EvaluateQuestCandidate(
    Quest quest,
    IReadOnlyList<RoleAssignment> assignment,
    HashSet<Actor> freeActors)
{
    float priority = quest.Priority;
    float roleCost = 0f;
    float actorOpportunityCost = 0f;
    float fitScore = 0f;

    foreach (var role in quest.Roles)
    {
        int compatibleCount = CountCompatibleActors(role, freeActors);
        roleCost += 1f / Math.Max(1, compatibleCount);
    }

    foreach (var item in assignment)
    {
        actorOpportunityCost += GetActorVersatility(item.Actor);
        fitScore += GetActorRoleFit(item.Actor, item.Role);
    }

    return
        priority * 1000f
      + fitScore * 1f
      - roleCost * 100f
      - actorOpportunityCost * 10f;
}

Но если цель строго максимальное количество квестов, то priority лучше использовать как tie-breaker, а не как главный фактор.

Более подходящий score для maximum coverage

Я бы начал с такого:

score =
    1000f
  - totalAssignedActorOpportunityCost * 10f
  - totalRoleScarcityCost * 100f
  + questPriority * 1f
  + totalFitScore * 0.1f;

Почему 1000f?

Потому что каждый валидный квест даёт базовую ценность.
Алгоритм каждый раз выбирает один квест, но этот базовый вес помогает сделать шкалу понятной.

Фактически он говорит:

“Любой закрываемый квест полезен, но из них выбери тот, который меньше портит будущие возможности.”

Важное улучшение: dynamic recomputation

Не считай scarcity и versatility один раз в начале.

После каждого выбранного квеста пул свободных акторов меняется, поэтому нужно пересчитывать:

compatibleFreeActorsCount(role)
actorOpportunityCost(actor)
questFeasibility

Иначе алгоритм будет принимать решения по устаревшей картине.

Ещё лучше: lookahead на 1 шаг

Это всё ещё greedy, но гораздо качественнее.

Для каждого кандидата:

Временно назначить его.
Посчитать, сколько квестов останутся feasible после этого.
Чем больше осталось feasible, тем лучше.
score =
    feasibleQuestCountAfterAssignment * 100f
  - actorOpportunityCost * 10f
  - roleScarcityCost * 50f
  + questPriority;

Это называется one-step lookahead greedy.

Он не перебирает все комбинации, но спрашивает:

“Если я сейчас возьму этот квест, насколько сильно я сломаю карту возможностей?”

Пример one-step lookahead
private float EvaluateQuestCandidateWithLookahead(
    Quest quest,
    IReadOnlyList<RoleAssignment> assignment,
    HashSet<Actor> freeActors,
    IReadOnlyCollection<Quest> remainingQuests)
{
    var simulatedFreeActors = new HashSet<Actor>(freeActors);

    foreach (var item in assignment)
        simulatedFreeActors.Remove(item.Actor);

    int feasibleAfter = 0;

    foreach (var otherQuest in remainingQuests)
    {
        if (otherQuest == quest)
            continue;

        if (CanQuestBeAssignedGreedy(otherQuest, simulatedFreeActors))
            feasibleAfter++;
    }

    float opportunityCost = assignment.Sum(x => GetActorVersatility(x.Actor));

    return
        feasibleAfter * 100f
      - opportunityCost * 10f
      + quest.Priority;
}

Для твоей задачи это, скорее всего, лучший компромисс.

Итоговый рекомендуемый вариант

Я бы сделал так:

Global loop
пока есть закрываемые квесты:
    найти все feasible quests
    для каждого построить greedy assignment
    оценить candidate через one-step lookahead
    выбрать лучший
    зафиксировать назначение
    удалить занятых акторов
Role assignment внутри квеста
сначала самые редкие роли
для каждой роли взять самого специализированного подходящего актора
Candidate score
максимизировать:
    сколько квестов останутся feasible после выбора

минимизировать:
    потерю универсальных акторов
    расход редких ролей

tie-breakers:
    quest priority
    actor-role fit

Коротко:

Greedy objective:
    выбрать квест, который можно закрыть сейчас
    и после которого останется максимально много закрываемых квестов

Это напрямую соответствует твоей цели: максимальное одновременное покрытие квестов.


----------------------------------------------------------------------------------------------------------------------

Дополнение к алгоритму:

Да, это уже другая модель: роль зависит не только от `Actor.Tags`, а от **графа отношений между назначенными акторами**.

То есть квест теперь собирает не просто группу акторов, а **маленький labeled graph**:

```text
Actor A -- relation tags --> Actor B
Actor B -- relation tags --> Actor C
```

И роль может требовать:

```text
роль Knight:
    ActorTags: Knight
    RelationToOthers: должен иметь relation NobleBond / SwornAlly / Enemy / WitnessOf ...
```

---

## Новая формализация

Раньше было:

```csharp
CanActorFillRole(actor, role)
```

Теперь нужно:

```csharp
CanActorFillRole(actor, role)
CanRelationsSatisfy(assignments)
```

Потому что relation-теги проверяются только когда есть **пара акторов**.

---

# Базовая модель данных

## Actor

```csharp
public sealed class Actor
{
    public int Id;
    public TagSet Tags;
    public ActorRelation[] Relations;
}
```

## ActorRelation

```csharp
public readonly struct ActorRelation
{
    public readonly int TargetActorId;
    public readonly TagSet Tags;

    public ActorRelation(int targetActorId, TagSet tags)
    {
        TargetActorId = targetActorId;
        Tags = tags;
    }
}
```

Например:

```text
Arthur:
    Tags: Knight, Noble

Relations:
    -> Lancel: Friend, SwornBrother
    -> Mordred: Enemy
```

---

# Role

У роли теперь есть два уровня требований:

```csharp
public sealed class Role
{
    public TagQuery ActorQuery;
    public RelationRequirement[] RelationRequirements;
}
```

`ActorQuery` проверяет самого актора:

```text
роль требует Knight
роль требует Priest
роль требует Witness
```

`RelationRequirements` проверяют связи с другими назначенными ролями/акторами.

---

# RelationRequirement

Тут важно понять: relation-требование обычно должно ссылаться не просто на “кого-то”, а на **другую роль**.

Например:

```text
Knight должен быть sworn ally с Noble
Priest не должен иметь Enemy relation с Acolyte
Witness должен знать Victim
```

Поэтому лучше не писать:

```text
Knight requires relation AnyOf(Friend) to someone
```

а писать:

```text
Knight requires relation AnyOf(Friend) to role Companion
```

Модель:

```csharp
public readonly struct RelationRequirement
{
    public readonly RoleId TargetRoleId;

    public readonly TagQuery RelationQuery;

    public readonly RelationDirection Direction;

    public readonly bool Required;
}
```

Направление:

```csharp
public enum RelationDirection
{
    FromSelfToTarget,
    FromTargetToSelf,
    BothDirections,
    AnyDirection
}
```

---

# Пример

```text
Role: Knight
ActorQuery:
    AllOf: Knight

RelationRequirements:
    TargetRole: Noble
    RelationQuery:
        AnyOf: SwornAlly, Bloodline, Vassal
    Direction:
        FromSelfToTarget
```

Это значит:

> актор, назначенный на Knight, должен иметь отношение к актору, назначенному на Noble, с одним из тегов `SwornAlly | Bloodline | Vassal`.

---

# Проверка relation

Удобно заранее построить быстрый индекс отношений:

```csharp
public sealed class RelationIndex
{
    private readonly Dictionary<(int From, int To), TagSet> _relations;

    public TagSet GetRelationTags(int fromActorId, int toActorId)
    {
        return _relations.TryGetValue((fromActorId, toActorId), out var tags)
            ? tags
            : TagSet.None;
    }
}
```

Тогда проверка:

```csharp
private bool MatchesRelation(
    Actor from,
    Actor to,
    TagQuery query,
    RelationIndex relationIndex)
{
    TagSet relationTags = relationIndex.GetRelationTags(from.Id, to.Id);
    return query.Matches(relationTags);
}
```

---

# Финальная проверка квеста

После того как роли назначены:

```csharp
private bool IsFinalAssignmentValid(
    IReadOnlyList<RoleAssignment> assignments,
    RelationIndex relationIndex)
{
    foreach (var assignment in assignments)
    {
        foreach (var requirement in assignment.Role.RelationRequirements)
        {
            if (!TryGetAssignmentByRole(
                assignments,
                requirement.TargetRoleId,
                out var targetAssignment))
            {
                return false;
            }

            if (!IsRelationRequirementSatisfied(
                assignment.Actor,
                targetAssignment.Actor,
                requirement,
                relationIndex))
            {
                return false;
            }
        }
    }

    return true;
}
```

---

# Проверка направления

```csharp
private bool IsRelationRequirementSatisfied(
    Actor self,
    Actor target,
    RelationRequirement requirement,
    RelationIndex relationIndex)
{
    return requirement.Direction switch
    {
        RelationDirection.FromSelfToTarget =>
            requirement.RelationQuery.Matches(
                relationIndex.GetRelationTags(self.Id, target.Id)),

        RelationDirection.FromTargetToSelf =>
            requirement.RelationQuery.Matches(
                relationIndex.GetRelationTags(target.Id, self.Id)),

        RelationDirection.AnyDirection =>
            requirement.RelationQuery.Matches(
                relationIndex.GetRelationTags(self.Id, target.Id))
            ||
            requirement.RelationQuery.Matches(
                relationIndex.GetRelationTags(target.Id, self.Id)),

        RelationDirection.BothDirections =>
            requirement.RelationQuery.Matches(
                relationIndex.GetRelationTags(self.Id, target.Id))
            &&
            requirement.RelationQuery.Matches(
                relationIndex.GetRelationTags(target.Id, self.Id)),

        _ => false
    };
}
```

---

# Как это влияет на greedy

Да, теперь обычная универсальность ещё слабее.

Раньше актор был полезен, если подходил ко многим ролям.

Теперь актор полезен, если:

```text
1. подходит по ActorTags;
2. имеет нужные relation-теги к другим подходящим акторам;
3. не нарушает forbidden relation-требования;
4. помогает закрывать relation-требования других ролей.
```

То есть актор стал не отдельным ресурсом, а **узлом графа**.

---

# Новый greedy внутри квеста

Старый порядок:

```text
самая редкая роль -> лучший актор
```

Новый порядок:

```text
самая ограниченная роль -> лучший актор с учётом связей
```

Роль считается ограниченной не только по числу акторов, но и по числу возможных relation-комбинаций.

---

## На каждом шаге

```csharp
while (unassignedRoles.Count > 0)
{
    Role bestRole = null;
    List<Actor> bestCandidates = null;

    foreach (var role in unassignedRoles)
    {
        var candidates = GetValidCandidatesForPartialAssignment(
            role,
            partialAssignments,
            freeActors,
            localUsed,
            relationIndex);

        if (candidates.Count == 0)
            return false;

        if (bestRole == null || candidates.Count < bestCandidates.Count)
        {
            bestRole = role;
            bestCandidates = candidates;
        }
    }

    Actor bestActor = SelectBestActor(
        bestRole,
        bestCandidates,
        partialAssignments,
        relationIndex);

    Assign(bestRole, bestActor);
}
```

---

# Partial validation

При частичном назначении нельзя требовать все связи сразу.

Например:

```text
Knight требует relation Friend к Bard
```

Если Knight уже назначен, а Bard ещё нет — это не ошибка.

Ошибка только если:

```text
среди оставшихся свободных акторов уже невозможно найти Bard,
с которым у Knight есть нужная связь.
```

Поэтому тебе нужны две проверки:

```csharp
IsPartialStillPossible(...)
IsFinalValid(...)
```

---

# GetValidCandidatesForPartialAssignment

Кандидат валиден, если:

```text
1. актор локально подходит на роль;
2. он не занят;
3. он не конфликтует с уже назначенными ролями;
4. все уже назначенные relation-требования после его добавления остаются либо выполненными, либо потенциально выполнимыми;
5. все его собственные relation-требования либо уже выполнены, либо потенциально выполнимы позже.
```

Скелет:

```csharp
private List<Actor> GetValidCandidatesForPartialAssignment(
    Role role,
    IReadOnlyList<RoleAssignment> partialAssignments,
    IReadOnlyList<Role> remainingRoles,
    IReadOnlyCollection<Actor> freeActors,
    HashSet<int> localUsedActorIds,
    RelationIndex relationIndex)
{
    var result = new List<Actor>();

    foreach (var actor in freeActors)
    {
        if (localUsedActorIds.Contains(actor.Id))
            continue;

        if (!role.ActorQuery.Matches(actor.Tags))
            continue;

        var hypothetical = AddTemporary(partialAssignments, role, actor);

        if (!IsPartialStillPossible(
            hypothetical,
            remainingRoles,
            freeActors,
            localUsedActorIds,
            relationIndex))
        {
            continue;
        }

        result.Add(actor);
    }

    return result;
}
```

---

# Как оценивать актора

Теперь `actorCost` должен учитывать relation-вклад:

```csharp
score =
    actorRoleFit * FitWeight
  + satisfiedRelationRequirements * RelationSatisfiedWeight
  + relationPotential * RelationPotentialWeight
  - relationConflicts * RelationConflictWeight
  - actorRemovalCost * OpportunityCostWeight;
```

Для greedy лучше выбирать **максимальный score**, а не минимальный cost.

---

## Relation contribution

```csharp
private float EvaluateRelationContribution(
    Actor actor,
    Role role,
    IReadOnlyList<RoleAssignment> partialAssignments,
    RelationIndex relationIndex)
{
    float score = 0f;

    foreach (var other in partialAssignments)
    {
        // Требования новой роли к уже назначенным
        foreach (var req in role.RelationRequirements)
        {
            if (req.TargetRoleId != other.Role.Id)
                continue;

            if (IsRelationRequirementSatisfied(actor, other.Actor, req, relationIndex))
                score += 10f;
            else
                score -= 1000f;
        }

        // Требования уже назначенных ролей к новой роли
        foreach (var req in other.Role.RelationRequirements)
        {
            if (req.TargetRoleId != role.Id)
                continue;

            if (IsRelationRequirementSatisfied(other.Actor, actor, req, relationIndex))
                score += 10f;
            else
                score -= 1000f;
        }
    }

    return score;
}
```

Это обрабатывает связи в обе стороны: новая роль может требовать связь к старой, и старая роль может требовать связь к новой.

---

# Важное: negative relation constraints

Например:

```text
Priest не хочет быть Enemy с Acolyte
```

Это лучше выражать через `NoneOf` в `RelationQuery`.

Например:

```csharp
RelationRequirement
{
    TargetRoleId = AcolyteRoleId,
    RelationQuery = new TagQuery
    {
        NoneOf = RelationTags.Enemy
    },
    Direction = RelationDirection.AnyDirection
}
```

Но есть нюанс.

Если отношения между двумя акторами нет, то `RelationTags = None`.

Тогда запрос:

```csharp
NoneOf = Enemy
```

будет истинным.

То есть отсутствие отношения считается допустимым.

Это обычно правильно для запрета:

> главное, чтобы они не были врагами.

Но для обязательного отношения нужен `AllOf` или `AnyOf`.

---

# Нужен ли Required bool

Я бы не добавлял отдельный `Required`.

Достаточно `TagQuery`:

```csharp
AllOf = Friend
```

значит связь обязательна.

```csharp
AnyOf = Friend | Ally
```

значит нужна хотя бы одна из связей.

```csharp
NoneOf = Enemy
```

значит связь не должна иметь Enemy.

```csharp
AllOf = None, AnyOf = None, NoneOf = Enemy
```

значит связи может не быть, но если есть Enemy — нельзя.

---

# Новый подсчёт “универсальности”

Статическая универсальность теперь может быть такой:

```text
actorRoleDegree =
    сколько ролей актор может закрыть по ActorTags

actorRelationDegree =
    сколько полезных relation edges у актора к другим свободным акторам

actorGraphValue =
    actorRoleDegree + actorRelationDegree * weight
```

Но для maximum coverage лучше динамический cost:

```text
actorRemovalCost =
    сколько квестов перестанут быть потенциально закрываемыми,
    если удалить этого актора из freeActors
```

Это остаётся лучшей метрикой.

---

# Быстрая feasible-проверка квеста

Для каждого квеста можно грубо оценить:

```text
1. для каждой роли есть хотя бы один актор по ActorTags;
2. для каждой relation-пары ролей есть хотя бы одна пара акторов,
   которая подходит по ролям и имеет нужные relation tags;
3. не возникает очевидного конфликта NoneOf.
```

Это не гарантирует назначаемость, но хорошо работает как дешёвый фильтр перед greedy assignment.

---

# Итоговая архитектура

Я бы разделил систему так:

```csharp
public interface IActorRoleMatcher
{
    bool Matches(Actor actor, Role role);
}

public interface IRelationMatcher
{
    bool Matches(Actor from, Actor to, RelationRequirement requirement);
}

public interface IQuestAssignmentValidator
{
    bool IsPartialStillPossible(...);
    bool IsFinalValid(...);
}

public interface IQuestAssignmentBuilder
{
    bool TryBuildAssignment(...);
}
```

И главный greedy остаётся таким же:

```text
пока можно выбрать квест:
    для каждого квеста:
        TryBuildAssignment с учётом ActorTags + RelationTags
        если получилось:
            симулировать удаление акторов
            посчитать feasible quests after
    выбрать квест, после которого остаётся максимум возможностей
```

---

## Короткий вывод

Да, теперь задача стала **graph-constrained greedy matching**.

Актор больше не просто “подходит на роль”. Он подходит на роль **в контексте других выбранных акторов и их отношений**.

Минимально правильная модель:

```text
Role.ActorQuery              // требования к самому актору
Role.RelationRequirements    // требования к связям с другими ролями
Actor.Relations              // directed/undirected edges with tags
```

А универсальность нужно считать не как “сколько ролей закрывает актор”, а как:

```text
сколько будущих квестов и relation-комбинаций ломается,
если этого актора потратить сейчас
```

То есть главный критерий для greedy остаётся прежним: **one-step lookahead по количеству feasible quests после назначения**, но feasible-проверка теперь должна учитывать граф отношений.

С точки зрения **правильного выбора алгоритма** для такой задачи я бы формулировал решение так:

> Это не классический matching, а **жадная задача построения допустимых групп с ограничениями по графу отношений**.
> Практически правильный выбор: **greedy construction + constraint validation + one-step lookahead**.

То есть не пытаться свести всё к обычному `Actor -> Role matching`.

## Правильная модель задачи

У тебя есть:

```text
Quest = набор ролей
Role = требования к актору + требования к отношениям с другими ролями
Actor = теги + отношения к другим акторам
```

Значит кандидат на квест — это не отдельное назначение:

```text
Actor -> Role
```

а целая структура:

```text
QuestAssignment =
    RoleA -> Actor1
    RoleB -> Actor2
    RoleC -> Actor3

    + проверка отношений Actor1 <-> Actor2 <-> Actor3
```

Поэтому алгоритм должен выбирать не “лучшего актора на роль”, а **лучший полностью собираемый квест**.

---

# Рекомендуемый алгоритм

## 1. Global greedy loop

На глобальном уровне:

```text
пока можно активировать хотя бы один квест:
    для каждого оставшегося квеста:
        попытаться построить валидное назначение ролей
        если получилось:
            оценить, насколько это назначение портит будущие возможности

    выбрать лучший кандидат
    зафиксировать его
    удалить использованных акторов
```

То есть:

```csharp
while (true)
{
    Candidate best = null;

    foreach (var quest in remainingQuests)
    {
        if (!TryBuildQuestAssignment(quest, freeActors, out var assignment))
            continue;

        var score = EvaluateCandidateWithLookahead(
            quest,
            assignment,
            freeActors,
            remainingQuests);

        if (best == null || score > best.Score)
            best = new Candidate(quest, assignment, score);
    }

    if (best == null)
        break;

    Apply(best);
}
```

Это основа.

---

# 2. Внутри квеста — greedy constraint construction

Для конкретного квеста:

```text
пока есть неназначенные роли:
    выбрать самую ограниченную роль
    найти валидных акторов-кандидатов
    выбрать лучшего актора
    добавить в partial assignment
    проверить, что partial assignment всё ещё потенциально завершим
```

Ключевая эвристика:

> Выбирать не первую роль по порядку, а роль с минимальным числом валидных кандидатов в текущем частичном состоянии.

Это MRV-эвристика:

```text
minimum remaining values
```

Для greedy она очень полезна.

---

# 3. Проверки должны быть двух типов

Обязательно раздели:

```csharp
IsFinalValid(...)
IsPartialStillPossible(...)
```

## `IsFinalValid`

Проверяет уже полностью собранный квест:

```text
все роли закрыты
все акторы подходят по тегам
все relation-требования выполнены
noneof-конфликты отсутствуют
```

## `IsPartialStillPossible`

Проверяет частично собранный квест:

```text
уже нет явных конфликтов
оставшиеся требования ещё можно теоретически выполнить
```

Например:

```text
Knight требует relation Ally к Noble
```

Если `Knight` уже назначен, а `Noble` ещё нет — это не ошибка.

Но если среди свободных акторов уже нет ни одного подходящего `Noble`, который имеет нужную связь с Knight, тогда partial assignment надо отбросить.

---

# 4. Основной критерий выбора — lookahead

Для максимального покрытия главный score должен быть не “качество текущего квеста”, а:

```text
сколько квестов останутся потенциально собираемыми после этого назначения
```

Пример:

```csharp
score =
    feasibleQuestsAfter * 10000f
  - brokenQuestCount * 5000f
  - actorRemovalCost * 100f
  - relationBottleneckCost * 50f
  + questPriority * 10f
  + assignmentFit * 1f;
```

Главный фактор:

```csharp
feasibleQuestsAfter
```

Потому что цель — **максимальное покрытие**, а не идеальный матч текущего квеста.

---

# Почему не обычный matching

Обычный bipartite matching подходит, когда есть только:

```text
Actor подходит / не подходит к Role
```

Но у тебя есть зависимости:

```text
Role A требует отношение к Role B
Role C запрещает отношение к Role D
Actor X подходит только если рядом Actor Y
```

Это уже не независимые назначения.

Поэтому обычный matching может выбрать локально корректные пары, но получить невалидную группу.

---

# Почему не точная оптимизация

Точное решение — это что-то вроде:

```text
constraint programming
integer programming
SAT / MaxSAT
backtracking search
```

Но если акторов много и нужно быстро, это не твой основной путь.

Твой путь:

```text
approximate greedy solver
```

с хорошими эвристиками и валидацией.

---

# Итоговое название алгоритма

Я бы называл это так:

> **Greedy graph-constrained quest packing with one-step lookahead**

По-русски:

> **жадная упаковка квестов с графовыми ограничениями и одношаговым прогнозом**

---

# Финальная рекомендация

Правильная архитектура алгоритма:

```text
1. Для каждого квеста пытаться построить валидный QuestAssignment.
2. QuestAssignment строить жадно, выбирая самую ограниченную роль.
3. При каждом назначении проверять partial validity.
4. После сборки проверять final validity.
5. На глобальном уровне выбирать не самый “красивый” квест, а тот, после которого остаётся максимум feasible-квестов.
6. Статическую универсальность акторов оставить только как tie-breaker.
```

То есть главный принцип:

> Не максимизировать качество текущего назначения, а минимизировать ущерб для будущих назначений.

Для твоей задачи это наиболее правильный greedy-подход.
