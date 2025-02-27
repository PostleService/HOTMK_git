using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorOpenerScript : MonoBehaviour
{
    public GameObject StageMonitorObserved;
    [Tooltip("Objects, which will be deleted upon receiving event trigger")]
    public List<GameObject> DoorObjects = new List<GameObject>();
    [Tooltip("Objects which will be spawned in place of these doors")]
    public List<GameObject> ObjectsToSpawn = new List<GameObject>();
    public bool MonitorOnlySomeObjects = false;
    [Header("Item details")]
    public List<GameObject> Items = new List<GameObject>() { };

    private void OnEnable()
    { 
        BossLevelArenaDecrementMonitor.OnDestroy += PerformRelevantAction;
        DarkObeliskScript.OnDie += ReactToDeath;
    }

    private void OnDisable()
    { 
        BossLevelArenaDecrementMonitor.OnDestroy -= PerformRelevantAction;
        DarkObeliskScript.OnDie -= ReactToDeath;
    }

    private void PerformRelevantAction(GameObject aGameObject)
    {
        if (aGameObject == StageMonitorObserved)
        {
            List<Vector3> locations = new List<Vector3>();
            foreach (GameObject go in DoorObjects) 
            { 
                locations.Add(go.transform.position);
                Destroy(go);
            }
            foreach (Vector3 location in locations)
            {
                foreach (GameObject go in ObjectsToSpawn)
                { Instantiate(go, location, new Quaternion(), GameObject.Find("TrapsAndTriggers").transform); }
            }
            Destroy(this.gameObject);
        }
    }
    private void ReactToDeath(int aInt, GameObject aGameObject)
    {
        if (MonitorOnlySomeObjects && Items.Contains(aGameObject))
        {
            List<Vector3> locations = new List<Vector3>();
            foreach (GameObject go in DoorObjects)
            {
                locations.Add(go.transform.position);
                Destroy(go);
            }
            foreach (Vector3 location in locations)
            {
                foreach (GameObject go in ObjectsToSpawn)
                { Instantiate(go, location, new Quaternion(), GameObject.Find("TrapsAndTriggers").transform); }
            }
            Destroy(this.gameObject);
        }
    }
}
