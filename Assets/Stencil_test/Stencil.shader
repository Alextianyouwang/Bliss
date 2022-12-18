Shader "Custom/Stencil"
{
   Properties
   {
       [IntRange] _StencilID ("Stencil ID", Range(0,255)) = 0
   }

    SubShader
    {

        Tags 
        { 
            "RenderType" = "Opaque"
            "Queue"="Geometry" 
        }

        //ColorMask 0
        //ZWrite On

        Pass 
        {
            Name "Stencil Pass"

            Blend Zero One
            ZWrite Off

            Stencil
            {
                Ref [_StencilID]
                Comp Always
                Pass Replace
                Fail Keep
            }
        }
    }
}
