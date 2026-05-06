namespace Etheria.Features.NarrativeGeneration
{
    public struct RelationStep
    {
        public byte SourceSlot;
        public RelationDirection Direction;
        public TagQuery RelationTagQuery;
        public TagQuery TargetEntityTagQuery;
        public byte BindTargetSlot;
    }
}
