#pragma pack_matrix(row_major)

float4x4 viewProjMatrix : register(c0);
float4 lightDir : register(c8);

struct VS_OUT {
	float4 position : POSITION;
	float4 color : COLOR0;
	float4 diffuse : COLOR1;
};

void main(
		float4 position : POSITION,
		float3 normal : NORMAL,
		float4 color : COLOR1,
		float4 rowX : TEXCOORD1,
		float4 rowY : TEXCOORD2,
		float4 rowZ : TEXCOORD3,
		float4 rowW : TEXCOORD4,
		out VS_OUT output) {
    float4x4 localToWorldMatrix;
    localToWorldMatrix._m00_m01_m02_m03 = rowX.xyzw;
    localToWorldMatrix._m10_m11_m12_m13 = rowY.xyzw;
    localToWorldMatrix._m20_m21_m22_m23 = rowZ.xyzw;
    localToWorldMatrix._m30_m31_m32_m33 = rowW.xyzw;

	output.position = mul(mul(position, localToWorldMatrix), viewProjMatrix);
	output.color = color;

	float4 world_normal = normalize(mul(normal, localToWorldMatrix));
	float nl = max(0, -dot(world_normal, lightDir));
	nl = (nl + 0.6) / 1.6;
	output.diffuse = float4((float3)nl, 1);
}
