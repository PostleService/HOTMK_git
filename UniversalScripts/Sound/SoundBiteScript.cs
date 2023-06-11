using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnityResonance;
using FMODUnity;
using FMOD;

public class SoundBiteScript : MonoBehaviour
{
    public bool AssignToHolder = false;
    public bool PlayOnStart = true;
    public string SoundName = "GenericName";

    public EventReference SoundToPlay = new EventReference();

    // Start is called before the first frame update
    void Start()
    {
        if (PlayOnStart == true)
        {
            if (AssignToHolder == true) PlayOnCall(GameObject.Find("SoundBitesHolder"), SoundName);
            else PlayOnCall(this.gameObject, SoundName);
        } 
    }

    public void PlayOnCall(GameObject aGO, string aName)
    {
        Transform parentTrsftm = null;
        if (AssignToHolder == true) parentTrsftm = GameObject.Find("SoundBitesHolder").transform;
        else parentTrsftm = aGO.transform;

        GameObject sbsGO = new GameObject();
        sbsGO.name = aName; sbsGO.transform.parent = parentTrsftm;

        // We create a separate class instance to be able to manipulate its lifetime and value updates
        SoundBiteInstance sbi = sbsGO.AddComponent(typeof(SoundBiteInstance)) as SoundBiteInstance;
        sbi.evInst = FMODUnity.RuntimeManager.CreateInstance(SoundToPlay);
        sbi.callingGO = this.gameObject;

        Destroy(this);
    }

}

public class SoundBiteInstance : MonoBehaviour
{
    public FMOD.Studio.EventInstance evInst;
    public GameObject callingGO;
    private float soundClipTimer = 0;
    private bool Started = false;
    private bool _menuOpen = false;

    private void OnEnable()
    { MenuManagerScript.OnMenuOpen += ReactToMenuOpenClose; }
    private void OnDisable()
    { MenuManagerScript.OnMenuOpen -= ReactToMenuOpenClose; }

    private void Start()
    {
        OnEventInstCreation();
    }

    private void FixedUpdate()
    {
        if (Started == true && _menuOpen == false)
        { UpdateValuesWhileSoundBiteLasts(); }
    }

    public void OnEventInstCreation()
    {
        evInst.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(callingGO));
        

        FMOD.Studio.EventDescription evDescr;
        evInst.getDescription(out evDescr);

        int lengthsOfClipMill = 0;
        evDescr.getLength(out lengthsOfClipMill);

        soundClipTimer = (float)lengthsOfClipMill * 0.001f;
        evInst.start(); Started = true;
        
    }

    private void ReactToMenuOpenClose(bool aBool) 
    { 
        _menuOpen = aBool;
        evInst.setPaused(aBool);
    }

    public bool DetermineVisibility()
    {
        bool returnValue = false;

        LayerMask layerMask = (1 << 6) | (1 << 8) | (1 << 10) | (1 << 15);

        GameObject _player = GameObject.Find("Player");
        Vector3 _playerPos = Vector3.zero;
        Vector2 _playerDir = Vector2.zero;
        if (_player != null) _playerPos = _player.transform.position;
        if (_player != null) _playerDir = GetDirection(_player.transform.position, transform.position);

        RaycastHit2D colliderHit = Physics2D.Raycast(transform.position, _playerDir, Vector3.Distance(transform.position, _playerPos), layerMask);
        
        if (colliderHit.collider != null)
        {
            if (colliderHit.collider.tag == "Player") returnValue = true;

            else
            {
                UnityEngine.Debug.DrawRay(transform.position, _playerDir * (Vector3.Distance(transform.position, _playerPos)), Color.cyan);
                returnValue = false; 
            }
        }
        return returnValue;
    }

    public Vector2Int GetDirection(Vector3 aPlayerPos, Vector3 aSoundPos)
    {
        float posX = aPlayerPos.x - aSoundPos.x;
        float posY = aPlayerPos.y - aSoundPos.y;
        float angle = Mathf.Atan2(posY, posX) * Mathf.Rad2Deg;

        GameObject temp = new GameObject();
        temp.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90));
        Vector2 dirTemp = temp.transform.up;
        Vector2Int direction = Vector2Int.RoundToInt(dirTemp);
        Destroy(temp);

        return direction;
    }

    private void UpdateValuesWhileSoundBiteLasts()
    {
        if (soundClipTimer >= 0)
        {
            soundClipTimer -= Time.fixedDeltaTime;
            int visible;
            if (DetermineVisibility() == true) visible = 1;
            else visible = 0;

            evInst.setParameterByName("Visible", visible);
        }
        else
        { StopImmediately(); }
        
    }

    public void StopImmediately()
    {
        evInst.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        evInst.release();
        Destroy(gameObject);
    }
}