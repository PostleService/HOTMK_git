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
    private GameObject _player;

    private float _visualElementsScale = 1f;
    public bool _screenResizeQueued = false;

    [Header("Screens")]
    public List<GameObject> Hearts = new List<GameObject>(3);
    public List<GameObject> ItemsPanel = new List<GameObject>(4);

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
    public Vector2 ArrowSize = new Vector2(50, 50);

    // THIS BIT IS FOR ITEM PULSING
    private bool _itemPulseInitiated = false;        
    private bool _pulseBottomReached = false;

    private void OnEnable()
    {
        PlayerScript.OnSpawn += AssignPlayer;
        PlayerScript.OnHealthUpdate += UpdateHearts;
        LevelManagerScript.OnLevelStageChange += UpdateItems;
        EnemyScript.OnDie += (aStageLevel, aEnemyObject) =>
        {
            InitiateLevelStageIconPulse(aStageLevel);
            LevelStagePointersDecision(aStageLevel);
            DecrementItemsCount(aStageLevel);
        }; // lambda with listed subscriptions that takes input from the event with 1 argument

        DarkObeliskScript.OnDie += (aStageLevel) =>
        {
            InitiateLevelStageIconPulse(aStageLevel);
            LevelStagePointersDecision(aStageLevel);
            DecrementItemsCount(aStageLevel);
        };
    }

    private void OnDisable()
    {
        PlayerScript.OnSpawn -= AssignPlayer;
        PlayerScript.OnHealthUpdate -= UpdateHearts;
        LevelManagerScript.OnLevelStageChange -= UpdateItems;
        EnemyScript.OnDie -= (aStageLevel, aEnemyObject) =>
        {
            InitiateLevelStageIconPulse(aStageLevel);
            LevelStagePointersDecision(aStageLevel);
            DecrementItemsCount(aStageLevel);
        };
        DarkObeliskScript.OnDie -= (aStageLevel) =>
        {
            InitiateLevelStageIconPulse(aStageLevel);
            LevelStagePointersDecision(aStageLevel);
            DecrementItemsCount(aStageLevel);
        };
    }

    private void Start()
    { _lm = GameObject.Find("LevelManager").GetComponent<LevelManagerScript>(); }

    private void FixedUpdate()
    {
        if (_itemPulseInitiated && LevelStageEndPointer != null) { StageIconPulse(); }
    }

    private void AssignPlayer(GameObject aGameObject)
    { _player = aGameObject; }

    #region ITEMS

    private void UpdateItems(int aLevelStage, int aCurrentItems, int aDefaultItems)
    {
        if (aLevelStage < 3)
        {
            // Enable item counter and image of current objective at the start of the level
            foreach (GameObject go in ItemsPanel) { go.SetActive(true); }
            
            // Update the item counter and image when level up is occuring
            // initiate counters for current stage
            TextMeshProUGUI outOfTxt = ItemsPanel[0].GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI currTxt = ItemsPanel[2].GetComponent<TextMeshProUGUI>();
            string currValue;
            if (aLevelStage == 0) { currValue = (aDefaultItems - aCurrentItems).ToString(); }
            else { currValue = aCurrentItems.ToString(); }

            outOfTxt.text = aDefaultItems.ToString();
            currTxt.text = currValue;

            // initiate icon for current stage
            float iconScaleDownValue = 3f;

            ItemsPanel[3].GetComponent<Image>().sprite = LevelStageIcons[aLevelStage];
            RectTransform imageObjRT = ItemsPanel[3].GetComponent<RectTransform>();
            Image imageObjImg = ItemsPanel[3].GetComponent<Image>();
            Vector2 imageObjSize = new Vector2(imageObjImg.sprite.rect.width, imageObjImg.sprite.rect.height);
            imageObjRT.sizeDelta = new Vector2((imageObjSize.x * _visualElementsScale) * iconScaleDownValue, (imageObjSize.y * _visualElementsScale) * iconScaleDownValue);

            InitiateLevelStageIconPulse(aLevelStage);
        }
        else { foreach (GameObject go in ItemsPanel) { go.SetActive(false); } }
    }

    private void InitiateLevelStageIconPulse(int aLevelStage)
    {
        if (_lm != null) 
        {
            if (aLevelStage == _lm.LevelStage)
            { _itemPulseInitiated = true; }
        }
    }

    private void StageIconPulse()
    {
        // if boolean has not been flipped - keep decreasing Image scale until it reaches PulseObjectIconUntilScale
        if (!_pulseBottomReached)
        {
            Vector3 LocalScale = ItemsPanel[3].GetComponent<RectTransform>().localScale;
            Vector3 newLocalScale = LocalScale - (new Vector3(Time.deltaTime, Time.deltaTime, Time.deltaTime) * SpeedOfPulse);
            ItemsPanel[3].GetComponent<RectTransform>().localScale = newLocalScale;

            if (ItemsPanel[3].GetComponent<RectTransform>().localScale.x <= PulseObjectIconUntilScale)
            { _pulseBottomReached = true; }
        }
        // Once boolean is flipped - decrease image scale until it's back to 1 and flip both pickup and pulse bottom booleans
        if (_pulseBottomReached)
        {
            Vector3 LocalScale = ItemsPanel[3].GetComponent<RectTransform>().localScale;
            Vector3 newLocalScale = LocalScale + (new Vector3(Time.deltaTime, Time.deltaTime, Time.deltaTime) * SpeedOfPulse);
            ItemsPanel[3].GetComponent<RectTransform>().localScale = newLocalScale;

            if (ItemsPanel[3].GetComponent<RectTransform>().localScale.x >= 1)
            { _pulseBottomReached = false; _itemPulseInitiated = false; }
        }
    }

    private void DecrementItemsCount(int aLevelStage)
    {
        if (aLevelStage < 3 && aLevelStage == _lm.LevelStage)
        {
            TextMeshProUGUI currTxt = ItemsPanel[2].GetComponent<TextMeshProUGUI>();
            string currValue;
            if (aLevelStage == 0) { currValue = (int.Parse(currTxt.text) + 1).ToString(); }
            else { currValue = (int.Parse(currTxt.text) - 1).ToString(); }

            currTxt.text = currValue;
        }
    }

    #endregion ITEMS

    #region HEARTS

    private void UpdateHearts(int aCurrentLives, int aMaxLives, string aCommand)
    {
        // Enable health counter
        // subscribe to PlayerScript.OnSpawn
    }

    #endregion HEARTS

    private void LevelStagePointersDecision(int aInt) 
    { }

/*
    // UPDATE FUNCTIONS

    // ARROW POINTERS

    private void MonitorLevelStagePointers()
    {
        if (_lm.LevelStage >= 0)
        {
            if (_lm._currentItemsCount[_lm.LevelStage] <= _lm.DefaultItemsCount[_lm.LevelStage] * ShowPointersAtPercentage || _lm._currentItemsCount[_lm.LevelStage] == FailsafeObjectCount)
            {
                List<GameObject> gameObjects = new List<GameObject>();
                if (!_lm._remainingItemsFound)
                {
                    gameObjects = FindRemainingObjects();
                    foreach (GameObject go in gameObjects)
                    {
                        GameObject arrow = Instantiate(LevelStageEndPointer);
                        arrow.transform.SetParent(GameObject.Find("LevelStageEndPointers").transform, false); // necessary to set worldPositionStays to false to retain proper scaling
                        RectTransform arrowRT = arrow.GetComponent<RectTransform>();
                        arrowRT.sizeDelta = new Vector2(ArrowSize.x * _visualElementsScale, ArrowSize.y * _visualElementsScale);
                        arrow.GetComponent<LevelStageEndPointer>().Target = go;
                    }
                    _lm._remainingItemsFound = true;
                }
            }
        }
    }

    private List<GameObject> FindRemainingObjects()
    {
        List<GameObject> result = new List<GameObject>();
        GameObject[] enemyObjects = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject[] itemObjects = GameObject.FindGameObjectsWithTag("Item");
        GameObject[] allObjects = enemyObjects.Concat(itemObjects).ToArray();

        foreach (GameObject go in allObjects)
        {
            if (_lm.LevelStage == 0 || _lm.LevelStage == 2)
            {
                if (go.GetComponent<DarkObeliskScript>() != null)
                {
                    if (go.GetComponent<DarkObeliskScript>().ItemStageLevel == _lm.LevelStage)
                    { result.Add(go); }
                }
            }
            else if (_lm.LevelStage == 1 || _lm.LevelStage == 3)
            {
                if (go.GetComponent<EnemyScript>() != null)
                {
                    if (go.GetComponent<EnemyScript>().ItemStageLevel == _lm.LevelStage)
                    { result.Add(go); }
                }
            }
        }
        return result;
    }

    // HEALTH DISPLAY

    // uses a single function to communicate with already existing hearts and hearts to spawn in their stead
    // because hearts have spawn and despawn details, it's okay to use single function
    // those spawning will simply not react to irrelevant instructions

    public void DrawHearts(string aRedrawDetails)
    {
        List<string> AcceptableRedrawDetails = new List<string>() { "start", "heal", "levelup", "damage", "death", "none" };
        string instructions;
        if (AcceptableRedrawDetails.Contains(aRedrawDetails))
        { instructions = aRedrawDetails; }
        else { instructions = "none"; }

        // Debug.LogWarning("Received instructions: " + instructions);

        if (_player != null)
        {

            // despawn previously drawn hearts
            Transform healthdisplayTr = GameObject.Find("HealthDisplay").transform;
            foreach (Transform tr in healthdisplayTr)
            {
                if (tr.gameObject.GetComponent<Heart>() != null)
                { tr.gameObject.GetComponent<Heart>().DespawnInstructions(instructions); }
            }

            // draw new hearts in place of old ones
            for (int i = 0; i < _player.GetComponent<PlayerScript>().CurrentLives; i++)
            {
                if (i < _player.GetComponent<PlayerScript>().CurrentLives)
                {
                    Vector2 anchoredPos;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(GameObject.Find("HealthDisplay").GetComponent<RectTransform>(), _heartSpawnPositions[i], Camera.main, out anchoredPos);

                    GameObject heart = Instantiate(Heart, anchoredPos, new Quaternion());
                    heart.transform.SetParent(GameObject.Find("HealthDisplay").transform, false); // necessary to set worldPositionStays to false to retain proper scaling
                    RectTransform heartRT = heart.GetComponent<RectTransform>();
                    heartRT.sizeDelta = new Vector2(heartRT.sizeDelta.x * _visualElementsScale, heartRT.sizeDelta.y * _visualElementsScale);

                    // If the loop is on the last iteration, set IsLast value for the heart to true
                    if (i == (_player.GetComponent<PlayerScript>().CurrentLives - 1))
                    { heart.GetComponent<Heart>().IsLast = true; }

                    heart.GetComponent<Heart>().SpawnInstructions(instructions);
                }
            }
        }
    }
 
    public void DrawLevelStageObjects()
    {
        LevelManagerScript lms = GameObject.Find("LevelManager").GetComponent<LevelManagerScript>();
        float iconScaleDownValue = 3f;

        if (StartItemDisplay)
        {
            int OutOfValue = lms.DefaultItemsCount[lms.LevelStage];
            int CurrentValue = lms._currentItemsCount[lms.LevelStage];
            _itemPickupChecker = CurrentValue;

            // only create UI elements once and add them to the _levelStageObjectCounterElements for further modification
            if (!_levelStageObjectCounterElements.Any() && _levelStageObjectDecremented)
            {
                List<Vector2> anchoredPosList = new List<Vector2>();
                for (int i = 0; i < _elemSpawnPositions.Count; i++)
                {
                    Vector2 anchoredPos;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(GameObject.Find("ItemDisplay").GetComponent<RectTransform>(), _elemSpawnPositions[i], Camera.main, out anchoredPos);
                    anchoredPosList.Add(anchoredPos);
                }

                for (int i = 0; i < anchoredPosList.Count; i++)
                {
                    GameObject go;
                    // Create text elements
                    if (i < (anchoredPosList.Count - 1))
                    {
                        GameObject textObj = new GameObject("textObj " + i, typeof(RectTransform));
                        go = textObj;
                        textObj.transform.position = anchoredPosList[i];
                        textObj.transform.SetParent(GameObject.Find("ItemDisplay").transform, false); // necessary to set worldPositionStays to false to retain proper scaling
                        if (i == 0)
                        {
                            textObj.AddComponent<TextMeshProUGUI>().text = OutOfValue.ToString();
                        }
                        if (i == 1)
                        { textObj.AddComponent<TextMeshProUGUI>().text = "/"; }
                        if (i == 2)
                        { textObj.AddComponent<TextMeshProUGUI>().text = (OutOfValue - CurrentValue).ToString(); }
                        textObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
                        textObj.GetComponent<TextMeshProUGUI>().fontSize = 36 * _visualElementsScale;
                    }
                    // Create visual element
                    else
                    {
                        GameObject imageObj = new GameObject("imageObj", typeof(RectTransform));
                        go = imageObj;
                        imageObj.transform.position = anchoredPosList[i];
                        imageObj.transform.SetParent(GameObject.Find("ItemDisplay").transform, false); // necessary to set worldPositionStays to false to retain proper scaling            
                        imageObj.AddComponent<Image>().sprite = LevelStageObject[lms.LevelStage];
                        RectTransform imageObjRT = imageObj.GetComponent<RectTransform>();
                        Image imageObjImg = imageObj.GetComponent<Image>();
                        Vector2 imageObjSize = new Vector2(imageObjImg.sprite.rect.width, imageObjImg.sprite.rect.height);
                        imageObjRT.sizeDelta = new Vector2((imageObjSize.x * _visualElementsScale) * iconScaleDownValue, (imageObjSize.y * _visualElementsScale) * iconScaleDownValue);
                    }
                    _levelStageObjectCounterElements.Add(go);
                }
            }
            // If screen is updated
            else if (_levelStageObjectCounterElements.Any() && _screenResizeQueued)
            {
                List<Vector2> anchoredPosList = new List<Vector2>();
                for (int i = 0; i < _elemSpawnPositions.Count; i++)
                {
                    Vector2 anchoredPos;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(GameObject.Find("ItemDisplay").GetComponent<RectTransform>(), _elemSpawnPositions[i], Camera.main, out anchoredPos);
                    anchoredPosList.Add(anchoredPos);
                }

                for (int i = 0; i < anchoredPosList.Count; i++)
                {
                    // update text elements
                    if (i < (anchoredPosList.Count - 1))
                    {
                        _levelStageObjectCounterElements[i].transform.localPosition = anchoredPosList[i];
                        _levelStageObjectCounterElements[i].GetComponent<TextMeshProUGUI>().fontSize = 36 * _visualElementsScale;
                    }
                    else
                    {
                        _levelStageObjectCounterElements[i].transform.localPosition = anchoredPosList[i];
                        RectTransform imageObjRT = _levelStageObjectCounterElements[i].GetComponent<RectTransform>();
                        Image imageObjImg = _levelStageObjectCounterElements[i].GetComponent<Image>();
                        Vector2 imageObjSize = new Vector2(imageObjImg.sprite.rect.width, imageObjImg.sprite.rect.height);
                        imageObjRT.sizeDelta = new Vector2((imageObjSize.x * _visualElementsScale) * iconScaleDownValue, (imageObjSize.y * _visualElementsScale) * iconScaleDownValue);
                    }
                }

            }
            // If item count is decremented
            else if (_levelStageObjectDecremented)
            {

                _levelStageObjectCounterElements[0].GetComponent<TextMeshProUGUI>().text = OutOfValue.ToString();
                if (lms.LevelStage == 0 || lms.LevelStage == 2)
                { _levelStageObjectCounterElements[2].GetComponent<TextMeshProUGUI>().text = (OutOfValue - CurrentValue).ToString(); }
                else if (lms.LevelStage == 1 || lms.LevelStage == 3)
                { _levelStageObjectCounterElements[2].GetComponent<TextMeshProUGUI>().text = CurrentValue.ToString(); }
                _levelStageObjectCounterElements[3].GetComponent<Image>().sprite = LevelStageObject[lms.LevelStage];
                RectTransform imageObjRT = _levelStageObjectCounterElements[3].GetComponent<RectTransform>();
                Image imageObjImg = _levelStageObjectCounterElements[3].GetComponent<Image>();
                Vector2 imageObjSize = new Vector2(imageObjImg.sprite.rect.width, imageObjImg.sprite.rect.height);
                imageObjRT.sizeDelta = new Vector2((imageObjSize.x * _visualElementsScale) * iconScaleDownValue, (imageObjSize.y * _visualElementsScale) * iconScaleDownValue);
            }
        }
        else
        {
            if (_levelStageObjectCounterElements.Any())
            {
                foreach (GameObject go in _levelStageObjectCounterElements)
                { Destroy(go); }
                _levelStageObjectCounterElements.Clear();
            }
        }
    }

    // Resizes heart and item pickup containers percentage-wise in relation to 2560x1440
    // Changes display element positions
    public void ResizeDisplay()
    {
        RectTransform heartDisplayRT = GameObject.Find("HealthDisplay").GetComponent<RectTransform>();
        RectTransform itemsDisplayRT = GameObject.Find("ItemDisplay").GetComponent<RectTransform>();
        RectTransform commonDisplayRT = GameObject.Find("HealthAndItemPanel").GetComponent<RectTransform>();

        // Calculating coefficients in hardcode due to bugs in preinitialized lists - cannot reference positions in specific order
        float HL = HeartContOffsetMin.x / DevelopmentScreenSize.x; float HB = HeartContOffsetMin.y / DevelopmentScreenSize.y; float HR = HeartContOffsetMax.x / DevelopmentScreenSize.x; float HT = HeartContOffsetMax.y / DevelopmentScreenSize.y;
        float IL = ItemContOffsetMin.x / DevelopmentScreenSize.x; float IB = ItemContOffsetMin.y / DevelopmentScreenSize.y; float IR = ItemContOffsetMax.x / DevelopmentScreenSize.x; float IT = ItemContOffsetMax.y / DevelopmentScreenSize.y;
        float CL = CommonContOffsetMin.x / DevelopmentScreenSize.x; float CB = CommonContOffsetMin.y / DevelopmentScreenSize.y; float CR = CommonContOffsetMax.x / DevelopmentScreenSize.x; float CT = CommonContOffsetMax.y / DevelopmentScreenSize.y;

        // offsetMin.x - left ; offsetMin.y - bottom; offsetMax.x - right ; offsetMax.y - top
        // Setting sizes of bounding boxes for UI displays by multiplying current screen res by static coeffs

        // Issue with sign reversal of right top values to negative when setting, assigning negative values
        heartDisplayRT.offsetMin = new Vector2(Screen.width * HL, Screen.height * HB);
        heartDisplayRT.offsetMax = new Vector2(-(Screen.width * HR), -(Screen.height * HT));
        itemsDisplayRT.offsetMin = new Vector2(Screen.width * IL, Screen.height * IB);
        itemsDisplayRT.offsetMax = new Vector2(-(Screen.width * IR), -(Screen.height * IT));
        commonDisplayRT.offsetMin = new Vector2(Screen.width * CL, Screen.height * CB);
        commonDisplayRT.offsetMax = new Vector2(-(Screen.width * CR), -(Screen.height * CT));

        _visualElementsScale = Screen.width / DevelopmentScreenSize.x;

        // Recalculates new element positions within resized display boxes
        // Redrawing UI elements
        _heartSpawnPositions = DefineHeartPositions();
        DrawHearts("start");
        _elemSpawnPositions = DefineLevelStageObjectsCounterPositions();
        DrawLevelStageObjects();

        foreach (Transform chTF in GameObject.Find("LevelStageEndPointers").transform)
        {
            RectTransform arrowRT = chTF.GetComponent<RectTransform>();
            arrowRT.sizeDelta = new Vector2(ArrowSize.x * _visualElementsScale, ArrowSize.y * _visualElementsScale);
        }
        _screenResizeQueued = false;

    }
    */

}
