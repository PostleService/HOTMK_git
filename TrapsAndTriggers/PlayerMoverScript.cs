using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoverScript : MonoBehaviour
{
    [Tooltip("Multiplier by which direction is incremented")]
    public float Distance = 1f;
    [Tooltip("left: -1,0 , right: 1,0 , up: 0,1 , down: 0,-1")]
    public Vector2 Direction = new Vector2();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            collision.GetComponent<PlayerScript>().MoveMovementPoint
                (false, (Direction.x * Distance), (Direction.y * Distance)); 
        }
    }
}
