using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

// A substitution for a material. Assign to Image with Mask component and make another image child of it. The child's Image should be substituted with this script.
// This allows to 'cut out' a filter area from an image
public class UIMaskFilter : Image
{
    public override Material materialForRendering 
    {
        get
        {
            Material material = new Material(base.materialForRendering);
            material.SetFloat("_StencilComp", (float)CompareFunction.NotEqual);
            return material;
        }
    }
}
