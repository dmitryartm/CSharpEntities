using System;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using CSlns.Std;
using CSlns.Wpf;
using CSlns.Wpf.TreeListView.ViewModels;
using ReactiveUI;

namespace ECS.WPF.ViewModels {
    public class ComponentSystemTreeViewItemContent : ReactiveObject, ITreeItemContent<ComponentSystemTreeViewItemContent>, IDisposable {
        public ComponentSystemTreeViewItemContent(IComponentSystem system) {
            _system = system;

            this._executionTime.RaisePropertyChanged(this, nameof(this.ExecutionTimeMs));
            
            _executionTimeMsStr =
                this._executionTime
                    .Window(TimeSpan.FromSeconds(0.7))
                    .SelectMany(x => x.StartWith(0.0f).Max())
                    .Select(x => x.ToString("0.00"))
                    .ToProperty(this, nameof(ExecutionTimeMsStr));

            
            var stopwatch = new Stopwatch();
            system
                .BeforeExecute
                .Subscribe(_ => {
                    stopwatch.Restart();
                })
                .DisposeWith(_disposable);

            system
                .AfterExecute
                .Subscribe(_ => {
                    stopwatch.Stop();
                    ExecutionTimeMs = (stopwatch.ElapsedTicks / (float) TimeSpan.TicksPerMillisecond);
                })
                .DisposeWith(_disposable);
        }

        public void Dispose() {
            _disposable.Dispose();
            _disposable.Clear();
        }

        
        private readonly CompositeDisposable _disposable = new CompositeDisposable();

        private readonly IComponentSystem _system;
            
        private readonly Behaviour<float> _executionTime = new(0f);

        public string Name => _system.Name;

        
        public float ExecutionTimeMs {
            get => _executionTime.Value;
            set => _executionTime.Value = value;
        }

        private readonly ObservableAsPropertyHelper<string> _executionTimeMsStr;

        public string ExecutionTimeMsStr => _executionTimeMsStr.Value;

        public ITreeViewItemModel<ComponentSystemTreeViewItemContent> TreeItem { get; set; }
        
        ITreeViewItemModel ITreeItemContent.TreeItem {
            get => TreeItem;
            set => TreeItem = (ITreeViewItemModel<ComponentSystemTreeViewItemContent>) value;
        }
    }
}