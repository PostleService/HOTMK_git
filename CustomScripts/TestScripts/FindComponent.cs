using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindComponent : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Component[] comps = gameObject.GetComponents(typeof(Component));
        foreach (Component cp in comps)
        {
            if (cp.GetType() == typeof(EnemyScript))
                Debug.LogWarning(cp.GetType() + " found ");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
