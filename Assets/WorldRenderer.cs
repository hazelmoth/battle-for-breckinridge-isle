using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

public class WorldRenderer : MonoBehaviour
{
    static WorldRenderer instance;

    [SerializeField] GameObject tileMapPrefab = null;
	[SerializeField] GameObject armyTextPrefab = null;
	[SerializeField] GameObject expendedArmyTextPrefab = null;

	Dictionary<Vector3, TextMeshProUGUI> armyTexts;
	Dictionary<Vector3, TextMeshProUGUI> expendedArmyTexts;

	Tilemap gameTilemap;

	public static Tilemap GetTilemap()
	{
		return instance.gameTilemap;
	}

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
		armyTexts = new Dictionary<Vector3, TextMeshProUGUI>();
		expendedArmyTexts = new Dictionary<Vector3, TextMeshProUGUI>();
    }
    private void OnDestroy()
    {
        instance = null;
    }


    public static void RenderWorld (Dictionary<Vector2Int, GameTile> world)
    {
        if (instance.gameTilemap == null)
        {
            GameObject tilemapObject = Instantiate(instance.tileMapPrefab);
            instance.gameTilemap = tilemapObject.GetComponentInChildren<Tilemap>();
            if (instance.gameTilemap == null)
            {
                throw new UnityException("No tilemap found!");
            }
        }

        foreach (Vector2Int location in world.Keys)
        {
            instance.gameTilemap.SetTile(new Vector3Int(location.x, location.y, 0), world[location].type.tilePrefab);

			Vector3 textPos = instance.gameTilemap.CellToWorld(new Vector3Int(location.x, location.y, 0));
			textPos += new Vector3(0.5f, 0.5f, 0);
			Vector3 expendedTextPos = instance.gameTilemap.CellToWorld(new Vector3Int(location.x, location.y, 0));
			expendedTextPos += new Vector3(0.5f, 0.5f, 0);
			TextMeshProUGUI armyText;
			TextMeshProUGUI expendedArmyText;
			if (instance.armyTexts.ContainsKey(textPos) && instance.armyTexts[textPos] != null)
			{
				armyText = instance.armyTexts[textPos];
			}
			else
			{
				GameObject textObj = GameObject.Instantiate(instance.armyTextPrefab, textPos, Quaternion.identity);
				armyText = textObj.GetComponentInChildren<TextMeshProUGUI>();
				instance.armyTexts.Add(textPos, armyText);
			}
			if (instance.expendedArmyTexts.ContainsKey(textPos) && instance.expendedArmyTexts[textPos] != null)
			{
				expendedArmyText = instance.expendedArmyTexts[textPos];
			}
			else
			{
				GameObject textObj = GameObject.Instantiate(instance.expendedArmyTextPrefab, expendedTextPos, Quaternion.identity);
				expendedArmyText = textObj.GetComponentInChildren<TextMeshProUGUI>();
				instance.expendedArmyTexts.Add(textPos, expendedArmyText);
			}

			if (world[location].owner != null)
			{
				armyText.text = (world[location].armies - world[location].expendedArmies).ToString();
				expendedArmyText.text =  "+" + world[location].expendedArmies.ToString();

				armyText.color = world[location].owner.color;
				expendedArmyText.color = world[location].owner.color;
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
		instance.gameTilemap.RefreshAllTiles();
    }
}
