using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class AnimationEndDetection_VictoryTrigger : MonoBehaviour
{
    public string AnimationName1;
    public GameObject PlaceHolderForFadeout;
    private Animator _thisObjectAnimator;

    void Start()
    {
        if (gameObject.GetComponent<Animator>() != null)
        { _thisObjectAnimator = gameObject.GetComponent<Animator>(); }
    }

    void FixedUpdate()
    { CheckAnimationsEnd(); }

    public void OnAnimationFinish1()
    {
        MenuManagerScript mm = GameObject.Find("MenuManager").GetComponent<MenuManagerScript>();
        mm.ReactToVictory();
        Destroy(gameObject);
        
        if (PlaceHolderForFadeout != null) { Instantiate(PlaceHolderForFadeout, transform.position, new Quaternion(), null); }
    }

    public void CheckAnimationsEnd()
    {
        if (_thisObjectAnimator.GetCurrentAnimatorStateInfo(0).IsName(AnimationName1))
        { OnAnimationFinish1(); }
    }

}
