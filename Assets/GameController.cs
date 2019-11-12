using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
	public static GameController instance;

    public const int WORLD_X = 7;
    public const int WORLD_Y = 4;
	public const int BASE_ARMIES_PER_TURN = 1;

	public delegate void GameControllerEvent();
	public event GameControllerEvent OnSetupComplete;

    public Dictionary<Vector2Int, GameTile> gameMap;
	public List<Player> startingPlayers;
	public List<Player> remainingPlayers;

	public bool setupHasFinished = false;
	public bool gameEnded;

    // Start is called before the first frame update
    void Start()
    {
		instance = this;
		gameMap = new Dictionary<Vector2Int, GameTile>();

		// Make up some players to spawn
		Player player1 = new Player();
		player1.nationName = "The Union";
		player1.color = Color.blue;

		Player player2 = new Player();
		player2.color = Color.red;
		player2.nationName = "The Confederacy";

		startingPlayers = new List<Player> { player1, player2 };

		WorldMapGenerator.StartGeneration(WORLD_X, WORLD_Y, UnityEngine.Random.value * 1000, this.startingPlayers.Count, OnGenFinished, this);
    }

    private void OnGenFinished(WorldMap world)
    {
        foreach(Vector2Int position in world.mapDict.Keys)
        {
            GameTile newGameTile = new GameTile();
            newGameTile.armies = 0;
            newGameTile.type = world.mapDict[position];
            if (newGameTile.type != null)
                gameMap.Add(position, newGameTile);

            WorldRenderer.RenderWorld(gameMap);
        }


		remainingPlayers = new List<Player>(startingPlayers);
		PlayerSpawner.SpawnPlayers(remainingPlayers, 4);

		UpdateWorldRender();

		HUDManager.SetPlayerIndicator(TurnHandler.GetCurrentPlayer());

		OnSetupComplete?.Invoke();
		setupHasFinished = true;
	}


	private void UpdateWorldRender ()
	{
		WorldRenderer.RenderWorld(gameMap);
	}
	public GameTile GetTile(Vector2Int pos)
	{
		if (gameMap.ContainsKey(pos))
		{
			return gameMap[pos];
		}
		else
		{
			return null;
		}
	}
	public void EvaluateRemainingPlayers()
	{
		List<Player> playersWithTerritory = new List<Player>();
		foreach (Vector2Int position in gameMap.Keys)
		{
			GameTile gameTile = gameMap[position];

			if (gameTile == null)
				continue;

			if (gameTile.owner != null && !playersWithTerritory.Contains(gameTile.owner))
			{
				playersWithTerritory.Add(gameTile.owner);
			}
				
		}
		for (int i = remainingPlayers.Count - 1; i >= 0; i--)
		{
			Player player = remainingPlayers[i];
			if (!playersWithTerritory.Contains(player))
			{
				remainingPlayers.Remove(player);
			}
		}
		if(remainingPlayers.Count <= 1)
		{
			VictoryScreenManager.LaunchVictoryScreen(remainingPlayers[0]);
			gameEnded = true;
		}
	}
	public void LaunchAttack(Vector2Int startTilePos, Vector2Int endTilePos)
	{
		GameTile startTile = gameMap[startTilePos];
		GameTile endTile = gameMap[endTilePos];

		if (startTile.armies - startTile.expendedArmies < 1 || endTile.armies < 1)
		{
			Debug.LogError("either the attacking or defending tile had no armies");
			return;
		}

		if (UnityEngine.Random.value < 0.45 * (1 + (startTile.type.attackBonus - endTile.type.defenseBonus)))
		{
			endTile.armies--;
		}
		else
		{
			startTile.armies--;
		}
		SetOwnerNullIfNoArmies(startTile);
		SetOwnerNullIfNoArmies(endTile);
		UpdateWorldRender();
	}

	public void MoveArmies(Vector2Int startTilePos, Vector2Int endTilePos)
	{
		GameTile startTile = gameMap[startTilePos];
		GameTile endTile = gameMap[endTilePos];
		if (startTile.armies - startTile.expendedArmies < 1)
		{
			Debug.LogError("no armies to move");
			return;
		}
		// TODO check that recieving tile is friendly
		if (endTile.owner == null)
			endTile.owner = startTile.owner;
		startTile.armies--;
		endTile.armies++;
		endTile.expendedArmies++;
		SetOwnerNullIfNoArmies(startTile);
		UpdateWorldRender();
	}
	public bool PlaceArmyIfAvailable (Player player, Vector2Int pos)
	{
		if (player.armiesToPlace >= 1)
		{
			GameTile tile = gameMap[pos];
			if (tile.owner == player)
			{
				tile.armies++;
				player.armiesToPlace--;
				UpdateWorldRender();
				return true;
			}
		}
		UpdateWorldRender();
		return false;
	}
	public void CollectArmies (Player player)
	{
		float armies = BASE_ARMIES_PER_TURN;
		foreach (Vector2Int position in gameMap.Keys)
		{
			GameTile gameTile = gameMap[position];

			if (gameTile == null)
				continue;

			if (gameTile.owner == player)
				armies += gameTile.type.armyProduction;
		}
		player.armiesToPlace = Mathf.FloorToInt(armies);
	}
	public void ResetAllExpendedArmies ()
	{
		foreach (Vector2Int position in gameMap.Keys)
		{
			GameTile gameTile = gameMap[position];

			if (gameTile != null)
				gameTile.expendedArmies = 0;
		}
		UpdateWorldRender();
	}
	public void SetTileOwner (Vector2Int tilePos, Player newOwner)
	{
		GameTile tile = gameMap[tilePos];
		SetTileOwner(tile, newOwner);
	}
	public void SetTileOwner(GameTile tile, Player newOwner)
	{
		tile.owner = newOwner;
		UpdateWorldRender();
	}
	public List<Vector2Int> GetAdjacentTiles (Vector2Int tilePos)
	{
		List<Vector2Int> results = new List<Vector2Int>();
		for (int x = -1; x <= 1; x++)
		{
			for (int y = -1; y <=1; y++)
			{
				if (Mathf.Abs(x) == Mathf.Abs(y))
				{
					continue;
				}

				if (gameMap.ContainsKey(new Vector2Int(tilePos.x + x, tilePos.y + y)))
				{
					results.Add(new Vector2Int(tilePos.x + x, tilePos.y + y));
				}
			}
		}
		return results;
	}
	void SetOwnerNullIfNoArmies (GameTile tile)
	{
		if (tile.armies == 0)
			tile.owner = null;
	}
}
