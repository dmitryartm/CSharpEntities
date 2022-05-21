namespace CSharpEntities {
    public struct EntityArchetypeChange {
        public EntityId Entity;
        public Archetype OldArchetype;
        public Archetype NewArchetype;

        public EntityArchetypeChange(EntityId entity, Archetype oldArchetype, Archetype newArchetype) {
            this.Entity = entity;
            this.OldArchetype = oldArchetype;
            this.NewArchetype = newArchetype;
        }
    }
}
