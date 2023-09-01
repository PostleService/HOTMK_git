using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoverScript : MonoBehaviour
{
    [Tooltip("Multiplier by which direction is incremented")]
    public float Distance = 1f;
    [Tooltip("left: -1,0 , right: 1,0 , up: 0,1 , down: 0,-1")]
    public Vector2 Direction = new Vector2();

    public bool OnlyIfFullyStopped = false;
    private bool _passedInfo = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player" && OnlyIfFullyStopped == false)
        {
            PlayerScript ps = collision.GetComponent<PlayerScript>();
            PassMoveCommand(ps);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Player" && OnlyIfFullyStopped == true && _passedInfo == false)
        {
            PlayerScript ps = collision.GetComponent<PlayerScript>();
            PassMoveCommand(ps);
            _passedInfo = true;
        }

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player" && OnlyIfFullyStopped == true)
        { _passedInfo = false; }
    }

    private void PassMoveCommand(PlayerScript aPS)
    { aPS.MoveMovementPoint(false, (Direction.x * Distance), (Direction.y * Distance)); }
}
