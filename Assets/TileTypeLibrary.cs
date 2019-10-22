using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileTypeLibrary : MonoBehaviour
{
	static TileTypeLibrary instance;
	[SerializeField] List<TileType> tileTypes = null;

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
        
    }
    public static TileType GetTileType (string id)
	{
        foreach(TileType type in instance.tileTypes)
		{
			if (type.id == id)
				return type;
		}
		return null;
	}
}
