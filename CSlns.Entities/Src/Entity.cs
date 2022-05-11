namespace CSlns.Entities {
    public readonly struct Entity : IEquatable<Entity> {
        public readonly EntityId Id;
        public readonly EntityManager EntityManager;

        public Entity(EntityId id, EntityManager entityManager) {
            this.Id = id;
            this.EntityManager = entityManager;
        }

        public static implicit operator EntityId(in Entity entity) => entity.Id; 
        
        #region Overloads

        public override string ToString() {
            return this.Id.ToString();
        }

        public override bool Equals(object obj) {
            return obj is Entity entity && this.Equals(entity);
        }

        public bool Equals(Entity other) {
            return this.Id.Equals(other.Id) &&
                   EqualityComparer<EntityManager>.Default.Equals(this.EntityManager, other.EntityManager);
        }

        public override int GetHashCode() {
            var hashCode = 1806410092;
            hashCode = hashCode * -1521134295 + this.Id.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<EntityManager>.Default.GetHashCode(this.EntityManager);
            return hashCode;
        }

        public static bool operator ==(Entity left, Entity right) {
            return left.Equals(right);
        }

        public static bool operator !=(Entity left, Entity right) {
            return !(left == right);
        }
        
        #endregion
        
        #region Extensions

        public ref T Ref<T>() => ref this.EntityManager.Ref<T>(this.Id);

        public ref readonly T Get<T>() => ref this.Ref<T>();

        public bool Has<T>() => this.EntityManager.HasComponent<T>(this.Id);

        public bool TryGet<T>(out T value) => this.EntityManager.TryGet(this.Id, out value);

        public void Set<T>(in T value) => this.Ref<T>() = value;

        public void Destroy() => this.EntityManager.DestroyEntity(this.Id);

        public void Add<T>(T value = default) {
            this.EntityManager.AddComponents(this.Id, value);
        }

        public void Add<T0, T1>(in T0 value0 = default, in T1 value1 = default) {
            this.EntityManager.AddComponents(this.Id, value0, value1);
        }

        public void Add<T0, T1, T2>(in T0 value0 = default, in T1 value1 = default, in T2 value2 = default) {
            this.EntityManager.AddComponents(this.Id, value0, value1, value2);
        }

        public void Add<T0, T1, T2, T3>(in T0 value0 = default, in T1 value1 = default, in T2 value2 = default, in T3 value3 = default) {
            this.EntityManager.AddComponents(this.Id, value0, value1, value2, value3);
        }

        public void Add<T0, T1, T2, T3, T4>(in T0 value0 = default, in T1 value1 = default, in T2 value2 = default, in T3 value3 = default, in T4 value4 = default) {
            this.EntityManager.AddComponents(this.Id, value0, value1, value2, value3, value4);
        }

        public T GetShared<T>() where T : struct, ISharedComponent<T> {
            return this.EntityManager.GetShared<T>(this.Id);
        }

        public bool TryGetShared<T>(out T value) where T : struct, ISharedComponent<T> {
            return this.EntityManager.TryGetShared(this.Id, out value);
        }

        public void SetShared<T>(in T value) where T : struct, ISharedComponent<T> {
            this.EntityManager.SetShared(this.Id, value);
        }

        #endregion
    }
}
