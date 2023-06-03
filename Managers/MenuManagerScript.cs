using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using TMPro;
using FMODUnityResonance;
using FMODUnity;
using FMOD;

public class MenuManagerScript : MonoBehaviour
{
    private AudioManager _audioManager;
    private Volume _postProcessingManager;
    private GameManager _gameManager;
    [Header("Cursor Settings")]
    public bool ConcealCursorInGame = true;
    public Texture2D CursorSkin;

    // Input controls
    private InputControl _inputControl;
    private bool _menuCall = false;
    private bool _menuCancelCall = false;
    private bool _submenuCancelCall = false;

    [Header("Menu objects")]
    public GameObject _menuObject;
    private bool _menuCalled = false;
    private bool _submenuCalled = false;
    public GameObject _resetProgressMenuObject;
    private bool _resetProgressMenuCalled = false;
    public GameObject _restartMenuObject;
    private bool _restartMenuCalled = false;
    public GameObject _chooseLevelMenuObject;
    private bool _chooseLevelMenuCalled = false;
    public GameObject _optionsMenuObject;
    private bool _optionsMenuCalled = false;
    public GameObject _optionsMenuGraphicsObject;
    private bool _optionsMenuGraphicsCalled = false;
    public GameObject _optionsMenuAudioObject;
    private bool _optionsMenuAudioCalled = false;
    public GameObject _acceptChangesOrLeaveMenuObject;
    private bool _acceptChangesOrLeaveMenuCalled = false;
    public GameObject _quitToMainMenuMenuObject;
    private bool _quitToMainMenuMenuCalled = false;
    public GameObject _quitToOSMenuObject;
    private bool _quitToOSMenuCalled = false;

    public GameObject _defeatScreenObject;
    private bool _defeatScreenCalled = false;
    public GameObject _victoryScreenObject;
    private bool _victoryScreenCalled = false;
    private bool _tutorialWindowCalled = false;

    public float DeathToDefeatScreenSec = 1.3f;
    public float ToVictoryScreenSec = 1.3f;
    private bool _playerDead = false;
    private bool _playerWon = false;
    private bool _defeatShown = false;

    [Header("Menu navigation")]
    public GameObject EventSystemObject;
    public GameObject LastSelectedButton;

    [Header("OptionsMenu")]
    [Header("Volume")]
    public GameObject VolumeMusicSlider;
    public GameObject VolumeMusicText;
    public GameObject VolumeAmbientSlider;
    public GameObject VolumeAmbientText;
    public GameObject VolumeSoundEffectsSlider;
    public GameObject VolumeSoundEffectsText;


    [Header("Tutorials")]
    public GameObject CurrentTutorialObject;

    [Header("GraphicsAndTutorials")]
    public GameObject BrightnessSlider;
    public GameObject BrightnessText;
    public GameObject DropdownResolutions;
    public GameObject ToggleFullScreen;
    public GameObject ToggleVSync;
    public GameObject ToggleTutorials;
    private Resolution[] _resolutionsArray;
    List<Resolution> _resolutionsList = new List<Resolution>();

    // Options menu settings

    public float CurrentBrightnessSetting;
    public Vector2 CurrentScreenResolution;
    public bool CurrentVSyncSetting;
    public bool CurrentFullScreenSetting;
    public bool CurrentTutorialSetting;
    public float CurrentMusicVolumeSetting;
    public float CurrentAmbientVolumeSetting;
    public float CurrentSoundEffectsVolumeSetting;

    public float TempBrightnessSetting;
    public Vector2 TempScreenResolution;
    public bool TempVSyncSetting;
    public bool TempFullScreenSetting;
    public bool TempTutorialSetting;
    public float TempMusicVolumeSetting;
    public float TempAmbientVolumeSetting;
    public float TempSoundEffectsVolumeSetting;

    public Dictionary<int, bool> LevelProgress =
        new Dictionary<int, bool>()
        {
            { 1, true },
            { 2, false },
            { 3, false },
            { 4, false },
            { 5, false },
            { 6, false },
            { 7, false },
            { 8, false },
            { 9, false },
            { 10, false },
            { 11, false },
            { 12, false }
        };
    public bool SettingsLoaded = false;

    public static event Action OnUntoggleTutorials;

    private void OnEnable()
    { 
        AnimationEndDetection_PlayerDeath.OnDie += ReactToPlayerDeath;
        _inputControl.MenuInputMonitor.Enable();
    }

    private void OnDisable()
    { Unsubscribe(); }

    private void Awake()
    {
        _inputControl = new InputControl();

        _inputControl.MenuInputMonitor.MenuCall.started += (value) => _menuCall = true;
        _inputControl.MenuInputMonitor.MenuCall.canceled += (value) => _menuCall = false;

        _inputControl.MenuInputMonitor.MenuCallCancel.started += (value) => _menuCancelCall = true;
        _inputControl.MenuInputMonitor.MenuCallCancel.canceled += (value) => _menuCancelCall = false;

        _inputControl.MenuInputMonitor.SubmenuCallCancel.started += (value) => _submenuCancelCall = true;
        _inputControl.MenuInputMonitor.SubmenuCallCancel.canceled += (value) => _submenuCancelCall = false;
    }

    private void Start()
    {
        Cursor.SetCursor(CursorSkin, Vector2.zero, CursorMode.Auto);
        if (ConcealCursorInGame) CursorVisibility(false);

        _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        _audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
        _postProcessingManager = GameObject.Find("PostProcessingManager").GetComponent<Volume>();

        foreach (Transform ChTr in GameObject.Find("Menu").transform)
        { ChTr.GetComponent<Canvas>().worldCamera = Camera.main; }

        LoadSettings();
        GatherResolutions();

        if (SceneManager.GetActiveScene().name == "MainMenu")
        { OpenMenu(); }
    }

    // Update is called once per frame
    void Update()
    {
        MonitorForMenuCall();
        MonitorSubmenuEscape();
        MonitorForCurrentlySelectedButton();

        CountDownToVictoryScreen();
        CountDownToDefeatScreen();
    }

    public void Unsubscribe()
    {
        AnimationEndDetection_PlayerDeath.OnDie -= ReactToPlayerDeath;
        _inputControl.MenuInputMonitor.Disable();
    }

    public void UnsubscribeAll()
    {
        if (GameObject.Find("LevelManager") != null) { GameObject.Find("LevelManager").GetComponent<LevelManagerScript>().Unsubscribe(); }
        if (GameObject.Find("UIManager") != null) { GameObject.Find("UIManager").GetComponent<UIManager>().Unsubscribe(); }
        Unsubscribe();
    }

    public void CursorVisibility(bool aValue)
    { Cursor.visible = aValue; }

    private void LoadSettings()
    {
        _gameManager.LoadGame();
        CurrentBrightnessSetting = _gameManager.BrightnessSetting;
        CurrentScreenResolution = new Vector2(_gameManager.ScreenResolution.x, _gameManager.ScreenResolution.y);
        CurrentVSyncSetting = _gameManager.VSyncSetting;
        CurrentFullScreenSetting = _gameManager.FullScreenSetting;
        CurrentTutorialSetting = _gameManager.TutorialSetting;
        CurrentMusicVolumeSetting = _gameManager.MusicVolumeSetting;
        CurrentAmbientVolumeSetting = _gameManager.AmbientVolumeSetting;
        CurrentSoundEffectsVolumeSetting = _gameManager.SoundEffectsVolumeSetting;

        TempBrightnessSetting = CurrentBrightnessSetting;
        TempScreenResolution = CurrentScreenResolution;
        TempVSyncSetting = CurrentVSyncSetting;
        TempFullScreenSetting = CurrentFullScreenSetting;
        TempTutorialSetting = CurrentTutorialSetting;
        TempMusicVolumeSetting = CurrentMusicVolumeSetting;
        TempAmbientVolumeSetting = CurrentAmbientVolumeSetting;
        TempSoundEffectsVolumeSetting = CurrentSoundEffectsVolumeSetting;

        LevelProgress[1] = _gameManager.LevelProgress[1];
        LevelProgress[2] = _gameManager.LevelProgress[2];
        LevelProgress[3] = _gameManager.LevelProgress[3];
        LevelProgress[4] = _gameManager.LevelProgress[4];
        LevelProgress[5] = _gameManager.LevelProgress[5];
        LevelProgress[6] = _gameManager.LevelProgress[6];
        LevelProgress[7] = _gameManager.LevelProgress[7];
        LevelProgress[8] = _gameManager.LevelProgress[8];
        LevelProgress[9] = _gameManager.LevelProgress[9];
        LevelProgress[10] = _gameManager.LevelProgress[10];
        LevelProgress[11] = _gameManager.LevelProgress[11];
        LevelProgress[12] = _gameManager.LevelProgress[12];

        // Set all menu values apart from resolution, which is done in GatherResolutions()

        _audioManager.ChangeMusicVolume(CurrentMusicVolumeSetting * 100);
        VolumeMusicSlider.GetComponent<Slider>().value = CurrentMusicVolumeSetting;
        float ResultingValueMusic = CurrentMusicVolumeSetting * 100;
        VolumeMusicText.GetComponent<TextMeshProUGUI>().text = ((int)ResultingValueMusic).ToString();

        _audioManager.ChangeAmbientVolume(CurrentAmbientVolumeSetting * 100);
        VolumeAmbientSlider.GetComponent<Slider>().value = CurrentAmbientVolumeSetting;
        float ResultingValueAmbient = CurrentAmbientVolumeSetting * 100;
        VolumeAmbientText.GetComponent<TextMeshProUGUI>().text = ((int)ResultingValueAmbient).ToString();

        _audioManager.ChangeSoundEffectsVolume(CurrentSoundEffectsVolumeSetting * 100);
        VolumeSoundEffectsSlider.GetComponent<Slider>().value = CurrentSoundEffectsVolumeSetting;
        float ResultingValueSFX = CurrentSoundEffectsVolumeSetting * 100;
        VolumeSoundEffectsText.GetComponent<TextMeshProUGUI>().text = ((int)ResultingValueSFX).ToString();

        VolumeProfile vp = _postProcessingManager.profile;
        UnityEngine.Rendering.Universal.ColorAdjustments colorAdjustments;
        vp.TryGet(out colorAdjustments);
        colorAdjustments.postExposure.Override(CurrentBrightnessSetting);
        float SliderMinValue = BrightnessSlider.GetComponent<Slider>().minValue; float SliderMaxValue = BrightnessSlider.GetComponent<Slider>().maxValue; float SliderRange = SliderMaxValue - SliderMinValue;
        float IntMinValue = 0; float IntMaxValue = 100; float IntRange = IntMaxValue - IntMinValue;
        float ResultingValueBrightness = (((CurrentBrightnessSetting - SliderMinValue) * IntRange) / SliderRange) + IntMinValue;
        BrightnessText.GetComponent<TextMeshProUGUI>().text = ((int)ResultingValueBrightness).ToString();
        BrightnessSlider.GetComponent<Slider>().value = CurrentBrightnessSetting;

        ToggleFullScreen.GetComponent<Toggle>().isOn = CurrentFullScreenSetting;
        Screen.fullScreen = CurrentFullScreenSetting;

        ToggleVSync.GetComponent<Toggle>().isOn = CurrentVSyncSetting;
        if (CurrentVSyncSetting) { QualitySettings.vSyncCount = 1; }
        else { QualitySettings.vSyncCount = 0; }

        ToggleTutorials.GetComponent<Toggle>().isOn = CurrentTutorialSetting;
    }

    // Gathers resolutions available on the current client and adds them to the dropdown as options to choose from
    private void GatherResolutions()
    {
        _resolutionsArray = Screen.resolutions;
        foreach (Resolution res in _resolutionsArray)
        { if (res.width >= 800) { _resolutionsList.Add(res); } }

        List<string> resArrayString = new List<string>();

        int currentResolutionIndex = 0;
        for (int i = 0; i < _resolutionsList.Count; i++)
        {
            string option = _resolutionsList[i].width + " x " + _resolutionsList[i].height + " " + _resolutionsList[i].refreshRate + "Hz";
            resArrayString.Add(option);

            if (_resolutionsList[i].width == CurrentScreenResolution.x && _resolutionsList[i].height == CurrentScreenResolution.y)
            {
                currentResolutionIndex = i;
                Screen.SetResolution(_resolutionsList[i].width, _resolutionsList[i].height, Screen.fullScreen);
            }
        }

        TMP_Dropdown dropdown = DropdownResolutions.GetComponent<TMP_Dropdown>();

        dropdown.ClearOptions();
        dropdown.AddOptions(resArrayString);
        // Changing value causes more trouble than benefit. Will be fixed when saving settings is introduced
        dropdown.value = currentResolutionIndex;
        dropdown.RefreshShownValue();

        SettingsLoaded = true;
    }

    private void MonitorForMenuCall()
    {
        if (!_menuCalled && !_submenuCalled && !_tutorialWindowCalled)
        { if (_menuCall) { OpenMenu(); } }
        else if (_menuCalled)
        { if (_menuCancelCall) { CloseMenu(); } }
    }

    private void MonitorSubmenuEscape()
    {
        if (_resetProgressMenuCalled || _restartMenuCalled || _chooseLevelMenuCalled || _quitToMainMenuMenuCalled || _quitToOSMenuCalled)
        {
            if (_submenuCancelCall)
            { CloseSubmenu(); }
        }
        else if (_optionsMenuGraphicsCalled || _optionsMenuAudioCalled)
        {
            if (_submenuCancelCall)
            { OptionsButton(); }
        }
        else if (_optionsMenuCalled)
        {
            if (_submenuCancelCall)
            {
                if (_optionsMenuCalled)
                { OptionsButton_Back(); }
            }
        }
        else if (_acceptChangesOrLeaveMenuCalled)
        {
            if (_submenuCancelCall)
                OptionsButton_BackLeave();
        }
        else if (_defeatScreenCalled || _victoryScreenCalled)
        {
            if (_submenuCancelCall)
            {
                ExitToMainMenuButton_Yes();
            }
        }
        else if (_tutorialWindowCalled)
        {
            if (_submenuCancelCall)
            {
                SpawnTutorialWindowConfirm();
            }
        }
    }

    private void MonitorForCurrentlySelectedButton()
    {
        if (EventSystemObject.GetComponent<EventSystem>().currentSelectedGameObject != null)
        { LastSelectedButton = EventSystemObject.GetComponent<EventSystem>().currentSelectedGameObject; }
        else { EventSystemObject.GetComponent<EventSystem>().SetSelectedGameObject(LastSelectedButton); }
    }

    public void ChangeActiveButtonOnMouseOver(GameObject aButton)
    {
        EventSystemObject.GetComponent<EventSystem>().SetSelectedGameObject(aButton);
    }

    private void OpenMenu()
    {
        if (ConcealCursorInGame) CursorVisibility(true);
        _menuCalled = true;

        if (_playerDead)
        { _menuObject.transform.Find("ResumeButton").GetComponent<Button>().interactable = false; }

        int lastActiveLevel = 1;
        for (int i = 1; i <= LevelProgress.Count; i++)
        {
            if (LevelProgress[i] == true) { lastActiveLevel = i; }
            else break;
        }

        if (_menuObject.transform.Find("ContinueButton") != null)
        {
            if (lastActiveLevel == 1)
            { _menuObject.transform.Find("ContinueButton").GetChild(0).GetComponent<TextMeshProUGUI>().text = "New Game"; }
            else
            { _menuObject.transform.Find("ContinueButton").GetChild(0).GetComponent<TextMeshProUGUI>().text = "Continue"; }
        }

        EventSystemObject.GetComponent<EventSystem>().SetSelectedGameObject(null);
        EventSystemObject.GetComponent<EventSystem>().SetSelectedGameObject(_menuObject.GetComponent<DefaultButton>().DefaultButtonOfMenu);

        _menuObject.SetActive(true);
        Time.timeScale = 0;
        if (SceneManager.GetActiveScene().name != "MainMenu") _audioManager.MuffleMusicMenuOpen(1);
        else _audioManager.MuffleMusicMenuOpen(0);

        _menuCall = false;
        _menuCancelCall = false;
        _submenuCancelCall = false;
    }

    private void CloseMenu()
    {
        if (!_playerDead && SceneManager.GetActiveScene().name != "MainMenu")
        {
            if (ConcealCursorInGame && GameObject.FindGameObjectWithTag("TutorialMessageFading") == null) CursorVisibility(false);
            _menuCalled = false;

            _menuObject.SetActive(false);
            Time.timeScale = 1;
            _audioManager.MuffleMusicMenuOpen(0);

        }
        _menuCall = false;
        _menuCancelCall = false;
        _submenuCancelCall = false;
    }

    private void ShowDefeatScreen()
    {
        if (!_defeatShown)
        {
            EnterSubmenu();
            _defeatScreenCalled = true;
            _submenuCalled = true;

            EventSystemObject.GetComponent<EventSystem>().SetSelectedGameObject(null);
            EventSystemObject.GetComponent<EventSystem>().SetSelectedGameObject(_defeatScreenObject.GetComponent<DefaultButton>().DefaultButtonOfMenu);

            _defeatScreenObject.SetActive(true);
            _defeatShown = true;
        }
    }

    private void ShowVictoryScreen()
    {
        if (!_victoryScreenCalled)
        {
            EnterSubmenu();
            _victoryScreenCalled = true;
            _submenuCalled = true;

            EventSystemObject.GetComponent<EventSystem>().SetSelectedGameObject(null);
            EventSystemObject.GetComponent<EventSystem>().SetSelectedGameObject(_victoryScreenObject.GetComponent<DefaultButton>().DefaultButtonOfMenu);

            _victoryScreenObject.SetActive(true);
        }
    }

    public void VictoryScreen_NextLevel()
    {
        // change back to if (SceneManager.GetActiveScene().buildIndex < LevelProgress.Count) after more levels are ready
        // if the level is last level, quit to main menu
        if (SceneManager.GetActiveScene().buildIndex < 3)
        {
            UnsubscribeAll();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        else
        {
            UnsubscribeAll();
            SceneManager.LoadScene(4); 
        }
    }

    // reassign the script to be subscribed to event triggered by death animation when animations are finished
    private void ReactToPlayerDeath()
    { _playerDead = true; }

    private void CountDownToDefeatScreen()
    {
        if (_playerDead && DeathToDefeatScreenSec > 0) { DeathToDefeatScreenSec -= Time.deltaTime; }
        if (DeathToDefeatScreenSec <= 0) { ShowDefeatScreen(); }
    }

    private void CountDownToVictoryScreen()
    {
        if (_playerWon && ToVictoryScreenSec > 0) { ToVictoryScreenSec -= Time.deltaTime; }
        if (ToVictoryScreenSec <= 0) { ShowVictoryScreen(); }

    }

    public void ReactToVictory() { _playerWon = true; }

    // set main menu as inactive while in submenu
    private void EnterSubmenu()
    {
        if (ConcealCursorInGame) CursorVisibility(true);
        _menuCalled = false;
        _menuObject.SetActive(false);
    }

    private void CloseSubmenu()
    {
        _restartMenuObject.SetActive(false);
        _chooseLevelMenuObject.SetActive(false);
        _optionsMenuObject.SetActive(false);
        _quitToMainMenuMenuObject.SetActive(false);
        _quitToOSMenuObject.SetActive(false);
        _acceptChangesOrLeaveMenuObject.SetActive(false);
        if (_resetProgressMenuObject != null)
            _resetProgressMenuObject.SetActive(false);

        _restartMenuCalled = false;
        _chooseLevelMenuCalled = false;
        _optionsMenuCalled = false;
        _quitToMainMenuMenuCalled = false;
        _quitToOSMenuCalled = false;
        _acceptChangesOrLeaveMenuCalled = false;
        _resetProgressMenuCalled = false;

        _submenuCalled = false;

        OpenMenu();

        _menuCall = false;
        _menuCancelCall = false;
        _submenuCancelCall = false;
    }

    // LEVEL PROGRESSION

    public void ResumeButton() { CloseMenu(); }

    public void ContinueButton()
    {
        int lastActiveLevel = 1;
        for (int i = 1; i <= LevelProgress.Count; i++)
        {
            if (LevelProgress[i] == true) { lastActiveLevel = i; }
            else break;
        }
        Time.timeScale = 1;
        
        UnsubscribeAll();
        SceneManager.LoadScene(lastActiveLevel);
    }

    public void RestartButton()
    {
        EnterSubmenu();
        _restartMenuCalled = true;
        _submenuCalled = true;

        EventSystemObject.GetComponent<EventSystem>().SetSelectedGameObject(null);
        EventSystemObject.GetComponent<EventSystem>().SetSelectedGameObject(_restartMenuObject.GetComponent<DefaultButton>().DefaultButtonOfMenu);

        _restartMenuObject.SetActive(true);
    }

    public void RestartButton_Yes()
    {
        _menuCalled = false;

        _menuObject.SetActive(false);
        Time.timeScale = 1;

        UnsubscribeAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    }

    public void RestartButton_No()
    { CloseSubmenu(); }

    public void ChooseLevelButton()
    {
        EnterSubmenu();
        _chooseLevelMenuCalled = true;
        _submenuCalled = true;

        EventSystemObject.GetComponent<EventSystem>().SetSelectedGameObject(null);
        EventSystemObject.GetComponent<EventSystem>().SetSelectedGameObject(_chooseLevelMenuObject.GetComponent<DefaultButton>().DefaultButtonOfMenu);

        foreach (Transform childtrf in _chooseLevelMenuObject.transform)
        {
            if (childtrf.GetComponent<LevelChoiceButton>() != null)
            { childtrf.GetComponent<Button>().interactable = LevelProgress[childtrf.GetComponent<LevelChoiceButton>().LevelIndexToLoad]; }
        }

        _chooseLevelMenuObject.SetActive(true);
    }

    public void ChooseLevel_ChangeLevel(Button aButton)
    {
        CloseMenu();
        CloseSubmenu();
        Time.timeScale = 1;

        // if loading scene fails, load main menu
        UnsubscribeAll();
        SceneManager.LoadScene(aButton.GetComponent<LevelChoiceButton>().LevelIndexToLoad);
    }

    public void ChooseLevel_Back()
    { CloseSubmenu(); }

    public void ResetProgressButton()
    {
        EnterSubmenu();
        _resetProgressMenuCalled = true;
        _submenuCalled = true;

        EventSystemObject.GetComponent<EventSystem>().SetSelectedGameObject(null);
        EventSystemObject.GetComponent<EventSystem>().SetSelectedGameObject(_resetProgressMenuObject.GetComponent<DefaultButton>().DefaultButtonOfMenu);

        _resetProgressMenuObject.SetActive(true);
    }

    public void ResetProgressButton_Yes()
    {
        // start from lvl2 and and equalize all dictionary values to false
        for (int i = 2; i <= LevelProgress.Count; i++)
        {
            LevelProgress[i] = false;
            _gameManager.LevelProgress[i] = false;
            _gameManager.SaveGame();
        }
        CloseSubmenu();
    }

    public void ResetProgressButton_No()
    { CloseSubmenu(); }

    // OPTIONS MENU

    public void OptionsButton()
    {
        EnterSubmenu();
        _optionsMenuCalled = true;
        _submenuCalled = true;

        _optionsMenuGraphicsCalled = false; _optionsMenuAudioCalled = false;
        _optionsMenuGraphicsObject.SetActive(false); _optionsMenuAudioObject.SetActive(false);

        EventSystemObject.GetComponent<EventSystem>().SetSelectedGameObject(null);
        EventSystemObject.GetComponent<EventSystem>().SetSelectedGameObject(_optionsMenuObject.GetComponent<DefaultButton>().DefaultButtonOfMenu);

        _optionsMenuObject.SetActive(true);
        _menuCancelCall = false;
        _submenuCancelCall = false;
    }

    public void OptionsGraphicsButton()
    {
        _optionsMenuObject.SetActive(false);
        _optionsMenuCalled = false;

        _optionsMenuGraphicsCalled = true;
        EventSystemObject.GetComponent<EventSystem>().SetSelectedGameObject(null);
        EventSystemObject.GetComponent<EventSystem>().SetSelectedGameObject(_optionsMenuGraphicsObject.GetComponent<DefaultButton>().DefaultButtonOfMenu);
        _optionsMenuGraphicsObject.SetActive(true);
    }

    public void OptionsAudioButton()
    {
        _optionsMenuObject.SetActive(false);
        _optionsMenuCalled = false;

        _optionsMenuAudioCalled = true;
        EventSystemObject.GetComponent<EventSystem>().SetSelectedGameObject(null);
        EventSystemObject.GetComponent<EventSystem>().SetSelectedGameObject(_optionsMenuAudioObject.GetComponent<DefaultButton>().DefaultButtonOfMenu);
        _optionsMenuAudioObject.SetActive(true);
    }

    public void SetVolume_Music(Slider aSlider)
    {
        if (SettingsLoaded)
        {
            TempMusicVolumeSetting = aSlider.value;
            _audioManager.ChangeMusicVolume(TempMusicVolumeSetting * 100);
            float ResultingValueMus = TempMusicVolumeSetting * 100;
            VolumeMusicText.GetComponent<TextMeshProUGUI>().text = ((int)ResultingValueMus).ToString();
        }
    }

    public void SetVolume_Ambient(Slider aSlider)
    {
        if (SettingsLoaded)
        {
            TempAmbientVolumeSetting = aSlider.value;
            _audioManager.ChangeAmbientVolume(TempAmbientVolumeSetting * 100);
            float ResultingValueAmbient = TempAmbientVolumeSetting * 100;
            VolumeAmbientText.GetComponent<TextMeshProUGUI>().text = ((int)ResultingValueAmbient).ToString();
        }
    }

    public void SetVolume_SoundEffects(Slider aSlider)
    {
        if (SettingsLoaded)
        {
            TempSoundEffectsVolumeSetting = aSlider.value;
            _audioManager.ChangeSoundEffectsVolume(TempSoundEffectsVolumeSetting * 100);
            float ResultingValueSFX = TempSoundEffectsVolumeSetting * 100;
            VolumeSoundEffectsText.GetComponent<TextMeshProUGUI>().text = ((int)ResultingValueSFX).ToString();
        }
    }

    public void SetBrightness(Slider aSlider)
    {
        if (SettingsLoaded)
        {
            TempBrightnessSetting = aSlider.value;

            VolumeProfile vp = _postProcessingManager.profile;
            UnityEngine.Rendering.Universal.ColorAdjustments colorAdjustments;
            vp.TryGet(out colorAdjustments);

            colorAdjustments.postExposure.Override(TempBrightnessSetting);

            float SliderMinValue = aSlider.minValue; float SliderMaxValue = aSlider.maxValue; float SliderRange = SliderMaxValue - SliderMinValue;
            float IntMinValue = 0; float IntMaxValue = 100; float IntRange = IntMaxValue - IntMinValue;
            float ResultingValueBrightness = (((TempBrightnessSetting - SliderMinValue) * IntRange) / SliderRange) + IntMinValue;
            BrightnessText.GetComponent<TextMeshProUGUI>().text = ((int)ResultingValueBrightness).ToString();
        }
    }

    public void Dropdown_Resolution()
    {
        if (SettingsLoaded)
        {
            Resolution res = _resolutionsList[DropdownResolutions.GetComponent<TMP_Dropdown>().value];
            TempScreenResolution = new Vector2(res.width, res.height);
        }
    }

    public void Toggle_FullScreen()
    {
        if (SettingsLoaded)
        { TempFullScreenSetting = ToggleFullScreen.GetComponent<Toggle>().isOn; }
    }

    public void Toggle_VSync()
    {
        if (SettingsLoaded)
        { TempVSyncSetting = ToggleVSync.GetComponent<Toggle>().isOn; }
    }

    public void Toggle_Tutorials()
    {
        if (SettingsLoaded)
        {
            bool ToggleValue = ToggleTutorials.GetComponent<Toggle>().isOn;
            TempTutorialSetting = ToggleValue;
            if (!ToggleValue) { DerenderTutorials(); }
            
            // Adjust toggle value for any fade-in tutorial windows
            if (GameObject.FindGameObjectWithTag("TutorialMessageFading") != null)
            { 
                GameObject tutorialPanel = GameObject.FindGameObjectWithTag("TutorialMessageFading");
                foreach (Transform chtr in tutorialPanel.transform)
                {
                    if (chtr.gameObject.name == "ShowTutorials")
                    { chtr.gameObject.GetComponent<Toggle>().isOn = ToggleValue; }
                }
            }

        }
    }

    public void DerenderTutorials() { OnUntoggleTutorials?.Invoke(); }

    public void SpawnTutorialWindow(GameObject aTutorialWindow)
    {
        if (ConcealCursorInGame) CursorVisibility(true);
        _tutorialWindowCalled = true;
        Time.timeScale = 0;
        
        GameObject TutorialGO = Instantiate(aTutorialWindow, Vector3.zero, Quaternion.identity, GameObject.Find("Menu").transform);
        TutorialGO.name = aTutorialWindow.name;
        TutorialGO.GetComponent<Canvas>().worldCamera = Camera.main;
        CurrentTutorialObject = TutorialGO;

        void AddTrigger(GameObject aButton)
        {
            EventTrigger trigger = aButton.GetComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerEnter;
            entry.callback.AddListener((eventData) => { ChangeActiveButtonOnMouseOver(aButton); });
            trigger.triggers.Add(entry);
        }

        if (CurrentTutorialObject != null)
        {
            Transform ConfirmButtonTr = CurrentTutorialObject.transform.Find("ConfirmButton");
            if (ConfirmButtonTr != null)
            {
                GameObject ConfirmButton = ConfirmButtonTr.gameObject;
                EventSystemObject.GetComponent<EventSystem>().SetSelectedGameObject(ConfirmButton);
                ConfirmButton.GetComponent<Button>().onClick.AddListener(SpawnTutorialWindowConfirm);

                AddTrigger(ConfirmButton);
            }
            Transform TutorialToggleTr = CurrentTutorialObject.transform.Find("ShowTutorials");
            if (TutorialToggleTr != null)
            {
                GameObject TutorialToggle = TutorialToggleTr.gameObject;
                TutorialToggle.GetComponent<Toggle>().onValueChanged.AddListener(delegate { SpawnTutorialWindowToggle(TutorialToggle.GetComponent<Toggle>()); });
                TutorialToggle.GetComponent<Toggle>().isOn = TempTutorialSetting;
            }
            Transform PreviousButtonTr = CurrentTutorialObject.transform.Find("PreviousButton");
            if (PreviousButtonTr != null)
            {
                GameObject PreviousButton = PreviousButtonTr.gameObject;
                GameObject PreviousWindow = PreviousButton.GetComponent<TutorialButton_PreviousNextScreen>().TutorialWindowToSpawn;
                PreviousButton.GetComponent<Button>().onClick.AddListener(delegate { SpawnTutorialWindowPreviousNext(PreviousWindow); });

                AddTrigger(PreviousButton);
            }
            Transform NextButtonTr = CurrentTutorialObject.transform.Find("NextButton");
            if (NextButtonTr != null)
            {
                GameObject NextButton = NextButtonTr.gameObject;
                GameObject NextWindow = NextButton.GetComponent<TutorialButton_PreviousNextScreen>().TutorialWindowToSpawn;
                NextButton.GetComponent<Button>().onClick.AddListener(delegate { SpawnTutorialWindowPreviousNext(NextWindow); });

                AddTrigger(NextButton);
            }
        }
    }

    public void SpawnTutorialWindowConfirm()
    {
        EventSystemObject.GetComponent<EventSystem>().SetSelectedGameObject(_menuObject.GetComponent<DefaultButton>().DefaultButtonOfMenu);
        Destroy(CurrentTutorialObject);

        _tutorialWindowCalled = false;
        if (ConcealCursorInGame) CursorVisibility(false);
        Time.timeScale = 1;

        CurrentTutorialSetting = TempTutorialSetting;
        ToggleTutorials.GetComponent<Toggle>().isOn = CurrentTutorialSetting;
        _gameManager.TutorialSetting = CurrentTutorialSetting;
        _gameManager.SaveGame();
    }

    public void SpawnTutorialWindowToggle(Toggle aToggle)
    {
        TempTutorialSetting = aToggle.isOn;
    }

    public void SpawnTutorialWindowPreviousNext(GameObject aTutorialWindow)
    {
        SpawnTutorialWindowConfirm();
        SpawnTutorialWindow(aTutorialWindow);
    }

    public void OptionsButton_AcceptChanges()
    {
        _audioManager.ChangeMusicVolume(TempMusicVolumeSetting * 100);
        float ResultingValueMus = TempMusicVolumeSetting * 100;
        VolumeMusicText.GetComponent<TextMeshProUGUI>().text = ((int)ResultingValueMus).ToString();

        _audioManager.ChangeAmbientVolume(TempAmbientVolumeSetting * 100);
        float ResultingValueAmbient = TempAmbientVolumeSetting * 100;
        VolumeAmbientText.GetComponent<TextMeshProUGUI>().text = ((int)ResultingValueAmbient).ToString();

        _audioManager.ChangeSoundEffectsVolume(TempSoundEffectsVolumeSetting * 100);
        float ResultingValueSFX = TempSoundEffectsVolumeSetting * 100;
        VolumeSoundEffectsText.GetComponent<TextMeshProUGUI>().text = ((int)ResultingValueSFX).ToString();

        VolumeProfile vp = _postProcessingManager.profile;
        UnityEngine.Rendering.Universal.ColorAdjustments colorAdjustments;
        vp.TryGet(out colorAdjustments);
        colorAdjustments.postExposure.Override(TempBrightnessSetting);
        float SliderMinValue = BrightnessSlider.GetComponent<Slider>().minValue; float SliderMaxValue = BrightnessSlider.GetComponent<Slider>().maxValue; float SliderRange = SliderMaxValue - SliderMinValue;
        float IntMinValue = 0; float IntMaxValue = 100; float IntRange = IntMaxValue - IntMinValue;
        float ResultingValueBrightness = (((TempBrightnessSetting - SliderMinValue) * IntRange) / SliderRange) + IntMinValue;
        BrightnessText.GetComponent<TextMeshProUGUI>().text = ((int)ResultingValueBrightness).ToString();

        Screen.SetResolution((int)TempScreenResolution.x, (int)TempScreenResolution.y, Screen.fullScreen);

        Screen.fullScreen = TempFullScreenSetting;
        if (TempVSyncSetting) { QualitySettings.vSyncCount = 1; }
        else { QualitySettings.vSyncCount = 0; }

        CurrentBrightnessSetting = TempBrightnessSetting;
        CurrentScreenResolution = TempScreenResolution;
        CurrentVSyncSetting = TempVSyncSetting;
        CurrentFullScreenSetting = TempFullScreenSetting;
        CurrentTutorialSetting = TempTutorialSetting;
        CurrentMusicVolumeSetting = TempMusicVolumeSetting;
        CurrentAmbientVolumeSetting = TempAmbientVolumeSetting;
        CurrentSoundEffectsVolumeSetting = TempSoundEffectsVolumeSetting;

        _gameManager.BrightnessSetting = TempBrightnessSetting;
        _gameManager.ScreenResolution = TempScreenResolution;
        _gameManager.VSyncSetting = TempVSyncSetting;
        _gameManager.FullScreenSetting = TempFullScreenSetting;
        _gameManager.TutorialSetting = TempTutorialSetting;
        _gameManager.MusicVolumeSetting = TempMusicVolumeSetting;
        _gameManager.AmbientVolumeSetting = TempAmbientVolumeSetting;
        _gameManager.SoundEffectsVolumeSetting = TempSoundEffectsVolumeSetting;
        _gameManager.SaveGame();
    }

    public void OptionsButton_Back()
    {
        if (CurrentBrightnessSetting == TempBrightnessSetting &&
        CurrentScreenResolution == TempScreenResolution &&
        CurrentVSyncSetting == TempVSyncSetting &&
        CurrentFullScreenSetting == TempFullScreenSetting &&
        CurrentTutorialSetting == TempTutorialSetting &&
        CurrentMusicVolumeSetting == TempMusicVolumeSetting &&
        CurrentAmbientVolumeSetting == TempAmbientVolumeSetting &&
        CurrentSoundEffectsVolumeSetting == TempSoundEffectsVolumeSetting)
        { CloseSubmenu(); }
        else 
        {
            _optionsMenuObject.SetActive(false);
            _optionsMenuCalled = false;
            _acceptChangesOrLeaveMenuObject.SetActive(true);
            _acceptChangesOrLeaveMenuCalled = true;

            EventSystemObject.GetComponent<EventSystem>().SetSelectedGameObject(null);
            EventSystemObject.GetComponent<EventSystem>().SetSelectedGameObject(_acceptChangesOrLeaveMenuObject.GetComponent<DefaultButton>().DefaultButtonOfMenu);

            _menuCancelCall = false;
            _submenuCancelCall = false;
        }
    }

    public void OptionsButton_BackAccept()
    {
        OptionsButton_AcceptChanges();
        _acceptChangesOrLeaveMenuObject.SetActive(false);
        CloseSubmenu();
    }

    public void OptionsButton_BackLeave()
    {
        // Reset temp values back to current ones
        TempBrightnessSetting = CurrentBrightnessSetting;
        TempScreenResolution = CurrentScreenResolution;
        TempVSyncSetting = CurrentVSyncSetting;
        TempFullScreenSetting = CurrentFullScreenSetting;
        TempTutorialSetting = CurrentTutorialSetting;
        TempMusicVolumeSetting = CurrentMusicVolumeSetting;
        TempAmbientVolumeSetting = CurrentAmbientVolumeSetting;
        TempSoundEffectsVolumeSetting = CurrentSoundEffectsVolumeSetting;

        // reset option menu toggles, sliders and dropdown
        _audioManager.ChangeMusicVolume(CurrentMusicVolumeSetting * 100);
        float ResultingValueMus = CurrentMusicVolumeSetting * 100;
        VolumeMusicSlider.GetComponent<Slider>().value = CurrentMusicVolumeSetting;
        VolumeMusicText.GetComponent<TextMeshProUGUI>().text = ((int)ResultingValueMus).ToString();

        _audioManager.ChangeAmbientVolume(CurrentAmbientVolumeSetting * 100);
        float ResultingValueAmbient = CurrentAmbientVolumeSetting * 100;
        VolumeAmbientSlider.GetComponent<Slider>().value = CurrentAmbientVolumeSetting;
        VolumeAmbientText.GetComponent<TextMeshProUGUI>().text = ((int)ResultingValueAmbient).ToString();

        _audioManager.ChangeSoundEffectsVolume(CurrentSoundEffectsVolumeSetting * 100);
        float ResultingValueSFX = CurrentSoundEffectsVolumeSetting * 100;
        VolumeSoundEffectsSlider.GetComponent<Slider>().value = CurrentSoundEffectsVolumeSetting;
        VolumeSoundEffectsText.GetComponent<TextMeshProUGUI>().text = ((int)ResultingValueSFX).ToString();

        VolumeProfile vp = _postProcessingManager.profile;
        UnityEngine.Rendering.Universal.ColorAdjustments colorAdjustments;
        vp.TryGet(out colorAdjustments);
        colorAdjustments.postExposure.Override(CurrentBrightnessSetting);
        float SliderMinValue = BrightnessSlider.GetComponent<Slider>().minValue; float SliderMaxValue = BrightnessSlider.GetComponent<Slider>().maxValue; float SliderRange = SliderMaxValue - SliderMinValue;
        float IntMinValue = 0; float IntMaxValue = 100; float IntRange = IntMaxValue - IntMinValue;
        float ResultingValueBrightness = (((CurrentBrightnessSetting - SliderMinValue) * IntRange) / SliderRange) + IntMinValue;
        BrightnessText.GetComponent<TextMeshProUGUI>().text = ((int)ResultingValueBrightness).ToString();

        int currentResolutionIndex = 0;
        for (int i = 0; i < _resolutionsList.Count; i++)
        {
            if (_resolutionsList[i].width == CurrentScreenResolution.x && _resolutionsList[i].height == CurrentScreenResolution.y)
            { currentResolutionIndex = i; }
        }

        TMP_Dropdown dropdown = DropdownResolutions.GetComponent<TMP_Dropdown>();
        // Changing value causes more trouble than benefit. Will be fixed when saving settings is introduced
        dropdown.value = currentResolutionIndex;
        dropdown.RefreshShownValue();

        ToggleFullScreen.GetComponent<Toggle>().isOn = CurrentFullScreenSetting;
        ToggleVSync.GetComponent<Toggle>().isOn = CurrentVSyncSetting;
        ToggleTutorials.GetComponent<Toggle>().isOn = CurrentTutorialSetting;

        _acceptChangesOrLeaveMenuObject.SetActive(false);
        CloseSubmenu();
    }

    // QUITTING A GAME

    // QUIT TO MAIN MENU
    public void ExitToMainMenuButton()
    {
        EnterSubmenu();
        _quitToMainMenuMenuCalled = true;
        _submenuCalled = true;

        EventSystemObject.GetComponent<EventSystem>().SetSelectedGameObject(null);
        EventSystemObject.GetComponent<EventSystem>().SetSelectedGameObject(_quitToMainMenuMenuObject.GetComponent<DefaultButton>().DefaultButtonOfMenu);

        _quitToMainMenuMenuObject.SetActive(true);
    }

    public void ExitToMainMenuButton_Yes()
    {
        Time.timeScale = 1;
        UnsubscribeAll();
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    public void ExitToMainMenuButton_No()
    { CloseSubmenu(); }

    public void LoadCreditsScene()
    {
        UnsubscribeAll();
        SceneManager.LoadScene(4); 
    }

    // QUIT TO OS
    public void ExitToOSButton()
    {
        EnterSubmenu();
        _quitToOSMenuCalled = true;
        _submenuCalled = true;

        EventSystemObject.GetComponent<EventSystem>().SetSelectedGameObject(null);
        EventSystemObject.GetComponent<EventSystem>().SetSelectedGameObject(_quitToOSMenuObject.GetComponent<DefaultButton>().DefaultButtonOfMenu);

        _quitToOSMenuObject.SetActive(true);
    }

    public void ExitToOSButton_Yes()
    {
        Time.timeScale = 1;
        Application.Quit();
    }

    public void ExitToOSButton_No()
    { CloseSubmenu(); }



    

}
