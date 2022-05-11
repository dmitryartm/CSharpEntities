using System.Collections.Immutable;


namespace CSlns.Entities.Threading {
    public class ParallelFor : IDisposable {
        public ParallelFor(int threads) {
            var workersCount = threads - 1;
            var workersBuilder = ImmutableArray.CreateBuilder<WorkerThread>(workersCount);
            for (var i = 0; i < workersCount; ++i) {
                workersBuilder.Add(new WorkerThread());
            }
    
            this._workers = workersBuilder.MoveToImmutable();
        }
        
        public void Dispose() {
            foreach (var worker in this._workers) {
                worker.Dispose();
            }
        }
    
        
        private readonly ImmutableArray<WorkerThread> _workers;


        public int WorkersCount => this._workers.Length + 1;
        

        private void Schedule(int i, Action action) {
            if (i == 0) {
                action();
            }
            else {
                this._workers[i - 1].Schedule(action);
            }
        }


        private bool WorkerIsRunning(int i) {
            if (i == 0) {
                return false;
            }
            else {
                return this._workers[i - 1].IsRunning;
            }
        }


        private bool IsRunning {
            get {
                for (var i = this.WorkersCount - 1; i >= 0; --i) {
                    if (this.WorkerIsRunning(i)) {
                        return true;
                    }
                }

                return false;
            }
        }

        private void Wait() {
            while (this.IsRunning) { }
        }
        
        public void Execute(IReadOnlyList<Action> actions) {
            if (actions.Count > this.WorkersCount) {
                throw new ArgumentException("Unable to Execute parallel actions. More actions than workers.");
            }

            // Reverted order because first Schedule is synchronous
            for (var i = actions.Count - 1; i >= 0 ; --i) {
                this.Schedule(i, actions[i]);
            }

            this.Wait();
        }


        public void Execute(IReadOnlyList<QueryParallelActionObj> actionObjs, IReadOnlyList<ArchetypeEntityCollection> archetypeEntities, int minBatchSize) {
            if (archetypeEntities.Count == 0) {
                return;
            }
            
            var workersCount = this.WorkersCount;
            var minEntityCount = minBatchSize * workersCount;

            var entityCount = 0;
            for (var i = 0; i < archetypeEntities.Count; ++i) {
                entityCount += archetypeEntities[0].EntityCount;

                if (entityCount >= minEntityCount) {
                    break;
                }
            }

            if (entityCount < minEntityCount) {
                workersCount = 1;
            }
            
            for (var workerIndex = 0; workerIndex < workersCount; ++workerIndex) {
                actionObjs[workerIndex].SetParameters(archetypeEntities, workerIndex, workersCount);
            }

            // Reverted order because first Schedule is synchronous
            for (var i = workersCount - 1; i >= 0 ; --i) {
                this.Schedule(i, actionObjs[i].Action);
            }

            this.Wait();
        }
        
    }
}