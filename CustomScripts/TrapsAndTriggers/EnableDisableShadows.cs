using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableDisableShadows : MonoBehaviour
{
    public List<GameObject> ShadowsToDisable;
    public GameObject ShadowsToEnable;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            foreach (GameObject go in ShadowsToDisable) { go.SetActive(false); }
            ShadowsToEnable.SetActive(true);
            Destroy(gameObject);
        }
    }
}
