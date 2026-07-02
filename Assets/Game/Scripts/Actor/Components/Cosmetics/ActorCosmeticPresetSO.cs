using UnityEngine;

namespace Game.Actor
{
    [CreateAssetMenu(
        fileName = "ActorCosmeticPreset",
        menuName = "Game/Actor/Actor Cosmetic Preset")]
    public sealed class ActorCosmeticPresetSO : ScriptableObject
    {
        [field: SerializeField] public bool Sword { get; private set; }
        [field: SerializeField] public bool Dagger { get; private set; }
        [field: SerializeField] public bool Axe { get; private set; }
        [field: SerializeField] public bool Shield { get; private set; }
        [field: SerializeField] public bool CoinBag { get; private set; }
        [field: SerializeField] public bool Horn { get; private set; }
        [field: SerializeField] public bool Pouch { get; private set; }
        [field: SerializeField] public bool SwordHolder { get; private set; }
        [field: SerializeField] public bool SwordSheath { get; private set; }
        [field: SerializeField] public bool ShoulderArmorLeft { get; private set; }
        [field: SerializeField] public bool ShoulderArmorRight { get; private set; }
        [field: SerializeField] public bool WaterBladder { get; private set; }
        [field: SerializeField] public bool WaterCanteen { get; private set; }

        public bool IsVisible(ActorCosmeticSlot slot)
        {
            return slot switch
            {
                ActorCosmeticSlot.Sword => Sword,
                ActorCosmeticSlot.Dagger => Dagger,
                ActorCosmeticSlot.Axe => Axe,
                ActorCosmeticSlot.Shield => Shield,
                ActorCosmeticSlot.CoinBag => CoinBag,
                ActorCosmeticSlot.Horn => Horn,
                ActorCosmeticSlot.Pouch => Pouch,
                ActorCosmeticSlot.SwordHolder => SwordHolder,
                ActorCosmeticSlot.SwordSheath => SwordSheath,
                ActorCosmeticSlot.ShoulderArmorLeft => ShoulderArmorLeft,
                ActorCosmeticSlot.ShoulderArmorRight => ShoulderArmorRight,
                ActorCosmeticSlot.WaterBladder => WaterBladder,
                ActorCosmeticSlot.WaterCanteen => WaterCanteen,
                _ => false
            };
        }
    }
}