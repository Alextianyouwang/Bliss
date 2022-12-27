Shader "Custom/Stencil/MaskStencil"
{
    SubShader {
        // Render the mask after regular geometry, but before masked geometry and
        // transparent things.
 
        Tags {"Queue" = "Geometry+10" }
 
        // Don't draw in the RGBA channels; just the depth buffer
 
            ZWrite On
            ColorMask 0
            Stencil
            {
                Ref 2
                Comp Always
            }
 
        // Do nothing specific in the pass:
 
        Pass {}
    }
}