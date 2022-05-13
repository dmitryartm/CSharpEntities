using SharpDX;
using SharpDX.Direct3D9;
using SharpDX.Mathematics.Interop;


namespace glTFViewer.Utils;


internal static class ShaderRegisters {
    public static void SetLightDir(this Device device, Vector3 lightdir) {
        device.SetVertexShaderConstant(8, ToArray(new Vector4(lightdir, 0f)));
    }


    public static unsafe void SetLocalToWorld(this Device device, Matrix matrix) {
        var rawMatrixRef = (RawMatrix*) &matrix;
        device.SetVertexShaderConstant(4, rawMatrixRef);
    }


    public static unsafe void SetViewProj(this Device device, Matrix matrix) {
        var rawMatrixRef = (RawMatrix*) &matrix;
        device.SetVertexShaderConstant(0, rawMatrixRef);
    }


    public static void SetColor(this Device device, ColorBGRA color) {
        device.SetVertexShaderConstant(9, ToArray(color.ToVector4()));
    }


    public static void SetColor(this Device device, Color4 color) {
        device.SetVertexShaderConstant(9, ToArray(color));
    }


    public static void SetColor(this Device device, Color color) {
        device.SetVertexShaderConstant(9, ToArray(color.ToVector4()));
    }


    public static void SetBorderThickness(this Device device, Size2 textureSize, float borderThickness) {
        device.SetPixelShaderConstant(11, ToArray(new Vector4(borderThickness / textureSize.Width)));
        device.SetPixelShaderConstant(12, ToArray(new Vector4(borderThickness / textureSize.Height)));
    }


    public static void SetSelectionBorderColor(this Device device, Color4 color) {
        device.SetPixelShaderConstant(13, ToArray(color));
    }


    public static void SetTextureSelectionColor(this Device device, Color4 color) {
        device.SetPixelShaderConstant(14, ToArray(color));
    }


    public static void SetAlpha(this Device device, float value) {
        device.SetPixelShaderConstant(10, ToArray(new Vector4(value)));
    }


    private static float[] ToArray(in Vector4 value) {
        _bufferFloat4[0] = value.X;
        _bufferFloat4[1] = value.Y;
        _bufferFloat4[2] = value.Z;
        _bufferFloat4[3] = value.W;
        return _bufferFloat4;
    }


    private static readonly float[] _bufferFloat4 = new float[4];
}