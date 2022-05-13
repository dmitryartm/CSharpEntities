float4x4 worldViewProjMatrix : register(c0);

float4 main(float4 position : POSITION) : POSITION {
	return mul(position, worldViewProjMatrix);
}
