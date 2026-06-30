using System;
using Etheria.Game.World;

namespace Etheria.Navigation
{
    public sealed class NavigationGraphProvider :
        INavigationGraphProvider
    {
        public NavigationGraph Graph { get; }

        public NavigationGraphProvider(
            NavigationGraph graph)
        {
            Graph = graph ?? throw new ArgumentNullException(nameof(graph));
        }
    }
}