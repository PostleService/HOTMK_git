using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenAnimationActivator : MonoBehaviour
{
    // enable animator so it can play a one-time animation
    private void OnEnable()
    { this.gameObject.GetComponent<Animator>().enabled = true; }
}
