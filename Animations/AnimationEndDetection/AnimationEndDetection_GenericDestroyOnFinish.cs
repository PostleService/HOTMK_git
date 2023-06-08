using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class AnimationEndDetection_GenericDestroyOnFinish : AnimationEndDetection
{ public override void OnAnimationFinish() { Destroy(gameObject); } }

