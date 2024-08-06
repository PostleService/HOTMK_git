using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public float ScreenResolutionX;
    public float ScreenResolutionY;
    public bool VSyncSetting;
    public bool FullScreenSetting;
    public float BrightnessSetting;
    public bool TutorialSetting;
    public float MasterVolumeSetting;
    public float MusicVolumeSetting;
    public float AmbientVolumeSetting;
    public float SoundEffectsVolumeSetting;
    public bool TutorialPassed;

    // Level states broken down in separate bools to be able to pass to SaveSystem
    public bool LevelState_lvl1 = true;
    public bool LevelState_lvl2;
    public bool LevelState_lvl3;
    public bool LevelState_lvl4;
    public bool LevelState_lvl5;
    public bool LevelState_lvl6;
    public bool LevelState_lvl7;
    public bool LevelState_lvl8;
    public bool LevelState_lvl9;
    public bool LevelState_lvl10;
    public bool LevelState_lvl11;
    public bool LevelState_lvl12;

    // for loading data when no save file exists
    public SaveData()
    {
        ScreenResolutionX = Screen.currentResolution.width;
        ScreenResolutionY = Screen.currentResolution.height;
        VSyncSetting = true;
        FullScreenSetting = true;
        BrightnessSetting = 0;
        TutorialSetting = true;
        MasterVolumeSetting = 1;
        MusicVolumeSetting = 1;
        AmbientVolumeSetting = 1;
        SoundEffectsVolumeSetting = 1;
        TutorialPassed = false;

        LevelState_lvl1 = true;
        LevelState_lvl2 = false;
        LevelState_lvl3 = false;
        LevelState_lvl4 = false;
        LevelState_lvl5 = false;
        LevelState_lvl6 = false;
        LevelState_lvl7 = false;
        LevelState_lvl8 = false;
        LevelState_lvl9 = false;
        LevelState_lvl10 = false;
        LevelState_lvl11 = false;
        LevelState_lvl12 = false;
    }

    // For saving data
    public SaveData(GameManager aGameManager)
    {
        ScreenResolutionX = aGameManager.ScreenResolution.x;
        ScreenResolutionY = aGameManager.ScreenResolution.y;
        VSyncSetting = aGameManager.VSyncSetting;
        FullScreenSetting = aGameManager.FullScreenSetting;
        BrightnessSetting = aGameManager.BrightnessSetting;
        TutorialSetting = aGameManager.TutorialSetting;
        MasterVolumeSetting = aGameManager.MasterVolumeSetting;
        MusicVolumeSetting = aGameManager.MusicVolumeSetting;
        AmbientVolumeSetting = aGameManager.AmbientVolumeSetting;
        SoundEffectsVolumeSetting = aGameManager.SoundEffectsVolumeSetting;
        TutorialPassed = aGameManager.TutorialPassed;

        LevelState_lvl1 = aGameManager.LevelProgress[1];
        LevelState_lvl2 = aGameManager.LevelProgress[2];
        LevelState_lvl3 = aGameManager.LevelProgress[3];
        LevelState_lvl4 = aGameManager.LevelProgress[4];
        LevelState_lvl5 = aGameManager.LevelProgress[5];
        LevelState_lvl6 = aGameManager.LevelProgress[6];
        LevelState_lvl7 = aGameManager.LevelProgress[7];
        LevelState_lvl8 = aGameManager.LevelProgress[8];
        LevelState_lvl9 = aGameManager.LevelProgress[9];
        LevelState_lvl10 = aGameManager.LevelProgress[10];
        LevelState_lvl11 = aGameManager.LevelProgress[11];
        LevelState_lvl12 = aGameManager.LevelProgress[12];
    }
}
