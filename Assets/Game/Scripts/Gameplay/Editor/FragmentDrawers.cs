#if UNITY_EDITOR

using Game.Actor;
using Game.Item;
using Game.Pickup;
using UnityEditor;

namespace Game.Editor
{
    [CustomPropertyDrawer(
        typeof(ActorFragment),
        useForChildren: true)]
    public sealed class ActorFragmentDrawer :
        ManagedReferenceFragmentDrawer<ActorFragment>
    {
    }

    [CustomPropertyDrawer(
        typeof(PickupFragment),
        useForChildren: true)]
    public sealed class PickupFragmentDrawer :
        ManagedReferenceFragmentDrawer<PickupFragment>
    {
    }

    [CustomPropertyDrawer(
        typeof(ItemFragment),
        useForChildren: true)]
    public sealed class ItemFragmentDrawer :
        ManagedReferenceFragmentDrawer<ItemFragment>
    {
    }
}

#endif