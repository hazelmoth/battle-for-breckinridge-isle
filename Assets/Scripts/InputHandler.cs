using ML;
using UnityEngine;
using UnityEngine.Tilemaps;

public class InputHandler : MonoBehaviour
{
	[SerializeField] private GameObject hoverSelectorPrefab;
	private GameObject _hoverSelector;
	public static InputHandler instance;
	private Vector3Int _firstSelectedTile = new Vector3Int(-1,  -1, 0);
	private Vector3Int _secondSelectedTile = new Vector3Int(-1, -1, 0);
	private bool _firstTileSelected = false;
	private bool _secondTileSelected = false;

	public bool placingArmies = false;
	// Start is called before the first frame update
	private void Start()
	{
		instance = this;
	}
	private void OnDestroy()
	{
		instance = null;
	}
	// Update is called once per frame
	private void Update()
	{
		if (_hoverSelector == null)
		{
			_hoverSelector = GameObject.Instantiate(hoverSelectorPrefab);
		}
		_hoverSelector.transform.position = GetTilePosUnderMouse();

		if (Input.GetKeyDown(KeyCode.T))
		{
			NnTrainer.StartTraining(25);
		}
		if (Input.GetKeyDown(KeyCode.Tab))
		{
			NnTrainer.doShowMoves = !NnTrainer.doShowMoves;
		}

		if (Input.GetMouseButtonDown(0))
		{
			Vector3Int tilePos = GetTilePosUnderMouse();
			RegisterClickOnTile(new Vector2Int(tilePos.x, tilePos.y));
		}
	}

	public void RegisterClickOnTile (Vector2Int tilePos)
	{
		Player currentPlayer = TurnHandler.GetCurrentPlayer();
		if (!currentPlayer.AllowHumanInput) return;

		GameTile tile = GameController.instance.GetTile(tilePos);

		if (tile == null) return;

		if (!_firstTileSelected)
		{
			// Verify that this is the current player's tile
			if (tile.owner != currentPlayer)
				return;

			_firstSelectedTile = new Vector3Int(tilePos.x, tilePos.y, 0);
			_firstTileSelected = true;
			if (!placingArmies)
				SelectionDisplayer.PlaceSelectionRing(new Vector2Int(_firstSelectedTile.x, _firstSelectedTile.y));

			if (placingArmies)
			{
				if (currentPlayer.ArmiesToPlace <= 0)
					placingArmies = false;
				else
				{
					GameController.instance.PlaceArmyIfAvailable(currentPlayer, new Vector2Int(tilePos.x, tilePos.y));
					_firstTileSelected = false;
					_secondTileSelected = false;

					if (currentPlayer.ArmiesToPlace <= 0)
					{
						placingArmies = false;
						HUDManager.HidePlaceArmyText();
					}
					else
					{
						HUDManager.ShowPlaceArmyText(currentPlayer.ArmiesToPlace);
					}
					return;
				}
			}
		}
		else
		{
			_secondSelectedTile = new Vector3Int(tilePos.x, tilePos.y, 0);
			_secondTileSelected = true;
		}

		if (_firstTileSelected && _secondTileSelected)
		{
			if (Mathf.Abs(_firstSelectedTile.x - _secondSelectedTile.x) == 1
				&& Mathf.Abs(_firstSelectedTile.y - _secondSelectedTile.y) == 0
				|| Mathf.Abs(_firstSelectedTile.x - _secondSelectedTile.x) == 0
				&& Mathf.Abs(_firstSelectedTile.y - _secondSelectedTile.y) == 1)
			{
				if (GameController.instance.GetTile(new Vector2Int(_firstSelectedTile.x, _firstSelectedTile.y)).owner == currentPlayer)
				{
					if (GameController.instance.GetTile(new Vector2Int(_secondSelectedTile.x, _secondSelectedTile.y)).owner == currentPlayer)
					{
						// Both tiles are the current player's
						// Perform troop movement
						GameController.instance.MoveArmy(new Vector2Int(_firstSelectedTile.x, _firstSelectedTile.y), new Vector2Int(_secondSelectedTile.x, _secondSelectedTile.y));

					}
					else if (GameController.instance.GetTile(new Vector2Int(_secondSelectedTile.x, _secondSelectedTile.y)).armies == 0)
					{
						// Move into empty territory
						GameController.instance.MoveArmy(new Vector2Int(_firstSelectedTile.x, _firstSelectedTile.y), new Vector2Int(_secondSelectedTile.x, _secondSelectedTile.y));

					}
					else
					{
						// Player attacking enemy
						GameController.instance.LaunchAttack(new Vector2Int(_firstSelectedTile.x, _firstSelectedTile.y), new Vector2Int(_secondSelectedTile.x, _secondSelectedTile.y));
					}
				}

			}
			_firstTileSelected = false;
			_secondTileSelected = false;
			SelectionDisplayer.ClearSelectionRing();
		}
	}

	public void OnEndTurnButton ()
	{
		_firstTileSelected = false;
		_secondTileSelected = false;
		placingArmies = false;
		SelectionDisplayer.ClearSelectionRing();
		HUDManager.HidePlaceArmyText();
		TurnHandler.ProgressTurn();
	}
	public void ClearTileSelection ()
	{
		_firstTileSelected = false;
		_secondTileSelected = false;
		SelectionDisplayer.ClearSelectionRing();
	}

	private Vector3Int GetTilePosUnderMouse ()
	{
		Tilemap tilemap = WorldRenderer.GetTilemap();
		if (tilemap == null)
			return Vector3Int.zero;

		Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		Vector3Int coordinate = tilemap.WorldToCell(mouseWorldPos);
		return coordinate;
	}
}
