using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Serilog;


namespace glTFViewer;


/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application {
    
    public App() {
        var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File(Path.Combine(dir, "Logs", "gltfViewer.log"))
            .CreateLogger();

        this.DispatcherUnhandledException += (_, args) => {
            const string msg = "Unhandled dispatcher exception";
            Log.Error(args.Exception, msg);
            MessageBox.Show(msg + Environment.NewLine + args.Exception.Message, "ERROR", MessageBoxButton.OK,
                MessageBoxImage.Error);
            this.Shutdown();
        };

        TaskScheduler.UnobservedTaskException += (_, args) => {
            const string msg = "Unobserved task exception";
            Log.Error(args.Exception, msg);
            MessageBox.Show(msg + Environment.NewLine + args.Exception.Message, "ERROR", MessageBoxButton.OK,
                MessageBoxImage.Error);
            this.Shutdown();
        };
    }
    
}