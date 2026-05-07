# StoryletSystem Implementation Overview

## Purpose of This Document

Этот документ описывает **текущую реализованную v1-имплементацию** `StoryletSystem` в `Assets/Etheria/Scripts/Features/StoryletSystem`.

Его цель:

- объяснить, как теперь работает алгоритм;
- показать, что именно осталось от старого matcher-подхода;
- объяснить, что именно было заменено planner-архитектурой;
- разложить систему по слоям абстракции и по основным классам;
- зафиксировать, что уже сделано из основных docs;
- зафиксировать, что пока **не** реализовано.

Это companion-док к коду, а не новый source of truth поверх него. Если код и этот обзор расходятся, приоритет у кода.

---

## 1. Краткий итог

До переработки система была в первую очередь **relation-aware greedy matcher**:

- на вход подавались `Entity`, `Storylet`, `Relation`;
- система искала, какие storylet-ы можно собрать из текущего пула сущностей;
- внутри одного storylet-а строился assignment ролей на сущности;
- сверху работал greedy-выбор следующего лучшего storylet-а;
- мир как последовательность `state transitions` ещё не был полноценным объектом модели.

После переработки система стала **planner-driven**:

- появился явный `StoryletWorldState`;
- storylet теперь описывается как `StoryletDefinition` с `Preconditions`, `Effects`, `RepeatabilityPolicy`, `SaliencePolicy`;
- локальный relation-aware assignment сохранён, но стал только **внутренним solver-ом инстанцирования одного storylet-а**;
- top-level выбор делает `BeamStoryletPlanner`;
- planner работает через симуляцию: `candidate -> apply effects -> next world -> future candidates`;
- появился explainable trace, который показывает кандидатов, surviving branches, pruning и итоговую winning branch.

Иными словами:

- **старый matcher не удалён**, но его роль сузилась;
- **новый planner слой стал главным уровнем принятия решений**.

---

## 1.1. Что точно НЕ вошло в текущую v1 реализацию

Ниже короткий список того, что **не было реализовано** в этом проходе, чтобы это можно было увидеть сразу, не дочитывая документ до конца.

### Не вошло в v1

- production gameplay integration
  - planner не встроен в боевой scene/gameplay flow;
  - есть только автономный engine + smoke runner.
- data-driven authoring layer
  - нет ScriptableObject authoring для `StoryletDefinition`;
  - storylet-ы smoke-сценария создаются кодом.
- editor tooling
  - нет custom inspector, graph view, authoring validator, trace viewer в editor UI.
- automated tests
  - нет unit tests, regression tests, snapshot tests для trace и planner behavior.
- advanced scoring model
  - scoring уже работает, но остаётся эвристическим и ещё требует тюнинга весов.
- advanced search features
  - нет adaptive beam width;
  - нет более глубокого future value estimation;
  - нет branch diversity policy как отдельного механизма.
- optimized immutable state representation
  - логическая immutability есть;
  - structural sharing / copy-on-write optimization специально не делались.
- richer domain model from future docs ideas
  - нет provenance/causality model для effects;
  - нет отдельной derived-state layer;
  - нет более богатой модели world markers beyond current tags/attributes/entities/relations.
- content pipeline beyond smoke
  - нет набора production storylets;
  - нет загрузки storylet-контента из assets/data.
- rename/cleanup pass
  - smoke entry point всё ещё называется `StoryletMatcherSmokeTestService`, хотя по смыслу это уже planner smoke runner.

### Что осознанно отложено

- точная драматургическая калибровка smoke-сценария;
- финальные веса scoring;
- точная authoring-модель для production content;
- интеграция planner-а в реальные игровые runtime systems.

### Что уже есть, но ещё не финализировано

- beam planner как архитектурный каркас;
- trace и explainability;
- repeatability/salience/scoring pipeline;
- smoke scenario с рабочими world transitions.

То есть текущая граница v1 такая:

- **реализован planner engine и smoke scenario**;
- **не реализованы production integration, authoring tools и финальный content pipeline**.

---

## 1.2. Как теперь разложен код по папкам

После cleanup-круга код `StoryletSystem` разделён по слоям:

- `Primitives/**`
- `Matching/**`
- `Planner/**`
- `Smoke/**`
- `Installers/**`
- `Legacy/**`
- `Documentation/**`

Ключевая практическая граница:

- active planner pipeline живёт в `Planner`, `Matching`, `Primitives`, `Smoke`, `Installers`;
- quarantined legacy matcher flow живёт в `Legacy`;
- старый `GreedyStoryletMatcher` больше не лежит рядом с активными planner services.

---

## 2. Главная архитектурная идея

Система теперь разделена на два разных класса задач.

### 2.1. Local instantiation problem

Вопрос:

> Можно ли на данном `WorldState` инстанцировать конкретный storylet и подобрать под его роли совместимые сущности?

Это локальная задача. Она по-прежнему близка к:

- CSP-like assignment;
- relation-aware matching;
- partial feasibility pruning.

Эту задачу решает:

- `StoryletInstantiationService`
- внутри него старый `GreedyStoryletAssignmentBuilder`
- и старые low-level типы `Role`, `RelationRequirement`, `Entity`, `EntityRelation`, `TagQuery`, `AttributeRequirement`

### 2.2. Planning over world transitions

Вопрос:

> В каком порядке лучше применять storylet-ы, если каждый из них меняет мир и открывает или закрывает будущие ветки?

Это уже не matching, а planner-задача. Её решает:

- `BeamStoryletPlanner`
- через `WorldState`, `Effects`, `Memory`, `Scoring`, `Beam Search`

Это и есть главный архитектурный сдвиг v1.

---

## 3. Слои абстракции

Ниже полезно смотреть на систему как на 5 слоёв.

### Layer 1. Low-level matching primitives

Старые базовые типы:

- `Entity`
- `EntityId`
- `EntityRelation`
- `Role`
- `RoleId`
- `RoleAssignment`
- `RelationRequirement`
- `RelationDirection`
- `RelationIndex`
- `TagSet`
- `TagQuery`
- `AttributeSet`
- `AttributeId`
- `AttributeRequirement`
- `AttributePreference`

Их роль:

- описывать сущности и их свойства;
- описывать роли storylet-а;
- описывать relation constraints между ролями;
- проверять tag/attribute compatibility;
- строить relation index для быстрых lookup-ов.

Этот слой почти не знает ничего про planner.

### Layer 2. Local assignment engine

Классы:

- `IEntityRoleFitEvaluator`
- `AttributePreferenceEntityRoleFitEvaluator`
- `IStoryletAssignmentBuilder`
- `GreedyStoryletAssignmentBuilder`
- `GreedyStoryletMatcher`
- `StoryletMatchingContext`

Исторически это был основной алгоритм системы.

Теперь:

- `GreedyStoryletMatcher` больше не является главным orchestrator-ом фичи;
- `GreedyStoryletAssignmentBuilder` используется как **локальный solver** внутри planner pipeline;
- `StoryletMatchingContext` остался важным объектом для relation-aware оценки assignment.

### Layer 3. Planner domain model

Новые planner-facing сущности:

- `StoryletWorldState`
- `StoryletDefinition`
- `StoryletEffectBatch`
- `StoryletInstantiationCandidate`
- `StoryletPlannerMemory`
- `StoryletScoreBreakdown`
- `StoryletSalienceEvaluation`
- `StoryletRejectionReason`
- `StoryletPlannerResult`
- `StoryletPlannedStep`

Это уже явная доменная модель planning-задачи.

### Layer 4. Planner services

Новые сервисы:

- `IStoryletInstantiationService`
- `IStoryletEffectApplier`
- `IStoryletRepeatabilityService`
- `IStoryletSalienceEvaluator`
- `IStoryletScoringService`
- `IStoryletPlanner`
- `IStoryletTelemetryFormatter`

И concrete implementations:

- `StoryletInstantiationService`
- `StoryletEffectApplier`
- `StoryletRepeatabilityService`
- `StoryletSalienceEvaluator`
- `StoryletScoringService`
- `BeamStoryletPlanner`
- `StoryletTelemetryFormatter`

### Layer 5. Composition + smoke scenario

Composition root:

- `StoryletFeatureInstallerSO`

Smoke entry point:

- `StoryletMatcherSmokeTestService`

Этот слой отвечает за:

- DI wiring через VContainer;
- сборку smoke-world;
- code-authored storylet definitions;
- запуск planner-а и вывод trace в `Debug.Log`.

---

## 4. Что было заменено по сравнению со старым алгоритмом

### 4.1. Что было раньше

Старый top-level flow в `GreedyStoryletMatcher` работал так:

1. Получить список storylet-ов и список свободных `Entity`.
2. Для каждого storylet-а попробовать построить assignment через `_assignmentBuilder`.
3. Оценить storylet через greedy score.
4. Выбрать лучший storylet.
5. Удалить занятые сущности из `freeEntities`.
6. Повторить.

Важная особенность:

- он решал задачу **распределения текущего пула сущностей**;
- но не моделировал мир как evolving state;
- не было общего declarative слоя effects;
- не было явной planner memory;
- не было beam search-а;
- lookahead был только локальной эвристикой “сколько storylet-ов останется feasible после этого выбора”.

### 4.2. Что стало теперь

Новый top-level flow в `BeamStoryletPlanner` работает так:

1. Взять стартовый `StoryletWorldState`.
2. Для каждой активной beam-ветки собрать valid candidates.
3. Для каждого valid candidate:
   - построить assignment;
   - вычислить salience;
   - применить effects;
   - получить `nextWorldState`;
   - обновить planner memory;
   - посчитать future candidates;
   - посчитать score breakdown;
   - породить новую branch.
4. Дедуплицировать ветки по `(world fingerprint + memory fingerprint)`.
5. Оставить только top `beamWidth` веток.
6. Повторять до `maxDepth`.
7. Взять ветку с максимальным accumulated score.
8. Построить explainable trace по шагам этой winning branch.

То есть заменён именно **top-level orchestration**.

### 4.3. Что НЕ заменено

Не заменены low-level механизмы relation-aware role assignment.

Они по-прежнему используются и по-прежнему решают важную задачу:

> Как именно конкретный storylet привязать к конкретным сущностям.

Именно поэтому корректнее говорить не “старый алгоритм выкинули”, а:

> Старый matcher был понижен до уровня локального solver-а внутри более общей planner-архитектуры.

---

## 5. Основные новые сущности

## 5.1. `StoryletWorldState`

Файлы: `Planner/Domain/StoryletWorldState.cs` и соседние domain types в `Planner/Domain`

Это главный snapshot мира для planner-а.

Хранит:

- `SnapshotId`
- `Entities`
- `Relations`
- `WorldAttributes`
- `WorldTags`

Что важно:

- это логически immutable snapshot;
- planner мыслит последовательностью snapshots;
- каждая симуляция storylet-а возвращает **новый** logical state.

Также `StoryletWorldState` умеет:

- искать сущности по `EntityId`;
- проверять existence;
- проверять наличие сущностей по `TagQuery`;
- считать количество сущностей по query;
- строить `RelationIndex`;
- строить строковый `fingerprint` для dedup planner branches.

`GetFingerprint()` сейчас собирается из:

- identity/keys/tag-hash/entities attributes;
- relations and relation tag hashes;
- world tags;
- world attributes.

Это не криптографический hash, а практический dedup-key для beam planner-а.

## 5.2. `StoryletDefinition`

Файл: `Planner/Domain/StoryletDefinition.cs`

Это новая planner-facing форма storylet-а.

Содержит:

- `Id`
- `Key`
- `Priority`
- `Roles`
- `Preconditions`
- `Effects`
- `RepeatabilityPolicy`
- `SaliencePolicy`

Ключевая идея:

- старый `Storylet` был в основном carrier-ом ролей и priority;
- новый `StoryletDefinition` описывает **storylet как state-transition unit**.

Метод `ToStorylet()` нужен как адаптер назад к старому matcher-слою.

## 5.3. `StoryletEffectBatch`

Просто контейнер списка `StoryletEffect`.

Он нужен, чтобы storylet мыслился как:

- “условия + шаблон ролей + пакет изменений мира”.

## 5.4. `StoryletInstantiationCandidate`

Это уже “конкретно инстанцируемый candidate”, а не просто abstract definition.

Содержит:

- `Definition`
- `LegacyStorylet`
- `Assignment`
- `EffectPreview`
- `InstantiationQuality`
- `Salience`
- `ScoreBreakdown`

Это bridge-объект между:

- шаблоном storylet-а;
- конкретной привязкой ролей;
- planner scoring;
- trace.

## 5.5. `StoryletPlannerMemory`

Файл: `Planner/Domain/StoryletPlannerMemory.cs`

Это planner-side память о прошлом.

Содержит:

- `CurrentStep`
- `ExecutionStepByStoryletId`
- `ExecutionCountByStoryletId`
- `LastStepByRolePairingKey`
- `LastWorldFingerprintByStoryletId`

Зачем это нужно:

- блокировать `OnceEver` / `OncePerRun`;
- поддерживать cooldown;
- запрещать `RepeatableUntilStateChanges` на том же состоянии мира;
- штрафовать повтор storylet-а;
- штрафовать повтор той же пары `(storylet, role, entity)`.

Это важный шаг от “чисто stateless matcher-а” к реальному planner-у.

## 5.6. `StoryletScoreBreakdown`

Содержит 4 числа:

- `InstantiationQuality`
- `TransitionValue`
- `FuturePotential`
- `AntiRepetition`

Плюс `Total`.

Это специальная explainable-модель score-а, чтобы trace показывал не только результат, но и его причины.

## 5.7. `StoryletRejectionReason`

Содержит:

- `Category`
- `Message`

Используется, когда candidate invalid.

Это важный шаг в сторону explainability:

- не просто “storylet не подошёл”;
- а “почему именно не подошёл”.

---

## 6. Preconditions: как теперь проверяется доступность storylet-а

Файлы: `Planner/Preconditions/*.cs`

Все precondition-ы реализуют:

```csharp
IStoryletPrecondition
```

С методом:

```csharp
bool IsSatisfied(
    StoryletWorldState worldState,
    StoryletPlannerMemory memory,
    out StoryletRejectionReason rejectionReason)
```

Это значит:

- проверка идёт уже не только по локальным role constraints;
- storylet имеет верхнеуровневые гейты доступности.

Реализованные precondition-типы:

- `EntityTagPrecondition`
- `WorldTagPrecondition`
- `WorldAttributePrecondition`
- `EntityExistencePrecondition`
- `RelationPrecondition`

Что они покрывают:

- наличие или отсутствие сущностей с нужными тегами;
- world-level tags;
- world-level attribute thresholds;
- факт существования или отсутствия конкретной entity;
- наличие или отсутствие relation между конкретными entity.

Что важно:

- repeatability не реализована как отдельный precondition-класс;
- она вынесена в отдельный сервис `IStoryletRepeatabilityService`.

Это хорошее разделение:

- declarative world conditions живут в preconditions;
- temporal memory policy живёт в repeatability service.

---

## 7. Effects: как теперь storylet меняет мир

Файлы: `Planner/Effects/*.cs`

Базовый тип:

```csharp
StoryletEffect
```

Подтипы:

- `AddEntityTagEffect`
- `RemoveEntityTagEffect`
- `SetEntityAttributeEffect`
- `AddEntityAttributeEffect`
- `RemoveEntityAttributeEffect`
- `SetWorldAttributeEffect`
- `AddWorldAttributeEffect`
- `SpawnEntityEffect`
- `DespawnEntityEffect`
- `CreateRelationEffect`
- `RemoveRelationEffect`
- `AddRelationTagEffect`
- `RemoveRelationTagEffect`

Зачем это нужно:

- planner должен уметь симулировать storylet;
- изменения мира должны быть декларативными и обозримыми;
- trace должен уметь показать, что именно произошло;
- один и тот же storylet можно применять в simulation без скрытых side effects.

### Как работает применение effects

Сервис:

- `StoryletEffectApplier`

Алгоритм:

1. Копирует текущие entities в локальный `entityMap`.
2. Копирует relations в `relationMap`.
3. При необходимости расширяет `worldAttributes`.
4. По очереди применяет все effects.
5. Возвращает новый `StoryletWorldState` со `SnapshotId + 1`.

Важно:

- входной `worldState` не мутируется;
- возвращается новый логический snapshot;
- при `DespawnEntityEffect` централизованно удаляются и relation edges, завязанные на сущность.

Это один из центральных функциональных шагов всей переработки.

---

## 8. Как storylet инстанцируется на конкретном world state

Сервис:

- `StoryletInstantiationService`

Он отвечает на вопрос:

> Доступен ли этот storylet на текущем world state и можно ли построить ему корректный assignment?

Алгоритм:

1. Проверить все `definition.Preconditions`.
2. Сконвертировать `StoryletDefinition` в legacy `Storylet`.
3. Построить `StoryletMatchingContext`:
   - storylets
   - entities
   - relation index
4. Создать `freeEntities` на основе всех сущностей мира.
5. Вызвать старый `_assignmentBuilder.TryBuildAssignment(...)`.
6. Если assignment не найден:
   - вернуть invalid result с `invalid_role_binding`.
7. Иначе:
   - оценить `InstantiationQuality`;
   - создать `StoryletInstantiationCandidate`.

### Что считается `InstantiationQuality`

Сейчас это:

- сумма `IEntityRoleFitEvaluator` по всем role assignments;
- плюс relation score из `StoryletMatchingContext`;
- плюс вклад `definition.Priority`.

То есть локальная качественная оценка storylet-а всё ещё очень сильно опирается на старую relation-aware matching модель.

---

## 9. Repeatability и память о прошлом

Сервис:

- `StoryletRepeatabilityService`

Поддерживаемые режимы:

- `OnceEver`
- `OncePerRun`
- `RepeatableWithCooldown`
- `RepeatableUntilStateChanges`

### `IsBlocked(...)`

Проверяет, запрещён ли storylet сейчас.

Примеры:

- `OncePerRun`: если storylet уже запускался, он блокируется;
- `RepeatableWithCooldown`: storylet нельзя повторить, пока не прошёл cooldown;
- `RepeatableUntilStateChanges`: storylet нельзя повторять на том же world fingerprint.

### `Advance(...)`

После применения storylet-а строится новое `StoryletPlannerMemory`, в которое записываются:

- шаг последнего выполнения storylet-а;
- количество выполнений;
- world fingerprint после storylet-а;
- история role/entity pairings.

Это уже planner-time temporal logic, которой старый matcher не имел.

---

## 10. Salience и anti-repetition

Сервис:

- `StoryletSalienceEvaluator`

Политика задаётся через:

- `StoryletSaliencePolicy`

В ней есть:

- `BaseWeight`
- `UnlockBonus`
- `RecentRepeatPenalty`
- `RepeatedPairPenalty`

Смысл:

- `salience` моделирует не “логическую выполнимость”, а **желательность surfacing-а**;
- это слой narrative/authoring bias поверх механической feasibility.

Что делает evaluator:

- добавляет bonus storylet-у;
- снимает penalty, если storylet недавно уже выполнялся;
- снимает penalty, если те же entity-role pairings недавно уже использовались.

Это первый встроенный anti-noise механизм planner-а.

---

## 11. Scoring: как planner сравнивает ветки

Сервис:

- `StoryletScoringService`

Текущая score-модель разбита на 4 части.

### 11.1. Instantiation Quality

Это локальное качество конкретного assignment-а storylet-а.

Источники:

- fit ролей;
- relation compatibility;
- влияние priority на локальную привлекательность.

### 11.2. Transition Value

Сейчас в коде это примерно:

- `priority * 6`
- `salience bonus * 3`
- `+ effect count`

То есть storylet получает ценность за:

- драматическую/авторскую важность;
- salience policy;
- величину world mutation.

### 11.3. Future Potential

Сейчас считается просто как:

- `futureCandidateCount * 3`

То есть planner вознаграждает такие переходы, которые оставляют после себя больше доступных будущих ходов.

Это уже ближе к planner-логике, чем к старому greedy matcher-у.

### 11.4. Anti-Repetition

Берётся из salience evaluator-а как штраф.

### Важное замечание

Текущий scoring уже рабочий, но всё ещё эвристический и authoring-sensitive.

Это значит:

- инфраструктура scoring есть;
- explainable breakdown есть;
- но веса пока не являются “окончательной драматургической истиной”.

Именно поэтому последние итерации smoke-run были уже mostly tuning, а не фиксами архитектуры.

---

## 12. Beam Search: как устроен top-level planner

Главный сервис:

- `BeamStoryletPlanner`

Конфигурация:

- `beamWidth`
- `maxDepth`

Сейчас они задаются из installer-а:

- `beamWidth: 3`
- `maxDepth: 6`

### 12.1. Структура ветки

Внутренний private тип:

- `PlannerBranch`

Он хранит:

- `BranchId`
- `WorldState`
- `Memory`
- `Steps`
- `TotalScore`

То есть beam search работает не с “одним текущим миром”, а с несколькими параллельными гипотетическими мирами.

### 12.2. Основной цикл planner-а

`Plan(...)` делает следующее:

1. Стартует с одной ветки:
   - `branchId = root`
   - `initialWorldState`
   - `initialMemory` или `StoryletPlannerMemory.Empty`
   - `steps = []`
   - `totalScore = 0`

2. На каждом depth:
   - расширяет все активные ветки;
   - собирает valid candidates для каждой ветки;
   - для каждого candidate симулирует `nextWorldState`;
   - обновляет memory;
   - смотрит, сколько кандидатов открыто дальше;
   - считает `ScoreBreakdown`;
   - создаёт новую ветку.

3. Дедуплицирует ветки по:

```text
worldState.GetFingerprint() + "|" + memory.GetFingerprint()
```

4. Сортирует expanded branches по `TotalScore`.

5. Оставляет top `beamWidth`.

6. Логирует:
   - expanded branches
   - kept branches
   - pruned branches

7. Повторяет до `maxDepth` или пока расширения не закончились.

8. Выбирает ветку с максимальным accumulated `TotalScore`.

### 12.3. Почему это уже planner, а не greedy

Потому что planner:

- не коммитится сразу в один локально лучший следующий шаг;
- хранит несколько конкурентных будущих миров;
- сравнивает не один move, а accumulated branch score;
- умеет ветвить и prune-ить.

Даже с небольшим `beamWidth = 3` это уже принципиально другой класс поведения, чем старый top-level greedy.

---

## 13. Trace и explainability

Новые trace-типы:

- `StoryletPlannerTrace`
- `StoryletPlannerStepTrace`
- `StoryletBeamExpansionTrace`
- `StoryletBeamBranchTrace`
- `StoryletCandidateTrace`

### Что хранит trace

По шагам winning branch:

- `StepNumber`
- `CurrentSnapshotId`
- список кандидатов;
- статус кандидата;
- assignment summary;
- score breakdown;
- reason invalidation;
- surviving beam branches;
- выбранный шаг winning branch.

По каждому depth beam search:

- какие ветки были expanded;
- какие kept;
- какие pruned.

### Как строится trace

После завершения beam search planner:

1. Берёт winning branch.
2. Идёт по её шагам.
3. На каждом шаге заново reconstruct-ит candidate list для соответствующего `beforeWorldState`.
4. Помечает candidate как:
   - `invalid`
   - `valid_not_selected`
   - `winning_branch_step`

Это означает:

- trace не является сырым runtime log-ом каждого внутреннего вызова;
- это explainable reconstruction вокруг итоговой победившей ветки.

### Formatter

`StoryletTelemetryFormatter` превращает `StoryletPlannerResult` в человекочитаемый лог для `Debug.Log`.

Он выводит:

- шаги winning branch;
- кандидатов на шаге;
- surviving branches;
- score breakdown;
- applied effects;
- next snapshot id;
- beam expansions и pruning reasons.

Это закрывает требование из docs про explainable smoke output.

---

## 14. Smoke scenario: как собран демонстрационный world

Entry point:

- `StoryletMatcherSmokeTestService`

Несмотря на legacy-имя, по смыслу это уже **planner smoke runner**.

Что он делает:

1. Строит стартовый frontier-settlement world:
   - village elder
   - young knight
   - bandit scout
   - temple priest
   - wandering mage
   - militia guard
   - refugee child
   - settlement square

2. Строит relations между ними:
   - trust
   - vassal
   - enemy
   - caretaker

3. Строит 7 storylet definitions:
   - `storylet.border_warning`
   - `storylet.call_to_arms`
   - `storylet.arcane_ritual`
   - `storylet.bandit_raid`
   - `storylet.heal_the_wounded`
   - `storylet.search_for_missing_child`
   - `storylet.banish_the_raider`

4. Вызывает:

```csharp
_planner.Plan(BuildInitialWorldState(), BuildStorylets(), StoryletPlannerMemory.Empty)
```

5. Форматирует result и пишет его в `Debug.Log`.

### Почему smoke runner важен

Он проверяет сразу несколько свойств системы:

- relation-aware role binding;
- precondition gating;
- effect application;
- world mutation;
- branch competition;
- repeatability;
- anti-repetition;
- trace explainability.

То есть это не unit test, а системный smoke для всей planner pipeline.

---

## 15. DI и composition root

Composition root:

- `StoryletFeatureInstallerSO`

Через VContainer регистрируются:

- `AttributePreferenceEntityRoleFitEvaluator`
- `GreedyStoryletAssignmentBuilder`
- `StoryletInstantiationService`
- `StoryletEffectApplier`
- `StoryletRepeatabilityService`
- `StoryletSalienceEvaluator`
- `StoryletScoringService`
- `BeamStoryletPlanner`
- `StoryletTelemetryFormatter`
- `StoryletMatcherSmokeTestService` как `RegisterEntryPoint`

### Архитектурно важный момент

Это соответствует target-форме из docs:

- orchestration живёт не в `MonoBehaviour.Start`;
- plain C# services собраны в DI;
- smoke runner запускается как entry point.

То есть фича уже собрана в VContainer-native форме, а не как ad hoc scene script.

---

## 16. Что именно осталось от старой системы

Старые части, которые по-прежнему важны:

- `Entity`
- `EntityRelation`
- `Role`
- `RelationRequirement`
- `TagQuery`
- `AttributeRequirement`
- `Storylet`
- `StoryletMatchingContext`
- `GreedyStoryletAssignmentBuilder`
- `AttributePreferenceEntityRoleFitEvaluator`

Их новая роль:

- быть фундаментом local matching;
- не решать top-level sequencing storylet-ов;
- не быть главным representation evolving world-а.

Особенно важно понимать про старый `GreedyStoryletMatcher`:

- он сохранён как reference/legacy algorithm;
- теперь он физически лежит в `Legacy/Matching`;
- но **не используется как top-level planner**;
- финальная v1-архитектура не зависит от него как от orchestration-слоя.

---

## 17. Что уже реализовано из основных docs

Ниже список того, что в рамках v1 уже присутствует.

### Реализовано

- явный `StoryletWorldState`
- `StoryletDefinition` как planner-facing storylet model
- декларативные `Preconditions`
- декларативные `Effects`
- planner memory
- repeatability policies
- salience / anti-repetition слой
- explainable score breakdown
- `BeamStoryletPlanner`
- branch dedup по world + memory fingerprint
- explainable trace
- smoke scenario с несколькими ветками
- DI-регистрация через VContainer
- smoke запуск как `RegisterEntryPoint`

### Частично реализовано

- scoring model
  - слой есть, но веса пока эвристические и ещё тюнятся
- smoke scenario alignment
  - сценарий работает, но его exact narrative preferences ещё настраиваются
- immutability model
  - логическая immutability есть
  - structural sharing / performance optimization пока нет

---

## 18. Что пока НЕ реализовано из целевой картины

### 18.1. Полноценный authoring layer

Сейчас storylet-ы в smoke runner-е создаются кодом.

Не сделано:

- ScriptableObject authoring for `StoryletDefinition`
- data-driven content pipeline
- editor tooling для authoring/debugging

### 18.2. Gameplay integration

Сейчас это автономный engine + smoke.

Не сделано:

- интеграция в боевой scene flow;
- интеграция в игровые runtime systems;
- запуск planner-а из настоящего gameplay loop;
- применение результатов planner-а к production narrative pipeline.

### 18.3. Автотесты

Система проверялась вручную через smoke-runner.

Не сделано:

- unit tests;
- snapshot tests для trace;
- deterministic regression suite для beam/scoring behavior.

### 18.4. Более богатая world model

Сейчас `WorldState` уже есть, но он ещё компактный.

Пока нет:

- более сложной derived-state модели;
- явной модели causal provenance effects;
- richer world markers beyond current attribute/tag representation.

### 18.5. Более продвинутый scoring/search

Есть рабочий beam-search planner, но ещё нет:

- domain-specific branch diversity bias;
- более глубокого future value estimation;
- richer author goals;
- adaptive beam width;
- performance optimizations для больших content sets.

### 18.6. Rename pass

Технически smoke runner всё ещё называется:

- `StoryletMatcherSmokeTestService`

По смыслу это уже planner smoke runner, но rename пока не делался.

---

## 19. Где сейчас проходят реальные границы между “баг” и “тюнинг”

Это полезно явно зафиксировать.

### Уже не выглядит как инфраструктурный баг

На текущем этапе уже работают:

- DI registration;
- planner execution;
- state transitions;
- repeatability;
- beam pruning;
- trace generation;
- smoke log output.

То есть базовая архитектура и алгоритмический pipeline уже не выглядят “сломанными”.

### То, что сейчас ещё может меняться

Сейчас в основном ещё тюнятся:

- salience policy values;
- storylet priorities;
- precondition gates в smoke-authoring;
- relative scoring weights;
- narrative preference между competing branches.

Это уже в первую очередь **authoring and heuristic tuning**, а не structural planner bug fixing.

---

## 20. Ментальная модель системы в одной схеме

Полезно думать о текущей системе так:

1. `StoryletDefinition`
   - описывает storylet как шаблон действия над миром

2. `StoryletInstantiationService`
   - отвечает: “можно ли сейчас этот storylet конкретно собрать?”

3. `GreedyStoryletAssignmentBuilder`
   - решает локальную задачу привязки ролей к сущностям

4. `StoryletEffectApplier`
   - симулирует последствия storylet-а

5. `StoryletRepeatabilityService`
   - не даёт planner-у зациклиться и повторяться без правил

6. `StoryletSalienceEvaluator`
   - добавляет narrative surfacing bias

7. `StoryletScoringService`
   - переводит candidate + transition + future в explainable score

8. `BeamStoryletPlanner`
   - ищет хорошую последовательность storylet-ов через несколько competing branches

9. `StoryletTelemetryFormatter`
   - делает поведение planner-а читаемым человеком

Это и есть текущая рабочая архитектура StoryletSystem v1.

---

## 21. Итог

В результате переработки `StoryletSystem` перестал быть просто relation-aware greedy matcher-ом и стал **двухуровневой системой**:

- снизу: relation-aware local assignment solver;
- сверху: planner по world transitions.

Главный результат не только в том, что появился beam search, а в том, что теперь есть явная модель:

- мира;
- изменений мира;
- памяти о прошлом;
- объяснимого выбора веток.

Это уже хороший фундамент для дальнейших шагов:

- более богатого authoring;
- production integration;
- стабилизации scoring;
- редакторских инструментов;
- regression testing.

Если коротко:

- **старый matching не выброшен**;
- **новый planning слой стал главным**;
- **v1 engine уже собран**;
- дальше основная работа смещается из “строить архитектуру” в “докручивать authoring, scoring и интеграцию”.
