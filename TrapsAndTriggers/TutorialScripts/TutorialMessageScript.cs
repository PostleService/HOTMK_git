using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TutorialMessageScript : MonoBehaviour
{
    [Tooltip("Distance from player at which the message starts being visible")]
    public float DistanceFromPlayer = 3f;
    public GameObject TutorialWindowToSpawn;
    public GameObject DeathObject;

    private MenuManagerScript _menuManager;
    private GameObject _player;
    
    private SpriteRenderer _spriteRenderer;
    private Light2D _light2D;

    // Start is called before the first frame update
    void Start()
    {
        _menuManager = GameObject.Find("MenuManager").GetComponent<MenuManagerScript>();
        _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        _light2D = gameObject.GetComponent<Light2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        MonitorPlayerProximity();
    }

    private void OnEnable()
    {
        PlayerScript.OnSpawn += AssignPlayer;
        MenuManagerScript.OnUntoggleTutorials += InstantDerender;
    }

    private void OnDisable()
    {
        PlayerScript.OnSpawn -= AssignPlayer;
        MenuManagerScript.OnUntoggleTutorials -= InstantDerender;
    }

    void MonitorPlayerProximity()
    {
        if (_player != null && _menuManager.CurrentTutorialSetting == true)
        {
            if (!_spriteRenderer.enabled)
            {
                if (Vector3.Distance(gameObject.transform.position, _player.transform.position) <= DistanceFromPlayer)
                { Render(); }
            }
            else
            {
                if (Vector3.Distance(gameObject.transform.position, _player.transform.position) > DistanceFromPlayer)
                { InstantDerender(); }
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            if (_menuManager.CurrentTutorialSetting)
            {
                Destroy(gameObject);

                if (DeathObject != null)
                { Instantiate(DeathObject, transform.position, Quaternion.identity, GameObject.Find("EnemyCorpseHolder").transform); }

                if (TutorialWindowToSpawn != null)
                {
                    _menuManager.SpawnTutorialWindow(TutorialWindowToSpawn);
                }
            }
            else if (!_menuManager.CurrentTutorialSetting) Destroy(gameObject);
        }
    }

    private void AssignPlayer(GameObject aGameObject)
    { _player = aGameObject; }

    public void Render()
    {
        _spriteRenderer.enabled = true;
        _light2D.enabled = true;
    }

    public void InstantDerender()
    { 
        _spriteRenderer.enabled = false;
        _light2D.enabled = false;
    }

}
