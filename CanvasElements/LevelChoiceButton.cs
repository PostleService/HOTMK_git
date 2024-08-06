using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelChoiceButton : MonoBehaviour
{
    public bool TutorialButton = false;

    [Tooltip("This is fed to MenuManagerScript when making decision which level to load")]
    public int LevelIndexToLoad = 0;

    private void Awake()
    {
        if (TutorialButton)
        { LevelIndexToLoad = SceneUtility.GetBuildIndexByScenePath("Assets/Scenes/lvl0.unity"); }
    }
}


