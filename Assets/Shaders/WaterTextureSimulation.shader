Shader "Custom/WaterTextureSimulation"
{
    Properties
    {
        
    }

    CGINCLUDE

    #include "UnityCustomRenderTexture.cginc"

    float k1, k2, k3;
    int width, height;

    float invLerp(float from, float to, float value)
    {
        return (value - from) / (to - from);
    }

    //void CSMain (uint3 id : SV_DispatchThreadID)
    //{
    //    if(id.x == 0 || id.y == 0 || id.y == height-1 || id.x == width-1)
    //        return;

    //    PreviousBuffer[id.x + id.y * width].z = k1 * CurrentBuffer[id.x + id.y * width].z + k2 * PreviousBuffer[id.x + id.y * width].z +
    //    k3 * (CurrentBuffer[id.x + 1 + id.y * width].z + CurrentBuffer[id.x - 1 + id.y * width].z +
    //    CurrentBuffer[id.x + width + id.y * width].z + CurrentBuffer[id.x - width + id.y * width].z);

    //    Result[id.xy] = invLerp(-0.05f, 0.05f, CurrentBuffer[id.x + id.y * width].z);
    //}

    float4 frag(v2f_customrendertexture i) : SV_Target
    {
        return float4(i.globalTexcoord.x,0,0,1);

        //float2 uv = i.globalTexcoord;

        //float du = 1.0 / _CustomRenderTextureWidth;
        //float dv = 1.0 / _CustomRenderTextureHeight;
        //float3 duv = float3(du, dv, 0) * _DeltaUV;

        //float2 c = tex2D(_SelfTexture2D, uv);
        //float p = (2 * c.r - c.g + _S2 * (
        //    tex2D(_SelfTexture2D, uv - duv.zy).r +
        //    tex2D(_SelfTexture2D, uv + duv.zy).r +
        //    tex2D(_SelfTexture2D, uv - duv.xz).r +
        //    tex2D(_SelfTexture2D, uv + duv.xz).r - 4 * c.r)) * _Atten;

        //return float4(p, c.r, 0, 0);
    }

    float4 frag_left_click(v2f_customrendertexture i) : SV_Target
    {
        return float4(-1, 0, 0, 0);
    }

    float4 frag_right_click(v2f_customrendertexture i) : SV_Target
    {
        return float4(1, 0, 0, 0);
    }

    ENDCG

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            Name "Update"
            CGPROGRAM
            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment frag
            ENDCG
        }

        Pass
        {
            Name "LeftClick"
            CGPROGRAM
            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment frag_left_click
            ENDCG
        }

        Pass
        {
            Name "RightClick"
            CGPROGRAM
            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment frag_right_click
            ENDCG
        }
    }
}