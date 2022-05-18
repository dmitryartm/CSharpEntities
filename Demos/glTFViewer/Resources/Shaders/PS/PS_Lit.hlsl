float4 main(float4 color : COLOR0, float4 diffuse : COLOR1) : COLOR {
	return color * diffuse;
}