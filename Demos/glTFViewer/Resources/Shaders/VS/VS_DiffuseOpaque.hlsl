#pragma pack_matrix(row_major)

float4x4 viewProjMatrix : register(c0);
float4x4 localToWorldMatrix : register(c4);
float4 lightDir : register(c8);
float4 color : register(c9);

struct VS_OUT {
	float4 position : POSITION;
	float4 color : COLOR0;
	float4 diffuse : COLOR1;
};

void main(
		float4 position : POSITION,
		float3 normal : NORMAL,
		out VS_OUT output) {
	output.position = mul(mul(position, localToWorldMatrix), viewProjMatrix);
	output.color = color;

	float4 world_normal = normalize(mul(normal, localToWorldMatrix));
	float nl = max(0, -dot(world_normal, lightDir));
	nl = (nl + 0.6) / 1.6;
	output.diffuse = float4((float3)nl, 1);
}
