using System;

namespace ECS {
    class ComponentTypeIndexer : Indexer<Type> {
        public ComponentType ComponentType(Type type) => new ComponentType(this[type], type);
    }
}
