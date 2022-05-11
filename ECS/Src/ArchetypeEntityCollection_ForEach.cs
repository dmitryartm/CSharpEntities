namespace ECS {
    public static class ArchetypeEntityCollection_ForEach {
        public static void ForEach<T>(this ArchetypeEntityCollection entities, EntityAction<T> action) {

            var componentIndex = entities.ComponentIndex<T>();

            for (var chunkIndex = 0; chunkIndex < entities.ChunkCount; ++chunkIndex) {
                var chunk = entities.Chunk(chunkIndex);
                var count = entities.EntityCountInChunk(chunkIndex);
                var components = chunk.Components<T>(componentIndex);

                for (var entityIndex = 0; entityIndex < count; ++entityIndex) {
                    action(ref components[entityIndex]);
                }
            }
        }

        public static void ForEach<T0, T1>(this ArchetypeEntityCollection entities, EntityAction<T0, T1> action) {
            
            var componentIndex0 = entities.ComponentIndex<T0>();
            var componentIndex1 = entities.ComponentIndex<T1>();

            for (var chunkIndex = 0; chunkIndex < entities.ChunkCount; ++chunkIndex) {
                var chunk = entities.Chunk(chunkIndex);
                var count = entities.EntityCountInChunk(chunkIndex);
                var components0 = chunk.Components<T0>(componentIndex0);
                var components1 = chunk.Components<T1>(componentIndex1);

                for (var entityIndex = 0; entityIndex < count; ++entityIndex) {
                    action(ref components0[entityIndex], ref components1[entityIndex]);
                }
            }
        }

        public static void ForEach<T0, T1, T2>(this ArchetypeEntityCollection entities, EntityAction<T0, T1, T2> action) {

            var componentIndex0 = entities.ComponentIndex<T0>();
            var componentIndex1 = entities.ComponentIndex<T1>();
            var componentIndex2 = entities.ComponentIndex<T2>();

            for (var chunkIndex = 0; chunkIndex < entities.ChunkCount; ++chunkIndex) {
                var chunk = entities.Chunk(chunkIndex);
                var count = entities.EntityCountInChunk(chunkIndex);
                var components0 = chunk.Components<T0>(componentIndex0);
                var components1 = chunk.Components<T1>(componentIndex1);
                var components2 = chunk.Components<T2>(componentIndex2);

                for (var entityIndex = 0; entityIndex < count; ++entityIndex) {
                    action(
                        ref components0[entityIndex],
                        ref components1[entityIndex],
                        ref components2[entityIndex]
                    );
                }
            }
        }

        public static void ForEach<T0, T1, T2, T3>(this ArchetypeEntityCollection entities, EntityAction<T0, T1, T2, T3> action) {

            var componentIndex0 = entities.ComponentIndex<T0>();
            var componentIndex1 = entities.ComponentIndex<T1>();
            var componentIndex2 = entities.ComponentIndex<T2>();
            var componentIndex3 = entities.ComponentIndex<T3>();

            for (var chunkIndex = 0; chunkIndex < entities.ChunkCount; ++chunkIndex) {
                var chunk = entities.Chunk(chunkIndex);
                var count = entities.EntityCountInChunk(chunkIndex);

                var components0 = chunk.Components<T0>(componentIndex0);
                var components1 = chunk.Components<T1>(componentIndex1);
                var components2 = chunk.Components<T2>(componentIndex2);
                var components3 = chunk.Components<T3>(componentIndex3);

                for (var entityIndex = 0; entityIndex < count; ++entityIndex) {
                    action(
                        ref components0[entityIndex],
                        ref components1[entityIndex],
                        ref components2[entityIndex],
                        ref components3[entityIndex]
                    );
                }
            }
        }

        public static void ForEach<T>(this ArchetypeEntityCollection entities, EntityAction_E<T> action) {

            var componentIndex = entities.ComponentIndex<T>();

            for (var chunkIndex = 0; chunkIndex < entities.ChunkCount; ++chunkIndex) {
                var chunk = entities.Chunk(chunkIndex);
                var count = entities.EntityCountInChunk(chunkIndex);
                var components = chunk.Components<T>(componentIndex);

                for (var entityIndex = 0; entityIndex < count; ++entityIndex) {
                    var entity = new Entity(chunk[entityIndex], entities.EntityManager);
                    action(entity, ref components[entityIndex]);
                }
            }
        }

        public static void ForEach<T0, T1>(this ArchetypeEntityCollection entities, EntityAction_E<T0, T1> action) {

            var componentIndex0 = entities.ComponentIndex<T0>();
            var componentIndex1 = entities.ComponentIndex<T1>();

            for (var chunkIndex = 0; chunkIndex < entities.ChunkCount; ++chunkIndex) {
                var chunk = entities.Chunk(chunkIndex);
                var count = entities.EntityCountInChunk(chunkIndex);
                var components0 = chunk.Components<T0>(componentIndex0);
                var components1 = chunk.Components<T1>(componentIndex1);

                for (var entityIndex = 0; entityIndex < count; ++entityIndex) {
                    var entity = new Entity(chunk[entityIndex], entities.EntityManager);
                    action(
                        entity,
                        ref components0[entityIndex],
                        ref components1[entityIndex]
                    );
                }
            }
        }

        public static void ForEach<T0, T1, T2>(this ArchetypeEntityCollection entities, EntityAction_E<T0, T1, T2> action) {

            var componentIndex0 = entities.ComponentIndex<T0>();
            var componentIndex1 = entities.ComponentIndex<T1>();
            var componentIndex2 = entities.ComponentIndex<T2>();

            for (var chunkIndex = 0; chunkIndex < entities.ChunkCount; ++chunkIndex) {
                var chunk = entities.Chunk(chunkIndex);
                var count = entities.EntityCountInChunk(chunkIndex);
                var components0 = chunk.Components<T0>(componentIndex0);
                var components1 = chunk.Components<T1>(componentIndex1);
                var components2 = chunk.Components<T2>(componentIndex2);

                for (var entityIndex = 0; entityIndex < count; ++entityIndex) {
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
