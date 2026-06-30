using UnityEngine;

namespace Etheria.Game.UI
{
    public interface INpcNameLabelPoolRoots
    {
        RectTransform ActiveRoot { get; }
        RectTransform InactiveRoot { get; }
    }
}