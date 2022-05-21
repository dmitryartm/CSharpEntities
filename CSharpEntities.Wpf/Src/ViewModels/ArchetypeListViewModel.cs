using System.Collections.ObjectModel;
using DynamicData.Binding;


namespace CSharpEntities.Wpf.ViewModels {
    public class ArchetypeListViewModel {

        public ArchetypeListViewModel(EntityManager entities) {
            this.Entities = entities;

            this._archetypes = new ObservableCollectionExtended<ArchetypeListViewItemModel>();
            this.Items = new ReadOnlyObservableCollection<ArchetypeListViewItemModel>(this._archetypes);
        }
        
        
        public ReadOnlyObservableCollection<ArchetypeListViewItemModel> Items { get; }

        
        public void Update() {
            if (this._version != this.Entities.Version) {

                var newCount = this.Entities.ArchetypeEntityCollections.Count;
                if (this._archetypes.Count > newCount) {
                    this._archetypes.RemoveRange(newCount, this._archetypes.Count - newCount);
                }
                else if (this._archetypes.Count < newCount) {
                    var newItems = new ArchetypeListViewItemModel[newCount - this._archetypes.Count];
                    for (var i = 0; i < newItems.Length; ++i) {
                        newItems[i] = new ArchetypeListViewItemModel();
                    }
                    
                    this._archetypes.AddRange(newItems);
                }


                for (var i = 0; i < newCount; ++i) {
                    var archetypeEntities = this.Entities.ArchetypeEntityCollections[i];
                    var vm = this._archetypes[i];
                    vm.Set(i, archetypeEntities);
                }
                
                this._version = this.Entities.Version;
            }

            for (var i = 0; i < this._archetypes.Count; ++i) {
                this._archetypes[i].Update();
            }
        }

        
        private readonly EntityManager Entities;
        private int _version = -1;


        private readonly ObservableCollectionExtended<ArchetypeListViewItemModel> _archetypes;

    }
}