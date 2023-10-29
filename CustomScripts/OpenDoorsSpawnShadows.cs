using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenDoorsSpawnShadows : MonoBehaviour
{
    public List<GameObject> Gates = new List<GameObject>();
    public GameObject GateOpeningPrefab;
    public GameObject ShadowsToEnable;

    public int LevelStage = 1;

    private void OnEnable()
    {
        LevelManagerScript.OnLevelStageChange += ReactToLevelStageChange;
    }
    private void OnDisable()
    {
        LevelManagerScript.OnLevelStageChange -= ReactToLevelStageChange;
    }

    private void ReactToLevelStageChange(int aLevelStage, int aCurrentItems, int aDefaultItems, Sprite aSprite)
    {
        if (aLevelStage == LevelStage)
        {
            foreach (GameObject gate in Gates)
            {
                Vector2 GateLocation = gate.transform.position;
                Destroy(gate);
                if (GateOpeningPrefab != null) Instantiate(GateOpeningPrefab, GateLocation, new Quaternion(), GameObject.Find("TrapsAndTriggers").transform);
            }
            if (ShadowsToEnable != null) ShadowsToEnable.SetActive(true);
            Destroy(gameObject);
        }
    }
}
