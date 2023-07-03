using UnityEngine;
using UnityEngine.AI;

public class TrapScript : MonoBehaviour
{

    [Tooltip("If it is a collapsable trap, it will update navmesh to take tile out of Walkable pool")]
    public bool Collapsable = false;
    public bool NoCorpse = false;

    [Header("Interactions: Enemy, Player")]
    public bool[] Slows = new bool[] { false, false };
    public bool[] Stuns = new bool[] { false, false };
    public bool[] Damages = new bool[] { false, false };
    public int DamagePerHit = 1;

    public float[] SlowFor = new float[] { 2.15f , 1.125f };
    public float[] StunFor = new float[] { 2.15f , 1.75f };

    private bool _requestedTeleport = false;

    // if a wall collapses on top of a health potion
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.name.StartsWith("PotionOfHealth"))
        { collision.gameObject.GetComponent<HealthPickupScript>().DestroyItem();}

        if (collision.tag == "Enemy")
        {
            EnemyScript es = collision.gameObject.GetComponent<EnemyScript>();

            if (Collapsable && es.EnemyLevel >= 3 && !_requestedTeleport) { es.TeleportToSpawn(); _requestedTeleport = true; }

            if (Damages[0])
            { if (es.EnemyLevel < 3) es.Die(NoCorpse); }

            else
            {
                if (Slows[0] && Stuns[0] && es.Stunned != true) { es.Stun(StunFor[0]); }
                else if (Stuns[0] && es.Stunned != true) { es.Stun(StunFor[0]); }
                else if (Slows[0] && es.Slowed != true) { es.Slow(SlowFor[0]); }
                
            }
        }

        if (collision.tag == "Player")
        {
            PlayerScript ps = collision.gameObject.GetComponent<PlayerScript>();

            if (Slows[1] && Stuns[1] && ps.Stunned != true) { ps.Stun(StunFor[1]); }
            else if (Stuns[1] && ps.Stunned != true) { ps.Stun(StunFor[1]); }
            else if (Slows[1] && ps.Slowed != true) { ps.Slow(SlowFor[1]); }
            
        }
    }

}
