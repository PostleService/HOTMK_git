using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelStageEndPointer : MonoBehaviour
{
    public GameObject Target;
    private Vector3 _targetPosition;
    private Vector3 _targetPositionScreenPoint;
    private bool _isOffScreen;
    private MenuManagerScript _menuManager;

    public float PercentageOfScreenDistanceAway = 0.15f;

    public Sprite[] Sprites;

    private void Start()
    {
        _menuManager = GameObject.Find("MenuManager").GetComponent<MenuManagerScript>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Create ScreenPoints of target and the camera edge
        if (Target != null)
        {
            _targetPosition = Target.transform.position;
            _targetPositionScreenPoint = Camera.main.WorldToScreenPoint(_targetPosition);
            Vector3 cameraEdgeScreenPoint = Camera.main.ViewportToScreenPoint(new Vector3(1, 1, 0));

            // Check whether target is outside of boundaries of camera edge by comparing screen points
            if (_targetPositionScreenPoint.x <= 0 || _targetPositionScreenPoint.y <= 0 ||
                _targetPositionScreenPoint.x >= cameraEdgeScreenPoint.x || _targetPositionScreenPoint.y >= cameraEdgeScreenPoint.y)
            { _isOffScreen = true; }
            else
            { _isOffScreen = false; }

            // rectangular transform for rotating the sprite of the arrow
            RectTransform rt = this.gameObject.GetComponent<RectTransform>();

            // if enemy is off screen - cap the arrow to the bounds of the screen, rotate the arrow and set its position to the edge of the screen
            // either X or Y will allways be representing the location of the target
            // after that, convert the screen coordinates to world coordinates
            // change sprite based on _isOffScreen
            if (_isOffScreen)
            {
                this.gameObject.GetComponent<Image>().sprite = Sprites[0];
                float ScreenEdgeDistanceX = _menuManager.CurrentScreenResolution.x * PercentageOfScreenDistanceAway;
                float ScreenEdgeDistanceY = _menuManager.CurrentScreenResolution.y * PercentageOfScreenDistanceAway;


                Vector3 cappedTargetScreenPosition = _targetPositionScreenPoint;
                if (cappedTargetScreenPosition.x <= ScreenEdgeDistanceX)
                {
                    cappedTargetScreenPosition.x = 0f + ScreenEdgeDistanceX;
                    Quaternion rotation = Quaternion.Euler(0, 0, -90);
                    rt.rotation = rotation;
                }
                if (cappedTargetScreenPosition.x >= Screen.width - ScreenEdgeDistanceX)
                {
                    cappedTargetScreenPosition.x = Screen.width - ScreenEdgeDistanceX;
                    Quaternion rotation = Quaternion.Euler(0, 0, 90);
                    rt.rotation = rotation;
                }
                if (cappedTargetScreenPosition.y <= 0 + ScreenEdgeDistanceY)
                {
                    cappedTargetScreenPosition.y = 0f + ScreenEdgeDistanceY;
                    Quaternion rotation = Quaternion.Euler(0, 0, 0);
                    rt.rotation = rotation;
                }
                if (cappedTargetScreenPosition.y >= Screen.height - ScreenEdgeDistanceY)
                {
                    cappedTargetScreenPosition.y = Screen.height - ScreenEdgeDistanceY;
                    Quaternion rotation = Quaternion.Euler(0, 0, 180);
                    rt.rotation = rotation;
                }

                Vector3 arrowWorldPosition = Camera.main.ScreenToWorldPoint(cappedTargetScreenPosition);
                this.gameObject.transform.position = arrowWorldPosition;
            }
            else
            {
                this.gameObject.GetComponent<Image>().sprite = Sprites[1];
                Vector3 arrowWorldPosition = Camera.main.ScreenToWorldPoint(_targetPositionScreenPoint);
                Quaternion rotation = Quaternion.Euler(0, 0, 0);
                rt.rotation = rotation;
                this.gameObject.transform.position = new Vector3(arrowWorldPosition.x, arrowWorldPosition.y);
            }
        }
        else
        {
            Destroy(this.gameObject);
        }    

    }
}
