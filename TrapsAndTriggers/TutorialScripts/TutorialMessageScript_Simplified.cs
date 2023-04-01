using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TutorialMessageScript_Simplified : MonoBehaviour
{
    public GameObject TutorialWindowToSpawn;
    public GameObject DeathObject;

    private MenuManagerScript _menuManager;

    // Start is called before the first frame update
    void Start()
    { _menuManager = GameObject.Find("MenuManager").GetComponent<MenuManagerScript>(); }

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
                { _menuManager.SpawnTutorialWindow(TutorialWindowToSpawn); }
            }
            else if (!_menuManager.CurrentTutorialSetting) Destroy(gameObject);
        }
    }
}
