namespace ECS {
    public readonly struct EntityManagerWithArg<TArg> {
        public EntityManagerWithArg(EntityManager em) {
            this.EntityManager = em;
        }
        
        public readonly EntityManager EntityManager;
    }
}