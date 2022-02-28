using System.Collections.Generic;
using Players;
using UnityEngine;

public class GameController : MonoBehaviour
{
	public static GameController instance;

	public const int WorldX = 20;
	public const int WorldY = 12;
	private const int BaseArmiesPerTurn = 1;

	public delegate void GameControllerEvent();
	public event GameControllerEvent OnSetupComplete;

	public Dictionary<Vector2Int, GameTile> gameMap;
	public List<Player> startingPlayers;
	public List<Player> remainingPlayers;

	public bool setupHasFinished = false;
	public bool gameEnded;

	// Start is called before the first frame update
	private void Start()
	{
		instance = this;
		gameMap = new Dictionary<Vector2Int, GameTile>();

		// Make up some players to spawn
		Player player1 = new HumanPlayer("The Union", Color.blue);

		Player player2 = new BasicAIPlayer("The Confederacy", Color.red, 1.5f);

		Player player3 = new BasicAIPlayer("Germany", new Color(240, 195, 0), 1.5f);

		startingPlayers = new List<Player> { player1, player2, player3 };

		WorldMapGenerator.StartGeneration(WorldX, WorldY, UnityEngine.Random.value * 1000, this.startingPlayers.Count, OnGenFinished, this);
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

		HUDManager.ShowHudForPlayer(TurnHandler.GetCurrentPlayer());

		OnSetupComplete?.Invoke();
		setupHasFinished = true;
		TurnHandler.StartGame();
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

	public void MoveArmy(Vector2Int startTilePos, Vector2Int endTilePos)
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
		if (player.ArmiesToPlace >= 1)
		{
			GameTile tile = gameMap[pos];
			if (tile.owner == player)
			{
				tile.armies++;
				player.ArmiesToPlace--;
				UpdateWorldRender();
				return true;
			}
		}
		UpdateWorldRender();
		return false;
	}
	public void CollectArmies (Player player)
	{
		float armies = BaseArmiesPerTurn;
		foreach (Vector2Int position in gameMap.Keys)
		{
			GameTile gameTile = gameMap[position];

			if (gameTile == null)
				continue;

			if (gameTile.owner == player)
				armies += gameTile.type.armyProduction;
		}

		armies *= player.ArmyProductionMultiplier;
		player.ArmiesToPlace = Mathf.FloorToInt(armies);
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

	private void SetOwnerNullIfNoArmies (GameTile tile)
	{
		if (tile.armies == 0)
			tile.owner = null;
	}
}
