Shader "iD Unity Plugin/Camera Clip"
{
	SubShader
	{
		Tags
		{
			"Queue" = "Geometry-10"
		}

		LOD 100

		Fog { Mode Off }
		ColorMask 0
		ZWrite On
		Lighting Off

		Pass { }
	}
	
	Fallback Off
}