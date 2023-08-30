using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class AnimationEndDetection_DestroyOnFinish : AnimationEndDetection
{ public override void OnAnimationFinish() { Destroy(gameObject); } }

