using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleWallAnimationEffectSpawner : MonoBehaviour
{
    public GameObject EnemyCausingDestruction;
    public GameObject WallDestructionEffect;
    public float DelayInSpawningEffect = 0.1f;

    private bool _contained = false;
    private bool _spawnedEffect = false;
    private Bounds _bounds;

    private void Start()
    {
        _bounds = gameObject.GetComponent<BoxCollider2D>().bounds;
    }

    private void FixedUpdate()
    {
        if (_contained == true && DelayInSpawningEffect > 0) DelayInSpawningEffect -= Time.fixedDeltaTime;
        if (DelayInSpawningEffect <= 0 && _spawnedEffect == false)
        {
            _spawnedEffect = true;
            if (WallDestructionEffect != null) Instantiate(WallDestructionEffect, EnemyCausingDestruction.transform);
        }
    }

    /// <summary>
    /// If the enemy causing the destruction of the weak wall is the same and his collider2D fits within the bounds of this checking collider -
    /// react by spawning THE EFFECT on top of him
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (EnemyCausingDestruction != null && collision.gameObject == EnemyCausingDestruction && _contained == false)
        {
            Bounds collisionBounds = collision.gameObject.GetComponent<BoxCollider2D>().bounds;
            Vector2 bMin = collisionBounds.min; Vector2 bMax = collisionBounds.max;
            bool bMinCont = _bounds.Contains(bMin); bool bMaxCont = _bounds.Contains(bMax);

            if (bMinCont == true && bMaxCont == true)
            { _contained = true; }

        }
    }


}
