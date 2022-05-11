using System.Linq;
using CSlns.Std;
using CSlns.Wpf;
using ReactiveUI;

namespace ECS.WPF.ViewModels {
    public class ArchetypeListViewItemModel : ReactiveObject {

        public ArchetypeListViewItemModel() {
            this._components.RaisePropertyChanged(this, nameof(this.Components));
            this._componentIndices.RaisePropertyChanged(this, nameof(this.ComponentIndices));
            this._entityCount.RaisePropertyChanged(this, nameof(this.EntityCount));
            this._index.RaisePropertyChanged(this, nameof(this.Index));
        }

        
        private readonly Behaviour<string> _components = new(default);
        private readonly Behaviour<string> _componentIndices = new(default);
        private readonly Behaviour<int> _entityCount = new(default);
        private readonly Behaviour<int> _index = new(default);
        private ArchetypeEntityCollection _archetypeEntities;


        public string ComponentIndices {
            get => this._componentIndices.Value;
            set => this._componentIndices.Value = value;
        }


        public string Components {
            get => this._components.Value;
            set => this._components.Value = value;
        }


        public int EntityCount {
            get => this._entityCount.Value;
            set => this._entityCount.Value = value;
        }


        public int Index {
            get => this._index.Value;
            set => this._index.Value = value;
        }


        public void Set(int index, ArchetypeEntityCollection collection) {
            this._archetypeEntities = collection;
            
            var components = collection.Archetype.Components.Select(x => x.Name);
            var componentCount = collection.Archetype.ComponentsCount;
            var sharedComponents = collection.Archetype.SharedComponents.Select(x => x.ToString());
            
            this.Index = index;

            var allComponents = components.Concat(sharedComponents);

            this.ComponentIndices = string.Join("\r\n", allComponents.Select((_, i) => (i + 1).ToString("00")));
            this.Components = string.Join("\r\n", allComponents);
        }


        public void Update() {
            this.EntityCount = this._archetypeEntities.EntityCount;
        }
    }
}