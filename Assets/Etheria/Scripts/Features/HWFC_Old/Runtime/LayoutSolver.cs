using System;
using System.Collections.Generic;


namespace Etheria.Features.HWFC_Old
{
    public sealed class LayoutSolver
    {
        public LayoutGenerationResult Solve(LayoutGenerationRequest request)
        {
            var data = request.ModuleSetData;
            var validationError = Validate(request, data);
            if (validationError != null)
            {
                return CreateInvalidResult(request);
            }

            var cellCount = request.Width * request.Height * request.Depth;
            var moduleCount = data.ModuleCount;
            var domains = new ModuleSet[cellCount];
            var propagationQueue = new Queue<int>(cellCount);
            var random = new Random(request.Seed);
            var fullSet = ModuleSet.Filled(moduleCount);

            for (var cellIndex = 0; cellIndex < cellCount; cellIndex++)
            {
                domains[cellIndex] = fullSet;
            }

            while (true)
            {
                var cellToCollapse = SelectCellWithLowestEntropy(domains, data.Probabilities, random);
                if (cellToCollapse < 0)
                {
                    return CreateResult(request, LayoutGenerationStatus.Success, -1, domains);
                }

                var collapsedModule = SelectCollapsedModule(domains[cellToCollapse], data.Probabilities, random);
                domains[cellToCollapse] = ModuleSet.Single(moduleCount, collapsedModule);
                propagationQueue.Enqueue(cellToCollapse);

                if (!Propagate(request, data, domains, propagationQueue, out var contradictionCellIndex))
                {
                    return CreateResult(request, LayoutGenerationStatus.Contradiction, contradictionCellIndex, domains);
                }
            }
        }

        private static string Validate(LayoutGenerationRequest request, ModuleSetData data)
        {
            if (data == null)
            {
                return "ModuleSetData is null.";
            }

            if (request.Width <= 0 || request.Height <= 0 || request.Depth <= 0)
            {
                return "Volume dimensions must be positive.";
            }

            if (data.ModuleCount <= 0)
            {
                return "ModuleSetData must contain at least one module.";
            }

            if (data.DirectionCount != 6)
            {
                return "ModuleSetData must expose six directions.";
            }

            if (data.Variants == null || data.Variants.Length != data.ModuleCount)
            {
                return "Module variant count is inconsistent.";
            }

            if (data.Constraints == null || data.Constraints.Length != data.ModuleCount * data.DirectionCount)
            {
                return "Constraint count is inconsistent.";
            }

            if (data.Probabilities == null || data.Probabilities.Length != data.ModuleCount)
            {
                return "Probability count is inconsistent.";
            }

            for (var moduleIndex = 0; moduleIndex < data.ModuleCount; moduleIndex++)
            {
                if (data.Probabilities[moduleIndex] <= 0f)
                {
                    return "All module probabilities must be positive.";
                }
            }

            return null;
        }

        private static LayoutGenerationResult CreateInvalidResult(LayoutGenerationRequest request)
        {
            return new LayoutGenerationResult
            {
                Status = LayoutGenerationStatus.InvalidInput,
                Width = request.Width,
                Height = request.Height,
                Depth = request.Depth,
                Seed = request.Seed,
                ContradictionCellIndex = -1,
                CollapsedVariantIndices = Array.Empty<int>()
            };
        }

        private static LayoutGenerationResult CreateResult(
            LayoutGenerationRequest request,
            LayoutGenerationStatus status,
            int contradictionCellIndex,
            ModuleSet[] domains)
        {
            var collapsedVariantIndices = new int[domains.Length];

            for (var cellIndex = 0; cellIndex < domains.Length; cellIndex++)
            {
                collapsedVariantIndices[cellIndex] = TryGetSingleModuleIndex(domains[cellIndex]);
            }

            return new LayoutGenerationResult
            {
                Status = status,
                Width = request.Width,
                Height = request.Height,
                Depth = request.Depth,
                Seed = request.Seed,
                ContradictionCellIndex = contradictionCellIndex,
                CollapsedVariantIndices = collapsedVariantIndices
            };
        }

        private static int SelectCellWithLowestEntropy(ModuleSet[] domains, float[] probabilities, Random random)
        {
            var selectedCellIndex = -1;
            var bestEntropy = double.MaxValue;

            for (var cellIndex = 0; cellIndex < domains.Length; cellIndex++)
            {
                var domain = domains[cellIndex];
                var count = domain.CountBits();
                if (count <= 1)
                {
                    continue;
                }

                var entropy = CalculateShannonEntropy(domain, probabilities);
                if (entropy < bestEntropy)
                {
                    bestEntropy = entropy;
                    selectedCellIndex = cellIndex;
                    continue;
                }

                if (Math.Abs(entropy - bestEntropy) < 0.000001d && selectedCellIndex >= 0 && random.Next(0, 2) == 0)
                {
                    selectedCellIndex = cellIndex;
                }
            }

            return selectedCellIndex;
        }

        private static double CalculateShannonEntropy(ModuleSet domain, float[] probabilities)
        {
            double totalWeight = 0d;
            double totalWeightLogWeight = 0d;

            for (var moduleIndex = 0; moduleIndex < domain.ModuleCount; moduleIndex++)
            {
                if (!domain.Contains(moduleIndex))
                {
                    continue;
                }

                var weight = probabilities[moduleIndex];
                totalWeight += weight;
                totalWeightLogWeight += weight * Math.Log(weight);
            }

            if (totalWeight <= 0d)
            {
                return 0d;
            }

            return Math.Log(totalWeight) - (totalWeightLogWeight / totalWeight);
        }

        private static int SelectCollapsedModule(ModuleSet domain, float[] probabilities, Random random)
        {
            double totalWeight = 0d;

            for (var moduleIndex = 0; moduleIndex < domain.ModuleCount; moduleIndex++)
            {
                if (domain.Contains(moduleIndex))
                {
                    totalWeight += probabilities[moduleIndex];
                }
            }

            var threshold = random.NextDouble() * totalWeight;
            double cumulative = 0d;

            for (var moduleIndex = 0; moduleIndex < domain.ModuleCount; moduleIndex++)
            {
                if (!domain.Contains(moduleIndex))
                {
                    continue;
                }

                cumulative += probabilities[moduleIndex];
                if (threshold <= cumulative)
                {
                    return moduleIndex;
                }
            }

            for (var moduleIndex = domain.ModuleCount - 1; moduleIndex >= 0; moduleIndex--)
            {
                if (domain.Contains(moduleIndex))
                {
                    return moduleIndex;
                }
            }

            throw new InvalidOperationException("Cannot collapse an empty domain.");
        }

        private static bool Propagate(
            LayoutGenerationRequest request,
            ModuleSetData data,
            ModuleSet[] domains,
            Queue<int> propagationQueue,
            out int contradictionCellIndex)
        {
            contradictionCellIndex = -1;

            while (propagationQueue.Count > 0)
            {
                var sourceIndex = propagationQueue.Dequeue();
                var sourceDomain = domains[sourceIndex];

                for (var direction = 0; direction < data.DirectionCount; direction++)
                {
                    if (!TryGetNeighborIndex(sourceIndex, direction, request.Width, request.Height, request.Depth, out var neighborIndex))
                    {
                        continue;
                    }

                    var allowedNeighborModules = CollectAllowedNeighbors(sourceDomain, data, direction);
                    var previousNeighborDomain = domains[neighborIndex];
                    var updatedNeighborDomain = previousNeighborDomain;
                    updatedNeighborDomain.IntersectWith(allowedNeighborModules);

                    if (updatedNeighborDomain == previousNeighborDomain)
                    {
                        continue;
                    }

                    if (updatedNeighborDomain.IsEmpty)
                    {
                        contradictionCellIndex = neighborIndex;
                        return false;
                    }

                    domains[neighborIndex] = updatedNeighborDomain;
                    propagationQueue.Enqueue(neighborIndex);
                }
            }

            return true;
        }

        private static ModuleSet CollectAllowedNeighbors(ModuleSet sourceDomain, ModuleSetData data, int direction)
        {
            var allowed = ModuleSet.Empty(data.ModuleCount);

            for (var moduleIndex = 0; moduleIndex < sourceDomain.ModuleCount; moduleIndex++)
            {
                if (!sourceDomain.Contains(moduleIndex))
                {
                    continue;
                }

                allowed.UnionWith(data.GetConstraint(moduleIndex, direction));
            }

            return allowed;
        }

        private static bool TryGetNeighborIndex(
            int sourceIndex,
            int direction,
            int width,
            int height,
            int depth,
            out int neighborIndex)
        {
            neighborIndex = -1;

            var x = sourceIndex % width;
            var yz = sourceIndex / width;
            var y = yz / depth;
            var z = yz % depth;

            switch (direction)
            {
                case Orientations.LEFT:
                    if (x <= 0)
                    {
                        return false;
                    }

                    x--;
                    break;
                case Orientations.RIGHT:
                    if (x >= width - 1)
                    {
                        return false;
                    }

                    x++;
                    break;
                case Orientations.DOWN:
                    if (y <= 0)
                    {
                        return false;
                    }

                    y--;
                    break;
                case Orientations.UP:
                    if (y >= height - 1)
                    {
                        return false;
                    }

                    y++;
                    break;
                case Orientations.BACK:
                    if (z <= 0)
                    {
                        return false;
                    }

                    z--;
                    break;
                case Orientations.FORWARD:
                    if (z >= depth - 1)
                    {
                        return false;
                    }

                    z++;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction));
            }

            neighborIndex = x + width * (z + depth * y);
            return true;
        }

        private static int TryGetSingleModuleIndex(ModuleSet domain)
        {
            if (domain.CountBits() != 1)
            {
                return -1;
            }

            for (var moduleIndex = 0; moduleIndex < domain.ModuleCount; moduleIndex++)
            {
                if (domain.Contains(moduleIndex))
                {
                    return moduleIndex;
                }
            }

            return -1;
        }

    }
}
