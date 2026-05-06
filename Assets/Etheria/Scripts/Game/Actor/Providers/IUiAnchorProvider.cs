using UnityEngine;

namespace Etheria.Game.Actor
{
    public interface IUiAnchorProvider
    {
        Transform UiAnchor { get; }
    }
}