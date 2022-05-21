using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;


namespace CSharpEntities {
    [Serializable]
    public class EntityComponentArray : ISerializable {

        public EntityComponentArray(ComponentTypeLayout layout, int length) {
            var typeCount = layout.ComponentCount;
            this._entities = new EntityId[length];
            this._components = new Array[typeCount];
            this.ComponentTypeLayout = layout;

            for (var i = 0; i < typeCount; ++i) {
                this._components[i] = Array.CreateInstance(layout.Component(i), length);
            }
        }
        
        
        #region Public

        
        public IReadOnlyList<EntityId> Entities => this._entities;
        

        public EntityId this[int index] => this._entities[index];
        public int Length => this._entities.Length;

        
        public ref T Ref<T>(int index) {
            if (this.ComponentTypeLayout.TryIndex(typeof(T), out var arrayIndex)) {
                return ref ((T[]) this._components[arrayIndex])[index];
            }
            throw new ArgumentException($"Chunk does not contain component {typeof(T).FullName}");
        }

        
        public bool TryGet<T>(int index, out T value) {
            if (this.ComponentTypeLayout.TryIndex(typeof(T), out var arrayIndex)) {
                value = ((T[]) this._components[arrayIndex])[index];
                return true;
            }
            else {
                value = default;
                return false;
            }
        }


        public ComponentTypeLayout ComponentTypeLayout { get; }

        #endregion
        
        
        #region Internal

        
        internal static void Copy(EntityComponentArray source, int sourceIndex, EntityComponentArray dest, int destIndex, int length) {
            Array.Copy(source._entities, sourceIndex, dest._entities, destIndex, length);
            if (source.ComponentTypeLayout == dest.ComponentTypeLayout) {
                for (var i = 0; i < source._components.Length; ++i) {
                    Array.Copy(source._components[i], sourceIndex, dest._components[i], destIndex, length);
                }
            }
            else {
                for (var sourceArrayIndex = 0; sourceArrayIndex < source.ComponentTypeLayout.ComponentCount; ++sourceArrayIndex) {
                    var type = source.ComponentTypeLayout.Component(sourceArrayIndex);
                    if (dest.ComponentTypeLayout.TryIndex(type, out var destArrayIndex)) {
                        Array.Copy(source._components[sourceArrayIndex], sourceIndex, dest._components[destArrayIndex], destIndex, length);
                    }
                }
            }
        }

        
        internal void Clear(int index) {
            this._entities[index] = default;
            for (var i = 0; i < this.ComponentTypeLayout.ComponentCount; ++i) {
                Array.Clear(this._components[i], index, 1);
            }
        }

        
        internal void Clear() {
            Array.Clear(this._entities, 0, this.Length);
            for (var i = 0; i < this.ComponentTypeLayout.ComponentCount; ++i) {
                Array.Clear(this._components[i], 0, this.Length);
            }
        }
        
        
        internal void SetEntity(int index, EntityId entity) {
            this._entities[index] = entity;
        }

        
        internal T[] Components<T>(int componentIndex) => (T[]) this.Components(componentIndex);

        
        internal Array Components(int componentIndex) => this._components[componentIndex];
        
        
        #endregion
        
        
        #region Private

        private readonly EntityId[] _entities;
        private readonly Array[] _components;
        
        
        #endregion

        
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("Layout", this.ComponentTypeLayout);
            info.AddValue("Length", this.Length);
            info.AddValue("Entities", this._entities);

            var componentCount = this.ComponentTypeLayout.ComponentCount;
            
            for (var i = 0; i < componentCount; ++i) {
                var componentType = this.ComponentTypeLayout.Component(i);
                if (!componentType.IsDefined(typeof(NonSerializedComponentAttribute), false)) {
                    var componentName = i.ToString();
                    info.AddValue(componentName, this._components[i], this._components[i].GetType());
                }
            }
        }


        private EntityComponentArray(SerializationInfo info, StreamingContext context) {
            this.ComponentTypeLayout = info.GetValue<ComponentTypeLayout>("Layout");
            this._entities = info.GetValue<EntityId[]>("Entities");

            var componentCount = this.ComponentTypeLayout.ComponentCount;

            this._components = new Array[componentCount];

            
            var enumerator = info.GetEnumerator();
            while (enumerator.MoveNext()) {
                var entry = enumerator.Current;

                if (int.TryParse(entry.Name, out var componentIndex)) {
                    var arrayType = this.ComponentTypeLayout.Component(componentIndex).MakeArrayType();
                    this._components[componentIndex] = (Array) ((JArray) entry.Value).ToObject(arrayType);
                }
            }
            
            
            for (var i = 0; i < this._components.Length; ++i) {
                if (this._components[i] == null) {
                    var componentType = this.ComponentTypeLayout.Component(i);
                    this._components[i] = Array.CreateInstance(componentType, this._entities.Length);
                }
            }
        }
        
    }
}
