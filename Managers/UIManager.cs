using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using TMPro;

public class UIManager : MonoBehaviour
{
    private LevelManagerScript _lm;

    [Header("Screens")]
    public List<GameObject> Hearts = new List<GameObject>();
    public List<GameObject> ItemPanel = new List<GameObject>();

    [Header("StageItems")]
    [Tooltip("A visual representation of levelstage objective: enemy or item")]
    public List<Sprite> LevelStageIcons = new List<Sprite>() { };
    [Tooltip("When item is picked up, it will 'pulse' to this scale and back to 1")]
    public float PulseObjectIconUntilScale = 0.75f;
    public float SpeedOfPulse = 1f;

    [Header("Arrow Pointers")]
    public GameObject LevelStageEndPointer;
    [Tooltip("Multiplies default number of items per LevelStage by this number. Always set lower than one")]
    public float ShowPointersAtPercentage = 0.2f;
    [Tooltip("Failsafe number of items/enemies in case percentage does not land on full value")]
    public int FailsafeObjectCount = 1;

    // THIS BIT IS FOR ITEM PULSING
    private bool _itemPulseInitiated = false;        
    private bool _pulseBottomReached = false;

    private void OnEnable()
    {
        PlayerScript.OnHealthUpdate += UpdateHearts;
        LevelManagerScript.OnLevelStageChange += UpdateItems;
        LevelManagerScript.OnObjectiveDecrement += InitiateLevelStageIconPulse;
        LevelManagerScript.OnObjectiveDecrement += DecrementItemsCount;

    }

    private void OnDisable()
    { Unsubscribe(); }

    private void Awake()
    { _lm = GameObject.Find("LevelManager").GetComponent<LevelManagerScript>(); }

    private void FixedUpdate()
    { if (_itemPulseInitiated && LevelStageEndPointer != null) StageIconPulse(); }

    public void Unsubscribe()
    {
        PlayerScript.OnHealthUpdate -= UpdateHearts;
        LevelManagerScript.OnLevelStageChange -= UpdateItems;
        LevelManagerScript.OnObjectiveDecrement -= InitiateLevelStageIconPulse;
        LevelManagerScript.OnObjectiveDecrement -= DecrementItemsCount;
    }

    #region ITEMS

    private void UpdateItems(int aLevelStage, int aCurrentItems, int aDefaultItems)
    {
        if (aLevelStage < 3)
        {
            // Enable item counter and image of current objective at the start of the level
            foreach (GameObject go in ItemPanel) { go.SetActive(true); }
            
            // Update the item counter and image when level up is occuring
            // initiate counters for current stage
            TextMeshProUGUI outOfTxt = ItemPanel[0].GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI currTxt = ItemPanel[2].GetComponent<TextMeshProUGUI>();
            string currValue;
            if (aLevelStage == 0) { currValue = (aDefaultItems - aCurrentItems).ToString(); }
            else { currValue = aCurrentItems.ToString(); }

            outOfTxt.text = aDefaultItems.ToString();
            currTxt.text = currValue;

            // initiate icon for current stage
            float iconScaleDownValue = 3;

            ItemPanel[3].GetComponent<Image>().sprite = LevelStageIcons[aLevelStage];
            RectTransform imageObjRT = ItemPanel[3].GetComponent<RectTransform>();
            Image imageObjImg = ItemPanel[3].GetComponent<Image>();
            Vector2 imageObjSize = new Vector2(imageObjImg.sprite.rect.width, imageObjImg.sprite.rect.height);
            imageObjRT.sizeDelta = new Vector2(imageObjSize.x * iconScaleDownValue, imageObjSize.y * iconScaleDownValue);

            InitiateLevelStageIconPulse(aLevelStage, aCurrentItems, aDefaultItems);

            LevelStagePointersDecision(aLevelStage, aCurrentItems, aDefaultItems);
        }
        else { foreach (GameObject go in ItemPanel) { go.SetActive(false); } }
    }

    private void InitiateLevelStageIconPulse(int aLevelStage, int aCurrentItems, int aDefaultItems)
    {
        if (aLevelStage == _lm.LevelStage) { _itemPulseInitiated = true; }
    }

    private void StageIconPulse()
    {
        // if boolean has not been flipped - keep decreasing Image scale until it reaches PulseObjectIconUntilScale
        if (!_pulseBottomReached)
        {
            Vector3 LocalScale = ItemPanel[3].GetComponent<RectTransform>().localScale;
            Vector3 newLocalScale = LocalScale - (new Vector3(Time.deltaTime, Time.deltaTime, Time.deltaTime) * SpeedOfPulse);
            ItemPanel[3].GetComponent<RectTransform>().localScale = newLocalScale;

            if (ItemPanel[3].GetComponent<RectTransform>().localScale.x <= PulseObjectIconUntilScale)
            { _pulseBottomReached = true; }
        }
        // Once boolean is flipped - decrease image scale until it's back to 1 and flip both pickup and pulse bottom booleans
        if (_pulseBottomReached)
        {
            Vector3 LocalScale = ItemPanel[3].GetComponent<RectTransform>().localScale;
            Vector3 newLocalScale = LocalScale + (new Vector3(Time.deltaTime, Time.deltaTime, Time.deltaTime) * SpeedOfPulse);
            ItemPanel[3].GetComponent<RectTransform>().localScale = newLocalScale;

            if (ItemPanel[3].GetComponent<RectTransform>().localScale.x >= 1)
            { _pulseBottomReached = false; _itemPulseInitiated = false; }
        }
    }

    private void DecrementItemsCount(int aLevelStage, int aCurrentItems, int aDefaultItems)
    {
        if (aLevelStage < 3 && aLevelStage == _lm.LevelStage)
        {
            // this shiny piece of shit code is there because ONLY in this method there is a problem with referencing items on list after scene recreation
            // GO FUCKING FIGURE!!!
            GameObject CounterElement = null;
            foreach (Transform chTr in GameObject.Find("ItemDisplay").transform)
            { if (chTr.gameObject.GetComponent<ItemCounterElement>().Order == 2) CounterElement = chTr.gameObject; }

            TextMeshProUGUI currTxt = CounterElement.GetComponent<TextMeshProUGUI>();
            string currValue;
            if (aLevelStage == 0) { currValue = (_lm.DefaultItemsCount[aLevelStage] - aCurrentItems).ToString(); }
            else { currValue = (aCurrentItems).ToString(); }
            
            currTxt.text = currValue;

            LevelStagePointersDecision(aLevelStage, aCurrentItems, aDefaultItems);
        }
    }

    #endregion ITEMS

    private void UpdateHearts(int aCurrentLives, int aMaxLives, string aCommand)
    {
        List<string> AcceptableRedrawDetails = new List<string>() { "HeartSpawn", "HeartLevelUp", "HeartHeal", "HeartDamage", "HeartDeath" };
        string instructions;
        if (AcceptableRedrawDetails.Contains(aCommand)) { instructions = aCommand; }
        else { instructions = "other"; }

        // Conceal before redrawing
        foreach (GameObject heart in Hearts) 
        { heart.GetComponent<Heart>().DespawnInstructions(instructions); }

        for (int i = 0; i < aMaxLives; i++)
        {
            Heart heartScr = Hearts[i].GetComponent<Heart>();

            if (heartScr.Order == (aCurrentLives - 1)) { heartScr.IsLast = true; }
            else heartScr.IsLast = false;

            if (heartScr.Order <= aCurrentLives - 1) 
            {
                heartScr.gameObject.SetActive(true);
                heartScr.SpawnInstructions(instructions);
            }
        }
    }

    private void LevelStagePointersDecision(int aLevelStage, int aCurrentItemCount, int aDefaultItems)
    {
        if (aCurrentItemCount <= aDefaultItems * ShowPointersAtPercentage ||
            aCurrentItemCount == FailsafeObjectCount)
        {
            
            List<GameObject> goLis = new List<GameObject>();
            if (aLevelStage == 0) 
            {
                DarkObeliskScript[] goArr = GameObject.FindObjectsOfType<DarkObeliskScript>();
                foreach (DarkObeliskScript doS in goArr) { goLis.Add(doS.gameObject); }
            }
            else 
            {
                EnemyScript[] goArr = GameObject.FindObjectsOfType<EnemyScript>();
                foreach (EnemyScript ES in goArr) { if (ES.EnemyLevel == aLevelStage) goLis.Add(ES.gameObject); }
            }
            foreach (GameObject go in goLis) 
            {
                GameObject arrow = Instantiate(LevelStageEndPointer);
                arrow.transform.SetParent(GameObject.Find("LevelStageEndPointers").transform, false); // necessary to set worldPositionStays to false to retain proper scaling
                arrow.GetComponent<LevelStageEndPointer>().Target = go;
            }
        }
    }

}
