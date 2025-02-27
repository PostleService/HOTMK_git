using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BossLevelArenaDecrementMonitor : MonoBehaviour
{
    private bool _triggered = false;
    private UIManager _uiManager;

    [Header("Item details")]    
    public List<GameObject> Items = new List<GameObject>() { };

    [Tooltip("A number of items of each level the game will attempt to spawn")]
    private int DefaultItemsCount;
    private int _currentItemsCount;
    public int LevelStage = 2;

    [Tooltip("A visual representation of objectives in this area")]
    public Sprite Icon;

    [Header("Progression arrows")]
    public float ShowPointersAtPercentage = 0.2f;
    public int FailsafeObjectCount = 1;

    [Header("Counters")]
    public int KillBossAtItemAmount = 0;
    public int HideCounterAt = 0;

    public delegate void MyHandler(int aLevelStage, int aCurrentItems, int aDefaultItems, Sprite aSprite);
    public static event MyHandler OnPlayerCollision;
    
    public delegate void Decrementer(int aLevelStage, int aCurrentItems, int aDefaultItems, int aLevelStageItem);
    public static event Decrementer OnObjectiveDecrement;

    public delegate void DestructionMonitor(GameObject aGameObject); // passes to BossDespawnMonitor
    public static event DestructionMonitor OnDestroy;

    private void OnEnable()
    { DarkObeliskScript.OnDie += ReactToDeath; }

    private void OnDisable()
    { DarkObeliskScript.OnDie -= ReactToDeath; }

    // Start is called before the first frame update
    void Start()
    {
        DefaultItemsCount = Items.Count;
        _currentItemsCount = DefaultItemsCount;
        _uiManager = GameObject.Find("UIManager").GetComponent<UIManager>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        MonitorItems();
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" && _triggered == false)
        {
            OnPlayerCollision?.Invoke(LevelStage, _currentItemsCount, DefaultItemsCount, Icon);
            _uiManager.CurrentAreaProgressionMonitor = this.gameObject;
            _uiManager.ShowPointersAtPercentage = ShowPointersAtPercentage;
            _uiManager.FailsafeObjectCount = FailsafeObjectCount;
            _triggered = true;
        }
    }


    private void MonitorItems()
    {
        if (_triggered == true && _currentItemsCount <= KillBossAtItemAmount)
        { OnDestroy?.Invoke(this.gameObject); }
        if (_triggered == true && _currentItemsCount <= HideCounterAt)
        {
            _uiManager.ShowHideItems(false);
            Destroy(this.gameObject);
            OnDestroy?.Invoke(this.gameObject);
        }

    }

    private void ReactToDeath(int aInt, GameObject aGameObject)
    {
        if (Items.Contains(aGameObject))
        {
            _currentItemsCount -= 1;
            OnObjectiveDecrement?.Invoke(LevelStage, _currentItemsCount, DefaultItemsCount, aInt);
        }
    }

}
