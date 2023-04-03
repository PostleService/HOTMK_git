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

    [Header("UI Scaling")]
    public Vector2 DevelopmentScreenSize = new Vector2(2560, 1440);
    public Vector2 HeartContOffsetMin = new Vector2(1970, 1226);
    public Vector2 HeartContOffsetMax = new Vector2(49, 49);
    public Vector2 ItemContOffsetMin = new Vector2(1970, 1054);
    public Vector2 ItemContOffsetMax = new Vector2(49, 222);
    public Vector2 CommonContOffsetMin = new Vector2(2155, 1165);
    public Vector2 CommonContOffsetMax = new Vector2(95, 150);

    private float _visualElementsScale = 1f;
    public bool _screenResizeQueued = false;

    [Header("Arrow Pointers")]
    public GameObject LevelStageEndPointer;
    [Tooltip("Multiplies default number of items per LevelStage by this number. Always set lower than one")]
    public float ShowPointersAtPercentage = 0.2f;
    [Tooltip("Failsafe number of items/enemies in case percentage does not land on full value")]
    public int FailsafeObjectCount = 1;
    public Vector2 ArrowSize = new Vector2(50, 50);

    [Header("Health")]
    public GameObject Heart;
    [Tooltip("Spacial percentages along X axis of the HeartDisplay rectangle. From right edge to left edge")]
    public float[] HeartDisplayPercentages = new float[] { 0.75f, 0.5f, 0.25f };
    [HideInInspector]
    public List<Vector3> _heartSpawnPositions = new List<Vector3>();

    [Header("StageItems")]
    [Tooltip("A visual representation of levelstage objective: enemy or item")]
    public List<Sprite> LevelStageObject = new List<Sprite>() { };
    [Tooltip("Spacial percentages along X axis of the ItemsDisplay rectangle. From right edge to left edge")]
    public float[] ItemDisplayPercentages = new float[] { 0.8f, 0.7f, 0.6f, 0.4f };
    [Tooltip("When item is picked up, it will 'pulse' to this scale and back to 1")]
    public float PulseObjectIconUntilScale = 0.75f;
    public float SpeedOfPulse = 1f;

    private List<Vector3> _elemSpawnPositions = new List<Vector3>();
    public List<GameObject> _levelStageObjectCounterElements = new List<GameObject>() { };

    // THIS BIT IS FOR ITEM PULSING
    // serves as a switch to keep track of pickups to manipulate level stage goal icon
    [HideInInspector]
    public int ItemPickupChecker = 0;
    private int _itemPickupChecker
    {
        get { return ItemPickupChecker; }
        set
        {
            if (value != ItemPickupChecker)
            { ItemPickupChecker = value; _levelStageObjectDecremented = true; }
        }
    }
    private bool _levelStageObjectDecremented = false;
    private bool _pulseBottomReached = false;
    [HideInInspector]
    public bool StartItemDisplay = false;

    private void Start()
    {
        
    }

    private void OnEnable()
    { 
        PlayerScript.OnSpawn += AssignPlayer;
        EnemyScript.OnDie += (aStageLevel) =>
        {
            PulseLevelStageObjectIcon(aStageLevel);
            LevelStagePointersDecision(aStageLevel);
        }; // lambda with listed subscriptions that takes input from the event with 1 argument

        DarkObeliskScript.OnDie += (aStageLevel) =>
        {
            PulseLevelStageObjectIcon(aStageLevel);
            LevelStagePointersDecision(aStageLevel);
        };
    }

    private void OnDisable()
    {
        PlayerScript.OnSpawn -= AssignPlayer;
        
        EnemyScript.OnDie -= (aStageLevel) =>
        {
            PulseLevelStageObjectIcon(aStageLevel);
            LevelStagePointersDecision(aStageLevel);
        };
        DarkObeliskScript.OnDie -= (aStageLevel) =>
        {
            PulseLevelStageObjectIcon(aStageLevel);
            LevelStagePointersDecision(aStageLevel);
        };
    }

    private void AssignPlayer()
    { if (_player == null) { _player = GameObject.Find("Player"); } }

    private void PulseLevelStageObjectIcon(int aInt) 
    { }

    private void LevelStagePointersDecision(int aInt) 
    { }

    public void DrawHearts(string aRedrawDetails, int aNumberOfHearts)
    { }

    /*

    // Start is called before the first frame update
    void Start()
    {
        _screenResizeQueued = true;

        _lm = GameObject.Find("LevelManager").GetComponent<LevelManagerScript>();
        GameObject.Find("Canvas_UserInterface(BackGround)").GetComponent<Canvas>().worldCamera = Camera.main;
        GameObject.Find("Canvas_UserInterface(Display)").GetComponent<Canvas>().worldCamera = Camera.main;
        GameObject.Find("Canvas_UserInterface(Arrows)").GetComponent<Canvas>().worldCamera = Camera.main;

        _heartSpawnPositions = DefineHeartPositions();
        _elemSpawnPositions = DefineLevelStageObjectsCounterPositions();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (_lm.LevelStage >= 0 && _lm.LevelStage < 4) { StartItemDisplay = true; }
        else StartItemDisplay = false;
        DrawLevelStageObjects();

        MonitorLevelStagePointers();
        PulseLevelStageObjectIcon();

        if (_screenResizeQueued) { ResizeDisplay(); }
    }


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

    private void PulseLevelStageObjectIcon()
    {
        // if the object has been decremented or changed in any way
        // and the list of level stage objects is not empty
        if (_levelStageObjectDecremented && _levelStageObjectCounterElements.Any())
        {
            // if boolean has not been flipped - keep decreasing Image scale until it reaches PulseObjectIconUntilScale
            if (!_pulseBottomReached)
            {
                Vector3 LocalScale = _levelStageObjectCounterElements[3].GetComponent<RectTransform>().localScale;
                Vector3 newLocalScale = LocalScale - (new Vector3(Time.deltaTime, Time.deltaTime, Time.deltaTime) * SpeedOfPulse);
                _levelStageObjectCounterElements[3].GetComponent<RectTransform>().localScale = newLocalScale;

                if (_levelStageObjectCounterElements[3].GetComponent<RectTransform>().localScale.x <= PulseObjectIconUntilScale)
                { _pulseBottomReached = true; }
            }
            // Once boolean is flipped - decrease image scale until it's back to 1 and flip both pickup and pulse bottom booleans
            if (_pulseBottomReached)
            {
                Vector3 LocalScale = _levelStageObjectCounterElements[3].GetComponent<RectTransform>().localScale;
                Vector3 newLocalScale = LocalScale + (new Vector3(Time.deltaTime, Time.deltaTime, Time.deltaTime) * SpeedOfPulse);
                _levelStageObjectCounterElements[3].GetComponent<RectTransform>().localScale = newLocalScale;

                if (_levelStageObjectCounterElements[3].GetComponent<RectTransform>().localScale.x >= 1)
                { _levelStageObjectDecremented = false; _pulseBottomReached = false; }
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
