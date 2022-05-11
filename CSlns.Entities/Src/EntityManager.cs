using System.Text;
using CSlns.Entities.Threading;
using Newtonsoft.Json;


namespace CSlns.Entities {
    public class EntityManager : IDisposable {
        
        #region Public
        
        
        public EntityManager(int maxChunkCapacity = 512) {
            var workersCount = Environment.ProcessorCount - 1;
            this.ParallelFor = new ParallelFor(workersCount);
            this._chunkCapacity = maxChunkCapacity / workersCount * workersCount;

            this.EmptyArchetype = new Archetype(new ComponentTypeIndexer(), new SharedComponentIndexer());
        }

        public void Dispose() {
            this.ParallelFor.Dispose();
        }
        
        
        public ParallelFor ParallelFor { get; }
        
        
        public int Version { get; private set; }


        public Archetype EmptyArchetype { get; }
        
        
        public ArchetypeEntityCollection ArchetypeEntityCollection(EntityId entity) => this._entityArchetypes[entity];

        
        public Archetype EntityArchetype(EntityId entity) {
            return this.ArchetypeEntityCollection(entity).Archetype;
        }


        public IReadOnlyList<ArchetypeEntityCollection> ArchetypeEntityCollections => this._archetypes;


        public int EntityCount() {
            return this.ArchetypeEntityCollections.Sum(static entityCollection => entityCollection.EntityCount);
        }

        
        public Entity CreateEntity(Archetype archetype) {
            var entity = this.NewEntityId();
            var entities = this.ArchetypeEntityCollection(archetype);
            
            entities.Add(entity);

            this._entityArchetypes[entity] = entities;

            return new Entity(entity, this);
        }


        public Entity GetEntity(EntityId id) {
            return new Entity(id, this);
        }

        
        public void DestroyEntity(EntityId entity) {
            this.CheckEntity(entity);

            var entities = this.ArchetypeEntityCollection(entity);
            entities.Remove(entity);
            
            this._entityArchetypes.Remove(entity);
            this.RemoveArchetypeIfEmpty(entities);
        }

        
        public void DestroyAllEntities() {
            this._entityArchetypes.Clear();
            this._archetypeById.Clear();
            
            foreach (var collection in this.ArchetypeEntityCollections) {
                collection.Clear();
            }

            this._archetypes.Clear();
                
            this.IncreaseVersion();
        }

        
        public void DestroyAllEntities(Predicate<Archetype> selector) {
            var removedAny = false;
            foreach (var collection in this.ArchetypeEntityCollections.Where(x => selector(x.Archetype)).ToList()) {
                foreach (var entity in collection.Entities) {
                    this._entityArchetypes.Remove(entity);
                }
                
                collection.Clear();

                this._archetypeById.Remove(collection.Archetype.Id);
                this._archetypes.Remove(collection);
                    
                removedAny = true;
            }

            if (removedAny) {
                this.IncreaseVersion();
            }
        }

        
        public void ChangeEntityArchetype(EntityId entity, Archetype newArchetype) {
            this.CheckEntity(entity);

            var oldArchetype = this.EntityArchetype(entity);

            if (newArchetype.Id != oldArchetype.Id) {
                var oldEntities = this.ArchetypeEntityCollection(oldArchetype);
                var newEntities = this.ArchetypeEntityCollection(newArchetype);

                Entities.ArchetypeEntityCollection.Move(entity, oldEntities, newEntities);

                this._entityArchetypes[entity] = newEntities;
                this.RemoveArchetypeIfEmpty(oldEntities);
            }
        }

        
        public ref T Ref<T>(EntityId entity) {
            if (this._entityArchetypes.TryGetValue(entity, out var archetype)) {
                return ref archetype.Ref<T>(entity);
            }
            throw new Exception($"World does not contain entity {entity}");
        }


        public ref readonly T Get<T>(EntityId entity) {
            return ref this.Ref<T>(entity);
        }

        
        public bool TryGet<T>(EntityId entity, out T component) {
            if (this._entityArchetypes.TryGetValue(entity, out var collection)
                && collection.Archetype.HasComponents<T>()) {
                
                component = collection.Ref<T>(entity);
                return true;
            }
            else {
                component = default;
                return false;
            }
        }


        public bool TryGetShared<T>(EntityId entity, out T value) where T : struct, ISharedComponent<T> {
            this.CheckEntity(entity);
            var archetype = this.EntityArchetype(entity);
            return archetype.TryGetSharedComponent(out value);
        }

        
        public T GetShared<T>(EntityId entity) where T : struct, ISharedComponent<T> {
            if (this.TryGetShared(entity, out T value)) {
                return value;
            }
            else {
                throw new ArgumentException($"Entity {entity} does not contain shared component {typeof(T).FullName}");
            }
        }

        
        public void SetShared<T>(EntityId entity, T value) where T : struct, ISharedComponent<T> {
            this.CheckEntity(entity);

            var archetype = this.EntityArchetype(entity);
            if (archetype.TryGetSharedComponent<T>(out var oldValue)) {
                if (oldValue.Equals(value)) { return; }
            }

            var newArchetype = archetype.AddSharedComponent(value);
            this.ChangeEntityArchetype(entity, newArchetype);
        }


        public void SerializeJson(TextWriter textWriter, Predicate<Archetype> filter = null) {
            using var jsonWriter = new JsonTextWriter(textWriter) { CloseOutput = false };
            
            var serializer = JsonSerializer.CreateDefault();
            serializer.Formatting = Formatting.Indented;

            this.Serialize(serializer, jsonWriter, filter);
        }


        public string SerializeJson(Predicate<Archetype> filter = null) {
            var sb = new StringBuilder();
            using var sw = new StringWriter(sb);
            this.SerializeJson(sw, filter);

            return sb.ToString();
        }


        public void DeserializeJson(TextReader textReader) {
            using var jsonReader = new JsonTextReader(textReader) { CloseInput = false };
            this.Deserialize(JsonSerializer.CreateDefault(), jsonReader);
        }


        public void DeserializeJson(string json) {
            using var sr = new StringReader(json);
            this.DeserializeJson(sr);
        }

        
        public void Serialize(JsonSerializer serializer, JsonWriter jsonWriter, Predicate<Archetype> filter = null) {
            var array =
                this._archetypes
                    .Where(x => filter == null || filter(x.Archetype))
                    .Select(x => x.ToArray())
                    .ToList();
            
            serializer.Serialize(jsonWriter, array);
        }


        public void Deserialize(JsonSerializer serializer, JsonReader jsonReader) {
            if (this._archetypes.Count > 0) {
                throw new InvalidOperationException("Cannot deserialize EntityManager. EntityManager is not empty.");
            }
            
            var arrays = serializer.Deserialize<List<EntityComponentArray>>(jsonReader);

            if (arrays != null) {
                foreach (var array in arrays) {
                    if (array.Length > 0) {
                        var layout = array.ComponentTypeLayout;
                        var archetype = this.Archetype(layout.Components.ToArray());

                        var collection = this.ArchetypeEntityCollection(archetype);
                        collection.FromArray(array);

                        foreach (var entity in collection.Entities) {
                            this._entityArchetypes[entity] = collection;
                        }
                    }
                }
            }

            this.IncreaseVersion();
        }


        public string ToReadableString() {
            return string.Join(
                Environment.NewLine + Environment.NewLine,
                this.ArchetypeEntityCollections.Select(x => x.ToReadableString()));
        }
        
        
        #endregion
        
        
        #region Private


        private readonly List<ArchetypeEntityCollection> _archetypes = new List<ArchetypeEntityCollection>();
        private readonly Dictionary<string, ArchetypeEntityCollection> _archetypeById = new Dictionary<string, ArchetypeEntityCollection>();
        private readonly Dictionary<EntityId, ArchetypeEntityCollection> _entityArchetypes = new Dictionary<EntityId, ArchetypeEntityCollection>();
        
        private readonly int _chunkCapacity;
        private int _entityIdCounter;


        private void IncreaseVersion() {
            ++this.Version;
        }
        

        private EntityId NewEntityId() {
            while (this._entityArchetypes.ContainsKey(new EntityId(this._entityIdCounter))) {
                ++this._entityIdCounter;
            }
            var id = this._entityIdCounter++;
            return new EntityId(id);
        }

        
        private void CheckEntity(EntityId entity) {
            if (!this._entityArchetypes.ContainsKey(entity)) {
                throw new Exception($"Entity {entity} does not exist.");
            }
        }
        
        
        private ArchetypeEntityCollection ArchetypeEntityCollection(Archetype archetype) {
            if (this._archetypeById.TryGetValue(archetype.Id, out var collection)) {
                return collection;
            }
            else {
                collection = new ArchetypeEntityCollection(this, archetype, this._chunkCapacity);
                this._archetypeById[archetype.Id] = collection;
                this._archetypes.Add(collection);
                this.IncreaseVersion();
                
                return collection;
            }
        }


        private void RemoveArchetypeIfEmpty(ArchetypeEntityCollection collection) {
            if (collection.EntityCount == 0) {
                this._archetypeById.Remove(collection.Archetype.Id);
                this._archetypes.Remove(collection);
                this.IncreaseVersion();
            }
        }
        
        
        #endregion
        
    }
}
