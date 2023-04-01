using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelChoiceButton : MonoBehaviour
{
    [Tooltip("This is fed to MenuManagerScript when making decision which level to load")]
    public int LevelIndexToLoad = 0;
}
