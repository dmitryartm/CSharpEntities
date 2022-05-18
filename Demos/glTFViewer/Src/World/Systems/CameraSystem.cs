using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CSlns.Entities;
using CSlns.Entities.Systems;
using glTFViewer.World.Components;
using SharpDX;
using Quaternion = System.Numerics.Quaternion;


namespace glTFViewer.World.Systems;


public class CameraSystem : ComponentSystem<MainWorld> {


    public CameraSystem(MainWorld world) : base(world) {
    }


    protected override void OnStart() {
        this._surfaceManagerSystem = this.World.Get<SurfaceManagerSystem>();
        this.Entities.CreateEntity(new MainCamera() {
            RotationSpeed = 2f * (float) Math.PI / 15f,
            Theta = (float) Math.PI / 10f,
        });
    }


    protected override void OnExecute() {
        if (this._surfaceManagerSystem == null) {
            return;
        }

        var camera = this.Entities.Single<MainCamera>().Get<MainCamera>();
        camera.UpdateRotation(this.World.DeltaTime);

        var surfaceSize = this._surfaceManagerSystem.SurfaceSize;
        camera.SetAspectRatio(surfaceSize.Width, surfaceSize.Height);
    }


    private SurfaceManagerSystem? _surfaceManagerSystem;

}