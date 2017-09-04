//--------------------------------------------------------------------------------------
// 
// WPF ShaderEffect HLSL -- GreyscaleEffect
//
//--------------------------------------------------------------------------------------

//-----------------------------------------------------------------------------------------
// Shader constant register mappings (scalars - float, double, Point, Color, Point3D, etc.)
//-----------------------------------------------------------------------------------------


//--------------------------------------------------------------------------------------
// Sampler Inputs (Brushes, including ImplicitInput)
//--------------------------------------------------------------------------------------

sampler2D  implicitInputSampler : register(S0);

//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------

float4 main(float2 uv : TEXCOORD) : COLOR
{
	float2 texuv = uv;
	float4 finalColor;

	float4 srcColor = tex2D(implicitInputSampler, texuv);

	if( srcColor.a == 0 ) 
	{
		finalColor = srcColor;
	}
	else
	{
		float4 luminance = srcColor.r*0.30 + srcColor.g*0.59 + srcColor.b*0.11;
		luminance.a = srcColor.a;
		finalColor = luminance;
	}
	return finalColor;
}
