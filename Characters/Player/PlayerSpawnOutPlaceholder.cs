using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Rendering.Universal;

public class PlayerSpawnOutPlaceholder : MonoBehaviour
{
    public float FadeOutSpeedVsAnim = 3f;

    private float lifespan = 0.6f;
    private Light2D _lightElem;
    private float _fogManagerLowestIntensity;

    private void Start()
    {
        _fogManagerLowestIntensity = GameObject.Find("FogManager").GetComponent<FogManager>().LowestLightValue;
        _lightElem = gameObject.GetComponent<Light2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    { FadeOutIntensity(); }

    private void FadeOutIntensity()
    {
        if (_lightElem.pointLightOuterRadius > 0)
        {
            _lightElem.pointLightOuterRadius = Mathf.Lerp(_lightElem.pointLightOuterRadius, 0, (Time.fixedDeltaTime * FadeOutSpeedVsAnim) / lifespan);
            if (_lightElem.intensity > _fogManagerLowestIntensity)
            { _lightElem.intensity = Mathf.Lerp(_lightElem.intensity, _fogManagerLowestIntensity, (Time.fixedDeltaTime * FadeOutSpeedVsAnim) / lifespan); }
            lifespan -= Time.fixedDeltaTime;
        }
        else { Destroy(gameObject); }
    }
}
