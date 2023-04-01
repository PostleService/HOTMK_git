using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;

public class CreditsManager : MonoBehaviour
{
    public GameObject StartingCreditsScreen;
    public GameObject CurrentCreditsScreen;
    public float TimeOffsetToSpawnFirstScreen = 1f;
    public Texture2D CursorSkin;

    [HideInInspector]
    public GameObject EventSystemObject;
    private GameObject _confirmButton;
    private GameObject _previousButton;
    private GameObject _nextButton;
    private bool _startingScreenSpawned = false;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.SetCursor(CursorSkin, Vector2.zero, CursorMode.Auto);
        EventSystemObject = GameObject.Find("EventSystem");
    }

    // Update is called once per frame
    void Update()
    {
        DecrementTimer();
        MonitorInput();    
    }

    public void MonitorInput()
    {
        if (CurrentCreditsScreen != null)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            { if (_previousButton != null) _previousButton.GetComponent<Button>().onClick.Invoke(); }

            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            { if (_nextButton != null) _nextButton.GetComponent<Button>().onClick.Invoke(); }

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return))
            { if (_confirmButton != null) _confirmButton.GetComponent<Button>().onClick.Invoke(); }
        }
    }

    public void SpawnTutorialWindow(GameObject aTutorialWindow)
    {
        Time.timeScale = 0;

        GameObject TutorialGO = Instantiate(aTutorialWindow, Vector3.zero, Quaternion.identity, GameObject.Find("Menu").transform);
        TutorialGO.name = aTutorialWindow.name;
        CurrentCreditsScreen = TutorialGO;

        if (CurrentCreditsScreen != null)
        {
            Transform ConfirmButtonTr = CurrentCreditsScreen.transform.Find("ConfirmButton");
            if (ConfirmButtonTr != null)
            {
                GameObject ConfirmButton = ConfirmButtonTr.gameObject;
                _confirmButton = ConfirmButton;
                EventSystemObject.GetComponent<EventSystem>().SetSelectedGameObject(ConfirmButton);
                ConfirmButton.GetComponent<Button>().onClick.AddListener(BackToMain);
            }
            Transform PreviousButtonTr = CurrentCreditsScreen.transform.Find("PreviousButton");
            if (PreviousButtonTr != null)
            {
                GameObject PreviousButton = PreviousButtonTr.gameObject;
                _previousButton = PreviousButton;
                GameObject PreviousWindow = PreviousButton.GetComponent<TutorialButton_PreviousNextScreen>().TutorialWindowToSpawn;
                PreviousButton.GetComponent<Button>().onClick.AddListener(delegate { SpawnTutorialWindowPreviousNext(PreviousWindow); });

            }
            Transform NextButtonTr = CurrentCreditsScreen.transform.Find("NextButton");
            if (NextButtonTr != null)
            {
                GameObject NextButton = NextButtonTr.gameObject;
                _nextButton = NextButton;
                GameObject NextWindow = NextButton.GetComponent<TutorialButton_PreviousNextScreen>().TutorialWindowToSpawn;
                NextButton.GetComponent<Button>().onClick.AddListener(delegate { SpawnTutorialWindowPreviousNext(NextWindow); });
            }
        }
    }

    public void BackToMain()
    { SceneManager.LoadScene(0); }

    public void SpawnTutorialWindowPreviousNext(GameObject aTutorialWindow)
    {
        Destroy(CurrentCreditsScreen);
        SpawnTutorialWindow(aTutorialWindow);
    }

    public void DecrementTimer()
    {
        if (TimeOffsetToSpawnFirstScreen > 0)
        { TimeOffsetToSpawnFirstScreen -= Time.unscaledDeltaTime; }
        else
        { 
            if (StartingCreditsScreen != null && !_startingScreenSpawned) 
            { 
                SpawnTutorialWindow(StartingCreditsScreen);
                _startingScreenSpawned = true;
            } 
        }

    }
}
