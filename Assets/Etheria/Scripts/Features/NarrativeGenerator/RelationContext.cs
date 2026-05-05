namespace Etheria.Features.NarrativeGeneration
{
    public struct RelationContext
    {
        public EntityId Slot0;
        public EntityId Slot1;
        public EntityId Slot2;
        public EntityId Slot3;
        public EntityId Slot4;
        public EntityId Slot5;
        public EntityId Slot6;
        public EntityId Slot7;

        public void Initialize()
        {
            Slot0 = EntityId.Invalid;
            Slot1 = EntityId.Invalid;
            Slot2 = EntityId.Invalid;
            Slot3 = EntityId.Invalid;
            Slot4 = EntityId.Invalid;
            Slot5 = EntityId.Invalid;
            Slot6 = EntityId.Invalid;
            Slot7 = EntityId.Invalid;
        }

        public void SetSlot(byte slotIndex, EntityId value)
        {
            switch (slotIndex)
            {
                case 0: Slot0 = value; break;
                case 1: Slot1 = value; break;
                case 2: Slot2 = value; break;
                case 3: Slot3 = value; break;
                case 4: Slot4 = value; break;
                case 5: Slot5 = value; break;
                case 6: Slot6 = value; break;
                case 7: Slot7 = value; break;
            }
        }

        public readonly EntityId GetSlot(byte slotIndex)
        {
            return slotIndex switch
            {
                0 => Slot0,
                1 => Slot1,
                2 => Slot2,
                3 => Slot3,
                4 => Slot4,
                5 => Slot5,
                6 => Slot6,
                7 => Slot7,
                _ => EntityId.Invalid
            };
        }
    }
}
