using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class WorldRenderer : MonoBehaviour
{
	private static WorldRenderer _instance;

	[SerializeField] private GameObject tileMapPrefab = null;
	[SerializeField] private GameObject armyTextPrefab = null;
	[SerializeField] private GameObject expendedArmyTextPrefab = null;

	private Dictionary<Vector3, TextMeshProUGUI> _armyTexts;
	private Dictionary<Vector3, TextMeshProUGUI> _expendedArmyTexts;

	private Tilemap _gameTilemap;

	public static Tilemap GetTilemap()
	{
		return _instance._gameTilemap;
	}

	// Start is called before the first frame update
	private void Start()
	{
		_instance = this;
		_armyTexts = new Dictionary<Vector3, TextMeshProUGUI>();
		_expendedArmyTexts = new Dictionary<Vector3, TextMeshProUGUI>();
	}
	private void OnDestroy()
	{
		_instance = null;
	}


	public static void RenderWorld (Dictionary<Vector2Int, GameTile> world)
	{
		if (_instance._gameTilemap == null)
		{
			GameObject tilemapObject = Instantiate(_instance.tileMapPrefab);
			SceneManager.MoveGameObjectToScene(tilemapObject, SceneManager.GetSceneByBuildIndex(1));
			_instance._gameTilemap = tilemapObject.GetComponentInChildren<Tilemap>();
			if (_instance._gameTilemap == null)
			{
				throw new UnityException("No tilemap found!");
			}
		}

		foreach (Vector2Int location in world.Keys)
		{
			_instance._gameTilemap.SetTile(new Vector3Int(location.x, location.y, 0), world[location].type.tilePrefab);

			Vector3 textPos = _instance._gameTilemap.CellToWorld(new Vector3Int(location.x, location.y, 0));
			textPos += new Vector3(0.5f, 0.5f, 0);
			Vector3 expendedTextPos = _instance._gameTilemap.CellToWorld(new Vector3Int(location.x, location.y, 0));
			expendedTextPos += new Vector3(0.5f, 0.5f, 0);
			TextMeshProUGUI armyText;
			TextMeshProUGUI expendedArmyText;
			if (_instance._armyTexts.ContainsKey(textPos) && _instance._armyTexts[textPos] != null)
			{
				armyText = _instance._armyTexts[textPos];
			}
			else
			{
				GameObject textObj = GameObject.Instantiate(_instance.armyTextPrefab, textPos, Quaternion.identity);
				SceneManager.MoveGameObjectToScene(textObj, SceneManager.GetSceneByBuildIndex(1));
				armyText = textObj.GetComponentInChildren<TextMeshProUGUI>();
				_instance._armyTexts.Add(textPos, armyText);
			}
			if (_instance._expendedArmyTexts.ContainsKey(textPos) && _instance._expendedArmyTexts[textPos] != null)
			{
				expendedArmyText = _instance._expendedArmyTexts[textPos];
			}
			else
			{
				GameObject textObj = GameObject.Instantiate(_instance.expendedArmyTextPrefab, expendedTextPos, Quaternion.identity);
				SceneManager.MoveGameObjectToScene(textObj, SceneManager.GetSceneByBuildIndex(1));
				expendedArmyText = textObj.GetComponentInChildren<TextMeshProUGUI>();
				_instance._expendedArmyTexts.Add(textPos, expendedArmyText);
			}

			if (world[location].owner != null)
			{
				armyText.text = (world[location].armies - world[location].expendedArmies).ToString();
				expendedArmyText.text =  "+" + world[location].expendedArmies.ToString();

				armyText.color = world[location].owner.Color;
				expendedArmyText.color = world[location].owner.Color;
				expendedArmyText.alpha = 0.5f;

				if (world[location].armies - world[location].expendedArmies == 0)
				{
					if (world[location].expendedArmies > 0)
					{
						// No available armies but some expended ones;
						// use main text but faded
						armyText.text = world[location].expendedArmies.ToString();
						armyText.alpha = 0.5f;
						expendedArmyText.text = "";
					}
					else
					{
						armyText.text = "";
						expendedArmyText.text = "";
					}
				}
					
				if (world[location].expendedArmies == 0)
					expendedArmyText.text = "";
			}
			else
			{
				armyText.text = "";
				expendedArmyText.text = "";
			}
		}
		_instance._gameTilemap.RefreshAllTiles();
	}
}