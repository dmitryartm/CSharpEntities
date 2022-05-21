using System.Collections;
using System.Reactive;
using System.Reactive.Subjects;


namespace CSharpEntities.Systems {


    public interface IComponentSystem : IWorldSingleObject {
        string Name { get; }
        int Version { get; }
        ComponentSystem Owner { get; }
        IList<ComponentSystem> Subsystems { get; }
        bool Enabled { get; set; }
        bool ExecuteSubsystems { get; set; }
        void Start();
        void Execute();
        IObservable<Unit> BeforeExecute { get; }
        IObservable<Unit> AfterExecute { get; }
    }


    public static class ComponentSystemEx {
        public static void ExecuteIfEnabled(this IComponentSystem system) {
            if (system.Enabled) {
                system.Execute();
            }
        }
    }


    public interface IComponentSystem<out TWorld> : IComponentSystem, IWorldSingleObject<TWorld> where TWorld : World {
    }
    
    
    public abstract class ComponentSystem : IComponentSystem, IList<ComponentSystem> {
        
        protected ComponentSystem(World world) {
            this.World = world;
            this.Name = this.GetType().Name;
            this.World.RegisterObject(this);
        }
        
        
        #region Public

        
        public void Start() {
            this.OnStart();
            this._started = true;

            foreach (var subsystem in this.Subsystems) {
                subsystem.Start();
            }
        }

        
        public void Execute() {
            if (!this._started) {
                this.Start();
            }
        
            this._beforeExecute.OnNext(Unit.Default);
            this.OnExecute();
            if (this.ExecuteSubsystems) {
                this.DoExecuteSubsystems();
            }
            this._afterExecute.OnNext(Unit.Default);
        }

        
        public void Dispose() {
            this._onDispose.OnNext(Unit.Default);
            
            for (var i = 0; i < this.Subsystems.Count; ++i) {
                this.Subsystems[i].Dispose();
            }

            this._beforeExecute.OnCompleted();
            this._afterExecute.OnCompleted();
            this._onDispose.OnCompleted();
            this.World.UnregisterObject(this);
        }
        
        
        public string Name { get; protected set; }
        public World World { get; private set; }
        public bool Enabled { get; set; } = true;
        public bool ExecuteSubsystems { get; set; } = true;
        public EntityManager Entities => this.World.Entities;
        public IObservable<Unit> BeforeExecute => this._beforeExecute;
        public IObservable<Unit> AfterExecute => this._afterExecute;
        public IObservable<Unit> OnDispose => this._onDispose;
        public IList<ComponentSystem> Subsystems => this._subsystems;
        
        
        public int Version { get; private set; }
        
        
        public ComponentSystem Owner { get; private set; }
        
        
        /// <summary>
        /// Add Subsystem
        /// </summary>
        /// <param name="system"></param>
        public void Add(ComponentSystem system) {
            if (system != null) {
                system.SetOwner(this);
                this._subsystems.Add(system);
            }
        }


        public void AddRange(IEnumerable<ComponentSystem> systems) {
            foreach (var item in systems) {
                this.Add(item);
            }
        }


        public void AddRange(params ComponentSystem[] systems) {
            foreach (var item in systems) {
                this.Add(item);
            }
        }
        
        
        /// <summary>
        /// Remove Subsystem
        /// </summary>
        /// <param name="system"></param>
        /// <returns></returns>
        public bool Remove(ComponentSystem system) {
            if (system != null && system.Owner == this) {
                system.ClearOwner();
                this._subsystems.Remove(system);
                
                return true;
            }
            else {
                return false;
            }
        }
        
        
        /// <summary>
        /// Insert Subsystem
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public void Insert(int index, ComponentSystem item) {
            if (item != null) {
                item.SetOwner(this);
                this._subsystems.Insert(index, item);
            }
        }
        
        
        /// <summary>
        /// Remove Susbsystem
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index) {
            var subsystem = this._subsystems[index];
            subsystem.ClearOwner();
            this._subsystems.RemoveAt(index);
        }
        
        
        /// <summary>
        /// Access Subsystem
        /// </summary>
        /// <param name="index"></param>
        public ComponentSystem this[int index] {
            get => this._subsystems[index];
            set {
                value.SetOwner(this);
                
                var oldSystem = this._subsystems[index];
                oldSystem.ClearOwner();
                
                this._subsystems[index] = value;
            }
        }
        
        
        /// <summary>
        /// Clear Subsystems
        /// </summary>
        public void Clear() {
            foreach (var system in this._subsystems) {
                system.ClearOwner();
            }
            
            this._subsystems.Clear();
        }
        
        
        #endregion
        
        
        #region Protected

        
        protected virtual void OnStart() {}
        
        protected virtual void OnExecute() {}
        
        
        protected void DoExecuteSubsystems() {
            for (var i = 0; i < this.Subsystems.Count; ++i) {
                var subsystem = this.Subsystems[i];
                subsystem.ExecuteIfEnabled();
            }
        }
        
        
        #endregion

        
        #region Private
        
        
        private readonly Subject<Unit> _beforeExecute = new Subject<Unit>();
        private readonly Subject<Unit> _afterExecute = new Subject<Unit>();
        private readonly Subject<Unit> _onDispose = new Subject<Unit>();
        private readonly List<ComponentSystem> _subsystems = new List<ComponentSystem>();


        private bool _started;


        private void IncVersion() {
            ++this.Version;
            this.Owner?.IncVersion();
        }


        private void SetOwner(ComponentSystem owner) {
            if (this.Owner != null) {
                throw new InvalidOperationException($"Cannot set ComponentSystem owner. Already owned by {this.Owner.Name}");
            }

            this.Owner = owner;
            this.Owner.IncVersion();
        }


        private void ClearOwner() {
            if (this.Owner != null) {
                this.Owner.IncVersion();
                this.Owner = null;
            }
        }

        
        #endregion

        
        #region IList
        
        IEnumerator<ComponentSystem> IEnumerable<ComponentSystem>.GetEnumerator() {
            return this.Subsystems.GetEnumerator();
        }
        
        IEnumerator IEnumerable.GetEnumerator() {
            return this.Subsystems.GetEnumerator();
        }
        
        bool ICollection<ComponentSystem>.Contains(ComponentSystem item) {
            return this._subsystems.Contains(item);
        }
        
        void ICollection<ComponentSystem>.CopyTo(ComponentSystem[] array, int arrayIndex) {
            this._subsystems.CopyTo(array, arrayIndex);
        }
        
        int ICollection<ComponentSystem>.Count => this.Subsystems.Count;
        
        bool ICollection<ComponentSystem>.IsReadOnly => true;
        
        int IList<ComponentSystem>.IndexOf(ComponentSystem item) {
            return this._subsystems.IndexOf(item);
        }
    
        #endregion
        
    }
    
    
    public abstract class ComponentSystem<TWorld> : ComponentSystem, IComponentSystem<TWorld> where TWorld : World {
        protected ComponentSystem(TWorld world) : base(world) {
        }
        
        public new TWorld World => (TWorld) ((ComponentSystem) this).World;
    }
}