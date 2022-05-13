struct VS_OUT {
	float4 position : POSITION;
	float4 color : COLOR0;
	float2 tex : TEXCOORD0;
};

void main(
		float4 position : POSITION,
		float4 color : COLOR0,
		out VS_OUT output) {
	output.tex = float2(0.5, -0.5) * (float2)position + float2(0.5, 0.5);
	output.position = position;
	output.color = color;
}