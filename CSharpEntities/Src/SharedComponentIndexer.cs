namespace CSharpEntities {
    abstract class SharedComponentIndexerGenericBase {
        public abstract object Value(int index);
    }

    class SharedComponentIndexer<TSharedComponent> : SharedComponentIndexerGenericBase
            where TSharedComponent : ISharedComponent<TSharedComponent> {
        readonly Indexer<TSharedComponent> indexer = new Indexer<TSharedComponent>();

        public override object Value(int index) {
            return this.indexer[index];
        }

        public TSharedComponent this[int index] => this.indexer[index];

        public int this[TSharedComponent sharedValue] => this.indexer[sharedValue];

        public bool TryIndex(TSharedComponent sharedValue, out int index) => this.indexer.TryIndex(sharedValue, out index);
    }

    internal class SharedComponentIndexer {
        readonly List<SharedComponentIndexerGenericBase> indexers;
        readonly Dictionary<Type, int> indexerByType;

        public SharedComponentIndexer() {
            this.indexers = new List<SharedComponentIndexerGenericBase>();
            this.indexerByType = new Dictionary<Type, int>();
        }

        public bool TryIndex(Type type, out int index) {
            return this.indexerByType.TryGetValue(type, out index);
        }

        public bool TryIndex<T>(out int index) => this.TryIndex(typeof(T), out index);

        public bool TryIndex<T>(T value, out SharedComponentIndex index) where T : ISharedComponent<T> {
            if (this.TryIndex(typeof(T), out var typeIndex)
                && ((SharedComponentIndexer<T>)this.indexers[typeIndex]).TryIndex(value, out var valueIndex)) {
                index = new SharedComponentIndex(typeIndex, valueIndex);
                return true;
            }
            index = default;
            return false;
        }

        public SharedComponentIndex Index<T>(T sharedValue) where T : struct, ISharedComponent<T> {
            var type = typeof(T);
            if (!this.TryIndex(type, out var typeIndex)) {
                typeIndex = this.indexers.Count;
                this.indexers.Add(new SharedComponentIndexer<T>());
                this.indexerByType[type] = typeIndex;
            }

            var indexer = (SharedComponentIndexer<T>)this.indexers[typeIndex];
            var valueIndex = indexer[sharedValue];

            return new SharedComponentIndex(typeIndex, valueIndex);
        }

        public object Value(SharedComponentIndex index) {
            return this.indexers[index.TypeIndex].Value(index.ValueIndex);
        }

        public T Value<T>(SharedComponentIndex index) where T : ISharedComponent<T>, IEquatable<T> {
            return ((SharedComponentIndexer<T>)this.indexers[index.TypeIndex])[index.ValueIndex];
        }
    }
}
