// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}


#include <atlbase.h>

inline void check(const HRESULT& hr) {
	if (FAILED(hr)) {
		throw hr;
	}
}

extern "C" HRESULT WINAPI LoadVertexShaderFromFile(IDirect3DDevice9* device, LPCWSTR filename, IDirect3DVertexShader9** shader) {
	try {
		CComPtr<ID3DBlob> buffer;
		check(D3DReadFileToBlob(filename, &buffer));
		check(device->CreateVertexShader((DWORD*)buffer->GetBufferPointer(), shader));
		return S_OK;
	}
	catch (const HRESULT& hr) {
		return hr;
	}
}

extern "C" HRESULT WINAPI LoadPixelShaderFromFile(IDirect3DDevice9* device, LPCWSTR filename, IDirect3DPixelShader9** shader) {
	try {
		CComPtr<ID3DBlob> buffer;
		check(D3DReadFileToBlob(filename, &buffer));
		check(device->CreatePixelShader((DWORD*)buffer->GetBufferPointer(), shader));
		return S_OK;
	}
	catch (const HRESULT& hr) {
		return hr;
	}
}

extern "C" HRESULT WINAPI SetPixelShader(IDirect3DDevice9* device, IDirect3DPixelShader9* shader) {
	return device->SetPixelShader(shader);
}

extern "C" HRESULT WINAPI SetVertexShader(IDirect3DDevice9* device, IDirect3DVertexShader9* shader) {
	return device->SetVertexShader(shader);
}

const TCHAR* dummy_window_class_name = TEXT("_d3dimage");

extern "C" HWND CreateDummyWindow() {
	WNDCLASS wndclass;
	wndclass.style = CS_HREDRAW | CS_VREDRAW;
	wndclass.lpfnWndProc = DefWindowProc;
	wndclass.cbClsExtra = 0;
	wndclass.cbWndExtra = 0;
	wndclass.hInstance = NULL;
	wndclass.hIcon = LoadIcon(NULL, IDI_APPLICATION);
	wndclass.hCursor = LoadCursor(NULL, IDC_ARROW);
	wndclass.hbrBackground = (HBRUSH) GetStockObject (WHITE_BRUSH);
	wndclass.lpszMenuName = NULL;
	wndclass.lpszClassName = dummy_window_class_name;

	RegisterClass(&wndclass);

	return CreateWindow(dummy_window_class_name, dummy_window_class_name,
		WS_OVERLAPPEDWINDOW, 0, 0, 0, 0, nullptr, nullptr, nullptr, nullptr);
}

extern "C" void DestroyDummyWindow(HWND hwnd) {
	if (hwnd) {
		DestroyWindow(hwnd);
		UnregisterClassW(dummy_window_class_name, NULL);
	}
}
