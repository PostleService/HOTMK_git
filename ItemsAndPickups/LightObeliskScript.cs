using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightObeliskScript : MonoBehaviour
{
    private FogManager _fogManager;

    [Header("Vision")]
    public float RangeOfVision = 5f;
    [Tooltip("Offset from main raycast when looking for enemy")]
    public float DegreeOfSpread = 2f;

    // Start is called before the first frame update
    void Start()
    {
        _fogManager = GameObject.Find("FogManager").GetComponent<FogManager>();
        _fogManager.ListOfDeconcealingObjects.Add(this.gameObject);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
    }

}
