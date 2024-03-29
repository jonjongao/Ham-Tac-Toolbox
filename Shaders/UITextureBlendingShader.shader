Shader "Custom/UITextureBlendingShader"
{
    Properties
    {
        _MainTex ("Texture 1", 2D) = "white" {}
        _BlendTex ("Texture 2", 2D) = "white" {}
        _BlendAmount ("Blend Amount", Range(0, 1)) = 0.5
    }

    SubShader
    {
        Tags { "Queue"="Transparent" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _BlendTex;
            float _BlendAmount;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                half4 tex1 = tex2D(_MainTex, i.uv);
                half4 tex2 = tex2D(_BlendTex, i.uv);

                // Perform blending based on the blend amount
                half4 result = lerp(tex1, tex2, _BlendAmount);

                return result;
            }
            ENDCG
        }
    }
    FallBack "UI/Default"
}