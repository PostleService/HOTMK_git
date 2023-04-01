using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    public bool SlowsEnemies;
    public bool StunsEnemies;
    public bool DamagesEnemies;
    public bool ModifiersAffectPlayer;
    public int Damage;
    public Vector2Int Direction;
    public float Speed;
    public float ProjectileLifetime;
    private float _currentProjectileLifetime;
    [Tooltip("Basic projectile collision")]
    public LayerMask ProjectileCollision;

    private void Start()
    {
        _currentProjectileLifetime = ProjectileLifetime;

        if (Direction.x == -1 && Direction.y == 0) { transform.rotation = Quaternion.Euler(0f, 0f, 90f); }
        else if (Direction.x == 0 && Direction.y == 1) { transform.rotation = Quaternion.Euler(0f, 0f, 0f); }
        else if (Direction.x == 1 && Direction.y == 0) { transform.rotation = Quaternion.Euler(0f, 0f, -90f); }
        else if (Direction.x == 0 && Direction.y == -1) { transform.rotation = Quaternion.Euler(0f, 0f, 180f); }

        // If damages enemies - add enemy layer to layer mask
        if (SlowsEnemies || StunsEnemies || DamagesEnemies) { ProjectileCollision = ProjectileCollision | (1 << 7); }
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
            // Debug.LogWarning(collision.name);
            if (collision.tag == "Enemy")
            {
                EnemyScript es = collision.gameObject.GetComponent<EnemyScript>();
                
                if (DamagesEnemies && es.EnemyLevel < 3) { es.Die(); }

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

            DestroyProjectile();
        }
    }

    private void MoveProjectile()
    {
        transform.position += new Vector3((Direction.x * Speed * Time.deltaTime), (Direction.y * Speed * Time.deltaTime), 0);
    }

    private void DecrementLifetime()
    {
        if (_currentProjectileLifetime >= 0) { _currentProjectileLifetime -= Time.deltaTime; }
        else { DestroyProjectile(); }
    }

    // if need arises to expand destruction with animations and sound
    private void DestroyProjectile()
    { Destroy(gameObject); }

}
