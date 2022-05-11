using System;
using System.Collections.Generic;

namespace ECS {
    public class QueryAction {
        
        public QueryAction(Query query, Action<IReadOnlyList<ArchetypeEntityCollection>> action) {
            this.Query = query;
            this.Action = action;
        }
        
        
        public QueryAction(EntityManager entities, Predicate<Archetype> predicate, Action<IReadOnlyList<ArchetypeEntityCollection>> action)
            : this(new Query(entities, predicate), action) {}
        
        
        public readonly Query Query;
        public readonly Action<IReadOnlyList<ArchetypeEntityCollection>> Action;


        public void Execute() {
            this.Action(this.Query.Result.Collections);
        }

    }
}