// Upgrade NOTE: replaced 'UNITY_INSTANCE_ID' with 'UNITY_VERTEX_INPUT_INSTANCE_ID'
// Upgrade NOTE: upgraded instancing buffer 'MyProperties' to new syntax.

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Zoxel/BakedAnimation"
{
    Properties
    {
        //_Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
        _PosTex("Position Texture", 2D) = "black"{}
        _NmlTex("Normal Texture", 2D) = "white"{}
        //_FramesTotal ("Frames For Current Animation", Int) = 60	//Range(0, 256)
        _FramesAddition ("Frames Start", Int) = 0
        _FramesPerSecond ("Frames Per Second", Int) = 60
        _AnimationSpeed ("Animation Speed", Float) = 1
        _FrameIndex ("Frame Index", Int) = 0
		// time
		_CurrentTime("Time", Float) = 0
		_AnimationTime("AnimationTime", Float) = 1
		_TimeBegun("Time Begun", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        //Lighting On

        Pass
        {
            CGPROGRAM
            #include "UnityCG.cginc"
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            //#pragma multi_compile_instancing
            sampler2D _MainTex, _PosTex, _NmlTex;
												// texture size
												float4 _PosTex_TexelSize;
												// per animation values
												float _AnimationSpeed;
												float _TimeBegun;
												float _AnimationTime;
												int _FramesAddition;
												int _FramesPerSecond;
												// used incase stopped time
												int _FrameIndex;

            struct VertexInput
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
																uint vertexID : SV_VertexID;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct VertexOutput
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD1;
                UNITY_FOG_COORDS(2)
            };
			
												VertexOutput vert(VertexInput input)
												{
														VertexOutput output;
														float animationTime = _AnimationTime;
														int framesAddition = _FramesAddition;
														int framesPerSecond = _FramesPerSecond;

														if (_AnimationSpeed != 0) 
														{
															float _CurrentTime = ((_Time.y - _TimeBegun) * _AnimationSpeed) % animationTime;
															_FrameIndex = framesAddition + framesPerSecond * ( _CurrentTime);
														}
					
														float2 texture_size = float2( _PosTex_TexelSize.z, _PosTex_TexelSize.w);
														float x = (input.vertexID + 0.5) / texture_size.x;
														float y;
														if(_FrameIndex == 0)
														{
															y = 0;//1 / texture_size.y;
														}
														else
														{
															y = ((float)_FrameIndex) / texture_size.y;
														}
														//y = ((float)_FrameIndex) / texture_size.y;
														float4 texturePosition = float4(x, y, 0, 0);
														output.vertex = UnityObjectToClipPos(tex2Dlod(_PosTex, texturePosition));
														output.normal = tex2Dlod(_NmlTex, texturePosition);
														output.uv = input.uv;
														UNITY_TRANSFER_FOG(output, output.vertex);
														return output;
													}

            fixed4 frag (VertexOutput input) : SV_Target
            {
                fixed4 color = tex2D(_MainTex, input.uv);
                UNITY_APPLY_FOG(input.fogCoord, color);
                return color;
            }
            ENDCG
        }
    }
}

			//float _CurrentTime;
			//uniform float lower_buffer = 2;
			//uniform float upper_buffer = 1;
			//uniform float texture_width = 256.0;

																		// Calc normal and light dir.
																	//output.lightDir = normalize(ObjSpaceLightDir(output.vertex));
																		//output.vNormal = normalize(output.normal).xyz;

						// Calc spherical harmonics and vertex lights. Ripped from compiled surface shader.
																		//float3 worldPos;// = mul(unity_ObjectToWorld, output.vertex).xyz;
																		//worldPos = mul (unity_ObjectToWorld, vertex);

																		//float3 worldNormal = mul((float3x3)unity_ObjectToWorld, output.normal);
						//float3 shlight = ShadeSH9(float4(worldNormal, 1.0));
																		//output.vlight = shlight;
																	// output.vlight = float3(0, 0, 0);

				/*float atten = LIGHT_ATTENUATION(input);
																input.lightDir = normalize ( input.lightDir );
																input.vNormal = normalize ( input.vNormal );
																float3 color = float3(col.x, col.y, col.z);
																float NdotL = saturate( dot (input.vNormal, input.lightDir ));
																//color = UNITY_LIGHTMODEL_AMBIENT.rgb * 2;
																color += input.vlight;
																//color += _LightColor0.rgb * NdotL * ( atten * 2);*/

																//UNITY_SETUP_INSTANCE_ID(input);
																// sample the texture
                //UNITY_VERTEX_INPUT_INSTANCE_ID
																/*float3 lightDir : COLOR0;
																float3 vNormal : COLOR1;
																float3 vlight : COLOR2;
				LIGHTING_COORDS(0,1) */
				//UNITY_SETUP_INSTANCE_ID(input);

				// need to use GPU instanced properties for frames
				/*float animationTime = UNITY_ACCESS_INSTANCED_PROP(Props, _AnimationTime);
				int framesPerSecond = UNITY_ACCESS_INSTANCED_PROP(Props, _FramesPerSecond);
				int framesAddition = UNITY_ACCESS_INSTANCED_PROP(Props, _FramesAddition);*/
			
			/*UNITY_INSTANCING_BUFFER_START(Props)
				UNITY_DEFINE_INSTANCED_PROP (float, _AnimationTime)
				UNITY_DEFINE_INSTANCED_PROP (int, _FramesPerSecond)
				UNITY_DEFINE_INSTANCED_PROP (int, _FramesAddition)
			UNITY_INSTANCING_BUFFER_END(Props)*/