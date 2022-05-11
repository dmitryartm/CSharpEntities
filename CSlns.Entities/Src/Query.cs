namespace CSlns.Entities {
    public class Query {
        public Query(EntityManager entities, Predicate<Archetype> predicate) {
            this.Entities = entities;
            this.Predicate = predicate;
        }
        
        
        public readonly EntityManager Entities;
        public readonly Predicate<Archetype> Predicate;

        
        public (bool Changed, IReadOnlyList<ArchetypeEntityCollection> Collections) Result {
            get {
                if (this.Entities.Version != this._cachedVersion) {
                    for (var i = 0; i < this.Entities.ArchetypeEntityCollections.Count; ++i) {
                        var collection = this.Entities.ArchetypeEntityCollections[i];
                        if (this.Predicate(collection.Archetype)) {
                            this._collectionsTmp.Add(collection);
                        }
                    }

                    var changed = false;
                    if (this._collections.Count != this._collectionsTmp.Count) {
                        changed = true;
                    }
                    else {
                        for (var i = 0; i < this._collections.Count; ++i) {
                            if (this._collections[i] != this._collectionsTmp[i]) {
                                changed = true;
                                break;
                            }
                        }
                    }

                    if (changed) {
                        this.ExchangeBuffers();
                    }
                    
                    this._collectionsTmp.Clear();
                    this._cachedVersion = this.Entities.Version;

                    return (changed, this._collections);
                }
                else {
                    return (false, this._collections);
                }
            }
        }


        public bool HasEntities => this.Result.Collections.Count > 0;
        
        
        private List<ArchetypeEntityCollection> _collections = new List<ArchetypeEntityCollection>();
        private List<ArchetypeEntityCollection> _collectionsTmp = new List<ArchetypeEntityCollection>();
        private int _cachedVersion = -1;


        private void ExchangeBuffers() {
            var tmp = this._collectionsTmp;
            this._collectionsTmp = this._collections;
            this._collections = tmp;
        }
        
    }
}