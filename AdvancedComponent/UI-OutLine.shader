Shader "UI/OutLine"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
		
        _ColorMask ("Color Mask", Float) = 15
        _BorderWidth ("Border Width", Float) = 1
        _BorderColor ("Border Color", Color) = (0,0,0,1)

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile __ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 tangent : TANGENT;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                half4 clipRect : TEXCOORD2;
				half2 borderWidth : TEXCOORD3;
                UNITY_VERTEX_OUTPUT_STEREO
            };
			

            sampler2D _MainTex;
			half4 _MainTex_TexelSize;

            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
			half _BorderWidth;
			fixed4 _BorderColor;
				

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
				
                OUT.texcoord = v.texcoord;
				
				half2 borderWidth = _BorderWidth / _MainTex_TexelSize.zw;
				OUT.clipRect = half4(v.tangent.xy + borderWidth,v.tangent.zw - borderWidth);
                OUT.borderWidth = borderWidth;
				
                OUT.color = v.color * _Color;
				
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
				half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
				half4 border1 = tex2D(_MainTex, IN.texcoord + IN.borderWidth);
				half4 border2 = tex2D(_MainTex, IN.texcoord - IN.borderWidth);
				IN.borderWidth.x = -IN.borderWidth.x;
				half4 border3 = tex2D(_MainTex, IN.texcoord + IN.borderWidth);
				half4 border4 = tex2D(_MainTex, IN.texcoord - IN.borderWidth);
				half2 insideXY = step(IN.clipRect.xy,IN.texcoord.xy);
				half2 insideZW = step(IN.texcoord.xy,IN.clipRect.zw);
				border1 *= insideZW.x * insideZW.y;
				border2 *= insideXY.x * insideXY.y;
				border3 *= insideXY.x * insideZW.y;
				border4 *= insideZW.x * insideXY.y;
				half4 border = (saturate(border1 + border2 + border3 + border4) + _TextureSampleAdd) * _BorderColor;

				color = color * color.a + border * (1 - color.a);
				color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif
				
				//color.ba = fixed2(0,1);
				//color.rg = (IN.texcoord.xy - IN.clipRect.xy) /  (IN.clipRect.zw - IN.clipRect.xy);

				return color;
            }
        ENDCG
        }
    }
}
