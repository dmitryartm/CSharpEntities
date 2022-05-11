namespace CSlns.Entities {
    public class ArchetypeEntityCollection {
    
        internal ArchetypeEntityCollection(EntityManager entityManager, Archetype archetype, int chunkCapacity) {
            this.EntityManager = entityManager;
            this.Archetype = archetype;
            this._layout = archetype.Layout();
            this._chunkCapacity = chunkCapacity;
            this._chunks = new List<EntityComponentArray>() { new EntityComponentArray(this._layout, chunkCapacity) };
            this._entityIndices = new Dictionary<EntityId, Index>(chunkCapacity);
        }
        
        
        #region Public

        
        public readonly EntityManager EntityManager;
        public readonly Archetype Archetype;
        

        public int ChunkCount => this._chunks.Count;

        public int EntityCount => this._entityIndices.Count;
        
        public int EntityCountInChunk(int chunkIndex) => chunkIndex == this._chunks.Count - 1 ? this._lastChunkItemCount : this._chunkCapacity;


        public bool TryGetEntityIndex(EntityId entity, out Index index) {
            return this._entityIndices.TryGetValue(entity, out index);
        }

        
        public IReadOnlyCollection<EntityId> Entities => this._entityIndices.Keys;
        
        
        public EntityComponentArray Chunk(int chunkIndex) => this._chunks[chunkIndex];


        public ref T Ref<T>(EntityId entity) {
            if (this._entityIndices.TryGetValue(entity, out var index)) {
                var chunk = this.Chunk(index.ChunkIndex);
                return ref chunk.Ref<T>(index.ItemIndex);
            }

            throw new ArgumentException($"ChunkList does not contain entity {entity}");
        }


        public bool TryGet<T>(EntityId entity, out T value) {
            if (this._layout.TryIndex(typeof(T), out var layoutIndex)
                && this.TryGetEntityIndex(entity, out var entityIndex)) {

                value = this.Chunk(entityIndex.ChunkIndex).Components<T>(layoutIndex)[entityIndex.ItemIndex];
                return true;
            }
            else {
                value = default;
                return false;
            }
        }
        
        
        #endregion
        
        
        #region Internal

        
        internal Index EntityIndexUnsafe(EntityId entity) => this._entityIndices[entity];

        
        internal int ComponentIndex<T>() {
            if (this._layout.TryIndex(typeof(T), out var index)) {
                return index;
            }

            throw new ArgumentException($"ChunkList does not contain type {typeof(T).FullName}");
        }
        

        internal Index Add(EntityId entity) {
            ++this._lastChunkItemCount;
            var needToAddChunk = this.LastChunkIndex < 0;

            if (this._lastChunkItemCount > this._chunkCapacity) {
                this._lastChunkItemCount = 1;
                needToAddChunk = true;
            }

            if (needToAddChunk) {
                this._reservedChunk ??= new EntityComponentArray(this._layout, this._chunkCapacity);

                this._chunks.Add(this._reservedChunk);
                this._reservedChunk = null;
            }

            var index = this.LastIndex;
            this._entityIndices.Add(entity, index);
            this.Chunk(index.ChunkIndex).SetEntity(index.ItemIndex, entity);

            this.OnEntitiesChanged();

            return index;
        }


        internal void Remove(EntityId entity) {
            this.Remove(entity, this.EntityIndexUnsafe(entity));
        }


        internal void Remove(EntityId entity, Index index) {
            this.CheckIndex(index);
            this._entityIndices.Remove(entity);

            var lastIndex = this.LastIndex;
            var lastChunk = this._chunks[lastIndex.ChunkIndex];

            if (index != lastIndex) {
                var chunk = this._chunks[index.ChunkIndex];

                EntityComponentArray.Copy(lastChunk, lastIndex.ItemIndex, chunk, index.ItemIndex, 1);

                var movedEntity = chunk[index.ItemIndex];
                this._entityIndices[movedEntity] = index;
                this._entityIndices.Remove(entity);
            }

            lastChunk.Clear(lastIndex.ItemIndex);

            --this._lastChunkItemCount;
            if (this._lastChunkItemCount <= 0) {
                this._reservedChunk = this._chunks[this.LastChunkIndex];

                if (this._chunks.Count > 0) {
                    this._chunks.RemoveAt(this._chunks.Count - 1);
                }
                
                this._lastChunkItemCount = this._chunkCapacity;
            }

            this.OnEntitiesChanged();
        }


        internal void Clear() {
            foreach (var chunk in this._chunks) {
                chunk.Clear();
            }

            this._chunks.Clear();
            this._entityIndices.Clear();
            this._lastChunkItemCount = 0;

            this.OnEntitiesChanged();
        }


        internal static void Move(EntityId entity, ArchetypeEntityCollection source, ArchetypeEntityCollection dest) {
            var sourceIndex = source.EntityIndexUnsafe(entity);
            var destIndex = dest.Add(entity);

            Copy(source, sourceIndex, dest, destIndex);
            source.Remove(entity, sourceIndex);

            source.OnEntitiesChanged();
            dest.OnEntitiesChanged();
        }


        private static void Copy(ArchetypeEntityCollection source, Index sourceIndex, ArchetypeEntityCollection dest, Index destIndex) {
            var sourceChunk = source.Chunk(sourceIndex.ChunkIndex);
            var destChunk = dest.Chunk(destIndex.ChunkIndex);
            EntityComponentArray.Copy(sourceChunk, sourceIndex.ItemIndex, destChunk, destIndex.ItemIndex, 1);
        }


        internal void FromArray(EntityComponentArray array) {
            if (this.EntityCount > 0) {
                throw new InvalidOperationException($"Unable to fill {nameof(ArchetypeEntityCollection)} from {nameof(EntityComponentArray)}. Collection is not empty.");
            }


            foreach (var entity in array.Entities) {
                this.Add(entity);
            }


            for (var chunkIndex = 0; chunkIndex < this._chunks.Count; ++chunkIndex) {
                var chunk = this._chunks[chunkIndex];
                
                var entityCount = this.EntityCountInChunk(chunkIndex);
                var arrayStart = chunkIndex * this._chunkCapacity;

                EntityComponentArray.Copy(array, arrayStart, chunk, 0, entityCount);
            }
        }


        internal EntityComponentArray ToArray() {
            var array = new EntityComponentArray(this._layout, this.EntityCount);


            for (var chunkIndex = 0; chunkIndex < this._chunks.Count; ++chunkIndex) {
                var chunk = this._chunks[chunkIndex];
                
                var entityCount = this.EntityCountInChunk(chunkIndex);
                var arrayStart = chunkIndex * this._chunkCapacity;

                EntityComponentArray.Copy(chunk, 0, array, arrayStart, entityCount);
            }

            return array;
        }
        
        
        #endregion
        
        
        #region Private


        private int _version;
        private readonly ComponentTypeLayout _layout;
        private readonly List<EntityComponentArray> _chunks;
        private readonly int _chunkCapacity;
        private readonly Dictionary<EntityId, Index> _entityIndices;

        private int _lastChunkItemCount;
        private EntityComponentArray _reservedChunk;


        private int LastChunkIndex => this._chunks.Count - 1;

        
        private Index LastIndex => new Index(this.LastChunkIndex, this._lastChunkItemCount - 1);


        private void CheckIndex(Index index) {
            if (index.ChunkIndex < 0 || index.ItemIndex < 0
                                     || index.ChunkIndex >= this._chunks.Count
                                     || index.ItemIndex >= this._chunkCapacity
                                     || index.ChunkIndex == this._chunks.Count && index.ItemIndex >= this._lastChunkItemCount) {
                throw new IndexOutOfRangeException(
                    $"Index {index} is out of range. Chunk count = {this._chunks.Count}, last chunk item count = {this._lastChunkItemCount}");
            }
        }


        private void OnEntitiesChanged() => ++this._version;
        
        
        #endregion


        public readonly struct Index : IEquatable<Index> {
            public readonly int ChunkIndex;
            public readonly int ItemIndex;

            public Index(int chunk, int item) {
                this.ChunkIndex = chunk;
                this.ItemIndex = item;
            }

            public override bool Equals(object obj) {
                return obj is Index index && this.Equals(index);
            }

            public bool Equals(Index other) {
                return this.ChunkIndex == other.ChunkIndex &&
                       this.ItemIndex == other.ItemIndex;
            }

            public override int GetHashCode() {
                var hashCode = 1126963600;
                hashCode = hashCode * -1521134295 + this.ChunkIndex.GetHashCode();
                hashCode = hashCode * -1521134295 + this.ItemIndex.GetHashCode();
                return hashCode;
            }

            public override string ToString() {
                return $"{{ Chunk = {this.ChunkIndex}, Item = {this.ItemIndex} }}";
            }

            public static bool operator ==(Index left, Index right) {
                return left.Equals(right);
            }

            public static bool operator !=(Index left, Index right) {
                return !(left == right);
            }
        }

    }
}
