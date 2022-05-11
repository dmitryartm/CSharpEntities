using System.Runtime.Serialization;


namespace CSlns.Entities {
    
    [Serializable]
    internal class Indexer<T> : ISerializable {
        
        public Indexer() {
            this.values = new List<T>();
            this.indexByValue = new Dictionary<T, int>();
        }


        private readonly List<T> values;
        private readonly Dictionary<T, int> indexByValue;

        public int Count => this.values.Count;

        public int this[T value] {
            get {
                if (!this.indexByValue.TryGetValue(value, out var index)) {
                    index = this.Add(value);
                }
                return index;
            }
        }

        
        public int Add(T value) {
            var index = this.values.Count;
            this.values.Add(value);
            this.indexByValue[value] = index;
            return index;
        }

        
        public T this[int index] => this.values[index];


        public IReadOnlyList<T> Values => this.values;

        
        public bool TryIndex(T value, out int index) {
            return this.indexByValue.TryGetValue(value, out index);
        }

        
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("Values", this.values);
        }

        
        private Indexer(SerializationInfo info, StreamingContext context) {
            this.values = (List<T>) info.GetValue("Values", typeof(List<T>));
            this.indexByValue = new Dictionary<T, int>();

            for (var i = 0; i < this.values.Count; ++i) {
                this.indexByValue[this.values[i]] = i;
            }
        }

    }
}
