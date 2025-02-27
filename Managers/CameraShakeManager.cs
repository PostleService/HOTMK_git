using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CameraShakeManager : MonoBehaviour
{
    public float MaxReactToShakingDistance = 5f;
    public float ShakingIntervals = 0.1f;
    private float _currentShakingInterval;
    [RangeAttribute(0,2)] public float MaxShakeOffset = 2f;
    private bool _cameraPositionsChanged = false;

    private List<GameObject> _observedCameras = new List<GameObject>();
    public List<ScreenShakerOrigin> _observedShakers = new List<ScreenShakerOrigin>();

    public GameObject _player;

    private void OnEnable()
    {
        ScreenShaker.OnShake += UpdateShakerList;
        ScreenShakerOrigin.OnTimerEnd += ShakerDestroyAndListClear;
        // PlayerScript.OnSpawn += AddPlayer;
    }

    private void OnDisable()
    {
        ScreenShaker.OnShake -= UpdateShakerList;
        ScreenShakerOrigin.OnTimerEnd -= ShakerDestroyAndListClear;
        // PlayerScript.OnSpawn -= AddPlayer;
    }

    void Start()
    {
        AddObservedCameras();
        ResetShakingTickTimer();
        _player = GameObject.Find("PlayerCamera");
    }

    private void FixedUpdate()
    {
        if (ShakersExistOrInRange())
        { ShakingTickDecrement(); }
        else { if (_cameraPositionsChanged) ResetCameraLocalPositions(); }
    }

    private void AddPlayer(GameObject aPlayer)
    { _player = aPlayer; }

    private void AddObservedCameras()
    {
        Camera[] cameras = FindObjectsOfType<Camera>();
        foreach (Camera camera in cameras) { _observedCameras.Add(camera.gameObject); }
    }

    private void UpdateShakerList(Vector2 aPosition, float aIntensity, float aDuration, string aName)
    {
        GameObject shakerInstance = new GameObject();
        shakerInstance.transform.position = aPosition;
        shakerInstance.transform.SetParent(GameObject.Find("ScreenShakerHolder").transform);
        shakerInstance.name = (aName + "_screenShakeEffect");

        ScreenShakerOrigin sso = shakerInstance.AddComponent(typeof(ScreenShakerOrigin)) as ScreenShakerOrigin;
        sso.Intensity = aIntensity; sso.Duration = aDuration;

        _observedShakers.Add(sso);
    }

    private void ShakerDestroyAndListClear(ScreenShakerOrigin aSH)
    {
        if (_observedShakers.Contains(aSH))
        {
            _observedShakers.Remove(aSH);
            Destroy(aSH.gameObject);
        }
        if (!ShakersExistOrInRange()) ResetCameraLocalPositions();
    }

    /// <summary>
    /// If any shakers exist or are in range - true
    /// </summary>
    private bool ShakersExistOrInRange()
    {
        if (!_observedShakers.Any())
            {
            foreach (ScreenShakerOrigin sso in _observedShakers)
                {
                if (_player != null && Vector3.Distance(new Vector3(_player.transform.position.x, _player.transform.position.y, 0), sso.transform.position) < MaxReactToShakingDistance)
                    return true;
                }
            return false;
            }
        else return true;
    }

    private void ResetCameraLocalPositions()
    {
        foreach (GameObject cam in _observedCameras)
        {
            Vector3 curPos = cam.transform.localPosition;
            cam.transform.localPosition = new Vector3(0, 0, curPos.z);
        }
        _cameraPositionsChanged = false;
    }

    private void ShakingTickDecrement()
    {
        if (_currentShakingInterval >= 0)
        { _currentShakingInterval -= Time.fixedDeltaTime; }
        else
        { 
            CameraShakeStep();
            ResetShakingTickTimer();
        }
    }

    private void ResetShakingTickTimer()
    { _currentShakingInterval = ShakingIntervals; }

    /// <summary>
    /// Shake Observed Cameras once
    /// </summary>
    private void CameraShakeStep()
    {
        ScreenShakerOrigin sso = AssignShakerByPriority();
        if (sso != null)
        {
            foreach (GameObject cam in _observedCameras)
            {
                Vector3 curPos = cam.transform.localPosition;
                Vector3 newRandPos = NewCameraPosition(sso);
                cam.transform.localPosition = new Vector3(newRandPos.x, newRandPos.y, curPos.z);
            }
        }
        _cameraPositionsChanged = true;
    }

    private ScreenShakerOrigin AssignShakerByPriority()
    {
        float maxPrioritySoFar = 0;
        ScreenShakerOrigin currentSSO = null;
        foreach (ScreenShakerOrigin sso in _observedShakers)
        {
            // Basically, returns a coefficient of distance to max distance times intencity. if distance = 0, then coefficient is 1. 1 * 1 = 1. Everything else is lower
            float priority = sso.Intensity *
                ((MaxReactToShakingDistance - Vector3.Distance(sso.transform.position, new Vector3(_player.transform.position.x, _player.transform.position.y, 0))) / MaxReactToShakingDistance);


            if (maxPrioritySoFar < priority)
            {
                maxPrioritySoFar = priority;
                sso.AssignPriority(priority);
                currentSSO = sso;
                return currentSSO;
            }
        }
        return currentSSO;
    }

    /// <summary>
    /// Create a new random camera offset based on ScreenShakerOrigin Intensity and Distance
    /// </summary>
    private Vector3 NewCameraPosition(ScreenShakerOrigin aSSO)
    {
        float offsetByPriority = MaxShakeOffset * aSSO.Priority;
        Vector3 newVec = new Vector3(Random.Range(-offsetByPriority, offsetByPriority), Random.Range(-offsetByPriority, offsetByPriority), 0);
        return newVec;
    }

}

public class ScreenShakerOrigin : MonoBehaviour
{
    public Vector2 Origin = new Vector2(0, 0);
    public float Intensity = 0;
    public float Duration = 0;
    public float Priority = 0;

    public delegate void ShakeHandler(ScreenShakerOrigin aSH);
    public static event ShakeHandler OnTimerEnd;

    private void FixedUpdate()
    {
        DecrementDuration();
    }

    private void DecrementDuration()
    {
        if (Duration >= 0) Duration -= Time.fixedDeltaTime;
        else RequestDestruction(this);
    }

    private void RequestDestruction(ScreenShakerOrigin aSH)
    {
        OnTimerEnd?.Invoke(aSH);
    }

    /// <summary>
    /// Assign priority based on 
    /// sso.Intensity * ((MaxReactToShakingDistance - Vector3.Distance(sso.transform.position, _player.transform.position))/ MaxReactToShakingDistance);
    /// </summary>
    public void AssignPriority(float aPriority)
    { Priority = aPriority; }

}
