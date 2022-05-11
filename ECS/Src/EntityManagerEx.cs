using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ECS.Threading;

namespace ECS {
    public static class EntityManagerEx {
        
        #region Archetype
        
        public static Archetype Archetype(this EntityManager em, params Type[] components) {
            return em.EmptyArchetype.AddComponents(components);
        }

        public static Archetype Archetype(this EntityManager em) => em.EmptyArchetype;

        public static Archetype Archetype<T>(this EntityManager em) {
            return em.EmptyArchetype.AddComponents(typeof(T));
        }

        public static Archetype Archetype<T0, T1>(this EntityManager em) {
            return em.Archetype(typeof(T0), typeof(T1));
        }

        public static Archetype Archetype<T0, T1, T2>(this EntityManager em) {
            return em.Archetype(typeof(T0), typeof(T1), typeof(T2));
        }

        public static Archetype Archetype<T0, T1, T2, T3>(this EntityManager em) {
            return em.Archetype(typeof(T0), typeof(T1), typeof(T2), typeof(T3));
        }

        public static Archetype Archetype<T0, T1, T2, T3, T4>(this EntityManager em) {
            return em.Archetype(typeof(T0), typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        }
        
        public static Archetype Archetype<T0, T1, T2, T3, T4, T5>(this EntityManager em) {
            return em.Archetype(typeof(T0), typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        }
        
        public static Archetype Archetype<T0, T1, T2, T3, T4, T5, T6>(this EntityManager em) {
            return em.Archetype(typeof(T0), typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
        }
        
        #endregion
        
        #region CreateEntity

        public static Entity CreateEntity(this EntityManager em) {
            return em.CreateEntity(em.EmptyArchetype);
        }

        public static Entity CreateEntity<T>(this EntityManager em, in T component = default) {
            var archetype = em.EmptyArchetype.AddComponents(typeof(T));

            var entity = em.CreateEntity(archetype);
            em.Ref<T>(entity) = component;

            return entity;
        }

        public static Entity CreateEntity<T0, T1>(this EntityManager em, in T0 component0 = default, in T1 component1 = default) {
            var archetype = em.EmptyArchetype.AddComponents(typeof(T0), typeof(T1));

            var entity = em.CreateEntity(archetype);
            em.Ref<T0>(entity) = component0;
            em.Ref<T1>(entity) = component1;

            return entity;
        }

        public static Entity CreateEntity<T0, T1, T2>(this EntityManager em, in T0 component0 = default, in T1 component1 = default, in T2 component2 = default) {
            var archetype = em.EmptyArchetype.AddComponents(typeof(T0), typeof(T1), typeof(T2));

            var entity = em.CreateEntity(archetype);
            em.Ref<T0>(entity) = component0;
            em.Ref<T1>(entity) = component1;
            em.Ref<T2>(entity) = component2;

            return entity;
        }

        public static Entity CreateEntity<T0, T1, T2, T3>(this EntityManager em, in T0 component0 = default, in T1 component1 = default, in T2 component2 = default, in T3 component3 = default) {
            var archetype = em.EmptyArchetype.AddComponents(typeof(T0), typeof(T1), typeof(T2), typeof(T3));

            var entity = em.CreateEntity(archetype);
            em.Ref<T0>(entity) = component0;
            em.Ref<T1>(entity) = component1;
            em.Ref<T2>(entity) = component2;
            em.Ref<T3>(entity) = component3;

            return entity;
        }

        public static Entity CreateEntity<T0, T1, T2, T3, T4>(this EntityManager em, in T0 component0 = default, in T1 component1 = default, in T2 component2 = default, in T3 component3 = default, in T4 component4 = default) {
            var archetype = em.EmptyArchetype.AddComponents(typeof(T0), typeof(T1), typeof(T2), typeof(T3), typeof(T4));

            var entity = em.CreateEntity(archetype);
            em.Ref<T0>(entity) = component0;
            em.Ref<T1>(entity) = component1;
            em.Ref<T2>(entity) = component2;
            em.Ref<T3>(entity) = component3;
            em.Ref<T4>(entity) = component4;

            return entity;
        }

        public static Entity CreateEntity<T0, T1, T2, T3, T4, T5>(this EntityManager em, in T0 component0 = default, in T1 component1 = default, in T2 component2 = default, in T3 component3 = default, in T4 component4 = default, in T5 component5 = default) {
            var archetype = em.EmptyArchetype.AddComponents(typeof(T0), typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));

            var entity = em.CreateEntity(archetype);
            em.Ref<T0>(entity) = component0;
            em.Ref<T1>(entity) = component1;
            em.Ref<T2>(entity) = component2;
            em.Ref<T3>(entity) = component3;
            em.Ref<T4>(entity) = component4;
            em.Ref<T5>(entity) = component5;

            return entity;
        }

        public static Entity CreateEntity<T>(this EntityManager em, Archetype archetype, in T component) {
            if (!archetype.HasComponents<T>()) {
                throw new ArgumentException($"Archetype does not contain component {typeof(T).FullName}");
            }

            var entity = em.CreateEntity(archetype);
            em.Ref<T>(entity) = component;

            return entity;
        }
        
        #endregion

        #region AddComponents
        
        public static void AddComponents<T0>(this EntityManager em, EntityId entity, in T0 component0 = default) {
            em.AddComponents(entity, typeof(T0));
            em.Set(entity, component0);
        }

        public static void AddComponents<T0, T1>(this EntityManager em, EntityId entity, in T0 component0 = default, in T1 component1 = default) {
            AddComponents(em, entity, typeof(T0), typeof(T1));
            em.Set(entity, component0);
            em.Set(entity, component1);
        }

        public static void AddComponents<T0, T1, T2>(this EntityManager em, EntityId entity,
                in T0 component0 = default, in T1 component1 = default, in T2 component2 = default) {
            AddComponents(em, entity, typeof(T0), typeof(T1), typeof(T2));
            em.Set(entity, component0);
            em.Set(entity, component1);
            em.Set(entity, component2);
        }

        public static void AddComponents<T0, T1, T2, T3>(this EntityManager em, EntityId entity,
                in T0 component0 = default, in T1 component1 = default, in T2 component2 = default, in T3 component3 = default) {
            AddComponents(em, entity, typeof(T0), typeof(T1), typeof(T2), typeof(T3));
            em.Set(entity, component0);
            em.Set(entity, component1);
            em.Set(entity, component2);
            em.Set(entity, component3);
        }

        public static void AddComponents<T0, T1, T2, T3, T4>(this EntityManager em, EntityId entity,
                in T0 component0 = default, in T1 component1 = default, in T2 component2 = default, in T3 component3 = default, in T4 component4 = default) {
            AddComponents(em, entity, typeof(T0), typeof(T1), typeof(T2), typeof(T3), typeof(T4));
            em.Set(entity, component0);
            em.Set(entity, component1);
            em.Set(entity, component2);
            em.Set(entity, component3);
            em.Set(entity, component4);
        }

        public static void AddComponents(this EntityManager em, EntityId entity, params Type[] components) {
            var oldArchetype = em.EntityArchetype(entity);
            var newArchetype = oldArchetype.AddComponents(components);
            em.ChangeEntityArchetype(entity, newArchetype);
        }
        
        #endregion

        #region RemoveComponent
        
        public static void RemoveComponent(this EntityManager em, EntityId entity, Type type) {
            em.ChangeEntityArchetype(entity, em.EntityArchetype(entity).RemoveComponents(type));
        }

        public static void RemoveComponent<T>(this EntityManager em, EntityId entity) {
            em.RemoveComponent(entity, typeof(T));
        }
        
        #endregion

        #region ForEach

        internal static Predicate<Archetype> IncludeTypesInQuery(Predicate<Archetype> query, params Type[] types) {
            return archetype => {
                foreach (var type in types) {
                    if (!archetype.HasComponents(type)) {
                        return false;
                    }
                }
                return query == null || query(archetype);
            };
        }

        
        public static QueryAction ForEach<T>(this EntityManager em, EntityAction<T> action)
                where T : struct {
            return ForEach(em, null, action);
        }
        
        
        public static QueryAction ForEach<T0>(this EntityManager entities, Predicate<Archetype> query, EntityAction<T0> action)
            where T0 : struct {
            
            query = IncludeTypesInQuery(query, typeof(T0));

            return 
                new QueryAction(entities, query, result => {
                    for (var i = 0; i < result.Count; ++i) {
                        result[i].ForEach(action);
                    }
                });
        }

        public static QueryAction ForEach<T0, T1>(this EntityManager em, EntityAction<T0, T1> action) {
            return ForEach(em, null, action);
        }

        public static QueryAction ForEach<T0, T1>(this EntityManager em, Predicate<Archetype> query, EntityAction<T0, T1> action) {
            query = IncludeTypesInQuery(query, typeof(T0), typeof(T1));

            return 
                new QueryAction(em, query, result => {
                    for (var i = 0; i < result.Count; ++i) {
                        result[i].ForEach(action);
                    }
                });
        }

        public static QueryAction ForEach<T0, T1, T2>(this EntityManager em, EntityAction<T0, T1, T2> action) {
            return ForEach(em, null, action);
        }

        public static QueryAction ForEach<T0, T1, T2>(this EntityManager em, Predicate<Archetype> query, EntityAction<T0, T1, T2> action) {
            query = IncludeTypesInQuery(query, typeof(T0), typeof(T1), typeof(T2));

            return 
                new QueryAction(em, query, result => {
                    for (var i = 0; i < result.Count; ++i) {
                        result[i].ForEach(action);
                    }
                });
        }

        public static QueryAction ForEach<T0, T1, T2, T3>(this EntityManager em, EntityAction<T0, T1, T2, T3> action) {
            return ForEach(em, null, action);
        }

        public static QueryAction ForEach<T0, T1, T2, T3>(this EntityManager em, Predicate<Archetype> query, EntityAction<T0, T1, T2, T3> action) {
            query = IncludeTypesInQuery(query, typeof(T0), typeof(T1), typeof(T2), typeof(T3));

            return 
                new QueryAction(em, query, result => {
                    for (var i = 0; i < result.Count; ++i) {
                        result[i].ForEach(action);
                    }
                });
        }
        
        public static QueryAction ForEach<T0>(this EntityManager em, EntityAction_E<T0> action) {
            return ForEach(em, null, action);
        }

        public static QueryAction ForEach<T0>(this EntityManager em, Predicate<Archetype> query, EntityAction_E<T0> action) {
            query = IncludeTypesInQuery(query, typeof(T0));

            return 
                new QueryAction(em, query, result => {
                    for (var i = 0; i < result.Count; ++i) {
                        result[i].ForEach(action);
                    }
                });
        }

        public static QueryAction ForEach<T0, T1>(this EntityManager em, EntityAction_E<T0, T1> action) {
            return ForEach(em, null, action);
        }

        public static QueryAction ForEach<T0, T1>(this EntityManager em, Predicate<Archetype> query, EntityAction_E<T0, T1> action) {
            query = IncludeTypesInQuery(query, typeof(T0), typeof(T1));

            return 
                new QueryAction(em, query, result => {
                    for (var i = 0; i < result.Count; ++i) {
                        result[i].ForEach(action);
                    }
                });
        }

        public static QueryAction ForEach<T0, T1, T2>(this EntityManager em, EntityAction_E<T0, T1, T2> action) {
            return ForEach(em, null, action);
        }

        public static QueryAction ForEach<T0, T1, T2>(this EntityManager em, Predicate<Archetype> query, EntityAction_E<T0, T1, T2> action) {
            query = IncludeTypesInQuery(query, typeof(T0), typeof(T1), typeof(T2));

            return 
                new QueryAction(em, query, result => {
                    for (var i = 0; i < result.Count; ++i) {
                        result[i].ForEach(action);
                    }
                });
        }


        private static Action<IReadOnlyList<ArchetypeEntityCollection>> ForEachParallelAction(EntityManager entityManager, IImmutableList<QueryParallelActionObj> actionObjs, int minBatchSize) {
            return
                collections => {
                    entityManager.ParallelFor.Execute(actionObjs, collections, minBatchSize);
                };
        }

        
        public static QueryAction ForEachParallel<T0>(this EntityManager em, EntityAction<T0> action, int minBatchSize) {
            return ForEachParallel(em, null, action, minBatchSize);
        }

        public static QueryAction ForEachParallel<T0>(this EntityManager em, Predicate<Archetype> query, EntityAction<T0> action, int minBatchSize = MinParallelBatchSize) {
            return ForEachParallel(em, query, 
                (in Entity entity, ref T0 comp0) => action(ref comp0), minBatchSize);
        }

        public static QueryAction ForEachParallel<T0>(this EntityManager em, EntityAction_E<T0> action, int minBatchSize = MinParallelBatchSize) {
            return ForEachParallel(em, null, action, minBatchSize);
        }

        public static QueryAction ForEachParallel<T0>(this EntityManager em, Predicate<Archetype> query, EntityAction_E<T0> action, int minBatchSize = MinParallelBatchSize) {
            query = IncludeTypesInQuery(query, typeof(T0));
            return new QueryAction(em, query, ForEachParallelAction(em, QueryParallelActionObj.Array(action, em.ParallelFor.WorkersCount), minBatchSize));
        }

        
        public static QueryAction ForEachParallel<T0, T1>(this EntityManager em, EntityAction<T0, T1> action, int minBatchSize = MinParallelBatchSize) {
            return ForEachParallel(em, null, action, minBatchSize);
        }

        public static QueryAction ForEachParallel<T0, T1>(this EntityManager em, Predicate<Archetype> query, EntityAction<T0, T1> action, int minBatchSize = MinParallelBatchSize) {
            return ForEachParallel(em, query, 
                (in Entity entity, ref T0 comp0, ref T1 comp1) => action(ref comp0, ref comp1), minBatchSize);
        }

        public static QueryAction ForEachParallel<T0, T1>(this EntityManager em, EntityAction_E<T0, T1> action, int minBatchSize = MinParallelBatchSize) {
            return ForEachParallel(em, null, action, minBatchSize);
        }

        public static QueryAction ForEachParallel<T0, T1>(this EntityManager em, Predicate<Archetype> query, EntityAction_E<T0, T1> action, int minBatchSize = MinParallelBatchSize) {
            query = IncludeTypesInQuery(query, typeof(T0), typeof(T1));
            return new QueryAction(em, query, ForEachParallelAction(em, QueryParallelActionObj.Array(action, em.ParallelFor.WorkersCount), minBatchSize));
        }
        

        public static QueryAction ForEachParallel<T0, T1, T2>(this EntityManager em, EntityAction<T0, T1, T2> action, int minBatchSize = MinParallelBatchSize) {
            return ForEachParallel(em, null, action, minBatchSize);
        }

        public static QueryAction ForEachParallel<T0, T1, T2>(this EntityManager em, Predicate<Archetype> query, EntityAction<T0, T1, T2> action, int minBatchSize = MinParallelBatchSize) {
            return ForEachParallel(em, query, 
                (in Entity entity, ref T0 comp0, ref T1 comp1, ref T2 comp2) => action(ref comp0, ref comp1, ref comp2), minBatchSize);
        }

        public static QueryAction ForEachParallel<T0, T1, T2>(this EntityManager em, EntityAction_E<T0, T1, T2> action, int minBatchSize = MinParallelBatchSize) {
            return ForEachParallel(em, null, action, minBatchSize);
        }
        
        
        public static QueryAction ForEachParallel<T0, T1, T2>(this EntityManager em, Predicate<Archetype> query, EntityAction_E<T0, T1, T2> action, int minBatchSize = MinParallelBatchSize) {
            query = IncludeTypesInQuery(query, typeof(T0), typeof(T1), typeof(T2));
            return new QueryAction(em, query, ForEachParallelAction(em, QueryParallelActionObj.Array(action, em.ParallelFor.WorkersCount), minBatchSize));
        }
        

        public static QueryAction ForEachParallel<T0, T1, T2, T3>(this EntityManager em, EntityAction<T0, T1, T2, T3> action, int minBatchSize = MinParallelBatchSize) {
            return ForEachParallel(em, null, action, minBatchSize);
        }

        public static QueryAction ForEachParallel<T0, T1, T2, T3>(this EntityManager em, Predicate<Archetype> query, EntityAction<T0, T1, T2, T3> action, int minBatchSize = MinParallelBatchSize) {
            return ForEachParallel(em, query, 
                (in Entity entity, ref T0 comp0, ref T1 comp1, ref T2 comp2, ref T3 comp3) =>
                    action(ref comp0, ref comp1, ref comp2, ref comp3), minBatchSize);
        }

        public static QueryAction ForEachParallel<T0, T1, T2, T3>(this EntityManager em, EntityAction_E<T0, T1, T2, T3> action, int minBatchSize = MinParallelBatchSize) {
            return ForEachParallel(em, null, action, minBatchSize);
        }

        public static QueryAction ForEachParallel<T0, T1, T2, T3>(this EntityManager em, Predicate<Archetype> query, EntityAction_E<T0, T1, T2, T3> action, int minBatchSize = MinParallelBatchSize) {
            query = IncludeTypesInQuery(query, typeof(T0), typeof(T1), typeof(T2), typeof(T3));
            return new QueryAction(em, query, ForEachParallelAction(em, QueryParallelActionObj.Array(action, em.ParallelFor.WorkersCount), minBatchSize));
        }
        

        public static QueryAction ForEachParallel<T0, T1, T2, T3, T4>(this EntityManager em, EntityAction<T0, T1, T2, T3, T4> action, int minBatchSize = MinParallelBatchSize) {
            return ForEachParallel(em, null, action, minBatchSize);
        }

        public static QueryAction ForEachParallel<T0, T1, T2, T3, T4>(this EntityManager em, Predicate<Archetype> query, EntityAction<T0, T1, T2, T3, T4> action, int minBatchSize = MinParallelBatchSize) {
            return ForEachParallel(em, query, 
                (in Entity entity, ref T0 comp0, ref T1 comp1, ref T2 comp2, ref T3 comp3, ref T4 comp4) =>
                    action(ref comp0, ref comp1, ref comp2, ref comp3, ref comp4), minBatchSize);
        }

        public static QueryAction ForEachParallel<T0, T1, T2, T3, T4>(this EntityManager em, EntityAction_E<T0, T1, T2, T3, T4> action, int minBatchSize = MinParallelBatchSize) {
            return ForEachParallel(em, null, action, minBatchSize);
        }

        public static QueryAction ForEachParallel<T0, T1, T2, T3, T4>(this EntityManager em, Predicate<Archetype> query, EntityAction_E<T0, T1, T2, T3, T4> action, int minBatchSize = MinParallelBatchSize) {
            query = IncludeTypesInQuery(query, typeof(T0), typeof(T1), typeof(T2), typeof(T3), typeof(T4));
            return new QueryAction(em, query, ForEachParallelAction(em, QueryParallelActionObj.Array(action, em.ParallelFor.WorkersCount), minBatchSize));
        }
        

        public static QueryAction ForEachParallel<T0, T1, T2, T3, T4, T5>(this EntityManager em, EntityAction<T0, T1, T2, T3, T4, T5> action, int minBatchSize = MinParallelBatchSize) {
            return ForEachParallel(em, null, action, minBatchSize);
        }

        public static QueryAction ForEachParallel<T0, T1, T2, T3, T4, T5>(this EntityManager em, Predicate<Archetype> query, EntityAction<T0, T1, T2, T3, T4, T5> action, int minBatchSize = MinParallelBatchSize) {
            return ForEachParallel(em, query, 
                (in Entity entity, ref T0 comp0, ref T1 comp1, ref T2 comp2, ref T3 comp3, ref T4 comp4, ref T5 comp5) =>
                    action(ref comp0, ref comp1, ref comp2, ref comp3, ref comp4, ref comp5), minBatchSize);
        }

        public static QueryAction ForEachParallel<T0, T1, T2, T3, T4, T5>(this EntityManager em, EntityAction_E<T0, T1, T2, T3, T4, T5> action, int minBatchSize = MinParallelBatchSize) {
            return ForEachParallel(em, null, action, minBatchSize);
        }
        
        public static QueryAction ForEachParallel<T0, T1, T2, T3, T4, T5>(this EntityManager em, Predicate<Archetype> query, EntityAction_E<T0, T1, T2, T3, T4, T5> action, int minBatchSize = MinParallelBatchSize) {
            query = IncludeTypesInQuery(query, typeof(T0), typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
            return new QueryAction(em, query, ForEachParallelAction(em, QueryParallelActionObj.Array(action, em.ParallelFor.WorkersCount), minBatchSize));
        }

        private const int MinParallelBatchSize = 5;

        #endregion


        public static bool HasComponent<T>(this EntityManager em, EntityId entity) {
            return em.EntityArchetype(entity).HasComponents<T>();
        }

        public static void Set<T>(this EntityManager em, EntityId entity, in T component) {
            em.Ref<T>(entity) = component;
        }

        public static IEnumerable<Entity> Entities(this EntityManager em) {
            return 
                em
                .ArchetypeEntityCollections
                .SelectMany(entityCollection => entityCollection.Entities)
                .Select(em.GetEntity);
        }

        public static IEnumerable<Entity> Entities(this EntityManager em, Predicate<Archetype> predicate) {
            return 
                em
                .ArchetypeEntityCollections
                .Where(entityCollection => predicate(entityCollection.Archetype))
                .SelectMany(entityCollection => entityCollection.Entities)
                .Select(em.GetEntity);
        }

        public static IEnumerable<ArchetypeEntityCollection> Select(this EntityManager entities, Predicate<Archetype> predicate) {
            return entities.ArchetypeEntityCollections.Where(x => predicate(x.Archetype));
        }

        public static Query Query(this EntityManager entities, Predicate<Archetype> predicate) {
            return new Query(entities, predicate);
        }

        public static Entity Single<T>(this EntityManager entities) {
            var collection = entities.Select(archetype => archetype.HasComponents<T>()).Single();
            return new Entity(collection.Entities.Single(), entities);
        }

        public static IEnumerable<T> GetAllSharedComponentValues<T>(this EntityManager entities) where T : ISharedComponent<T> {
            return entities.ArchetypeEntityCollections
                .Where(x => x.Archetype.HasSharedComponent<T>())
                .Select(x => x.Archetype.GetSharedComponent<T>());
        } 
    }
}
