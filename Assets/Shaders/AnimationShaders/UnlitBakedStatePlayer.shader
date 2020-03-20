Shader "BakedState/UnLit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _PosTex("Position Texture", 2D) = "black"{}
        _NmlTex("Normal Texture", 2D) = "white"{}
    }
    SubShader
    {
        Pass
        {
            Tags {"LightMode"="ForwardBase"}
            Cull Off
            CGPROGRAM
                #pragma target 3.0
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"
                #pragma multi_compile_instancing
                #define ts _PosTex_TexelSize

                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    float3 normal : TEXCOORD1;
                    float4 pos : SV_POSITION;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };
                sampler2D _MainTex, _PosTex, _NmlTex;
                float4 _PosTex_TexelSize, _Color;

                UNITY_INSTANCING_BUFFER_START(Props)
                    UNITY_DEFINE_INSTANCED_PROP(float, _YPos)
                UNITY_INSTANCING_BUFFER_END(Props)

                v2f vert (appdata_base v, uint vid : SV_VertexID)
                {
                    UNITY_SETUP_INSTANCE_ID(v);
                    float x = (vid + 0.5) * ts.x;
                    float y = UNITY_ACCESS_INSTANCED_PROP(Props, _YPos);
                    if(y == 0) {
                        y = 1 * ts.y;
                    }
                    float4 pos = tex2Dlod(_PosTex, float4(x, y, 0, 0));
                    float3 normal = tex2Dlod(_NmlTex, float4(x, y, 0, 0));
                    v2f o;
                    v.vertex = o.pos = UnityObjectToClipPos(pos);
                    v.normal = o.normal = normal;
                    o.uv = v.texcoord;
                    return o;
                }
                fixed4 frag (v2f i) : SV_Target
                {
                    fixed4 col = tex2D(_MainTex, i.uv) * _Color * 1.0;
                    return col;
                }
            ENDCG
        }
    }
}
