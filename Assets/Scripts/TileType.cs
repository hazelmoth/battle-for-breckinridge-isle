using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "new_tile", menuName = "Tile Type")]
public class TileType : ScriptableObject
{
	[SerializeField] public string id;
	[SerializeField] public Sprite sprite;
	[SerializeField] public TileBase tilePrefab;
	[SerializeField] public float armyProduction;
	[SerializeField] public float attackBonus;
	[SerializeField] public float defenseBonus;
}