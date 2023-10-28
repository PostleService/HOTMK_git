using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ShadowOcclusion : MonoBehaviour
{
    public bool TransformCheck = false;
    private bool _occludeShadowsOnThisLevel = false;
    private ShadowCaster2D _shadow;
    private PolygonCollider2D _collider;
    private List<Vector2> ShadowCasterPoints = new List<Vector2>() { };

    // Start is called before the first frame update
    void Start()
    {
        _occludeShadowsOnThisLevel = GameObject.Find("FogManager").GetComponent<FogManager>().OccludeShadowOnThisLevel;
        _collider = GetComponent<PolygonCollider2D>();
        _shadow = GetComponent<ShadowCaster2D>();
        if (TransformCheck == false) { ShadowCasterPoints = GetListOfShadowCasterPoints(); }        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        /* Seems to only worsen performance Turning it off until it is clear how to improve performance
        if (_occludeShadowsOnThisLevel == true) OccludeShadow();
        */
    }

    private List<Vector2> GetListOfShadowCasterPoints()
    {
        List<Vector2> worldPoints = new List<Vector2>() { };
        Vector2[] relativePoints = new Vector2[] { };
        if (_collider != null) relativePoints = _collider.points;

        foreach (Vector2 vec2 in relativePoints)
        { worldPoints.Add(transform.TransformPoint(vec2)); }

        return worldPoints;
    }

    private bool BetweenZeroOne(float aValue)
    {
        if (aValue >= 0 && aValue <= 1) { return true; }
        else return false;
    }

    private bool TransformInView()
    {
        bool result = false;

        Vector2 ViewportPos = Camera.main.WorldToViewportPoint(transform.position);
        if (BetweenZeroOne(ViewportPos.x) && BetweenZeroOne(ViewportPos.y))
        { result = true; }

        return result;
    }

    private bool ShadowCasterInView()
    {
        bool result = false;
        foreach (Vector2 vec2 in ShadowCasterPoints)
        {
            Vector2 ViewportPos = Camera.main.WorldToViewportPoint(vec2);
            if (BetweenZeroOne(ViewportPos.x) && BetweenZeroOne(ViewportPos.y))
            { result = true; }
        }
        return result;
    }

    private void OccludeShadow()
    {
        if (TransformCheck == true)
        {
            if (_shadow.enabled != true && TransformInView() == true)
            { _shadow.enabled = true; }
            else if (TransformInView() != true && _shadow.enabled == true) _shadow.enabled = false;
        }

        else
        {
            if (_shadow.enabled != true && ShadowCasterInView() == true)
            { _shadow.enabled = true; }
            else if (ShadowCasterInView() != true && _shadow.enabled == true) _shadow.enabled = false;
        }
    }

}
