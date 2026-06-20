using UnityEngine;

namespace Etheria.Game.UI
{
    public interface ICharacterLabelPoolRoots
    {
        RectTransform ActiveRoot { get; }
        RectTransform InactiveRoot { get; }
    }
}