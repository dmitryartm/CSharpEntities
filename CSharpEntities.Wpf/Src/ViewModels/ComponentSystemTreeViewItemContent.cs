using System;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using CSharpEntities.Systems;
using CSharpEntities.Wpf.TreeListView.ViewModels;
using ReactiveUI;


namespace CSharpEntities.Wpf.ViewModels {
    public class ComponentSystemTreeViewItemContent : ReactiveObject, ITreeItemContent<ComponentSystemTreeViewItemContent>, IDisposable {
        public ComponentSystemTreeViewItemContent(IComponentSystem system) {
            this._system = system;

            this._executionTime.Subscribe(_ => this.RaisePropertyChanged(nameof(this.ExecutionTimeMs)));
            
            this._executionTimeMsStr =
                this._executionTime
                    .Window(TimeSpan.FromSeconds(0.7))
                    .SelectMany(x => x.StartWith(0.0f).Max())
                    .Select(x => x.ToString("0.00"))
                    .ToProperty(this, nameof(this.ExecutionTimeMsStr));

            
            var stopwatch = new Stopwatch();
            system
                .BeforeExecute
                .Subscribe(_ => {
                    stopwatch.Restart();
                })
                .DisposeWith(this._disposable);

            system
                .AfterExecute
                .Subscribe(_ => {
                    stopwatch.Stop();
                    this.ExecutionTimeMs = (stopwatch.ElapsedTicks / (float) TimeSpan.TicksPerMillisecond);
                })
                .DisposeWith(this._disposable);
        }

        public void Dispose() {
            this._disposable.Dispose();
            this._disposable.Clear();
        }

        
        private readonly CompositeDisposable _disposable = new CompositeDisposable();

        private readonly IComponentSystem _system;
            
        private readonly Behaviour<float> _executionTime = new(0f);

        public string Name => this._system.Name;

        
        public float ExecutionTimeMs {
            get => this._executionTime.Value;
            set => this._executionTime.Value = value;
        }

        private readonly ObservableAsPropertyHelper<string> _executionTimeMsStr;

        public string ExecutionTimeMsStr => this._executionTimeMsStr.Value;

        public ITreeViewItemModel<ComponentSystemTreeViewItemContent> TreeItem { get; set; }
        
        ITreeViewItemModel ITreeItemContent.TreeItem {
            get => this.TreeItem;
            set => this.TreeItem = (ITreeViewItemModel<ComponentSystemTreeViewItemContent>) value;
        }
    }
}