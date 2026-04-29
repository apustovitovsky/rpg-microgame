using System.Collections.Generic;
using UnityEngine;

namespace Etheria.Features.Targeting
{
    public sealed class RaycastHitDistanceComparer : IComparer<RaycastHit>
    {
        public static readonly RaycastHitDistanceComparer Instance = new();

        public int Compare(RaycastHit x, RaycastHit y)
        {
            return x.distance.CompareTo(y.distance);
        }
    }
}