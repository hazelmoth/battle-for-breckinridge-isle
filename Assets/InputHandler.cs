using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class InputHandler : MonoBehaviour
{
	[SerializeField] GameObject hoverSelectorPrefab;
	GameObject hoverSelector;
	public static InputHandler instance;
	Vector3Int FirstSelectedTile = new Vector3Int(-1, -1, 0);
	Vector3Int SecondSelectedTile = new Vector3Int(-1, -1, 0);
	bool firstTileSelected = false;
	bool secondTileSelected = false;

	public bool placingArmies = false;
	// Start is called before the first frame update
	void Start()
    {
		instance = this;
    }
	private void OnDestroy()
	{
		instance = null;
	}
	// Update is called once per frame
	void Update()
	{
		if (hoverSelector == null)
		{
			hoverSelector = GameObject.Instantiate(hoverSelectorPrefab);
		}
		hoverSelector.transform.position = GetTilePosUnderMouse();

		if (Input.GetMouseButtonDown(0))
		{
			Vector3Int tilePos = GetTilePosUnderMouse();
			RegisterClickOnTile(new Vector2Int(tilePos.x, tilePos.y));
		}

		if (Input.GetKeyDown(KeyCode.T))
		{
			NNTrainer.StartTraining(2, 2);
		}
	}

	public void RegisterClickOnTile (Vector2Int tilePos)
	{
		GameTile tile = GameController.instance.GetTile(tilePos);

		if (tile == null)
			return;
		// Verify that this is the current player's tile
		if (!firstTileSelected && tile.owner != TurnHandler.GetCurrentPlayer())
			return;

		if (!firstTileSelected)
		{
			if (tile == null)
				return;
			// Verify that this is the current player's tile
			if (tile.owner != TurnHandler.GetCurrentPlayer())
				return;

			FirstSelectedTile = new Vector3Int(tilePos.x, tilePos.y, 0);
			firstTileSelected = true;
			if (!placingArmies)
				SelectionDisplayer.PlaceSelectionRing(new Vector2Int(FirstSelectedTile.x, FirstSelectedTile.y));

			if (placingArmies)
			{
				if (TurnHandler.GetCurrentPlayer().armiesToPlace <= 0)
					placingArmies = false;
				else
				{
					GameController.instance.PlaceArmyIfAvailable(TurnHandler.GetCurrentPlayer(), new Vector2Int(tilePos.x, tilePos.y));
					firstTileSelected = false;
					secondTileSelected = false;

					if (TurnHandler.GetCurrentPlayer().armiesToPlace <= 0)
					{
						placingArmies = false;
						HUDManager.HidePlaceArmyText();
					}
					else
					{
						HUDManager.ShowPlaceArmyText(TurnHandler.GetCurrentPlayer().armiesToPlace);
					}
					return;
				}
			}
		}
		else
		{
			SecondSelectedTile = new Vector3Int(tilePos.x, tilePos.y, 0);
			secondTileSelected = true;
		}

		if (firstTileSelected && secondTileSelected)
		{
			Debug.Log("MOVE inputted");
			if (Mathf.Abs(FirstSelectedTile.x - SecondSelectedTile.x) == 1
				&& Mathf.Abs(FirstSelectedTile.y - SecondSelectedTile.y) == 0
				|| Mathf.Abs(FirstSelectedTile.x - SecondSelectedTile.x) == 0
				&& Mathf.Abs(FirstSelectedTile.y - SecondSelectedTile.y) == 1)
			{
				if (GameController.instance.GetTile(new Vector2Int(FirstSelectedTile.x, FirstSelectedTile.y)).owner == TurnHandler.GetCurrentPlayer())
				{
					if (GameController.instance.GetTile(new Vector2Int(SecondSelectedTile.x, SecondSelectedTile.y)).owner == TurnHandler.GetCurrentPlayer())
					{
						// Both tiles are the current player's
						// Perform troop movement
						GameController.instance.MoveArmies(new Vector2Int(FirstSelectedTile.x, FirstSelectedTile.y), new Vector2Int(SecondSelectedTile.x, SecondSelectedTile.y));

					}
					else if (GameController.instance.GetTile(new Vector2Int(SecondSelectedTile.x, SecondSelectedTile.y)).armies == 0)
					{
						// Move into empty territory
						GameController.instance.MoveArmies(new Vector2Int(FirstSelectedTile.x, FirstSelectedTile.y), new Vector2Int(SecondSelectedTile.x, SecondSelectedTile.y));

					}
					else
					{
						// Player attacking enemy
						GameController.instance.LaunchAttack(new Vector2Int(FirstSelectedTile.x, FirstSelectedTile.y), new Vector2Int(SecondSelectedTile.x, SecondSelectedTile.y));
					}
				}

			}
			firstTileSelected = false;
			secondTileSelected = false;
			SelectionDisplayer.ClearSelectionRing();
		}
	}

	public void OnEndTurnButton ()
	{
		firstTileSelected = false;
		secondTileSelected = false;
		placingArmies = false;
		HUDManager.HidePlaceArmyText();
		TurnHandler.ProgressTurn();
	}
	Vector3Int GetTilePosUnderMouse ()
	{
		Tilemap tilemap = WorldRenderer.GetTilemap();
		if (tilemap == null)
			return Vector3Int.zero;

		Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		Vector3Int coordinate = tilemap.WorldToCell(mouseWorldPos);
		return coordinate;
	}
}
