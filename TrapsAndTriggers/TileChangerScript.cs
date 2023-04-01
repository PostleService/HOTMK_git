using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileChangerScript : MonoBehaviour
{
    [Tooltip("LeftBottom, LeftCenter, LeftTop, MidBottom, MidCenter, MidTop, RightBottom, RightCenter, RightTop")]
    public TileBase[] TilesToSpawn = new TileBase[9];
    [HideInInspector]
    public Tilemap ObstacleTilemap;
    [Tooltip("In case we need to interact with different tilemaps other than obstacles for any reason")]
    public string NameOfTilemap = "Obstacles";
    private GridLayout _gridLayout;

    public void Start()
    {
        if (ObstacleTilemap == null) { ObstacleTilemap = GameObject.Find(NameOfTilemap).GetComponent<Tilemap>(); }
        _gridLayout = ObstacleTilemap.GetComponentInParent<GridLayout>();
    }

    public void UpdateTiles()
    {
        List<Vector3Int> TilePositionList = new List<Vector3Int>();
        Vector3Int ItemPosition = Vector3Int.FloorToInt(this.gameObject.transform.position);

        for (int x = (-1 + ItemPosition.x); x <= (1 + ItemPosition.x); x++)
        {
            for (int y = (-1 + ItemPosition.y); y <= (1 + ItemPosition.y); y++)
            {
                Vector3 position = new Vector3(x, y, 0);
                Vector3Int cellPosition = _gridLayout.WorldToCell(Vector3Int.RoundToInt(new Vector3(position.x, position.y, 0)));
                TilePositionList.Add(cellPosition);
            }
        }
        for (int i = 0; i < 9; i++)
        {
            if (TilesToSpawn[i] != null) 
            { ObstacleTilemap.SetTile(TilePositionList[i], TilesToSpawn[i]); } 
        }
    }
        
}
