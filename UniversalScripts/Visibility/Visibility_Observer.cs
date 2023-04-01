using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Visibility_Observer : MonoBehaviour
{
    [Tooltip("How far away will it render items and enemies")]
    public float RangeOfVision;
    [Tooltip("How far away will it clear fog")]
    public int FogClearDistance;
}
