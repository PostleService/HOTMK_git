using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportVoidZone : MonoBehaviour
{
    private float _lifetimeCounter = 0f;

    // Update is called once per frame
    void FixedUpdate()
    { IncrementLifetimeCounter(); }

    public void RequestDestruction()
    { Destroy(this.gameObject); }

    private void IncrementLifetimeCounter()
    { _lifetimeCounter += Time.fixedDeltaTime; }

}
