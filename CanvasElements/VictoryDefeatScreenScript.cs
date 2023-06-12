using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class VictoryDefeatScreenScript : MonoBehaviour
{
    public List<TextMeshProUGUI> Text_Stage1 = new List<TextMeshProUGUI>();
    public List<Image> Images_Stage1 = new List<Image>();
    public List<TextMeshProUGUI> Text_Stage2 = new List<TextMeshProUGUI>();
    public List<Image> Images_Stage2 = new List<Image>();
    public float IncrementMultiplier = 3f;

    private bool _enabled = false;
    private bool _alpha1Maxed = false;
    private bool _done = false;
    private float _alpha1 = 0f;
    private float _alpha2 = 0f;

    // enable animator so it can play a one-time animation
    private void OnEnable()
    {
        gameObject.GetComponent<VictoryDefeatScreenTaunt>().enabled = true;
        _enabled = true; 
    }

    private void FixedUpdate()
    {
        if (_enabled == true && _done == false)
        {
            UpdateFirstStage();
            UpdateSecondStage();
        }
    }

    private void UpdateFirstStage()
    {
        if (_alpha1Maxed == false && _alpha1 < 1)
        {
            if (_alpha1 + (IncrementMultiplier * Time.fixedDeltaTime) <= 1)
                _alpha1 += (IncrementMultiplier * Time.fixedDeltaTime);
            else _alpha1 = 1;

            foreach (TextMeshProUGUI text in Text_Stage1)
            { text.color = new Color(text.color.r, text.color.g, text.color.b, _alpha1); }
            foreach (Image img in Images_Stage1)
            { img.color = new Color(img.color.r, img.color.g, img.color.b, _alpha1); }
        }
        else if (_alpha1 == 1)
        { _alpha1Maxed = true; }
    }

    private void UpdateSecondStage()
    {
        if (_alpha1Maxed == true && _alpha2 < 1)
        {
            if (_alpha2 + (IncrementMultiplier * Time.fixedDeltaTime) <= 1)
                _alpha2 += (IncrementMultiplier * Time.fixedDeltaTime);
            else _alpha2 = 1;

            foreach (TextMeshProUGUI text in Text_Stage2)
            { text.color = new Color(text.color.r, text.color.g, text.color.b, _alpha2); }
            foreach (Image img in Images_Stage2)
            { img.color = new Color(img.color.r, img.color.g, img.color.b, _alpha2); }
        }
        else if (_alpha2 == 1)
        { _done = true; }
    }
}
    