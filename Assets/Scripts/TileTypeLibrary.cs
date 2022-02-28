using System.Collections.Generic;
using UnityEngine;

public class TileTypeLibrary : MonoBehaviour
{
	private static TileTypeLibrary _instance;
	[SerializeField] private List<TileType> tileTypes = null;

	// Start is called before the first frame update
	private void Start()
	{
		_instance = this;
	}
	private void OnDestroy()
	{
		_instance = null;
	}
	// Update is called once per frame
	private void Update()
	{
        
	}
	public static TileType GetTileType (string id)
	{
		foreach(TileType type in _instance.tileTypes)
		{
			if (type.id == id)
				return type;
		}
		return null;
	}
}