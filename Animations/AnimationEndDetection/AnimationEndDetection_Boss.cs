using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEndDetection_Boss : MonoBehaviour
{
    public string AnimationNameIn;
    public string AnimationNameOut;
    private Animator _thisObjectAnimator;

    /// <summary>
    /// THIS SCRIPT WILL RELY ON DEFAULT ANIMATION STATE TRANSITIONING INTO A
    /// "DONE" STATE. AS SOON AS THE CONFIRMATION THAT THE STATE NAME == "DONE"
    /// APPROPRIATE ACTION WILL BE TAKEN
    /// 
    /// Instructions: 
    /// 1. create an animation state, name it appropriately,
    /// 2. transition into it and out of it.
    /// 3. Enter transition from animation in question, exit time: 1 (end of animation); transition duration: 0
    /// 4. Enter "done" state, transition: exit time: 0; transition duration: 0
    /// </summary>

    // Start is called before the first frame update
    void Start()
    {
        if (gameObject.GetComponent<Animator>() != null)
        { _thisObjectAnimator = gameObject.GetComponent<Animator>(); }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        CheckAnimationEnd();
    }

    public void CheckAnimationEnd()
    {
        if (_thisObjectAnimator.GetCurrentAnimatorStateInfo(0).IsName(AnimationNameIn))
        { OnAnimationFinishIn(); }
        if (_thisObjectAnimator.GetCurrentAnimatorStateInfo(0).IsName(AnimationNameOut))
        { OnAnimationFinishOut(); }
    }

    public void OnAnimationFinishIn()
    {
        EnemyScript es = gameObject.GetComponent<EnemyScript>();
        es.CurrentlyTeleporting = false;
    }

    public void OnAnimationFinishOut()
    {
        EnemyScript es = gameObject.GetComponent<EnemyScript>();
        es._agent.Warp(new Vector3(es.SpawnOutDestination.x, es.SpawnOutDestination.y, 0));
        gameObject.GetComponent<EnemyAnimation_Boss>().ExecuteAnimationSpawnIn();
    }

}
