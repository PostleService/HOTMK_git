using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestructOnAwake : MonoBehaviour
{
    private void Awake()
    { Destroy(gameObject); }
}
