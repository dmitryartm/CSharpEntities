using System;
using SharpDX;


namespace glTFViewer.World.Components;


public class MainCamera {
    public Vector3 Origin;
    public float Radius;
    public float Theta;
    public float Phi;
    public float RotationSpeed;

    public Size2F MinimumFrustumSize;
    public float AspectRatio;
    public float ZNear;
    public float ZFar;


    public Vector3 EyePosition() {
        return this.Origin + new Vector3(
            (float) (this.Radius * Math.Cos(this.Theta) * Math.Cos(this.Phi)),
            (float) (this.Radius * Math.Cos(this.Theta) * Math.Sin(this.Phi)),
            (float) (this.Radius * Math.Sin(this.Theta)));
    }


    public void UpdateRotation(float deltaTime) {
        this.Phi = (this.Phi + this.RotationSpeed * deltaTime) % (2f * (float) Math.PI);
    }


    public void SetAspectRatio(float width, float height) {
        this.AspectRatio = width / height;
    }


    public Size2F ViewFrustumSize() {
        var height = Math.Max(this.MinimumFrustumSize.Height, this.MinimumFrustumSize.Width / this.AspectRatio);
        var width = this.AspectRatio * height;
        return new Size2F(width, height);
    }


    public Matrix ViewMatrix() {
        return Matrix.LookAtRH(this.EyePosition(), this.Origin, new Vector3(0f, 0f, 1f));
    }


    public Matrix ProjectionMatrix() {
        var frustumSize = this.ViewFrustumSize();
        return Matrix.OrthoRH(frustumSize.Width, frustumSize.Height, this.ZNear, this.ZFar);
    }


    public Matrix ViewProjMatrix() => Matrix.Multiply(this.ViewMatrix(), this.ProjectionMatrix());

}