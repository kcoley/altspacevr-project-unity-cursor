Shader "Unlit/CursorShader"
{
	Properties
	{
		_Color ("Main Color", Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags { "Queue"="Transparent" }

		ZTest Always
		ZWrite Off


		Pass
		{
		Color [_Color]
		}
	}
}