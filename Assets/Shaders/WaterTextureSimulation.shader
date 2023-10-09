Shader "Custom/WaterTextureSimulation"
{
    Properties
    {
        [NoScaleOffset]_PaintTexture("Paint Texture", 2D) = "black"
    }

    CGINCLUDE

    #include "UnityCustomRenderTexture.cginc"

    float k1, k2, k3;
    sampler2D _PaintTexture;
    float currentForce;

    float invLerp(float from, float to, float value)
    {
        return (value - from) / (to - from);
    }

    float4 frag(v2f_customrendertexture i) : SV_Target
    {
        float2 uv = i.globalTexcoord;
        float dx = 1 / _CustomRenderTextureWidth;
        float dy = 1 / _CustomRenderTextureHeight;

        float2 c = tex2D(_SelfTexture2D, uv);
        float p = (2 * c.r - c.g + 0.2 * (
            tex2D(_SelfTexture2D, uv  + float2(0,dy)).r +
            tex2D(_SelfTexture2D, uv  + float2(0,-dy)).r +
            tex2D(_SelfTexture2D, uv  + float2(-dx,0)).r +
            tex2D(_SelfTexture2D, uv  + float2(dx,0)).r - 4 * c.r)) * 0.999;

        return float4(p, c.r, 0, 0);
    }

    float4 frag_left_click(v2f_customrendertexture i) : SV_Target
    {
        return lerp(
            tex2D(_SelfTexture2D, i.globalTexcoord), 
            float4(-currentForce, 0, 0, 1),
            tex2D(_PaintTexture, i.localTexcoord).r);
    }

    float4 frag_right_click(v2f_customrendertexture i) : SV_Target
    {
        return float4(currentForce, 0, 0, 1) * tex2D(_PaintTexture, i.localTexcoord).r;
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