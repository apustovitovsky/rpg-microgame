using System;
using UnityEngine;


namespace Etheria.Features.HWFC_Old
{
    public static class Orientations
    {
        public const int LEFT = 0;
        public const int DOWN = 1;
        public const int BACK = 2;
        public const int RIGHT = 3;
        public const int UP = 4;
        public const int FORWARD = 5;

        public static readonly int[] HorizontalDirections = { LEFT, BACK, RIGHT, FORWARD };

        private static readonly Vector3[] Vectors =
        {
            Vector3.left,
            Vector3.down,
            Vector3.back,
            Vector3.right,
            Vector3.up,
            Vector3.forward
        };

        public static readonly Vector3Int[] Direction =
        {
            Vector3Int.left,
            Vector3Int.down,
            Vector3Int.back,
            Vector3Int.right,
            Vector3Int.up,
            Vector3Int.forward
        };

        public static readonly Quaternion[] Rotations =
        {
            Quaternion.LookRotation(Vectors[LEFT]),
            Quaternion.LookRotation(Vectors[DOWN]),
            Quaternion.LookRotation(Vectors[BACK]),
            Quaternion.LookRotation(Vectors[RIGHT]),
            Quaternion.LookRotation(Vectors[UP]),
            Quaternion.LookRotation(Vectors[FORWARD])
        };

        public static int Rotate(int direction, int amount)
        {
            if (direction == DOWN || direction == UP)
            {
                return direction;
            }

            return HorizontalDirections[(Array.IndexOf(HorizontalDirections, direction) + amount) % 4];
        }

        public static bool IsHorizontal(int orientation)
        {
            return orientation != DOWN && orientation != UP;
        }
    }
}
