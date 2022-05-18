using System;
using System.Runtime.InteropServices;
using SharpDX.Direct3D9;


namespace glTFViewer.Utils;


public interface IDirect3DLib {
    int LoadVertexShaderFromFile(IntPtr device, string filename, out IntPtr shader);
    int LoadPixelShaderFromFile(IntPtr device, string filename, out IntPtr shader);
    int SetPixelShader(IntPtr device, IntPtr shader);
    int SetVertexShader(IntPtr device, IntPtr shader);
    IntPtr CreateDummyWindow();
    void DestroyDummyWindow(IntPtr hwnd);
}


internal static class Direct3DLib {

    public static readonly IDirect3DLib Inst;


    static Direct3DLib() {
        Inst = Environment.Is64BitProcess ? new Impl_x64() : new Impl_x86();
    }


    private class Impl_x64 : IDirect3DLib {

        private static class Native {
            [DllImport("x64/Direct3DLib", CharSet = CharSet.Unicode)]
            public static extern int LoadVertexShaderFromFile(IntPtr device, string filename, out IntPtr shader);


            [DllImport("x64/Direct3DLib", CharSet = CharSet.Unicode)]
            public static extern int LoadPixelShaderFromFile(IntPtr device, string filename, out IntPtr shader);


            [DllImport("x64/Direct3DLib")]
            public static extern int SetPixelShader(IntPtr device, IntPtr shader);


            [DllImport("x64/Direct3DLib")]
            public static extern int SetVertexShader(IntPtr device, IntPtr shader);


            [DllImport("x64/Direct3DLib")]
            public static extern IntPtr CreateDummyWindow();


            [DllImport("x64/Direct3DLib")]
            public static extern void DestroyDummyWindow(IntPtr hwnd);
        }


        public int LoadVertexShaderFromFile(IntPtr device, string filename, out IntPtr shader) =>
            Native.LoadVertexShaderFromFile(device, filename, out shader);


        public int LoadPixelShaderFromFile(IntPtr device, string filename, out IntPtr shader) =>
            Native.LoadPixelShaderFromFile(device, filename, out shader);


        public int SetPixelShader(IntPtr device, IntPtr shader) =>
            Native.SetPixelShader(device, shader);


        public int SetVertexShader(IntPtr device, IntPtr shader) =>
            Native.SetVertexShader(device, shader);


        public IntPtr CreateDummyWindow() =>
            Native.CreateDummyWindow();


        public void DestroyDummyWindow(IntPtr hwnd) =>
            Native.DestroyDummyWindow(hwnd);
    }


    private class Impl_x86 : IDirect3DLib {

        private static class Native {
            [DllImport("x86/Direct3DLib", CharSet = CharSet.Unicode)]
            public static extern int LoadVertexShaderFromFile(IntPtr device, string filename, out IntPtr shader);


            [DllImport("x86/Direct3DLib", CharSet = CharSet.Unicode)]
            public static extern int LoadPixelShaderFromFile(IntPtr device, string filename, out IntPtr shader);


            [DllImport("x86/Direct3DLib")]
            public static extern int SetPixelShader(IntPtr device, IntPtr shader);


            [DllImport("x86/Direct3DLib")]
            public static extern int SetVertexShader(IntPtr device, IntPtr shader);


            [DllImport("x86/Direct3DLib")]
            public static extern IntPtr CreateDummyWindow();


            [DllImport("x86/Direct3DLib")]
            public static extern void DestroyDummyWindow(IntPtr hwnd);
        }


        public int LoadVertexShaderFromFile(IntPtr device, string filename, out IntPtr shader) =>
            Native.LoadVertexShaderFromFile(device, filename, out shader);


        public int LoadPixelShaderFromFile(IntPtr device, string filename, out IntPtr shader) =>
            Native.LoadPixelShaderFromFile(device, filename, out shader);


        public int SetPixelShader(IntPtr device, IntPtr shader) =>
            Native.SetPixelShader(device, shader);


        public int SetVertexShader(IntPtr device, IntPtr shader) =>
            Native.SetVertexShader(device, shader);


        public IntPtr CreateDummyWindow() =>
            Native.CreateDummyWindow();


        public void DestroyDummyWindow(IntPtr hwnd) =>
            Native.DestroyDummyWindow(hwnd);
    }

}


internal static class _Device {
    public static VertexShaderPtr LoadVertexShaderFromFile(this Device device, string filename) {
        return new VertexShaderPtr(device, filename);
    }


    public static PixelShaderPtr LoadPixelShaderFromFile(this Device device, string filename) {
        return new PixelShaderPtr(device, filename);
    }


    public static void SetVertexShader(this Device device, VertexShaderPtr vs) {
        HRESULT.Check(Direct3DLib.Inst.SetVertexShader(device.NativePointer, vs.NativePointer));
    }


    public static void SetPixelShader(this Device device, PixelShaderPtr ps) {
        HRESULT.Check(Direct3DLib.Inst.SetPixelShader(device.NativePointer, ps.NativePointer));
    }
}


internal class VertexShaderPtr : IDisposable {
    public VertexShaderPtr(Device device, string fileName) {
        HRESULT.Check(Direct3DLib.Inst.LoadVertexShaderFromFile(device.NativePointer, fileName, out this._shader));
    }


    readonly IntPtr _shader;

    public IntPtr NativePointer => this._shader;


    public void Dispose() {
        Marshal.Release(this._shader);
    }
}


internal class PixelShaderPtr : IDisposable {
    public PixelShaderPtr(Device device, string fileName) {
        HRESULT.Check(Direct3DLib.Inst.LoadPixelShaderFromFile(device.NativePointer, fileName, out this._shader));
    }


    readonly IntPtr _shader;

    public IntPtr NativePointer => this._shader;


    public void Dispose() {
        Marshal.Release(this._shader);
    }
}