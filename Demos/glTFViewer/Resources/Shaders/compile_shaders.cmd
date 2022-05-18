@echo off

"C:\Program Files (x86)\Windows Kits\10\bin\10.0.18362.0\x86\fxc.exe" /T vs_2_0 /Fo Compiled\VS_DiffuseOpaque.o .\VS\VS_DiffuseOpaque.hlsl
"C:\Program Files (x86)\Windows Kits\10\bin\10.0.18362.0\x86\fxc.exe" /T vs_2_0 /Fo Compiled\VS_DiffuseOpaqueInstanced.o .\VS\VS_DiffuseOpaqueInstanced.hlsl
"C:\Program Files (x86)\Windows Kits\10\bin\10.0.18362.0\x86\fxc.exe" /T vs_2_0 /Fo Compiled\VS_DiffuseOpaqueUniformColorInstanced.o .\VS\VS_DiffuseOpaqueUniformColorInstanced.hlsl
"C:\Program Files (x86)\Windows Kits\10\bin\10.0.18362.0\x86\fxc.exe" /T vs_2_0 /Fo Compiled\VS_DiffuseOpaqueVertexColors.o .\VS\VS_DiffuseOpaqueVertexColors.hlsl
"C:\Program Files (x86)\Windows Kits\10\bin\10.0.18362.0\x86\fxc.exe" /T vs_2_0 /Fo Compiled\VS_DepthOnly.o .\VS\VS_DepthOnly.hlsl
"C:\Program Files (x86)\Windows Kits\10\bin\10.0.18362.0\x86\fxc.exe" /T vs_2_0 /Fo Compiled\VS_ScreenTexture.o .\VS\VS_ScreenTexture.hlsl
"C:\Program Files (x86)\Windows Kits\10\bin\10.0.18362.0\x86\fxc.exe" /T vs_2_0 /Fo Compiled\VS_DiffuseOpaqueVertexColorsInstanced.o .\VS\VS_DiffuseOpaqueVertexColorsInstanced.hlsl
"C:\Program Files (x86)\Windows Kits\10\bin\10.0.18362.0\x86\fxc.exe" /T vs_2_0 /Fo Compiled\VS_UnlitInstanced.o .\VS\VS_UnlitInstanced.hlsl
"C:\Program Files (x86)\Windows Kits\10\bin\10.0.18362.0\x86\fxc.exe" /T vs_2_0 /Fo Compiled\VS_TestTexture.o .\VS\VS_TestTexture.hlsl

"C:\Program Files (x86)\Windows Kits\10\bin\10.0.18362.0\x86\fxc.exe" /T ps_2_0 /Fo Compiled\PS_Lit.o .\PS\PS_Lit.hlsl
"C:\Program Files (x86)\Windows Kits\10\bin\10.0.18362.0\x86\fxc.exe" /T ps_2_0 /Fo Compiled\PS_Unlit.o .\PS\PS_Unlit.hlsl
"C:\Program Files (x86)\Windows Kits\10\bin\10.0.18362.0\x86\fxc.exe" /T ps_2_0 /Fo Compiled\PS_UnlitTexture.o .\PS\PS_UnlitTexture.hlsl
"C:\Program Files (x86)\Windows Kits\10\bin\10.0.18362.0\x86\fxc.exe" /T ps_2_0 /Fo Compiled\PS_DepthOnly.o .\PS\PS_DepthOnly.hlsl
"C:\Program Files (x86)\Windows Kits\10\bin\10.0.18362.0\x86\fxc.exe" /T ps_2_0 /Fo Compiled\PS_TestTexture.o .\PS\PS_TestTexture.hlsl