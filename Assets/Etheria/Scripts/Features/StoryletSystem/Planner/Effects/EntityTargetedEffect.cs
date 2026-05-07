namespace Etheria.Features.StoryletSystem
{
    public abstract class EntityTargetedEffect : StoryletEffect
    {
        protected EntityTargetedEffect(RoleId roleId)
        {
            RoleId = roleId;
        }

        public RoleId RoleId { get; }
    }
}
