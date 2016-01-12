//--------------------------------------------------------------------------------------
// 
// WPF ShaderEffect HLSL -- DesaturateEffect
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
	float maxColor;
	float minColor;
	float gColor;

	float4 srcColor = tex2D(implicitInputSampler, texuv);

	if( srcColor.a == 0 ) 
	{
		finalColor = srcColor;
	}
	else
	{
		// Desaturate algorithm.  Replicates the Photoshop Desaturate filter.
		// However this may not properly take into account the Alpha Channel.
		
		maxColor = srcColor.r;
		if (maxColor < srcColor.g)
			maxColor = srcColor.g;
		if (maxColor < srcColor.b)
			maxColor = srcColor.b;
		
		minColor = srcColor.r;
		if (minColor > srcColor.g)
			minColor = srcColor.g;
		if (minColor > srcColor.b)
			minColor = srcColor.b;
			
		float4 luminance = (minColor + maxColor) / 2.00;
		luminance.a = srcColor.a;
		finalColor = luminance;
	}
	return finalColor;
}

