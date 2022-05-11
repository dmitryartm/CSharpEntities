using System.IO;
using System.Reflection;
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
            .WriteTo.File(Path.Combine(dir, "Logs", "example.log"))
            .CreateLogger();
    }
}