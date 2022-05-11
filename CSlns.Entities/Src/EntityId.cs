namespace CSlns.Entities {
    public struct EntityId : IEquatable<EntityId> {
        public int Value;

        public EntityId(int id) {
            this.Value = id;
        }

        public override bool Equals(object obj) {
            return obj is EntityId entity && this.Equals(entity);
        }

        public bool Equals(EntityId other) {
            return this.Value == other.Value;
        }

        public override int GetHashCode() {
            return 2108858624 + this.Value.GetHashCode();
        }

        public override string ToString() {
            return $"{{ Id = {this.Value} }}";
        }

        public static bool operator ==(EntityId left, EntityId right) {
            return left.Equals(right);
        }

        public static bool operator !=(EntityId left, EntityId right) {
            return !(left == right);
        }
    }
}
