using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive;
using System.Reactive.Subjects;
using CSlns.Std;

namespace ECS {
    public abstract class World {
        
        #region Public
        
        public string Name { get; protected set; }
        public EntityManager Entities { get; protected set; }
        public IComponentSystem RootSystem { get; protected set; }


        public Option<T> TryGet<T>() where T : IWorldSingleObject {
            if (this._objects.TryGetValue(typeof(T), out var system)) {
                return Option.Some((T) system);
            }
            else {
                return default;
            }
        }


        public T Get<T>() where T : IWorldSingleObject  {
            if (this.TryGet<T>().TryGetValue(out var system)) {
                return system;
            }
            else {
                throw new ArgumentException($"Unable to get system {typeof(T).Name}. System not found.");
            }
        }


        public void Get<T>(out T system) where T : IWorldSingleObject {
            system = this.Get<T>();
        }


        public IObservable<Unit> AfterLoad => this._afterLoadSubject;
        
        
        #endregion
        
        
        #region Private


        private readonly Dictionary<Type, IWorldSingleObject> _objects = new();
        private readonly Subject<Unit> _afterLoadSubject = new();


        internal void RegisterObject(IWorldSingleObject system) {
            if (system is not IAnonymousWorldObject) {
                this._objects.Add(system.GetType(), system);
            }
        }


        internal void UnregisterObject(IWorldSingleObject system) {
            this._objects.Remove(system.GetType());
        }
        
        
        #endregion
        
    }
    
    
    public abstract class World<TWorld> : World, IDisposable where TWorld : World<TWorld> {
        
        protected World() {
            this.Name = this.GetType().Name;
        }
        

        public void Dispose() {
            this.RootSystem?.Dispose();
            this.Entities?.Dispose();
            
            this._onDispose.OnNext(Unit.Default);
            this._onDispose.OnCompleted();
        }

        
        private readonly Subject<Unit> _onDispose = new Subject<Unit>();

        public IObservable<Unit> OnDispose => this._onDispose;


        public new IComponentSystem<TWorld> RootSystem {
            get => (IComponentSystem<TWorld>) base.RootSystem;
            protected set => base.RootSystem = value;
        }


        public void StartSystems() {
            this.RootSystem.Start();
        }
        

        public void ExecuteSystems() {
            this.RootSystem.ExecuteIfEnabled();
        }


        protected class RootComponentSystem : ComponentSystem<TWorld> {

            public RootComponentSystem(TWorld world) : base(world) {
                this.Name = this.World.Name;
            }
            
        }

    }
}