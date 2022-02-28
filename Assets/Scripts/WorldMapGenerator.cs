using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal static class WorldMapGenerator
{
	// TODO define plants and generation parameters in a seperate file or object
	public delegate void WorldFinishedEvent(WorldMap world);

	private const string PlainsTileId = "plains";
	private const string WaterTileId = "water";
	private const string FarmTileId = "farm";
	private const string ForestTileId = "forest";

	private const float ForestProbability = 0.3f;
	private const float FarmProbability = 0.2f;

	// higher frequency is grainier
	private const float NoiseFrequencyLayer1 = 0.2f;
	private const float NoiseFrequencyLayer2 = 1f;
	private const float NoiseFrequencyLayer3 = 1.5f;
	private const float NoiseFrequencyLayer4 = 3.5f;
	// how much each level affects the terrain
	private const float NoiseDepthLayer1 = 0.8f;
	private const float NoiseDepthLayer2 = 0.6f;
	private const float NoiseDepthLayer3 = 0.6f;
	private const float NoiseDepthLayer4 = 0.7f;

	private const float WaterLevel = 0.1f;

	public static void StartGeneration (int sizeX, int sizeY, float seed, int minSpawnPoints, WorldFinishedEvent callback, MonoBehaviour genObject)
	{
		//float seed = Random.value * 1000;
		void OnFinished(WorldMap world)
		{
			callback(world);
		}
		genObject.StartCoroutine(GenerateCoroutine(sizeX, sizeY, seed, minSpawnPoints, OnFinished));
	}

	private static IEnumerator GenerateCoroutine (int sizeX, int sizeY, float seed, int minSpawnPoints, WorldFinishedEvent callback)
	{
		WorldMap world;

		// Repeat until we generate a valid world
		while (true) 
		{
			world = new WorldMap
			{
				mapDict = new Dictionary<Vector2Int, TileType>()
			};

			int tilesPerFrame = 100;
			int tilesDoneSinceFrame = 0;

			Dictionary<Vector2Int, float> heightmap = GenerateHeightMap(sizeX, sizeY, seed);

			// Loop through every tile defined by the size, fill it with grass and maybe add a plant
			for (int y = 0; y < sizeY; y++) {
				for (int x = 0; x < sizeX; x++) {
					Vector2Int currentPosition = new Vector2Int(x, y);

					TileType newTileType;

					// Assign ground material based on height
					if (heightmap[currentPosition] > WaterLevel)
					{
						if (UnityEngine.Random.value < ForestProbability)
						{
							newTileType = TileTypeLibrary.GetTileType(ForestTileId);
						}
						else if (UnityEngine.Random.value < FarmProbability)
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

					world.mapDict.Add(currentPosition, newTileType);


					tilesDoneSinceFrame++;
					if (tilesDoneSinceFrame >= tilesPerFrame)
					{
						tilesDoneSinceFrame = 0;
						yield return null;
					}
				}
			}
			if (!WorldIsValid(world, minSpawnPoints))
			{
				yield return new WaitForSeconds(0.1f);
				seed = Random.value * 1000;
				continue;
			}

			break;
		}

		callback(world);
	}

	private static Dictionary<Vector2Int, float> GenerateHeightMap (int sizeX, int sizeY, float seed)
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
				h = h * Mathf.PerlinNoise((NoiseFrequencyLayer1 / 10) * x + seed, (NoiseFrequencyLayer1 / 10) * y + seed) * NoiseDepthLayer1 + h * (1 - NoiseDepthLayer1);
				h = h * Mathf.PerlinNoise((NoiseFrequencyLayer2 / 10) * x + seed, (NoiseFrequencyLayer2 / 10) * y + seed) * NoiseDepthLayer2 + h * (1 - NoiseDepthLayer2);
				h = h * Mathf.PerlinNoise((NoiseFrequencyLayer3 / 10) * x + seed, (NoiseFrequencyLayer3 / 10) * y + seed) * NoiseDepthLayer3 + h * (1 - NoiseDepthLayer3);
				h = h * Mathf.PerlinNoise((NoiseFrequencyLayer4 / 10) * x + seed, (NoiseFrequencyLayer4 / 10) * y + seed) * NoiseDepthLayer4 + h * (1 - NoiseDepthLayer4);

				heightmap.Add(currentPosition, h);
			}
		}
		return heightmap;
	}

	// Returns a value between 0 and 1 based on where a point is between the origin and a surrounding ellipse.
	// 1 is the center of the ellipse, 0 is the outside.
	private static float EllipseGradient(Vector2 point, float width, float height)
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

	private static bool WorldIsValid (WorldMap world, int minSpawnPoints)
	{
		int plainsFound = 0;
		foreach (TileType tile in world.mapDict.Values)
		{
			if (tile.id == PlainsTileId)
			{
				plainsFound++;
			}
		}
		return (plainsFound >= minSpawnPoints);
	}
	// For randomly selecting plants
	private struct WeightedString {
		public string value;
		public float frequencyWeight;
		public WeightedString (string id, float freqMult) {
			this.value = id;
			this.frequencyWeight = freqMult;
		}
	}

	private static string GetWeightedRandomString (WeightedString[] arr) {
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