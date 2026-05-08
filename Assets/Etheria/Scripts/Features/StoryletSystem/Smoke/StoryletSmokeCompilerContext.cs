using System;
using System.Collections.Generic;

namespace Etheria.Features.StoryletSystem
{
    public sealed class StoryletSmokeCompilerContext
    {
        private readonly OrderedSymbolRegistry<TagSymbol> _tags;
        private readonly OrderedSymbolRegistry<AttributeSymbol> _attributes;
        private readonly OrderedSymbolRegistry<EntitySymbol> _entities;
        private readonly OrderedSymbolRegistry<RoleSymbol> _roles;
        private readonly OrderedSymbolRegistry<StoryletSymbol> _storylets;

        internal StoryletSmokeCompilerContext(
            OrderedSymbolRegistry<TagSymbol> tags,
            OrderedSymbolRegistry<AttributeSymbol> attributes,
            OrderedSymbolRegistry<EntitySymbol> entities,
            OrderedSymbolRegistry<RoleSymbol> roles,
            OrderedSymbolRegistry<StoryletSymbol> storylets)
        {
            _tags = tags ?? throw new ArgumentNullException(nameof(tags));
            _attributes = attributes ?? throw new ArgumentNullException(nameof(attributes));
            _entities = entities ?? throw new ArgumentNullException(nameof(entities));
            _roles = roles ?? throw new ArgumentNullException(nameof(roles));
            _storylets = storylets ?? throw new ArgumentNullException(nameof(storylets));
        }

        public StoryletId StoryletId(StoryletSymbol symbol) => new(_storylets.Resolve(symbol));
        public RoleId RoleId(RoleSymbol symbol) => new(_roles.Resolve(symbol));
        public EntityId EntityId(EntitySymbol symbol) => new(_entities.Resolve(symbol));
        public AttributeId AttributeId(AttributeSymbol symbol) => new(_attributes.Resolve(symbol));

        public TagSet Tags(params TagSymbol[] symbols)
        {
            var tagSet = TagSet.Empty;

            if (symbols == null)
            {
                return tagSet;
            }

            for (var i = 0; i < symbols.Length; i++)
            {
                tagSet |= TagSet.FromIndex(_tags.Resolve(symbols[i]));
            }

            return tagSet;
        }

        public TagQuery Query(
            IReadOnlyList<TagSymbol> allOf = null,
            IReadOnlyList<TagSymbol> anyOf = null,
            IReadOnlyList<TagSymbol> noneOf = null)
        {
            return new TagQuery(
                TagsFromList(allOf),
                TagsFromList(anyOf),
                TagsFromList(noneOf));
        }

        public AttributeSet Attributes(params (AttributeSymbol Symbol, float Value)[] pairs)
        {
            if (pairs == null || pairs.Length == 0)
            {
                return default;
            }

            var values = new float[TagSet.Capacity];
            var maxIndex = -1;

            for (var i = 0; i < pairs.Length; i++)
            {
                var index = AttributeId(pairs[i].Symbol).Value;
                values[index] = pairs[i].Value;

                if (index > maxIndex)
                {
                    maxIndex = index;
                }
            }

            if (maxIndex < 0)
            {
                return default;
            }

            var compact = new float[maxIndex + 1];
            Array.Copy(values, compact, compact.Length);
            return new AttributeSet(compact);
        }

        public Entity Entity(
            EntitySymbol symbol,
            TagSet tags,
            params (AttributeSymbol Symbol, float Value)[] attributes)
        {
            return new Entity(
                EntityId(symbol),
                symbol.Key,
                tags,
                Attributes(attributes));
        }

        public EntityRelation Relation(EntitySymbol from, EntitySymbol to, TagSet tags)
        {
            return new EntityRelation(EntityId(from), EntityId(to), tags);
        }

        public Role Role(
            RoleSymbol symbol,
            TagQuery query,
            AttributeRequirement[] attributeRequirements = null,
            AttributePreference[] attributePreferences = null,
            RelationRequirement[] relationRequirements = null)
        {
            return new Role(
                RoleId(symbol),
                symbol.Key,
                query,
                attributeRequirements,
                attributePreferences,
                relationRequirements);
        }

        public RelationRequirement RelationRequirement(
            RoleSymbol targetRole,
            TagQuery relationQuery,
            RelationDirection direction)
        {
            return new RelationRequirement(RoleId(targetRole), relationQuery, direction);
        }

        public AttributeRequirement Min(AttributeSymbol symbol, float minValue)
        {
            return AttributeRequirement.Min(AttributeId(symbol), minValue);
        }

        public AttributeRequirement Max(AttributeSymbol symbol, float maxValue)
        {
            return AttributeRequirement.Max(AttributeId(symbol), maxValue);
        }

        public AttributePreference Preference(AttributeSymbol symbol, float weight, float start, float end)
        {
            return new AttributePreference(AttributeId(symbol), weight, start, end);
        }

        public EntityTagPrecondition EntityTagExists(TagQuery query, string description)
        {
            return new EntityTagPrecondition(query, true, description);
        }

        public EntityTagPrecondition EntityTagMissing(TagQuery query, string description)
        {
            return new EntityTagPrecondition(query, false, description);
        }

        public EntityExistencePrecondition EntityExists(EntitySymbol symbol, string description)
        {
            return new EntityExistencePrecondition(EntityId(symbol), true, description);
        }

        public EntityExistencePrecondition EntityMissing(EntitySymbol symbol, string description)
        {
            return new EntityExistencePrecondition(EntityId(symbol), false, description);
        }

        public RelationPrecondition RelationExists(
            EntitySymbol from,
            EntitySymbol to,
            TagQuery query,
            string description)
        {
            return new RelationPrecondition(EntityId(from), EntityId(to), query, true, description);
        }

        public WorldAttributePrecondition WorldAttributeMin(AttributeSymbol symbol, float minValue, string description)
        {
            return new WorldAttributePrecondition(Min(symbol, minValue), description);
        }

        public WorldTagPrecondition WorldTagRequired(TagQuery query, string description)
        {
            return new WorldTagPrecondition(query, description);
        }

        public AddEntityTagEffect AddEntityTag(RoleSymbol role, TagSymbol tag)
        {
            return new AddEntityTagEffect(RoleId(role), Tags(tag));
        }

        public RemoveEntityTagEffect RemoveEntityTag(RoleSymbol role, TagSymbol tag)
        {
            return new RemoveEntityTagEffect(RoleId(role), Tags(tag));
        }

        public AddEntityAttributeEffect AddEntityAttribute(RoleSymbol role, AttributeSymbol attribute, float delta)
        {
            return new AddEntityAttributeEffect(RoleId(role), AttributeId(attribute), delta);
        }

        public RemoveEntityAttributeEffect RemoveEntityAttribute(RoleSymbol role, AttributeSymbol attribute)
        {
            return new RemoveEntityAttributeEffect(RoleId(role), AttributeId(attribute));
        }

        public SetEntityAttributeEffect SetEntityAttribute(RoleSymbol role, AttributeSymbol attribute, float value)
        {
            return new SetEntityAttributeEffect(RoleId(role), AttributeId(attribute), value);
        }

        public AddWorldAttributeEffect AddWorldAttribute(AttributeSymbol attribute, float delta)
        {
            return new AddWorldAttributeEffect(AttributeId(attribute), delta);
        }

        public SetWorldAttributeEffect SetWorldAttribute(AttributeSymbol attribute, float value)
        {
            return new SetWorldAttributeEffect(AttributeId(attribute), value);
        }

        public AddRelationTagEffect AddRelationTag(RoleSymbol from, RoleSymbol to, TagSymbol tag)
        {
            return new AddRelationTagEffect(RoleId(from), RoleId(to), Tags(tag));
        }

        public RemoveRelationTagEffect RemoveRelationTag(RoleSymbol from, RoleSymbol to, TagSymbol tag)
        {
            return new RemoveRelationTagEffect(RoleId(from), RoleId(to), Tags(tag));
        }

        public CreateRelationEffect CreateRelation(RoleSymbol from, RoleSymbol to, params TagSymbol[] tags)
        {
            return new CreateRelationEffect(RoleId(from), RoleId(to), Tags(tags));
        }

        public RemoveRelationEffect RemoveRelation(RoleSymbol from, RoleSymbol to)
        {
            return new RemoveRelationEffect(RoleId(from), RoleId(to));
        }

        public SpawnEntityEffect SpawnEntity(
            EntitySymbol symbol,
            TagSet tags,
            params (AttributeSymbol Symbol, float Value)[] attributes)
        {
            return new SpawnEntityEffect(Entity(symbol, tags, attributes));
        }

        public DespawnEntityEffect DespawnEntity(RoleSymbol role)
        {
            return new DespawnEntityEffect(RoleId(role));
        }

        public StoryletDefinition Storylet(
            StoryletSymbol symbol,
            float priority,
            IReadOnlyList<Role> roles,
            IReadOnlyList<IStoryletPrecondition> preconditions,
            StoryletEffect[] effects,
            StoryletRepeatabilityPolicy repeatability,
            StoryletSaliencePolicy salience)
        {
            return new StoryletDefinition(
                StoryletId(symbol),
                symbol.Key,
                priority,
                roles,
                preconditions,
                new StoryletEffectBatch(effects),
                repeatability,
                salience);
        }

        private TagSet TagsFromList(IReadOnlyList<TagSymbol> symbols)
        {
            if (symbols == null || symbols.Count == 0)
            {
                return TagSet.Empty;
            }

            var tagSet = TagSet.Empty;

            for (var i = 0; i < symbols.Count; i++)
            {
                tagSet |= TagSet.FromIndex(_tags.Resolve(symbols[i]));
            }

            return tagSet;
        }
    }
}
