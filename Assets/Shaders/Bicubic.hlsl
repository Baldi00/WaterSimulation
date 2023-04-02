#ifndef MYHLSLINCLUDE_INCLUDED
#define MYHLSLINCLUDE_INCLUDED

void BicubicTexture_float(float2 UV, UnityTexture2D Texture, float TextureWidth, float TextureHeight, out float4 Bicubic)
{
    float2 texSize = float2(TextureWidth, TextureHeight);
    float2 invTexSize = 1.0 / texSize;
    UV = UV * texSize - 0.5;
   
    float2 fxy = frac(UV);
    UV -= fxy;

    float4 n = float4(1.0, 2.0, 3.0, 4.0) - fxy.x;
    float4 st = n * n * n;
    float x = st.x;
    float y = st.y - 4.0 * st.x;
    float z = st.z - 4.0 * st.y + 6.0 * st.x;
    float w = 6.0 - x - y - z;
    float4 xcubic = float4(x, y, z, w) * (1.0 / 6.0);

    n = float4(1.0, 2.0, 3.0, 4.0) - fxy.y;
    st = n * n * n;
    x = st.x;
    y = st.y - 4.0 * st.x;
    z = st.z - 4.0 * st.y + 6.0 * st.x;
    w = 6.0 - x - y - z;
    float4 ycubic = float4(x, y, z, w) * (1.0 / 6.0);

    float4 c = UV.xxyy + float2(-0.5, +1.5).xyxy;
    
    float4 s = float4(xcubic.xz + xcubic.yw, ycubic.xz + ycubic.yw);
    float4 offset = c + float4(xcubic.yw, ycubic.yw) / s;
    
    offset *= invTexSize.xxyy;
    
    float4 sample0 = tex2Dlod(Texture, float4(offset.xz, 1, 1));
    float4 sample1 = tex2Dlod(Texture, float4(offset.yz, 1, 1));
    float4 sample2 = tex2Dlod(Texture, float4(offset.xw, 1, 1));
    float4 sample3 = tex2Dlod(Texture, float4(offset.yw, 1, 1));

    float sx = s.x / (s.x + s.y);
    float sy = s.z / (s.z + s.w);

    Bicubic = lerp(lerp(sample3, sample2, sx), lerp(sample1, sample0, sx), sy);
}

#endif