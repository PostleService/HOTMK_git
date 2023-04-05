using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public Vector2 ScreenResolution;
    public bool VSyncSetting;
    public bool FullScreenSetting;
    public float BrightnessSetting;
    public bool TutorialSetting;
    public float MusicVolumeSetting;
    public float AmbientVolumeSetting;
    public float SoundEffectsVolumeSetting;
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

    private void Awake()
    {    
        // If there are no other instances of GameManager created, GameManager class will refer to this particular instance.
        // Otherwise, any copy of this object will be destroyed.
        // Do not child the object containing this script to anything or DoNotDestroyOnLoad cannot apply
        if (Instance == null) 
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        { Destroy(gameObject); }
    }

    public void SaveGame()
    { SaveSystem.SaveGame(this); }

    public void LoadGame()
    {
        SaveData saveData = SaveSystem.LoadGame();
        ScreenResolution = new Vector2 (saveData.ScreenResolutionX, saveData.ScreenResolutionY);
        VSyncSetting = saveData.VSyncSetting;
        FullScreenSetting = saveData.FullScreenSetting;
        BrightnessSetting = saveData.BrightnessSetting;
        TutorialSetting = saveData.TutorialSetting;
        MusicVolumeSetting = saveData.MusicVolumeSetting;
        AmbientVolumeSetting = saveData.AmbientVolumeSetting;
        SoundEffectsVolumeSetting = saveData.SoundEffectsVolumeSetting;

        LevelProgress[1] = saveData.LevelState_lvl1;
        LevelProgress[2] = saveData.LevelState_lvl2;
        LevelProgress[3] = saveData.LevelState_lvl3;
        LevelProgress[4] = saveData.LevelState_lvl4;
        LevelProgress[5] = saveData.LevelState_lvl5;
        LevelProgress[6] = saveData.LevelState_lvl6;
        LevelProgress[7] = saveData.LevelState_lvl7;
        LevelProgress[8] = saveData.LevelState_lvl8;
        LevelProgress[9] = saveData.LevelState_lvl9;
        LevelProgress[10] = saveData.LevelState_lvl10;
        LevelProgress[11] = saveData.LevelState_lvl11;
        LevelProgress[12] = saveData.LevelState_lvl12;

    }
}
