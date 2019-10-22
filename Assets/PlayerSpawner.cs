using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
	static List<string> suitableStartTiles =
	new List<string>
	{
		"plains"
	};
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public static void SpawnPlayers (List<Player> players, int startArmies)
	{
		List<Vector2Int> possibleStarts = new List<Vector2Int>();
		foreach(Vector2Int pos in GameController.instance.gameMap.Keys)
		{
			foreach (string id in suitableStartTiles) {
				if (GameController.instance.gameMap[pos].type.id == id)
				{
					possibleStarts.Add(pos);
				}
			}
		}
		foreach (Player player in players)
		{
			int startIndex = Random.Range(0, possibleStarts.Count);
			Vector2Int spawnLocation = possibleStarts[startIndex];
			possibleStarts.RemoveAt(startIndex);

			GameController.instance.gameMap[spawnLocation].owner = player;
			GameController.instance.gameMap[spawnLocation].armies = startArmies;
		}
	}
}
