using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpriteSheetSwapper_PlayerDeath : MonoBehaviour
{
    public int _playerLevel = 0;
    private SpriteRenderer _spriteRenderer;
    [Header("SpriteSheet path")]
    [Tooltip("Folder path to spritesheets")]
    public string SpriteSheetPath = "SpriteSheets/Canvas/SpriteSheet_PlayerCharacter_Level";
    [Tooltip("Name of the substitute spritesheet")]
    public string SubstituteSpriteSheetName = "1";

    private void Start()
    { _spriteRenderer = this.gameObject.GetComponent<SpriteRenderer>(); }

    // Needs to be done in LateUpdate, otherwise runs too late to catch up with rendering
    private void LateUpdate()
    {

        SubstituteSpriteSheetName = (_playerLevel + 1).ToString();
        // Currently the easiest way to do it is to put it into an update function
        // it simply swaps sprites within renderer and does not work with animator or anything else

        // This bit accesses the Resources folder using the specified path and 
        // creates an array populated with (in this case) sprites under a specific sprite sheet
        var subSprites = Resources.LoadAll<Sprite>(SpriteSheetPath + SubstituteSpriteSheetName);

        // This bit requests the name of the current sprite 
        // and compares its name to the sprites from the array above
        string spriteName = _spriteRenderer.sprite.name;
        var newSprite = Array.Find(subSprites, item => item.name == spriteName);
        

        if (newSprite)
        { _spriteRenderer.sprite = newSprite; }
    }
}
