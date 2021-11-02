Shader "Gerson/DoubleFaceShader" {
	Properties{
		_Color("Main Color", Color) = (1,1,1,1)//Tint Color  
		_MainTex("Base (RGB)", 2D) = "white" {}
	}

		SubShader{
		Tags{ "RenderType" = "Transparent" }
		
		LOD 100

		Pass{
		Cull Off//直接删除剔除  
		Lighting Off
		SetTexture[_MainTex]{ combine texture }
		SetTexture[_MainTex]
	{
		ConstantColor[_Color]
		Combine Previous * Constant
	}
	}
	}
}