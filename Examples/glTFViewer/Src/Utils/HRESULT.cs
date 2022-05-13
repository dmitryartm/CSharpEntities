using System.Runtime.InteropServices;
using System.Security.Permissions;


namespace glTFViewer.Utils; 


public static class HRESULT {
    public static void Check(int hr) {
        Marshal.ThrowExceptionForHR(hr);
    }
}