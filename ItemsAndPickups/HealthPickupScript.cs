using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPickupScript : MonoBehaviour
{
    [HideInInspector]
    public LevelManagerScript _levelManager;
    public int HealsBy = 1;
    private bool _hasHealed = false;

    private void Start()
    {
        _levelManager = GameObject.Find("LevelManager").GetComponent<LevelManagerScript>();
        if (!_levelManager._playerCanSeeThroughWalls)
        { this.gameObject.GetComponent<SpriteRenderer>().enabled = false; }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            // if player's health is lower than maximum
            if (collision.GetComponent<PlayerScript>().CurrentLives < collision.GetComponent<PlayerScript>().MaxPlayerLives && !_hasHealed)
            {
                // basically decreases the player's health by negative of what it's supposed to heal - adding the value
                collision.GetComponent<PlayerScript>().TakeDamage(-HealsBy);
                _hasHealed = true; // prevents from executing more than once
                DestroyItem();
            }
        }
    }

    // Callable from outside in case a wall collapses on top of it
    public void DestroyItem()
    { Destroy(this.gameObject); }
}
