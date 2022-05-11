using System.Threading;
using CSlns.Entities;
using CSlns.Entities.Systems;


namespace glTFViewer.World; 


public class MainWorld : CSlns.Entities.World {

    public MainWorld() {
        this.RootSystem = new InlineSystem(this, "RootSystem") {
            new InlineSystem(this, "Child 0", _ => Thread.Sleep(2)),
            new InlineSystem(this, "Child 1", _ => Thread.Sleep(4)),
        };

        for (var i = 0; i < 10; ++i) {
            this.Entities.CreateEntity<int, float, string>();
        }

        for (var i = 0; i < 5; ++i) {
            this.Entities.CreateEntity<int, float>();
        }
    }

}