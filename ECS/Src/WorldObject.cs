using System;
using System.Reactive;
using System.Reactive.Subjects;
using CSlns.Std;

namespace ECS {

    public interface IWorldSingleObject : IDisposable {
        World World { get; }
        IObservable<Unit> OnDispose { get; }
    }

    
    public interface IWorldSingleObject<out TWorld> : IWorldSingleObject where TWorld : World {
        new TWorld World { get; }
    }
    
    
    public abstract class WorldSingleObject : IWorldSingleObject {
        protected WorldSingleObject(World world) {
            this.World = world;
            this.World.RegisterObject(this);
        }
        
        
        public World World { get; }
        public EntityManager Entities => this.World.Entities;
        
        
        public IObservable<Unit> OnDispose => this._onDispose;


        public void Dispose() {
            this._onDispose.OnNext(Unit.Default);
            this._onDispose.OnCompleted();
            this.World.UnregisterObject(this);
        }
        
        
        private readonly Subject<Unit> _onDispose = new();
    }

    
    public abstract class WorldSingleObject<TWorld> : WorldSingleObject, IWorldSingleObject<TWorld> where TWorld : World {
        protected WorldSingleObject(TWorld world) : base(world) {
        }

        public new TWorld World => (TWorld) ((WorldSingleObject) this).World;
    }
    
}