using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Input format (3 * tilecount):
//
// - an integer for each tile on map to denote tile type
//		0 = water
//		1 = plains
//		2 = forest
//		3 = farm
//
// - an integer for each tile to denote friendly or not
//		0 = unoccupied
//		1 = friendly
//		2 = enemy
//
// - an integer for each tile to denote army count

// Output format (2 * tilecount + 2):
//
// - a float for preference of not ending turn
// - a float for preference for ending turn
// - a float for each tile to denote chance of choosing as first tile
// - a float for each tile to denote chance of choosing as target tile


public static class NNTrainer
{
	const int HIDDEN_LAYER_COUNT = 3;
	const int MAIN_SCENE_INDEX = 1;

	public static void StartTraining (int numGenerations, int modelsPerGen)
	{
		GameObject trainerObject = new GameObject("Trainer Object");
		MonoBehaviour trainerBehaviour = trainerObject.AddComponent<MonoBehaviour>();
		trainerBehaviour.StartCoroutine(Train(numGenerations, modelsPerGen));
	}

	static IEnumerator Train (int numGenerations, int modelsPerGen)
	{
		Dictionary<Vector2Int, GameTile> gameMap = GameController.instance.gameMap;

		int inputNodeCount = gameMap.Keys.Count * 3;
		int outputNodeCount = 2 + gameMap.Keys.Count * 2;
		int nodeCountPerLayer = inputNodeCount;

		int[] perceptronArray = new int[HIDDEN_LAYER_COUNT];
		for (int i = 0; i < perceptronArray.Length; i++)
		{
			// Have the number of perceptrons in each layer equal to number of inputs
			perceptronArray[i] = nodeCountPerLayer;
		}

		if (modelsPerGen % 2 != 0)
		{
			// Make sure the number of models per generation is even so we always
			// have two models to pit against each other
			Debug.LogWarning("Number of models per gen is odd. Fixing.");
			modelsPerGen++;
		}

		NeuralNetwork nn = new NeuralNetwork(inputNodeCount, outputNodeCount, perceptronArray);

		float[][][][] genWeights = new float[modelsPerGen][][][];

		// Randomize weights
		for (int m = 0; m < modelsPerGen; m++)
		{
			genWeights[m] = new float[HIDDEN_LAYER_COUNT][][];
			for (int l = 0; l < HIDDEN_LAYER_COUNT; l++)
			{
				genWeights[m][l] = new float[nodeCountPerLayer][];
				for (int p = 0; p < nodeCountPerLayer; p++)
				{
					genWeights[m][l][p] = new float[nn.]
					for (int w = 0; w < )
				}
			}
		}

		// TRAINING PROCESS
		for (int gen = 0; gen < numGenerations; gen++)
		{
			for (int model = 0; model < modelsPerGen; model += 2)
			{
				SceneManager.LoadScene(MAIN_SCENE_INDEX);

				// Wait a frame for scene to load
				yield return null;

				bool setupFinished = false;
				GameController.instance.OnSetupComplete += (() => setupFinished = true);
				while (setupFinished == false)
				{
					// TODO timeout
					yield return null;
				}

				// Play through a game for every two models
				while (GameController.instance.gameEnded == false)
				{
					// TODO set weights for both models
					if (TurnHandler.CurrentTurn % 2 == 1) // Turn is odd
					{
						// > Set NN to have weights for odd model
						nn.SetWeights(genWeights[model]);
					}
					else // Turn is even
					{
						// > Set NN to have weights for even model
					}

					float[] output = nn.Calculate(GatherInputs());
					while (output[0] < output[1]) // Repeat until turn end
					{
						// > Input move

						output = nn.Calculate(GatherInputs());
					}
				}
			}
			
		}
	}

	static float[] GatherInputs()
	{
		Dictionary<Vector2Int, GameTile> gameMap = GameController.instance.gameMap;
		int inputNodeCount = gameMap.Keys.Count * 3;

		float[] inputs = new float[inputNodeCount];

		Vector2Int[] mapKeys = new Vector2Int[gameMap.Keys.Count];
		gameMap.Keys.CopyTo(mapKeys, 0);

		System.Array.Sort(mapKeys, MathHelper.Vector2IntCompare);

		for (int i = 0; i < mapKeys.Length; i++)
		{
			switch (gameMap[mapKeys[i]].type.id)
			{
				case "water":
					inputs[i] = 0;
					break;
				case "plains":
					inputs[i] = 1;
					break;
				case "forest":
					inputs[i] = 2;
					break;
				case "farm":
				default:
					inputs[i] = 3;
					break;
			}
		}

		int baseIndex = mapKeys.Length;

		for (int i = 0; i < mapKeys.Length; i++)
		{
			if (gameMap[mapKeys[i]].owner == null)
			{
				inputs[i + baseIndex] = 0; // unoccupied
			}
			else if (gameMap[mapKeys[i]].owner == TurnHandler.GetCurrentPlayer())
			{
				inputs[i + baseIndex] = 1; // friendly territory
			}
			else 
			{
				inputs[i + baseIndex] = 2; // enemy territory
			}
		}

		baseIndex = mapKeys.Length * 2;

		for (int i = 0; i < mapKeys.Length; i++)
		{
			inputs[i + baseIndex] = gameMap[mapKeys[i]].armies;
		}

		return inputs;
	}


}



