using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class CarParkGenerator : MonoBehaviour
{
    [SerializeField] private GameObject[] tilePrefabs;
    [SerializeField] private GameObject endTilePrefab;
    [SerializeField] private int seed;
    [SerializeField] private int size;

    private int loopCount = 0;

    private List<Vector2Int> _tiles = new();

    private void Start()
    {
        Random.InitState(seed);
        
        // place the first tile at the origin
        var obj = Instantiate(tilePrefabs[0], Vector3.zero, Quaternion.identity);
        var currentTile = obj.GetComponent<CarParkTile>();

        GenerateNeighbourTiles(currentTile);

        var spaces = GameObject.FindGameObjectsWithTag("CarParkSpace");
        // remove 1 of the spaces
        for (var i = 0; i < 1; ++i)
        {
            var space = spaces[Random.Range(0, spaces.Length)];
            space.transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    private void GenerateNeighbourTiles(CarParkTile currentTile)
    {
        loopCount++;
        if (loopCount > 500) return;
        
        for (var i = 0; i < currentTile.connectionsParent.childCount; ++i)
        {
            // skip this connection if it's disabled
            if (!currentTile.connectionsParent.GetChild(i).gameObject.activeSelf) continue;
            
            // choose a random tile
            var randomTile = tilePrefabs[Random.Range(0, tilePrefabs.Length)];
            
            if (currentTile.transform.position.x > size 
                || currentTile.transform.position.z > size 
                || currentTile.transform.position.x < -size 
                || currentTile.transform.position.z < -size)
                randomTile = endTilePrefab;
            
            // find the distance to that connection
            var connection = currentTile.connectionsParent.GetChild(i);
            
            // if this connection is disabled then skip it
            if (!connection.gameObject.activeSelf) continue;
            
            var distance = connection.position - currentTile.transform.position;

            var newPos = connection.position + distance;

            var coords = new Vector2Int(Mathf.RoundToInt(newPos.x), Mathf.RoundToInt(newPos.z));
            if (_tiles.Contains(coords)) continue;
            
            _tiles.Add(coords);
            
            // instance the random tile at double the distance away
            var newTile = Instantiate(randomTile, newPos, Quaternion.identity);
            
            // rotate the tile a random amount around the y axis to make it look more random but keep it aligned to the grid
            newTile.transform.rotation = Quaternion.Euler(0, Random.Range(0, 4) * 90, 0);
            
            // rotate the new tile up to 4 times to make sure one connection matches the current tile
            for (var j = 0; j < 4; ++j)
            {
                // get connections of the newTile
                var newTileConnections = newTile.GetComponent<CarParkTile>().connectionsParent;

                var shouldRotate = true;
                
                for (var k = 0; k < newTileConnections.childCount; ++k)
                {
                    // get the connection
                    var newTileConnection = newTileConnections.GetChild(k);
                    
                    // skip this connection if it's disabled
                    if (!newTileConnection.gameObject.activeSelf) continue;
                    
                    // find the distance between the new connection and our connection
                    var newDistance = newTileConnection.position - connection.position;
                    
                    // if the distance is less than 0.1f then we have a match
                    if (newDistance.sqrMagnitude > 0.1f) continue;
                    shouldRotate = false;
                    
                    // disable this connection
                    newTileConnection.gameObject.SetActive(false);
                    // and disable the connection on the current tile
                    connection.gameObject.SetActive(false);
                    
                    break;
                }

                if (shouldRotate)
                {
                    // if it doesn't then rotate the new tile and try again
                    newTile.transform.rotation *= Quaternion.Euler(0, 90, 0);
                }
                else
                {
                    // if it does then we're done
                    break;
                }
            }
            GenerateNeighbourTiles(newTile.GetComponent<CarParkTile>());
        }
    }
}
