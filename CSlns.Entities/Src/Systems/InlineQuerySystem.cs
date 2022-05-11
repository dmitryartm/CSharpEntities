namespace CSlns.Entities.Systems {
    public interface IAnonymousWorldObject {
    }
    
    public abstract class QueryActionSystem : ComponentSystem {
        protected QueryActionSystem(World world) : base(world) {
        }

        protected abstract QueryAction QueryAction { get; }

        protected override void OnExecute() {
            this.QueryAction.Execute();
        }
    }
    
    public class InlineQuerySystem : ComponentSystem, IAnonymousWorldObject {
        public InlineQuerySystem(World world, string name, QueryAction action, Action<QueryAction> onExecute = null) : base(world) {
            this.Name = name;
            this._queryAction = action;

            this._onExecute = onExecute;
        }

        private readonly QueryAction _queryAction;
        private readonly Action<QueryAction> _onExecute;
        
        protected override void OnExecute() {
            if (this._onExecute != null) {
                this._onExecute(this._queryAction);
            }
            else {
                this._queryAction.Execute();
            }
        }
    }
    
    public class InlineSystem : ComponentSystem, IAnonymousWorldObject {
        public InlineSystem(World world, string name, Action<ComponentSystem> action = null) : base(world) {
            this.Name = name;
            this.Action = action;
        }

        protected readonly Action<ComponentSystem> Action;

        protected override void OnExecute() {
            this.Action?.Invoke(this);
        }
    }

    public class InlineSystem<TState> : ComponentSystem, IAnonymousWorldObject {
        public InlineSystem(World world, string name, Func<TState> onStart, Action<InlineSystem<TState>> onExecute, Action<TState> onDispose = null) : base(world) {
            this._onStart = onStart;
            this._onExecute = onExecute;

            if (onDispose != null) {
                this.OnDispose.Subscribe(_ => onDispose(this.State));
            }
        }

        private readonly Func<TState> _onStart;
        private readonly Action<InlineSystem<TState>> _onExecute;
        
        public TState State;

        protected override void OnStart() {
            this.State = this._onStart();
        }
        
        protected override void OnExecute() {
            this._onExecute(this);
        }
    }
    
    public static class QuerySystemEx {
        public static ComponentSystem ToSystem(this QueryAction action, World world, string name) {
            return new InlineQuerySystem(world, name, action);
        }
    }
}