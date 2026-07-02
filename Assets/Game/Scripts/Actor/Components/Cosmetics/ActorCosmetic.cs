using System;
using UnityEngine;

namespace Game.Actor
{
    public enum ActorCosmeticSlot
    {
        Sword,
        Dagger,
        Axe,
        Shield,
        CoinBag,
        Horn,
        Pouch,
        SwordHolder,
        SwordSheath,
        ShoulderArmorLeft,
        ShoulderArmorRight,
        WaterBladder,
        WaterCanteen
    }

    public sealed class ActorCosmetic : MonoBehaviour
    {
        [Serializable]
        private sealed class Item
        {
            [SerializeField] private ActorCosmeticSlot _slot;
            [SerializeField] private GameObject _object;
            [SerializeField] private bool _visible;

            public void Apply(ActorCosmeticPresetSO preset)
            {
                bool visible = preset != null
                    ? preset.IsVisible(_slot)
                    : _visible;

                if (_object != null)
                    _object.SetActive(visible);
            }
        }

        [SerializeField] private ActorCosmeticPresetSO _preset;
        [SerializeField] private Item[] _cosmetics;

        private void Awake()
        {
            Apply();
        }

        [ContextMenu("Apply Appearance")]
        private void Apply()
        {
            if (_cosmetics == null)
                return;

            foreach (Item item in _cosmetics)
                item?.Apply(_preset);
        }
    }
}