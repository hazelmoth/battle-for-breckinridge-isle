using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static class WorldMapGenerator
{
    // TODO define plants and generation parameters in a seperate file or object
    public delegate void WorldFinishedEvent(WorldMap world);

	const string PlainsTileId = "plains";
	const string WaterTileId = "water";
	const string FarmTileId = "farm";
	const string ForestTileId = "forest";

	const float forestProbability = 0.3f;
	const float farmProbability = 0.2f;

	// higher frequency is grainier
	const float noiseFrequencyLayer1 = 0.2f;
	const float noiseFrequencyLayer2 = 1f;
	const float noiseFrequencyLayer3 = 1.5f;
	const float noiseFrequencyLayer4 = 3.5f;
	// how much each level affects the terrain
	const float noiseDepthLayer1 = 0.8f;
	const float noiseDepthLayer2 = 0.6f;
	const float noiseDepthLayer3 = 0.6f;
	const float noiseDepthLayer4 = 0.7f;

	const float waterLevel = 0.1f;

	public static void StartGeneration (int sizeX, int sizeY, float seed, WorldFinishedEvent callback, MonoBehaviour genObject)
	{
		//float seed = Random.value * 1000;
		void OnFinished(WorldMap world)
        {
            callback(world);
        }
        genObject.StartCoroutine(GenerateCoroutine(sizeX, sizeY, seed, OnFinished));
    }

    static IEnumerator GenerateCoroutine (int sizeX, int sizeY, float seed, WorldFinishedEvent callback)
    {
        WorldMap world = new WorldMap
        {
            mapDict = new Dictionary<Vector2Int, TileType>()
        };

        int tilesPerFrame = 100;
        int tilesDoneSinceFrame = 0;

        Dictionary<Vector2Int, float> heightmap = GenerateHeightMap(sizeX, sizeY, seed);

		// Loop through every tile defined by the size, fill it with grass and maybe add a plant
		for (int y = 0; y < sizeY; y++) {
			for (int x = 0; x < sizeX; x++) {
				Vector2Int currentPosition = new Vector2Int (x, y);

                TileType newTileType;

                // Assign ground material based on height
                if (heightmap[currentPosition] > waterLevel)
                {
					if (UnityEngine.Random.value < forestProbability)
					{
						newTileType = TileTypeLibrary.GetTileType(ForestTileId);
					}
					else if (UnityEngine.Random.value < farmProbability)
					{
						newTileType = TileTypeLibrary.GetTileType(FarmTileId);
					}
					else 
					{
						newTileType = TileTypeLibrary.GetTileType(PlainsTileId);
					}
                }
                else
                {
                    newTileType = TileTypeLibrary.GetTileType(WaterTileId);
                }

                world.mapDict.Add (currentPosition, newTileType);


                tilesDoneSinceFrame++;
                if (tilesDoneSinceFrame >= tilesPerFrame)
                {
                    tilesDoneSinceFrame = 0;
                    yield return null;
                }
            }
		}
        callback(world);
	}

    static Dictionary<Vector2Int, float> GenerateHeightMap (int sizeX, int sizeY, float seed)
    {
        Dictionary<Vector2Int, float> heightmap = new Dictionary<Vector2Int, float>();

        for (int y = 0; y < sizeY; y++)
        {
            for (int x = 0; x < sizeX; x++)
            {
                Vector2Int currentPosition = new Vector2Int(x, y);

                // Start with a nice height gradient from center to edges
                float h = EllipseGradient(new Vector2(x - sizeX / 2, y - sizeY / 2), sizeX, sizeY);
                // Round off the height with a log function
                if (h > 0)
                    h = Mathf.Log(h + 1, 2);

                // Multiply layers of noise so the map is more interesting
                h = h * Mathf.PerlinNoise((noiseFrequencyLayer1 / 10) * x + seed, (noiseFrequencyLayer1 / 10) * y + seed) * noiseDepthLayer1 + h * (1 - noiseDepthLayer1);
                h = h * Mathf.PerlinNoise((noiseFrequencyLayer2 / 10) * x + seed, (noiseFrequencyLayer2 / 10) * y + seed) * noiseDepthLayer2 + h * (1 - noiseDepthLayer2);
                h = h * Mathf.PerlinNoise((noiseFrequencyLayer3 / 10) * x + seed, (noiseFrequencyLayer3 / 10) * y + seed) * noiseDepthLayer3 + h * (1 - noiseDepthLayer3);
                h = h * Mathf.PerlinNoise((noiseFrequencyLayer4 / 10) * x + seed, (noiseFrequencyLayer4 / 10) * y + seed) * noiseDepthLayer4 + h * (1 - noiseDepthLayer4);

                heightmap.Add(currentPosition, h);
            }
        }
        return heightmap;
    }

	// Returns a value between 0 and 1 based on where a point is between the origin and a surrounding ellipse.
	// 1 is the center of the ellipse, 0 is the outside.
	static float EllipseGradient(Vector2 point, float width, float height)
	{
		// diameters to radii
		width /= 2;
		height /= 2;
		// Find point on ellipse that is on the line between the origin and input point
		float x = Mathf.Sqrt( Mathf.Pow(width * height * point.x, 2f) / (Mathf.Pow(point.x * height, 2) + Mathf.Pow(point.y * width, 2)) );
		float y = height * Mathf.Sqrt(1 - Mathf.Pow(x / width, 2));
		if (point.x < 0)
			x *= -1;
		if (point.y < 0)
			y *= -1;
		Vector2 ellipsePoint = new Vector2(x, y);

		float z = Vector2.Distance(Vector2.zero, point) / Vector2.Distance(Vector2.zero, ellipsePoint);
		z = Mathf.Clamp01(z);
		// Invert so 1 is the center
		z = 1 - z;
		return z;
	}
		
	// For randomly selecting plants
	struct WeightedString {
		public string value;
		public float frequencyWeight;
		public WeightedString (string id, float freqMult) {
			this.value = id;
			this.frequencyWeight = freqMult;
		}
	}
	static string GetWeightedRandomString (WeightedString[] arr) {
		float weightSum = 0f;
		string result = null;
		foreach (WeightedString option in arr) {
			weightSum += option.frequencyWeight;
		}
		float throwValue = Random.Range(0f, weightSum);
		foreach (WeightedString option in arr) {
			if (throwValue < option.frequencyWeight) {
				result = option.value;
				break;
			}
			throwValue -= option.frequencyWeight;
		}
		return result;
	}
}
