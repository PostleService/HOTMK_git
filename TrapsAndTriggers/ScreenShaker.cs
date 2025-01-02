using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Intended to be assigned as a script to destructible animations (such as laser trap windup, ceiling collapse or wall destruction
/// Passes necessary attributes to camerashaker manager once upon start
/// </summary>
public class ScreenShaker : MonoBehaviour
{
    /// <summary>
    /// Nominal intensity = 1; 
    /// </summary>
    [RangeAttribute(0,1)] public float Intensity = 1f;
    public float Duration = 1f;
    public delegate void ShakeHandler (Vector2 aPosition, float aIntensity, float aDuration, string aName);
    public static event ShakeHandler OnShake;

    private void Start()
    { PassToCameraShakeManager(); }

    /// <summary>
    /// Make CameShakeManager add a new ScreenShaker instance to monitor for a duration
    /// </summary>
    public void PassToCameraShakeManager()
    { OnShake?.Invoke(transform.position, Intensity, Duration, gameObject.name); }


}
