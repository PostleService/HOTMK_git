using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialFadingScript : MonoBehaviour
{
    public GameObject TutorialWindow;
    [Tooltip("Mostly for area collisions teleporting on stage change")] public bool DestroyTriggerOnExit = false;
    [Tooltip("Close any open fading tutorials")] public bool OverridePreviouslyOpenWindows = false;
    private GameObject _currentlyShownTutorialWindow;

    private MenuManagerScript _menuManager;
    private bool _overridePerformed = false;

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
            // if the message is to override others - it will close any message currently open in the area
            if (_overridePerformed == false)
            {
                GameObject otherOpenWindows = GameObject.FindGameObjectWithTag("TutorialMessageFading");
                if (OverridePreviouslyOpenWindows && otherOpenWindows != null)
                { if (otherOpenWindows.name != gameObject.name) otherOpenWindows.GetComponent<TutorialFadingPanelScript>().CloseTutorialButton(); }
                _overridePerformed = true;
            }

            if (GameObject.FindGameObjectWithTag("TutorialMessageFading") == null)
            {
                // spawn new tutorial window if none exists so far and none is present on the scene
                if (_currentlyShownTutorialWindow == null && TutorialWindow != null)
                {
                    Transform parentTr = GameObject.Find("Canvas_UserInterface(BackGround)").transform;
                    _currentlyShownTutorialWindow = Instantiate(TutorialWindow, parentTr);
                    _currentlyShownTutorialWindow.transform.SetSiblingIndex(0);
                    _currentlyShownTutorialWindow.GetComponent<TutorialFadingPanelScript>().CallingTrigger = gameObject;
                    _currentlyShownTutorialWindow.GetComponent<TutorialFadingPanelScript>().MenuManager = _menuManager;
                }
                // fade in existing tutorial window object if it hasn't fully faded out and got removed
                else if (_currentlyShownTutorialWindow != null)
                { _currentlyShownTutorialWindow.GetComponent<TutorialFadingPanelScript>().ChangeFadeInValue(true); }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player" && 
            _currentlyShownTutorialWindow != null)
        { _currentlyShownTutorialWindow.GetComponent<TutorialFadingPanelScript>().ChangeFadeInValue(false); }
        if (collision.tag == "Player" && DestroyTriggerOnExit) Destroy(gameObject);
    }

}
