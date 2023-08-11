using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoTileSpriteWithMaterial : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        SpriteRenderer rend = this.gameObject.GetComponent<SpriteRenderer>();
        rend.material.mainTextureScale = transform.localScale;
    }
}
