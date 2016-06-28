// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Grid" {

	Properties{
		_GridThickness("Grid Thickness", Float) = 0.01
		_GridSpacing("Grid Spacing", Float) = 10.0
		_GridColour("Grid Colour", Color) = (0.5, 1.0, 1.0, 1.0)
		_BaseColour("Base Colour", Color) = (0.0, 0.0, 0.0, 0.0)
	}

		SubShader{
		Tags{ "Queue" = "Transparent" }

		Pass{
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		CGPROGRAM

		// Define the vertex and fragment shader functions
#pragma vertex vert
#pragma fragment frag

		// Access Shaderlab properties
		uniform float _GridThickness;
	uniform float _GridSpacing;
	uniform float4 _GridColour;
	uniform float4 _BaseColour;

	// Input into the vertex shader
	struct vertexInput {
		float4 vertex : POSITION;
	};

	// Output from vertex shader into fragment shader
	struct vertexOutput {
		float4 pos : SV_POSITION;
		float4 worldPos : TEXCOORD0;
	};

	// VERTEX SHADER
	vertexOutput vert(vertexInput input) {
		vertexOutput output;
		output.pos = mul(UNITY_MATRIX_MVP, input.vertex);
		// Calculate the world position coordinates to pass to the fragment shader
		output.worldPos = mul(unity_ObjectToWorld, input.vertex);
		return output;
	}

	// FRAGMENT SHADER
	float4 frag(vertexOutput input) : COLOR{
		if (frac(input.worldPos.x / _GridSpacing) < _GridThickness || frac(input.worldPos.z / _GridSpacing) < _GridThickness) {
			return _GridColour;
		}
		else {
			return _BaseColour;
		}
	}
		ENDCG
	}
	}
}