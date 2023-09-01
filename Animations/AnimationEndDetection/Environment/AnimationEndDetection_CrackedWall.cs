using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class AnimationEndDetection_CrackedWall : AnimationEndDetection
{
    public bool Main = false;

    public override void OnAnimationFinish()
    {
        // perform this action only for the destructible element to which the tilechanger copy has been assigned
        if (gameObject.GetComponent<TileChangerScript>() != null)
        { gameObject.GetComponent<TileChangerScript>().UpdateTiles(); }
        Destroy(gameObject);
    }
}
