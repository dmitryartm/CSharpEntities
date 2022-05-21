namespace CSharpEntities {
    public readonly struct ComponentType : IEquatable<ComponentType> {
        public readonly int Id;
        public readonly Type Type;

        public ComponentType(int id, Type type) {
            this.Id = id;
            this.Type = type;
        }

        public readonly override bool Equals(object obj) {
            return obj is ComponentType type && this.Equals(type);
        }

        public readonly bool Equals(ComponentType other) {
            return this.Id == other.Id &&
                   EqualityComparer<Type>.Default.Equals(this.Type, other.Type);
        }

        public readonly override int GetHashCode() {
            var hashCode = 1325953389;
            hashCode = hashCode * -1521134295 + this.Id.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(this.Type);
            return hashCode;
        }

        public readonly override string ToString() {
            return $"Id = {this.Id}; Type = {this.Type.FullName}";
        }

        public static bool operator ==(ComponentType left, ComponentType right) {
            return left.Equals(right);
        }

        public static bool operator !=(ComponentType left, ComponentType right) {
            return !(left == right);
        }
    }
}
