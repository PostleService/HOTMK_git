using UnityEngine;
using UnityEngine.AI;

public class TrapScript : MonoBehaviour
{

    [Tooltip("If it is a collapsable trap, it will update navmesh to take tile out of Walkable pool")]
    public bool Collapsable = false;
    [Tooltip("Either stuns or damages enemies. If stuns - stuns lvl3 as well. If damages - kills enemies < lvl3")]
    public bool SlowsEnemies = false;
    public bool StunsEnemies = false;
    public bool DamagesEnemies = true;
    public bool ModifiersAffectPlayer = true;
    public int DamagePerHit = 1;

    private bool _requestedTeleport = false;

    // if a wall collapses on top of a health potion
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.name.StartsWith("PotionOfHealth"))
        { collision.gameObject.GetComponent<HealthPickupScript>().DestroyItem();}

        if (collision.tag == "Enemy")
        {
            EnemyScript es = collision.gameObject.GetComponent<EnemyScript>();

            if (DamagesEnemies && es.EnemyLevel < 3) { es.Die(); }
            if (Collapsable && es.EnemyLevel >= 3 && !_requestedTeleport) { es.TeleportToSpawn(); _requestedTeleport = true; }

            if (SlowsEnemies && StunsEnemies) { es.Stun(); }
            else if (SlowsEnemies) { es.Slow(); }
            else if (StunsEnemies) { es.Stun(); }
        }
        if (ModifiersAffectPlayer && collision.tag == "Player")
        {
            PlayerScript ps = collision.gameObject.GetComponent<PlayerScript>();

            if (SlowsEnemies && StunsEnemies) { ps.Stun(); }
            else if (SlowsEnemies) { ps.Slow(); }
            else if (StunsEnemies) { ps.Stun(); }
        }
    }

}
