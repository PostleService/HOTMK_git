using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class VictoryDefeatScreenTaunt : MonoBehaviour
{
    public enum VictoryOrDefeat 
    { 
        victory,
        defeat
    }
    public VictoryOrDefeat TypeOfScreen;
    public string[] VictoryPhrases = new string[] { "VICTORY ACHIEVED" , "VICTORY ACHIEVED... FINALLY" };
    public string[] DefeatPhrases = new string[] { "YOU DIED", "YOU DIED... AGAIN" };

    public TextMeshProUGUI TextObj;
    private GameManager _gm;

    private void OnEnable()
    { _gm = GameObject.Find("GameManager").GetComponent<GameManager>(); }

    private void Start()
    {
        if (TextObj != null)
        {
            if (TypeOfScreen == VictoryOrDefeat.victory)
            {
                if (_gm._deathCounter < 4) { TextObj.text = VictoryPhrases[0]; }
                else { TextObj.text = VictoryPhrases[1]; }
            }
            else if (TypeOfScreen == VictoryOrDefeat.defeat)
            {
                if (_gm._deathCounter < 4) { TextObj.text = DefeatPhrases[0]; }
                else { TextObj.text = DefeatPhrases[1]; }
            }

        }
    }
}
