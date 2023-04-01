using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpriteSheetSwapper_Canvas : MonoBehaviour
{
    private Image _image;
    [Header("SpriteSheet path")]
    [Tooltip("Folder path to spritesheets")]
    public string SpriteSheetPath = "SpriteSheets/Canvas/";
    [Tooltip("Name of the substitute spritesheet")]
    public string SubstituteSpriteSheetName;

    private void Start()
    { 
        _image = this.gameObject.GetComponent<Image>();
        ChangeCanvas();
    }

    private void OnEnable()
    {
        _image = this.gameObject.GetComponent<Image>();
        ChangeCanvas();
    }

    // Needs to be done in LateUpdate, otherwise runs too late to catch up with rendering
    private void LateUpdate()
    {

        // Currently the easiest way to do it is to put it into an update function
        // it simply swaps sprites within renderer and does not work with animator or anything else

        // This bit accesses the Resources folder using the specified path and 
        // creates an array populated with (in this case) sprites under a specific sprite sheet
        var subSprites = Resources.LoadAll<Sprite>(SpriteSheetPath + SubstituteSpriteSheetName);
        
        // This bit requests the name of the current sprite 
        // and compares its name to the sprites from the array above
        string spriteName = _image.sprite.name;
        var newSprite = Array.Find(subSprites, item => item.name == spriteName);
        
        if (newSprite) { _image.sprite = newSprite; }
    }

    // upon spawning, the heart checks whether the player exists and which spritesheet it should use
    public void ChangeCanvas()
    {
        int pl_lvl = 0;
        PlayerScript ps = GameObject.Find("Player").GetComponent<PlayerScript>();
        if (ps != null) { pl_lvl = ps.PlayerLevel; }
        else pl_lvl = 0;

        SubstituteSpriteSheetName = pl_lvl.ToString();
    }

}



