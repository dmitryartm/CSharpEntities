using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using glTFViewer.Utils;
using SharpDX;


namespace glTFViewer.Views;


public record struct DXRenderArg(float DeltaTime);


public partial class DXControl : UserControl {

    public DXControl() {
        this.InitializeComponent();

        var (dpiX, dpiY) = ScreenUtil.Dpi();
        this.D3DImage = new D3DImage(dpiX, dpiY);
        this._Image.Source = this.D3DImage;

        this.ImageSizeObs =
            Observable
                .FromEventPattern<SizeChangedEventArgs>(this, nameof(this.SizeChanged))
                .Select(x => x.EventArgs.NewSize)
                .StartWith(new Size(this.ActualWidth, this.ActualHeight))
                .Select(ScreenUtil.WpfToScreen)
                .DistinctUntilChanged()
                .Publish()
                .RefCount();

        this.ImageSizeObs.Subscribe(size => this.ImageSize = size);

        this.DXRender =
            Observable
                .FromEventPattern(
                    h => CompositionTarget.Rendering += h,
                    h => CompositionTarget.Rendering -= h)
                .Select(x => {
                    var renderingArgs = (RenderingEventArgs) x.EventArgs;
                    return renderingArgs.RenderingTime;
                })
                .DistinctUntilChanged()
                .Scan((prev: TimeSpan.Zero, current: TimeSpan.Zero), (pair, current) => (pair.current, current))
                .Select(time => new DXRenderArg((float) (time.current - time.prev).TotalSeconds))
                .Where(arg => this.ImageSizeIsValid)
                .Publish()
                .RefCount();
    }


    public D3DImage D3DImage { get; }


    public IObservable<Size2> ImageSizeObs { get; }
    
    
    public Size2 ImageSize { get; private set; }


    public bool ImageSizeIsValid => this.ImageSize.Height > 0 && this.ImageSize.Width > 0;
    
    
    public IObservable<DXRenderArg> DXRender { get; }

}