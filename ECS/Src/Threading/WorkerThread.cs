using System;
using System.Threading;

namespace ECS.Threading {
    public class WorkerThread : IDisposable {
        public WorkerThread() {
            this.Thread = new Thread(this.Loop) {
                IsBackground = true
            };

            this.StartEvent = new AutoResetEvent(false);
            this.Thread.Start();
        }

        public void Dispose() {
            if (!this.IsDisposed) {
                if (this.IsRunning) {
                    this.WaitUnsafe();
                }
            
                this.IsDisposed = true;
                this.Start();
                this.WaitUnsafe();
                
                this.StartEvent.Dispose();
            }
        }
        
        
        private int _isDisposed;

        public bool IsDisposed {
            get => this._isDisposed == 1;
            private set => Interlocked.Exchange(ref this._isDisposed, value ? 1 : 0);
        }
        

        private int _isRunning;
        
        public bool IsRunning {
            get => this._isRunning == 1;
            private set => Interlocked.Exchange(ref this._isRunning, value ? 1 : 0);
        }


        private readonly Thread Thread;
        private readonly AutoResetEvent StartEvent;

        
        private Action Action;

        
        private void Loop() {
            while (true) {
                this.StartEvent.WaitOne();

                if (this.IsDisposed) {
                    this.IsRunning = false;
                    return;
                }

                this.Action();

                this.IsRunning = false;
            }
        }

        
        public void Schedule(Action action) {
            if (this.IsRunning) {
                throw new InvalidOperationException($"{nameof(WorkerThread)} cannot Schedule. Already running.");
            }

            this.Action = action;

            this.Start();
        }


        private void Start() {
            this.IsRunning = true;
            this.StartEvent.Set();
        }


        public void WaitUnsafe() {
            while (this.IsRunning) {
                Thread.Yield();
            }
        }
    }
}