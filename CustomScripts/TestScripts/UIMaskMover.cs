using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMaskMover : MonoBehaviour
{
    public Camera MainCamera;
    public Canvas ParentCanvas;
    public GameObject ObjectToFollow;
    public GameObject MaskFilterToMove;
    private RectTransform _maskFilterRectTransform;

    private void Start()
    { _maskFilterRectTransform = MaskFilterToMove.GetComponent<RectTransform>(); }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 ObjectToFollowScreenPosition = MainCamera.WorldToScreenPoint(ObjectToFollow.transform.position);

        Vector2 movPos;

        RectTransformUtility.ScreenPointToLocalPointInRectangle
            (ParentCanvas.transform as RectTransform, 
            ObjectToFollowScreenPosition, MainCamera, out movPos);
        Vector3 result = ParentCanvas.transform.TransformPoint(movPos);
        transform.position = result;
    } 
}
