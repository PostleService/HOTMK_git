using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnityResonance;
using FMODUnity;
using FMOD;

public class AudioManager : MonoBehaviour
{
    private LevelManagerScript _levelManager;
    private bool _levelStageChanged = false;
    private bool _instantCheck = false;
    [Range(0,4)]
    [Tooltip("Only change in editor from 0 if you want to change initial music stage value")]
    public float LevelStageValue = 0f;
    private float _levelStageCurrent = 0f;
    [Tooltip("multiplier of level stage value shift")]
    public float LevelStageValueChangeSpeed = 1.25f;
    [Range(0, 100)]
    public float MasterVolume = 0f;
    [Range(0,100)]
    public float MusicVolume = 0f;
    [Range(0, 100)]
    public float AmbientVolume = 0f;
    [Range(0, 100)]
    public float SoundEffectsVolume = 0f;

    private StudioEventEmitter _musicSource;

    private void OnEnable()
    { LevelManagerScript.OnLevelStageChange += ReactToLvlChange; }

    private void OnDisable()
    { LevelManagerScript.OnLevelStageChange -= ReactToLvlChange; }

    private void Start()
    {
        MuffleMusicMenuOpen(0);

        _musicSource = GameObject.Find("PlayerCamera").GetComponent<StudioEventEmitter>();
        if (GameObject.Find("LevelManager") != null) 
        { _levelManager = GameObject.Find("LevelManager").GetComponent<LevelManagerScript>(); }

        _levelStageCurrent = LevelStageValue;
        _musicSource.SetParameter("LevelStage", _levelStageCurrent, true);
    }

    private void Update()
    {
        LevelStageTransition();
    }

    public void ReactToLvlChange(int aLevelStage, int aCurrentItems, int aDefaultItems, Sprite aSprite) 
    {
        LevelStageValue = aLevelStage;
        _levelStageChanged = true; 
        _instantCheck = false;
        UnityEngine.Debug.LogWarning("Reacting to level statge change" + aLevelStage);
    }

    public void LevelStageTransition()
    {
        if (_levelStageChanged)
        {
            // this bit makes sure that level stage is not higher than expected (preset in this code to be higher to skip to later music stage, e.g.)

            if (_instantCheck == false)
            {
                if (_levelStageCurrent > LevelStageValue) { _levelStageChanged = false; }
                _instantCheck = true;
            }
            // after instantCheck
            else if (_instantCheck == true)
            {
                // make sure we don't overflow the value of current level stage
                if (_levelStageCurrent < LevelStageValue && (_levelStageCurrent + (Time.unscaledDeltaTime * LevelStageValueChangeSpeed)) < LevelStageValue)
                {
                    _levelStageCurrent += (Time.unscaledDeltaTime * LevelStageValueChangeSpeed);
                    _musicSource.SetParameter("LevelStage", _levelStageCurrent, true);
                }
                else
                {
                    _levelStageCurrent = LevelStageValue;
                    _musicSource.SetParameter("LevelStage", _levelStageCurrent, true);
                    _levelStageChanged = false;
                }
            }

        }
    }

    #region ON CALL FUNCTIONS

    public void ChangeMasterVolume(float aVol)
    {
        if (MasterVolume != aVol)
        {
            MasterVolume = aVol;
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("MasterVolume", MasterVolume, true);
        }
    }

    public void ChangeMusicVolume(float aVol)
    {
        if (MusicVolume != aVol)
        {
            MusicVolume = aVol;
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("MusicVolume", MusicVolume, true); 
        }
    }

    public void ChangeAmbientVolume(float aVol)
    {
        if (AmbientVolume != aVol)
        {
            AmbientVolume = aVol;
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("AmbientVolume", AmbientVolume, true); 
        }
    }

    public void ChangeSoundEffectsVolume(float aVol)
    {
        if (SoundEffectsVolume != aVol)
        {
            SoundEffectsVolume = aVol;
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("SoundEffectsVolume", SoundEffectsVolume, true);
        }
    }

    public void MuffleMusicMenuOpen(int aValue)
    {
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("MenuOpen", aValue, true);
    }

    #endregion ON CALL FUNCTIONS
}
