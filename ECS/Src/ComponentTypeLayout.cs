using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ECS {
    [Serializable]
    public class ComponentTypeLayout : ISerializable {
        
        public ComponentTypeLayout(Archetype archetype) {
            this.indexer = new Indexer<Type>();

            foreach (var componentIndex in archetype.components) {
                var componentType = archetype.componentTypeIndexer[componentIndex];
                this.indexer.Add(componentType);
            }
        }

        
        public int ComponentCount => this.indexer.Count;

        
        public bool TryIndex(Type type, out int index) {
            return this.indexer.TryIndex(type, out index);
        }

        
        public Type Component(int index) => this.indexer[index];


        public IReadOnlyList<Type> Components => this.indexer.Values;
        
        
        private readonly Indexer<Type> indexer;

        
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("Indexer", this.indexer);
        }

        
        private ComponentTypeLayout(SerializationInfo info, StreamingContext context) {
            this.indexer = (Indexer<Type>) info.GetValue("Indexer", typeof(Indexer<Type>));
        }
        
    }
}
