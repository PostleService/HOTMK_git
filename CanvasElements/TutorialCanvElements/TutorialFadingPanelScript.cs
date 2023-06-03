using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class TutorialFadingPanelScript : MonoBehaviour
{
    public float FadeInSpeed = 2f;
    private bool FadeInElseOut
    {
        get { return _fadeInElseOut; }
        set
        {
            if (value != _fadeInElseOut)
            { _fadeInElseOut = value; }
        }
    }
    private bool _fadeInElseOut = false;
    private bool _initialFadeInReceived = false;
    private bool _fadeInMaxReached = false;

    private List<Image> _allImageElems = new List<Image>();
    private List<TextMeshProUGUI> _allTextElems = new List<TextMeshProUGUI>();
    
    [HideInInspector] public GameObject CallingTrigger;
    [HideInInspector] public MenuManagerScript MenuManager;
    private GameManager _gameManager;

    private void Awake()
    {
        _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        Cursor.visible = true;

        GetAllChildrenRecursive(gameObject);
        SetAlpha(0);
        ChangeFadeInValue(true);
    }

    private void FixedUpdate()
    { FadeMessageInOut(); }

    public void ChangeFadeInValue(bool aValue)
    { 
        FadeInElseOut = aValue;
        if (_initialFadeInReceived == false) _initialFadeInReceived = true;
    }

    private void GetAllChildrenRecursive(GameObject aGO)
    {
        void GetTxtAndImgElems(GameObject go)
        {
            if (go.GetComponent<Image>() != null)
            { _allImageElems.Add(go.GetComponent<Image>()); }
            if (go.GetComponent<TextMeshProUGUI>() != null)
            { _allTextElems.Add(go.GetComponent<TextMeshProUGUI>()); }
        }
        GetTxtAndImgElems(aGO);
        
        foreach (Transform chtr in aGO.transform)
        { 
            GetAllChildrenRecursive(chtr.gameObject);
            if (chtr.gameObject.name == "CloseButton") GameObject.Find("EventSystem").GetComponent<EventSystem>().SetSelectedGameObject(chtr.gameObject);
        }
    }

    private void FadeMessageInOut()
    {
        // Fadein logic        
        if (_fadeInElseOut == true && _fadeInMaxReached == false)
        {
            float currentAlpha = _allImageElems[_allImageElems.Count - 1].color.a;
            float newAlpha = currentAlpha + (Time.fixedDeltaTime * FadeInSpeed);
            if (currentAlpha < 1 && newAlpha <= 1) { SetAlpha(newAlpha); }
            else { SetAlpha(1); _fadeInMaxReached = true; }
        }
        // Fadeout logic
        else if (_fadeInElseOut == false && _initialFadeInReceived == true)
        {
            float currentAlpha = _allImageElems[_allImageElems.Count - 1].color.a;
            float newAlpha = currentAlpha - (Time.fixedDeltaTime * FadeInSpeed);
            if (currentAlpha > 0 && newAlpha >= 0) { SetAlpha(newAlpha); }
            else { SetAlpha(0); Destroy(gameObject); Cursor.visible = false; }
        }
    }

    private void SetAlpha(float aAlpha)
    {
        foreach (TextMeshProUGUI txt in _allTextElems)
        { txt.color = new Color(txt.color.r, txt.color.g, txt.color.b, aAlpha); }
        foreach (Image img in _allImageElems)
        { img.color = new Color(img.color.r, img.color.g, img.color.b, aAlpha); }
    }

    public void ShowTutorialsToggle(Toggle aToggle)
    {
        MenuManager.CurrentTutorialSetting = aToggle.isOn;
        MenuManager.TempTutorialSetting = aToggle.isOn;
        MenuManager.ToggleTutorials.GetComponent<Toggle>().isOn = aToggle.isOn;
        _gameManager.TutorialSetting = aToggle.isOn;
        _gameManager.SaveGame();
    }

    public void CloseTutorialButton(Button aButton)
    {
        Destroy(CallingTrigger);
        ChangeFadeInValue(false);
    }

    public void ChangeActiveButtonOnMouseOver(GameObject aButton)
    { GameObject.Find("EventSystem").GetComponent<EventSystem>().SetSelectedGameObject(aButton); }

}
