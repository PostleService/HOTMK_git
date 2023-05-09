using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FloorMaterialInfo : MonoBehaviour
{
    public Tilemap Tilemap;
    public List<FloorMaterialScript> FloorMaterials;

    private Dictionary<TileBase, FloorMaterialScript> _materialsDictionary = new Dictionary<TileBase, FloorMaterialScript>();

    private void Awake()
    {
        foreach (FloorMaterialScript fms in FloorMaterials)
        { foreach (TileBase tile in fms.Tiles) _materialsDictionary.Add(tile, fms); }
    }

    public string RequestMaterialAtLocation(Vector2 aRequestedLocation)
    {
        Vector3Int gridPos = Tilemap.WorldToCell(aRequestedLocation);
        TileBase clickedTile = Tilemap.GetTile(gridPos);
        string tileMaterial = _materialsDictionary[clickedTile].Material;
        
        return tileMaterial;
    }


}
