using System.Collections.Immutable;


namespace CSlns.Entities.Threading {
    public abstract class QueryParallelActionObj {

        public Action Action { get; protected set; }
        
        
        protected int WorkerIndex;

        protected int WorkerCount;

        
        protected int BatchSize(int chunkEntityCount) {
            return chunkEntityCount / this.WorkerCount;
        }

        protected int StartEntityIndex(int chunkEntityCount) {
            return this.BatchSize(chunkEntityCount) * this.WorkerIndex;
        }

        protected int EndEntityIndex(int chunkEntityCount) {
            return this.WorkerIndex == this.WorkerCount - 1 ? chunkEntityCount : (this.WorkerIndex + 1) * this.BatchSize(chunkEntityCount);
        }
        
        
        protected IReadOnlyList<ArchetypeEntityCollection> ArchetypeEntities;

        
        public void SetParameters(IReadOnlyList<ArchetypeEntityCollection> archetypeEntities, int workerIndex, int workerCount) {
            this.ArchetypeEntities = archetypeEntities;
            this.WorkerIndex = workerIndex;
            this.WorkerCount = workerCount;
        }


        public static ImmutableArray<QueryParallelActionObj> Array<T0>(EntityAction_E<T0> action, int length) {
            var objBuilder = ImmutableArray.CreateBuilder<QueryParallelActionObj>(length);
                
            for (var i = 0; i < length; ++i) {
                var obj = new QueryParallelActionObj<T0>(action);
                objBuilder.Add(obj);
            }

            return objBuilder.MoveToImmutable();
        }


        public static ImmutableArray<QueryParallelActionObj> Array<T0, T1>(EntityAction_E<T0, T1> action, int length) {
            var objBuilder = ImmutableArray.CreateBuilder<QueryParallelActionObj>(length);
                
            for (var i = 0; i < length; ++i) {
                var obj = new QueryParallelActionObj<T0, T1>(action);
                objBuilder.Add(obj);
            }

            return objBuilder.MoveToImmutable();
        }


        public static ImmutableArray<QueryParallelActionObj> Array<T0, T1, T2>(EntityAction_E<T0, T1, T2> action, int length) {
            var objBuilder = ImmutableArray.CreateBuilder<QueryParallelActionObj>(length);
                
            for (var i = 0; i < length; ++i) {
                var obj = new QueryParallelActionObj<T0, T1, T2>(action);
                objBuilder.Add(obj);
            }

            return objBuilder.MoveToImmutable();
        }


        public static ImmutableArray<QueryParallelActionObj> Array<T0, T1, T2, T3>(EntityAction_E<T0, T1, T2, T3> action, int length) {
            var objBuilder = ImmutableArray.CreateBuilder<QueryParallelActionObj>(length);
                
            for (var i = 0; i < length; ++i) {
                var obj = new QueryParallelActionObj<T0, T1, T2, T3>(action);
                objBuilder.Add(obj);
            }

            return objBuilder.MoveToImmutable();
        }


        public static ImmutableArray<QueryParallelActionObj> Array<T0, T1, T2, T3, T4>(EntityAction_E<T0, T1, T2, T3, T4> action, int length) {
            var objBuilder = ImmutableArray.CreateBuilder<QueryParallelActionObj>(length);
                
            for (var i = 0; i < length; ++i) {
                var obj = new QueryParallelActionObj<T0, T1, T2, T3, T4>(action);
                objBuilder.Add(obj);
            }

            return objBuilder.MoveToImmutable();
        }


        public static ImmutableArray<QueryParallelActionObj> Array<T0, T1, T2, T3, T4, T5>(EntityAction_E<T0, T1, T2, T3, T4, T5> action, int length) {
            var objBuilder = ImmutableArray.CreateBuilder<QueryParallelActionObj>(length);
                
            for (var i = 0; i < length; ++i) {
                var obj = new QueryParallelActionObj<T0, T1, T2, T3, T4, T5>(action);
                objBuilder.Add(obj);
            }

            return objBuilder.MoveToImmutable();
        }
    }


    public class QueryParallelActionObj<T0> : QueryParallelActionObj {
        public QueryParallelActionObj(EntityAction_E<T0> action) {
            this.Action = Execute;

            void Execute() {
                for (var i = 0; i < this.ArchetypeEntities.Count; ++i) {
                    var entities = this.ArchetypeEntities[i];

                    var componentIndex0 = entities.ComponentIndex<T0>();

                    for (var chunkIndex = 0; chunkIndex < entities.ChunkCount; ++chunkIndex) {
                        var chunk = entities.Chunk(chunkIndex);
                        var components0 = chunk.Components<T0>(componentIndex0);

                        var chunkEntityCount = entities.EntityCountInChunk(chunkIndex);
                        var endIndex = this.EndEntityIndex(chunkEntityCount);

                        for (var entityIndex = this.StartEntityIndex(chunkEntityCount); entityIndex < endIndex; ++entityIndex) {
                            var entity = new Entity(chunk[entityIndex], entities.EntityManager);
                            action(
                                entity,
                                ref components0[entityIndex]
                            );
                        }
                    }
                }
            }
        }
    }


    public class QueryParallelActionObj<T0, T1> : QueryParallelActionObj {
        public QueryParallelActionObj(EntityAction_E<T0, T1> action) {
            this.Action = Execute;

            void Execute() {
                for (var i = 0; i < this.ArchetypeEntities.Count; ++i) {
                    var entities = this.ArchetypeEntities[i];

                    var componentIndex0 = entities.ComponentIndex<T0>();
                    var componentIndex1 = entities.ComponentIndex<T1>();

                    for (var chunkIndex = 0; chunkIndex < entities.ChunkCount; ++chunkIndex) {
                        var chunk = entities.Chunk(chunkIndex);
                        var components0 = chunk.Components<T0>(componentIndex0);
                        var components1 = chunk.Components<T1>(componentIndex1);

                        var chunkEntityCount = entities.EntityCountInChunk(chunkIndex);
                        var endIndex = this.EndEntityIndex(chunkEntityCount);

                        for (var entityIndex = this.StartEntityIndex(chunkEntityCount); entityIndex < endIndex; ++entityIndex) {
                            var entity = new Entity(chunk[entityIndex], entities.EntityManager);
                            action(
                                entity,
                                ref components0[entityIndex],
                                ref components1[entityIndex]
                            );
                        }
                    }
                }
            }
        }
    }


    public class QueryParallelActionObj<T0, T1, T2> : QueryParallelActionObj {
        public QueryParallelActionObj(EntityAction_E<T0, T1, T2> action) {
            this.Action = Execute;

            void Execute() {
                for (var i = 0; i < this.ArchetypeEntities.Count; ++i) {
                    var entities = this.ArchetypeEntities[i];

                    var componentIndex0 = entities.ComponentIndex<T0>();
                    var componentIndex1 = entities.ComponentIndex<T1>();
                    var componentIndex2 = entities.ComponentIndex<T2>();

                    for (var chunkIndex = 0; chunkIndex < entities.ChunkCount; ++chunkIndex) {
                        var chunk = entities.Chunk(chunkIndex);
                        var components0 = chunk.Components<T0>(componentIndex0);
                        var components1 = chunk.Components<T1>(componentIndex1);
                        var components2 = chunk.Components<T2>(componentIndex2);

                        var chunkEntityCount = entities.EntityCountInChunk(chunkIndex);
                        var endIndex = this.EndEntityIndex(chunkEntityCount);

                        for (var entityIndex = this.StartEntityIndex(chunkEntityCount); entityIndex < endIndex; ++entityIndex) {
                            var entity = new Entity(chunk[entityIndex], entities.EntityManager);
                            action(
                                entity,
                                ref components0[entityIndex],
                                ref components1[entityIndex],
                                ref components2[entityIndex]
                            );
                        }
                    }
                }
            }
        }
    }

    
    public class QueryParallelActionObj<T0, T1, T2, T3> : QueryParallelActionObj {
        public QueryParallelActionObj(EntityAction_E<T0, T1, T2, T3> action) {
            this.Action = Execute;
        
            void Execute() {
                for (var i = 0; i < this.ArchetypeEntities.Count; ++i) {
                    var entities = this.ArchetypeEntities[i];

                    var componentIndex0 = entities.ComponentIndex<T0>();
                    var componentIndex1 = entities.ComponentIndex<T1>();
                    var componentIndex2 = entities.ComponentIndex<T2>();
                    var componentIndex3 = entities.ComponentIndex<T3>();

                    for (var chunkIndex = 0; chunkIndex < entities.ChunkCount; ++chunkIndex) {
                        var chunk = entities.Chunk(chunkIndex);
                        var components0 = chunk.Components<T0>(componentIndex0);
                        var components1 = chunk.Components<T1>(componentIndex1);
                        var components2 = chunk.Components<T2>(componentIndex2);
                        var components3 = chunk.Components<T3>(componentIndex3);

                        var chunkEntityCount = entities.EntityCountInChunk(chunkIndex);
                        var endIndex = this.EndEntityIndex(chunkEntityCount);
                        
                        for (var entityIndex = this.StartEntityIndex(chunkEntityCount); entityIndex < endIndex; ++entityIndex) {
                            var entity = new Entity(chunk[entityIndex], entities.EntityManager);
                            action(
                                entity,
                                ref components0[entityIndex],
                                ref components1[entityIndex],
                                ref components2[entityIndex],
                                ref components3[entityIndex]
                            );
                        }
                    }
                }
            }
        }
    }

    
    public class QueryParallelActionObj<T0, T1, T2, T3, T4> : QueryParallelActionObj {
        public QueryParallelActionObj(EntityAction_E<T0, T1, T2, T3, T4> action) {
            this.Action = Execute;
        
            void Execute() {
                for (var i = 0; i < this.ArchetypeEntities.Count; ++i) {
                    var entities = this.ArchetypeEntities[i];

                    var componentIndex0 = entities.ComponentIndex<T0>();
                    var componentIndex1 = entities.ComponentIndex<T1>();
                    var componentIndex2 = entities.ComponentIndex<T2>();
                    var componentIndex3 = entities.ComponentIndex<T3>();
                    var componentIndex4 = entities.ComponentIndex<T4>();

                    for (var chunkIndex = 0; chunkIndex < entities.ChunkCount; ++chunkIndex) {
                        var chunk = entities.Chunk(chunkIndex);
                        var components0 = chunk.Components<T0>(componentIndex0);
                        var components1 = chunk.Components<T1>(componentIndex1);
                        var components2 = chunk.Components<T2>(componentIndex2);
                        var components3 = chunk.Components<T3>(componentIndex3);
                        var components4 = chunk.Components<T4>(componentIndex4);

                        var chunkEntityCount = entities.EntityCountInChunk(chunkIndex);
                        var endIndex = this.EndEntityIndex(chunkEntityCount);
                        
                        for (var entityIndex = this.StartEntityIndex(chunkEntityCount); entityIndex < endIndex; ++entityIndex) {
                            var entity = new Entity(chunk[entityIndex], entities.EntityManager);
                            action(
                                entity,
                                ref components0[entityIndex],
                                ref components1[entityIndex],
                                ref components2[entityIndex],
                                ref components3[entityIndex],
                                ref components4[entityIndex]
                            );
                        }
                    }
                }
            }
        }
    }


    public class QueryParallelActionObj<T0, T1, T2, T3, T4, T5> : QueryParallelActionObj {
        public QueryParallelActionObj(EntityAction_E<T0, T1, T2, T3, T4, T5> action) {
            this.Action = Execute;
        
            void Execute() {
                for (var i = 0; i < this.ArchetypeEntities.Count; ++i) {
                    var entities = this.ArchetypeEntities[i];

                    var componentIndex0 = entities.ComponentIndex<T0>();
                    var componentIndex1 = entities.ComponentIndex<T1>();
                    var componentIndex2 = entities.ComponentIndex<T2>();
                    var componentIndex3 = entities.ComponentIndex<T3>();
                    var componentIndex4 = entities.ComponentIndex<T4>();
                    var componentIndex5 = entities.ComponentIndex<T5>();

                    for (var chunkIndex = 0; chunkIndex < entities.ChunkCount; ++chunkIndex) {
                        var chunk = entities.Chunk(chunkIndex);
                        var components0 = chunk.Components<T0>(componentIndex0);
                        var components1 = chunk.Components<T1>(componentIndex1);
                        var components2 = chunk.Components<T2>(componentIndex2);
                        var components3 = chunk.Components<T3>(componentIndex3);
                        var components4 = chunk.Components<T4>(componentIndex4);
                        var components5 = chunk.Components<T5>(componentIndex5);

                        var chunkEntityCount = entities.EntityCountInChunk(chunkIndex);
                        var endIndex = this.EndEntityIndex(chunkEntityCount);
                        
                        for (var entityIndex = this.StartEntityIndex(chunkEntityCount); entityIndex < endIndex; ++entityIndex) {
                            var entity = new Entity(chunk[entityIndex], entities.EntityManager);
                            action(
                                entity,
                                ref components0[entityIndex],
                                ref components1[entityIndex],
                                ref components2[entityIndex],
                                ref components3[entityIndex],
                                ref components4[entityIndex],
                                ref components5[entityIndex]
                            );
                        }
                    }
                }
            }
        }
    }
    
}