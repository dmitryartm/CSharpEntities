namespace CSharpEntities {
    internal readonly struct SharedComponentIndex : IEquatable<SharedComponentIndex>, IComparable<SharedComponentIndex> {
        public readonly int TypeIndex;
        public readonly int ValueIndex;

        public SharedComponentIndex(int type, int value) {
            this.TypeIndex = type;
            this.ValueIndex = value;
        }

        public int CompareTo(SharedComponentIndex other) {
            var type = this.TypeIndex.CompareTo(other.TypeIndex);
            return type != 0
                ? type
                : this.ValueIndex.CompareTo(other.ValueIndex);
        }

        public override bool Equals(object obj) {
            return obj is SharedComponentIndex index && this.Equals(index);
        }

        public bool Equals(SharedComponentIndex other) {
            return this.TypeIndex == other.TypeIndex &&
                   this.ValueIndex == other.ValueIndex;
        }

        public override int GetHashCode() {
            var hashCode = 1265339359;
            hashCode = hashCode * -1521134295 + this.TypeIndex.GetHashCode();
            hashCode = hashCode * -1521134295 + this.ValueIndex.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(SharedComponentIndex left, SharedComponentIndex right) {
            return left.Equals(right);
        }

        public static bool operator !=(SharedComponentIndex left, SharedComponentIndex right) {
            return !(left == right);
        }
    }
}
