#ifndef DEARIMGUI_BUILTIN_INCLUDED
#define DEARIMGUI_BUILTIN_INCLUDED

#include "UnityCG.cginc"
#include "Assets/Shaders/Common.hlsl"

sampler2D _Tex;

float4 unpack_color(uint c)
{
    half4 color = half4(
        (c      ) & 0xff,
        (c >>  8) & 0xff,
        (c >> 16) & 0xff,
        (c >> 24) & 0xff
    ) / 255;
    color.rgb = GammaToLinearSpace(color.rgb);
    return color;
}

Varyings ImGuiPassVertex(ImVert input)
{
    Varyings output  = (Varyings)0;
    output.vertex    = UnityObjectToClipPos(float4(input.vertex, 0, 1));
    output.uv        = float2(input.uv.x, 1 - input.uv.y);
    output.color     = unpack_color(input.color);
    return output;
}

float4 ImGuiPassFrag(Varyings input) : SV_Target
{
    float4 cr = input.color * tex2D(_Tex, input.uv);
	return float4(cr.rgb * cr.a, cr.a);
}

#endif
