using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CreditsFadeIn : MonoBehaviour
{
    public float FadeInSpeed = 2f;
    public Image _buttonColor;
    public TextMeshProUGUI _textColor;

    // Start is called before the first frame update
    void Start()
    {
        if (gameObject.GetComponent<Button>() != null)
        {
            _buttonColor = gameObject.GetComponent<Image>();
            _buttonColor.color = new Color(_buttonColor.color.r, _buttonColor.color.g, _buttonColor.color.b, 0f);
            foreach (Transform t in gameObject.transform)
            { 
                _textColor = t.gameObject.GetComponent<TextMeshProUGUI>();
                _textColor.color = new Color(_textColor.color.r, _textColor.color.g, _textColor.color.b, 0f);
            }
        }
        else if (gameObject.GetComponent<TextMeshProUGUI>() != null)
        {
            _textColor = gameObject.GetComponent<TextMeshProUGUI>();
            _textColor.color = new Color(_textColor.color.r, _textColor.color.g, _textColor.color.b, 0f);
        }
            
    }

    // Update is called once per frame
    void Update()
    {
        FadeIn();
    }

    public void FadeIn()
    {
        if (_buttonColor != null)
        {
            if (_buttonColor.color.a < 255) 
            {
                _buttonColor.color = new Color(_buttonColor.color.r, _buttonColor.color.g, _buttonColor.color.b, _buttonColor.color.a + (Time.unscaledDeltaTime * FadeInSpeed));
            }
        }
        if (_textColor != null)
        {
            if (_textColor.color.a < 255)
            {
                _textColor.color = new Color(_textColor.color.r, _textColor.color.g, _textColor.color.b, _textColor.color.a + (Time.unscaledDeltaTime * FadeInSpeed));
            }
        }
    }
}
