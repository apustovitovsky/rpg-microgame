namespace Etheria.Features.StoryletSystem
{
    public sealed class AddEntityTagEffect : EntityTargetedEffect
    {
        public AddEntityTagEffect(RoleId roleId, TagSet tag) : base(roleId)
        {
            Tag = tag;
        }

        public TagSet Tag { get; }
    }
}
