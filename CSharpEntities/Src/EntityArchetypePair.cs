namespace CSharpEntities {
    public struct EntityArchetypePair {
        public EntityId Entity;
        public Archetype Archetype;

        public EntityArchetypePair(EntityId entity, Archetype archetype) {
            this.Entity = entity;
            this.Archetype = archetype;
        }
    }
}
