using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxScrollerFullscreenTexture : MonoBehaviour
{
    [Tooltip("Specify which object the parallax is going to be based off of")]
    public GameObject BaseParallaxOfObject;
    public Vector2 ParallaxOffset = new Vector2(1.5f, 1.5f);
    private Material _material;
    
    // Start is called before the first frame update
    void Start()
    { _material = gameObject.GetComponent<SpriteRenderer>().material; }

    //Attach this script to the object that is bearing a repeating texture

    // Update is called once per frame
    void FixedUpdate()
    {
        if (_material != null && BaseParallaxOfObject != null)
        {
            // Make texture object follow the camera not to create a bigger object
            Vector3 NewPosition = new Vector3(BaseParallaxOfObject.transform.position.x, BaseParallaxOfObject.transform.position.y, 0);
            gameObject.transform.position = NewPosition;
            
            // Create parallax effect
            Vector2 ResultingOffset = new Vector2(BaseParallaxOfObject.transform.position.x * ParallaxOffset.x, BaseParallaxOfObject.transform.position.y * ParallaxOffset.y);
            _material.SetTextureOffset("_MainTex", ResultingOffset);
        }
    }
}
