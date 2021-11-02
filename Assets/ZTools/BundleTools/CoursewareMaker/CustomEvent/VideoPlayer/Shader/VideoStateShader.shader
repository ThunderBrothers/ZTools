Shader "Custom/NewShader2" {
	Properties{
		// 一个图片贴图
		_MainTex("Base (RGB)", 2D) = "white" {}

	//定义一个范围为 0 - 10 的滑动条
	_TransVal("Transparency Value", Range(0,10)) = 1.0

	}
		SubShader{
		// 渲染类型为 Opaque    渲染队列为  Transparent 透明
		Tags{ "RenderType" = "Opaque" "Queue" = "Transparent" }

		LOD 200

		//关闭掉光照，模型贴图不再受光照影响，
		//如果需要光照影响可以不写或者  Liginting On
		Lighting Off

		// 混合方式 
		Blend  One OneMinusSrcAlpha
		//下面说下 Blend 包含的混合方式        
		//One 值为1，使用此因子来让帧缓冲区源颜色或是目标颜色完全的通过。 
		//Zero 值为0，使用此因子来删除帧缓冲区源颜色或目标颜色的值。 
		//SrcColor 使用此因子为将当前值乘以帧缓冲区源颜色的值 
		//SrcAlpha 使用此因子为将当前值乘以帧缓冲区源颜色Alpha的值。           
		//DstColor 使用此因子为将当前值乘以帧缓冲区目标颜色的值。 
		//DstAlpha 使用此因子为将当前值乘以帧缓冲区目标颜色Alpha分量的值。   
		//OneMinusSrcColor 使用此因子为将当前值乘以(1 -帧缓冲区源颜色值) 
		//OneMinusSrcAlpha 使用此因子为将当前值乘以(1 -帧缓冲区源颜色Alpha分量的值)         
		//OneMinusDstColor 使用此因子为将当前值乘以(1 –目标颜色值)    
		//OneMinusDstAlpha 使用此因子为将当前值乘以(1 –目标Alpha分量的值)


		CGPROGRAM

		//下面自定义了一个 函数 surf 系统会自动调用，
		//我们只需在下面写出 surf的实现即可
		// Lambert 是系统自带的一个光照函数 Lambert，
		//不用我们写他的实现

#pragma surface surf Lambert

		// 在 Unity安装目录下       Editor/Data/CGIncludes/Lighting.cginc
		//文件中，是这样实现 Lambert函数的
		/*
		inline fixed4 LightingLambert (SurfaceOutput s, fixed3 lightDir, fixed atten)
		{
		//对漫反射进行计算
		fixed diff = max (0, dot(s.Normal, lightDir));
		fixed4 col;
		//下面计算了物体表面的纹理颜色、光源颜色以及光源强度的影响
		col.rgb = s.Albedo * _LithgColor0.rgb * (diff * atten * 2);
		col.a = s.Alpha;
		return col;
		}
		*/

		sampler2D _MainTex;
	float _TransVal;

	struct Input {
		float2 uv_MainTex;
	};

	void surf(Input IN, inout SurfaceOutput o) {
		// 计算贴图颜色
		half4 c = tex2D(_MainTex, IN.uv_MainTex);
		o.Albedo = c.rgb;
		// 将贴图颜色赋值给自发光
		o.Emission = tex2D(_MainTex, IN.uv_MainTex);
		//设置Alpha的值
		// 由于我的目的是过滤掉黑色背景，黑色的 rgb值分别为(0, 0, 0)
		//所以 三者相加为 0 的位置 Alpha设置为 0
		o.Alpha = (c.r + c.g + c.b) *_TransVal;
	}
	ENDCG
	}
}