using System;

namespace Game.Navigation
{
    public sealed class NavigationGraphProvider :
        INavigationGraphProvider
    {
        public NavigationGraphProvider(
            NavigationGraph graph)
        {
            Graph = graph
                ?? throw new ArgumentNullException(nameof(graph));
        }

        public NavigationGraph Graph { get; }
    }
}