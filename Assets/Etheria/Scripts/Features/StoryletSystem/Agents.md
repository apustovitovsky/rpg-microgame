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