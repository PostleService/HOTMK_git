using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Visibility_Observed : MonoBehaviour
{
    public float FrequencyOfVisibilityChecks = 0.05f;
    public float RangeOfVisibilityCheck = 10f;
    [HideInInspector] public float DegreeOfSpread = 2f;
    private LevelManagerScript _levelManager;
    private GameObject _player;
    private List<SpriteRenderer> _listOfSpriteRenders = new List<SpriteRenderer>(); // for this object
    private bool _finallyVisible = false;
    public bool _alwaysVisible = false;

    private CustomFixedUpdate _fixedUpdate;

    private void OnEnable()
    {
        PlayerScript.OnSpawn += AssignPlayer;
        PlayerScript.OnEnemiesDeconceal += FinalRender;
        AnimationEndDetection_PlayerDeath.OnDie += InstantDerender;
    }

    private void OnDisable()
    {
        PlayerScript.OnSpawn -= AssignPlayer;
        PlayerScript.OnEnemiesDeconceal -= FinalRender;
        AnimationEndDetection_PlayerDeath.OnDie -= InstantDerender;
    }

    private void Start()
    {
        _levelManager = GameObject.Find("LevelManager").GetComponent<LevelManagerScript>();
        
        // populate list of transforms for quick render / derender
        _listOfSpriteRenders.Add(gameObject.GetComponent<SpriteRenderer>());
        foreach (Transform chTr in gameObject.transform)
        { if (chTr.GetComponent<SpriteRenderer>() != null) { _listOfSpriteRenders.Add(chTr.GetComponent<SpriteRenderer>()); } }

        if (!_alwaysVisible)
        {
            if (!_levelManager._playerCanSeeThroughWalls)
            { InstantDerender(); }
            else { FinalRender(); }
        }
        else FinalRender();

         _fixedUpdate = new CustomFixedUpdate(FrequencyOfVisibilityChecks, MyFixedUpdate);
    }

    private void Update()
    {
        _fixedUpdate.Update();
    }

    void MyFixedUpdate(float aFloat)
    { if (!_finallyVisible) ScanForObjects(); }

    private void AssignPlayer(GameObject aGameObject)
    { _player = aGameObject; }

    public void ScanForObjects()
    {
        Collider2D[] ColliderArray = new Collider2D[] { };
        LayerMask layerMask = (1 << 12); // layer 12 represents visibility layer. Add additional layers with | operator

        // If player cannot see through walls, he will cast an overlapcircle in his vicinity
        // Colliders returned in array of overlap circle are checked for compatibility with tags above
        // if they are compatible, their sprite renderers are requested and a raycast bundle is done
        // in their direction, which autodraws and returns true even if 1/3 raycasts hits
        // COLLIDERS SEARCHED ARE CHILDREN of actual objects, therefore, components are searched in parents!
        if (!_levelManager._playerCanSeeThroughWalls)
        { ColliderArray = Physics2D.OverlapCircleAll(transform.position, RangeOfVisibilityCheck, layerMask); }
        if (ColliderArray.Length > 0)
        {
            // first we gather results from each detected collider separately and check whether any of the raycasts cast in its direction returns true. 
            // If any of them return true, render, otherwise, derender
            bool SeenByAny = false;
            foreach (Collider2D col2D in ColliderArray)
            {
                if (col2D.gameObject.transform.parent.GetComponent<Visibility_Observer>() != null)
                {
                    if (col2D.tag == "PlayerVisibility" && Vector2.Distance(transform.position, col2D.gameObject.transform.position) <= col2D.gameObject.transform.parent.GetComponent<Visibility_Observer>().RangeOfVision)
                    {
                        if (RayCast(GetDirection(this.gameObject.transform.position, col2D.gameObject.transform.position), col2D.gameObject))
                        { SeenByAny = true; }
                    }
                }
            }
            if (SeenByAny) { InstantRender(); }
            else InstantDerender();
        }
    }

    // This is for getting three angles to maximize visibility around corners
    // number of raycasts can be increase to cover more angles
    public Vector2[] GetDirection(Vector3 aTargetPos, Vector3 aPlayerPos)
    {
        List<Vector2> vec2Lis = new List<Vector2>();
        float[] angles = new float[] { DegreeOfSpread, 0, (DegreeOfSpread * -1) }; // floats representing left, middle, and right side of cone of vision
        float posX = aTargetPos.x - aPlayerPos.x;
        float posY = aTargetPos.y - aPlayerPos.y;
        float angle = Mathf.Atan2(posY, posX) * Mathf.Rad2Deg;

        GameObject temp = new GameObject();
        foreach (float ang in angles)
        {
            temp.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90 + ang));
            Vector2 direction = temp.transform.up;
            vec2Lis.Add(direction);
        }
        Destroy(temp);
        Vector2[] vec2Arr = vec2Lis.ToArray();

        return vec2Arr;
    }

    public bool RayCast(Vector2[] aDirection, GameObject aGO)
    {
        RaycastHit2D[] collidersHit;
        LayerMask layerMask = (1 << 12); // layer 12 represents visibility layer. Add additional layers with | operator
        List<bool> results = new List<bool>();
        bool result = false;
        List<string> tagArray = new List<string> { "ItemVisibility", "EnemyVisibility" };

        // for each directional raycast
        foreach (Vector2 vec2 in aDirection)
        {
            // check the array of colliders the raycast returns
            collidersHit = Physics2D.RaycastAll(aGO.transform.position, vec2, Vector3.Distance(this.gameObject.transform.position, aGO.transform.position), layerMask);
            int OrderInCollidersHitArr = -1;
            bool ObjectVisible = false;

            // if the length of the array returned by raycast is longer than 0
            if (collidersHit.Length > 0)
            {
                for (int i = 0; i < collidersHit.Length; i++)
                {
                    if (collidersHit[i].collider != null)
                    {
                        // check whether collider tag is equal to the allowed collider list of the physics2D overlap request and the instance ID of the collider GO is the same, assign an analyzable position for it
                        if (tagArray.Contains(collidersHit[i].collider.gameObject.tag)) // substitute for tag reference to actual object, since it may as well not be the enemy
                        { OrderInCollidersHitArr = i; }
                    }
                }
            }
            if (OrderInCollidersHitArr >= 0 && !ObjectVisible)
            {
                if (OrderInCollidersHitArr == 0) { ObjectVisible = true; }
                else
                {
                    bool ObstaclePresent = false;
                    for (int i = 0; i < OrderInCollidersHitArr; i++)
                    {
                        if (collidersHit[i].collider != null)
                        {
                            if (collidersHit[i].collider.gameObject.tag == "ObstacleVisibility")
                                ObstaclePresent = true;
                        }
                    }
                    if (!ObstaclePresent) { ObjectVisible = true; }
                }
            }
            
            if (ObjectVisible)
            {
                Debug.DrawRay(aGO.transform.position, vec2 * Vector3.Distance(this.gameObject.transform.position, aGO.transform.position), Color.green);
                results.Add(true);
            }
            else
            {
                Debug.DrawRay(aGO.transform.position, vec2 * Vector3.Distance(this.gameObject.transform.position, aGO.transform.position), Color.blue);
                results.Add(false);
            }
        }
        foreach (bool res in results) { if (res == true) { result = true; } }
        return result;
    }

    public void InstantDerender()
    {
        foreach (SpriteRenderer SprRen in _listOfSpriteRenders)
        { SprRen.enabled = false; }
    }

    public void InstantRender()
    {
        foreach (SpriteRenderer SprRen in _listOfSpriteRenders)
        { SprRen.enabled = true; }
    }

    public void FinalRender()
    { InstantRender(); _finallyVisible = true; }
}
