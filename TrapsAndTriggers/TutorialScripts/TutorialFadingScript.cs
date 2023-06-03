using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialFadingScript : MonoBehaviour
{
    public GameObject TutorialWindow;
    private GameObject _currentlyShownTutorialWindow;

    private MenuManagerScript _menuManager;

    void Start()
    { _menuManager = GameObject.Find("MenuManager").GetComponent<MenuManagerScript>(); }

    // Update is called once per frame
    void FixedUpdate()
    {
        
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Player" && 
            _menuManager.CurrentTutorialSetting && 
            TutorialWindow != null)
        {
            // spawn new tutorial window if none exists so far
            if (_currentlyShownTutorialWindow == null && TutorialWindow != null)
            {
                _currentlyShownTutorialWindow = Instantiate(TutorialWindow, GameObject.Find("Canvas_UserInterface(BackGround)").transform);
                _currentlyShownTutorialWindow.GetComponent<TutorialFadingPanelScript>().CallingTrigger = gameObject;
                _currentlyShownTutorialWindow.GetComponent<TutorialFadingPanelScript>().MenuManager = _menuManager;
            }
            // fade in existing tutorial window object if it hasn't fully faded out and got removed
            else if (_currentlyShownTutorialWindow != null)
            { _currentlyShownTutorialWindow.GetComponent<TutorialFadingPanelScript>().ChangeFadeInValue(true); }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player" && 
            _currentlyShownTutorialWindow != null)
        { _currentlyShownTutorialWindow.GetComponent<TutorialFadingPanelScript>().ChangeFadeInValue(false); }
    }

}
