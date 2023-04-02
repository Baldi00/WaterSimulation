#ifndef MYHLSLINCLUDE_INCLUDED
#define MYHLSLINCLUDE_INCLUDED

void HeightMapParallaxOffset_float(float Height, float TextureHeight, float3 ViewDirection, out float2 Offset)
{
    Height = Height * TextureHeight - TextureHeight / 2.0;
    float3 v = normalize(ViewDirection);
    v.z += 0.42;
    Offset = Height * (v.xy / v.z);
}

#endif