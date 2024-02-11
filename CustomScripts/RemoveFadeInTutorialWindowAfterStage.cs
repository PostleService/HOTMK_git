using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveFadeInTutorialWindowAfterStage : MonoBehaviour
{
    public int LavelStage = 1;
    public string TutorialWindowName = "TutorialMessagesPanel_Enemies";
    public GameObject TutorialWindowCollision;

    private void OnEnable()
    {
        LevelManagerScript.OnLevelStageChange += RemoveTutorialWindow;
    }

    private void OnDisable()
    {
        LevelManagerScript.OnLevelStageChange -= RemoveTutorialWindow;
    }

    private void RemoveTutorialWindow(int aLevelStage, int aCurrentItems, int aDefaultItems, Sprite aSprite)
    {
        if (aLevelStage == LavelStage)
        {
            Destroy(TutorialWindowCollision);
            GameObject otherOpenWindow = GameObject.FindGameObjectWithTag("TutorialMessageFading");
            if (otherOpenWindow != null && otherOpenWindow.name.StartsWith(TutorialWindowName))
            { otherOpenWindow.GetComponent<TutorialFadingPanelScript>().CloseTutorialButton(); }
        }
        
    }
}
