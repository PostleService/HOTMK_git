using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnityResonance;
using FMODUnity;
using FMOD;

public class EnemySoundScript : MonoBehaviour
{
    [Tooltip("PlaySteps, PlayAggro, PlayFear, PlayDamagedGrunt")]
    public bool[] PlaySounds = new bool[] {false, false, false, false};
    [Tooltip("How likely are aggro and fear voicelines to play")]
    public int PercentageChanceAggroSound = 15;
    public int PercentageChanceFearSound = 15;
    [Tooltip("If haven't played aggression or fear, first time has percentage modified by value of")]
    public int FirstTimePercentageModifier = 50;
    public EventReference StepSound = new EventReference();
    public EventReference AggressionSound = new EventReference();
    public EventReference FearSound = new EventReference();
    public EventReference DamagedSound = new EventReference();

    public float DefaultAggressionTimer = 3f;
    private float _currentAggressionTimer;
    public float DefaultFearTimer = 3f;
    private float _currentFearTimer;
    private bool _aggressionSoundOnCooldown = false;
    private bool _fearSoundOnCooldown = false;

    private bool _revealedAggression = false;
    private bool _revealedFear = false;

    private void Awake()
    {
        ResetAggressionSoundTimer();
        ResetFearSoundTimer();
    }

    private void FixedUpdate()
    {
        DecrementAggressionSoundTimer();
        DecrementFearSoundTimer();
    }

    // Making this a local instance instead of a separate component because of resource cost otherwise
    public void PlayEnemyStepSound()
    {
        if (PlaySounds[0] == true)
        {
            FMOD.Studio.EventInstance evInst = FMODUnity.RuntimeManager.CreateInstance(StepSound);
            evInst.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));

            bool returnValue = false;
            int visible = 0;

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
                else returnValue = false;
            }
            if (returnValue == true) visible = 1;
            evInst.setParameterByName("Visible", visible);
            evInst.start();
            evInst.release();
        }
    }

    public void PlayAggroSound()
    {
        if (PlaySounds[1] == true && _aggressionSoundOnCooldown == false)
        {
            int RandNumber = new System.Random().Next(1, 101);
            int CompareAgainst = PercentageChanceAggroSound;
            if (_revealedAggression == false)
            {
                CompareAgainst = PercentageChanceAggroSound + FirstTimePercentageModifier;
                if (CompareAgainst > 100) CompareAgainst = 100;
            }
            if (RandNumber < CompareAgainst)
            {
                SoundBiteScript sbs = gameObject.AddComponent(typeof(SoundBiteScript)) as SoundBiteScript;
                sbs.DestroyInstance = true;
                sbs.DestroyInstanceTimer = 1f;
                sbs.SoundToPlay = AggressionSound;

                if (_revealedAggression == false) _revealedAggression = true;
                _aggressionSoundOnCooldown = true;
            }
        }
    }

    public void PlayFearSound()
    {
        if (PlaySounds[2] == true && _fearSoundOnCooldown == false)
        {
            int RandNumber = new System.Random().Next(1, 101);
            int CompareAgainst = PercentageChanceAggroSound;
            if (_revealedFear == false)
            {
                CompareAgainst = PercentageChanceAggroSound + FirstTimePercentageModifier;
                if (CompareAgainst > 100) CompareAgainst = 100;
            }
            if (RandNumber < CompareAgainst)
            {
                SoundBiteScript sbs = gameObject.AddComponent(typeof(SoundBiteScript)) as SoundBiteScript;
                sbs.DestroyInstance = true;
                sbs.DestroyInstanceTimer = 1f;
                sbs.SoundToPlay = FearSound;

                if (_revealedFear == false) _revealedFear = true;
                _fearSoundOnCooldown = true;
            }
        }
    }

    public void PlayDamagedSound()
    {
        if (PlaySounds[3] == true)
        {
            SoundBiteScript sbs = gameObject.AddComponent(typeof(SoundBiteScript)) as SoundBiteScript;
            sbs.DestroyInstance = true;
            sbs.DestroyInstanceTimer = 1f;
            sbs.SoundToPlay = DamagedSound;
        }
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

    private void ResetAggressionSoundTimer()
    { _currentAggressionTimer = DefaultAggressionTimer; _aggressionSoundOnCooldown = false;  }

    private void ResetFearSoundTimer()
    { _currentFearTimer = DefaultFearTimer; _fearSoundOnCooldown = false; }

    private void DecrementAggressionSoundTimer()
    {
        if (_currentAggressionTimer >= 0 && _aggressionSoundOnCooldown == true)
            _currentAggressionTimer -= Time.fixedDeltaTime;
        else ResetAggressionSoundTimer();
    }

    private void DecrementFearSoundTimer()
    {
        if (_currentFearTimer >= 0 && _fearSoundOnCooldown == true)
            _currentFearTimer -= Time.fixedDeltaTime;
        else ResetFearSoundTimer();
    }



}
