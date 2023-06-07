using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileScript : MonoBehaviour
{

    public Vector2Int Direction;
    public float ProjectileSpeed;
    public float ProjectileLifetime;
    private float _currentProjectileLifetime;
    [Tooltip("Basic projectile collision")]
    public LayerMask ProjectileCollision;

    public GameObject CollisionSoundObject;

    [Header("Interactions: Enemy, Player")]
    public bool[] Slows = new bool[] { false, false };
    public bool[] Stuns = new bool[] { false, false };
    public bool[] Damages = new bool[] { false, false };
    public int DamagePerHit = 1;

    public float[] SlowFor = new float[] { };
    public float[] StunFor = new float[] { };

    private void Start()
    {
        _currentProjectileLifetime = ProjectileLifetime;

        if (Direction.x == -1 && Direction.y == 0) { transform.rotation = Quaternion.Euler(0f, 0f, 90f); }
        else if (Direction.x == 0 && Direction.y == 1) { transform.rotation = Quaternion.Euler(0f, 0f, 0f); }
        else if (Direction.x == 1 && Direction.y == 0) { transform.rotation = Quaternion.Euler(0f, 0f, -90f); }
        else if (Direction.x == 0 && Direction.y == -1) { transform.rotation = Quaternion.Euler(0f, 0f, 180f); }

        // If damages enemies - add enemy layer to layer mask
        if (Slows[0] || Stuns[0] || Damages[0]) { ProjectileCollision = ProjectileCollision | (1 << 7); }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        DecrementLifetime();
        MoveProjectile();
    }

    // this motherfucking thing requires RigidBody2D to properly function. Otherwise it won't detect TilemapCollider2D
    private void OnTriggerStay2D(Collider2D collision)
    {
        // if layer of collision is contained within layers to interact with, react
        if ((ProjectileCollision & (1 << collision.gameObject.layer)) != 0)
        {
            if (collision.tag == "Enemy")
            {
                EnemyScript es = collision.gameObject.GetComponent<EnemyScript>();

                if (Damages[0])
                { if (es.EnemyLevel < 3) es.Die(false); }

                else
                {
                    if (Slows[0] && Stuns[0]) { es.Stun(StunFor[0]); }
                    else if (Slows[0]) { es.Slow(SlowFor[0]); }
                    else if (Stuns[0]) { es.Stun(StunFor[0]); }
                }
            }

            if (collision.tag == "Player")
            {
                PlayerScript ps = collision.gameObject.GetComponent<PlayerScript>();

                if (Slows[1] && Stuns[1]) { ps.Stun(StunFor[1]); }
                else if (Slows[1]) { ps.Slow(SlowFor[1]); }
                else if (Stuns[1]) { ps.Stun(StunFor[1]); }
            }

            DestroyProjectile();
        }
    }

    private void MoveProjectile()
    {
        transform.position += new Vector3((Direction.x * ProjectileSpeed * Time.deltaTime), (Direction.y * ProjectileSpeed * Time.deltaTime), 0);
    }

    private void DecrementLifetime()
    {
        if (_currentProjectileLifetime >= 0) { _currentProjectileLifetime -= Time.deltaTime; }
        else { DestroyProjectile(); }
    }

    // if need arises to expand destruction with animations and sound
    private void DestroyProjectile()
    {
        if (CollisionSoundObject != null) Instantiate(CollisionSoundObject, transform.position, new Quaternion(), null);
        Destroy(gameObject); 
    }

}
