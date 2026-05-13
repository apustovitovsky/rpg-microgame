using System;


namespace Etheria.Features.HWFC_Old
{
    public static class LayoutSemanticRules
    {
        private static readonly int[] CornerConnectors =
        {
            (int)SemanticConnectorType.CornerL,
            (int)SemanticConnectorType.CornerD,
            (int)SemanticConnectorType.CornerR,
            (int)SemanticConnectorType.CornerF,
            (int)SemanticConnectorType.CornerU,
            (int)SemanticConnectorType.CornerF
        };

        private static readonly int[] SideConnectors =
        {
            (int)SemanticConnectorType.SideL,
            (int)SemanticConnectorType.SideD,
            (int)SemanticConnectorType.SideB,
            (int)SemanticConnectorType.SideR,
            (int)SemanticConnectorType.SideU,
            (int)SemanticConnectorType.SideF
        };

        private static readonly int[] CenterConnectors =
        {
            (int)SemanticConnectorType.Center,
            (int)SemanticConnectorType.CenterD,
            (int)SemanticConnectorType.Center,
            (int)SemanticConnectorType.Center,
            (int)SemanticConnectorType.CenterU,
            (int)SemanticConnectorType.Center
        };

        private static readonly int[] SpaceConnectors =
        {
            (int)SemanticConnectorType.SpaceL,
            (int)SemanticConnectorType.SpaceD,
            (int)SemanticConnectorType.SpaceB,
            (int)SemanticConnectorType.SpaceR,
            (int)SemanticConnectorType.SpaceU,
            (int)SemanticConnectorType.SpaceF
        };

        private static readonly int[] AirConnectors =
        {
            (int)SemanticConnectorType.Air,
            (int)SemanticConnectorType.Air,
            (int)SemanticConnectorType.Air,
            (int)SemanticConnectorType.Air,
            (int)SemanticConnectorType.Air,
            (int)SemanticConnectorType.Air
        };

        public static int[] GetBaseConnectors(LayoutSemanticFamily family)
        {
            switch (family)
            {
                case LayoutSemanticFamily.Corner:
                    return (int[])CornerConnectors.Clone();
                case LayoutSemanticFamily.Side:
                    return (int[])SideConnectors.Clone();
                case LayoutSemanticFamily.Center:
                    return (int[])CenterConnectors.Clone();
                case LayoutSemanticFamily.Space:
                    return (int[])SpaceConnectors.Clone();
                case LayoutSemanticFamily.Air:
                    return (int[])AirConnectors.Clone();
                default:
                    throw new ArgumentOutOfRangeException(nameof(family));
            }
        }

        public static int[] RotateY(int[] connectors)
        {
            if (connectors == null || connectors.Length != 6)
            {
                throw new ArgumentException("Expected six connectors.", nameof(connectors));
            }

            return new[]
            {
                connectors[Orientations.BACK],
                connectors[Orientations.DOWN],
                connectors[Orientations.RIGHT],
                connectors[Orientations.FORWARD],
                connectors[Orientations.UP],
                connectors[Orientations.LEFT]
            };
        }

        public static int[] GetRotatedConnectors(LayoutSemanticFamily family, int rotation)
        {
            if (rotation < 0 || rotation > 3)
            {
                throw new ArgumentOutOfRangeException(nameof(rotation));
            }

            var connectors = GetBaseConnectors(family);
            for (var i = 0; i < rotation; i++)
            {
                connectors = RotateY(connectors);
            }

            return connectors;
        }

        public static bool CanConnect(int sourceConnector, int targetConnector)
        {
            var source = (SemanticConnectorType)sourceConnector;
            var target = (SemanticConnectorType)targetConnector;

            return source switch
            {
                SemanticConnectorType.CornerF =>
                    target == SemanticConnectorType.SpaceB,

                SemanticConnectorType.CornerL =>
                    target == SemanticConnectorType.CornerR ||
                    target == SemanticConnectorType.SideR,

                SemanticConnectorType.CornerR =>
                    target == SemanticConnectorType.CornerL ||
                    target == SemanticConnectorType.SideL,

                SemanticConnectorType.CornerU =>
                    target == SemanticConnectorType.CornerD ||
                    target == SemanticConnectorType.SpaceD ||
                    target == SemanticConnectorType.Air,

                SemanticConnectorType.CornerD =>
                    target == SemanticConnectorType.CornerU ||
                    target == SemanticConnectorType.CenterU ||
                    target == SemanticConnectorType.SideU,

                SemanticConnectorType.SideF =>
                    target == SemanticConnectorType.SpaceB,

                SemanticConnectorType.SideB =>
                    target == SemanticConnectorType.Center,

                SemanticConnectorType.SideL =>
                    target == SemanticConnectorType.SideR ||
                    target == SemanticConnectorType.CornerR,

                SemanticConnectorType.SideR =>
                    target == SemanticConnectorType.SideL ||
                    target == SemanticConnectorType.CornerL,

                SemanticConnectorType.SideU =>
                    target == SemanticConnectorType.SideD ||
                    target == SemanticConnectorType.SpaceD ||
                    target == SemanticConnectorType.Air ||
                    target == SemanticConnectorType.CornerD,

                SemanticConnectorType.SideD =>
                    target == SemanticConnectorType.SideU ||
                    target == SemanticConnectorType.CenterU,

                SemanticConnectorType.Center =>
                    target == SemanticConnectorType.Center ||
                    target == SemanticConnectorType.SideB,

                SemanticConnectorType.CenterU =>
                    target == SemanticConnectorType.CenterD ||
                    target == SemanticConnectorType.SpaceD ||
                    target == SemanticConnectorType.Air ||
                    target == SemanticConnectorType.CornerD ||
                    target == SemanticConnectorType.SideD,

                SemanticConnectorType.CenterD =>
                    target == SemanticConnectorType.CenterU,

                SemanticConnectorType.SpaceF =>
                    target == SemanticConnectorType.Air,

                SemanticConnectorType.SpaceB =>
                    target == SemanticConnectorType.CornerF ||
                    target == SemanticConnectorType.SideF,

                SemanticConnectorType.SpaceR =>
                    target == SemanticConnectorType.SpaceL ||
                    target == SemanticConnectorType.Air,

                SemanticConnectorType.SpaceL =>
                    target == SemanticConnectorType.SpaceR ||
                    target == SemanticConnectorType.Air,

                SemanticConnectorType.SpaceU =>
                    target == SemanticConnectorType.SpaceD ||
                    target == SemanticConnectorType.Air,

                SemanticConnectorType.SpaceD =>
                    target == SemanticConnectorType.SpaceU ||
                    target == SemanticConnectorType.CornerU ||
                    target == SemanticConnectorType.SideU ||
                    target == SemanticConnectorType.CenterU,

                SemanticConnectorType.Air =>
                    target == SemanticConnectorType.SpaceL ||
                    target == SemanticConnectorType.SpaceR ||
                    target == SemanticConnectorType.SpaceF ||
                    target == SemanticConnectorType.Air ||
                    target == SemanticConnectorType.CenterU ||
                    target == SemanticConnectorType.SideU ||
                    target == SemanticConnectorType.CornerU ||
                    target == SemanticConnectorType.SpaceU,

                _ => throw new ArgumentOutOfRangeException(nameof(sourceConnector)),
            };

        }

    }
}
