using System.Collections;
using System.Linq;
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

// Output format (2 * tilecount + 1):
//
// - a float for preference for ending turn
// - a float for each tile to denote chance of choosing as first tile
// - a float for each tile to denote chance of choosing as target tile


public static class NNTrainer
{
	const int HIDDEN_LAYER_COUNT = 12;
	const int MAIN_SCENE_INDEX = 1;
	const float WEIGHT_INIT_MIN = -1f;
	const float WEIGHT_INIT_MAX = 1f;

	public static void StartTraining (int numGenerations, int modelsPerGen)
	{
		//GameObject trainerObject = new GameObject("Trainer Object");
		//MonoBehaviour trainerBehaviour = trainerObject.AddComponent<MonoBehaviour>();
		SceneManager.CreateScene("Training_Scene");
		SceneManager.LoadScene("Training_Scene", LoadSceneMode.Additive);

		GameObject trainerObject = new GameObject();
		MonoBehaviour trainerBehaviour = trainerObject.AddComponent<EnumeratorRunner>();
		SceneManager.MoveGameObjectToScene(trainerObject, SceneManager.GetSceneByName("Training_Scene"));

		trainerBehaviour.StartCoroutine(Train(numGenerations, modelsPerGen));
	}

	static IEnumerator Train (int numGenerations, int modelsPerGen)
	{
		Dictionary<Vector2Int, GameTile> gameMap = GameController.instance.gameMap;

		int inputNodeCount = gameMap.Keys.Count * 3;
		int outputNodeCount = 1 + gameMap.Keys.Count * 2;
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
					int weightCount = nn.GetInputCount(l, p);
					genWeights[m][l][p] = new float[weightCount];
					for (int w = 0; w < weightCount; w++)
					{
						genWeights[m][l][p][w] = Random.Range(WEIGHT_INIT_MIN, WEIGHT_INIT_MAX);
					}
				}
			}
		}

		// TRAINING PROCESS
		// We'll be testing two models at a time, pitted against each other.
		for (int gen = 0; gen < numGenerations; gen++)
		{
			// One bool for each model in the current generation, to track whether it won
			bool[] wins = new bool[modelsPerGen];

			for (int model = 0; model < modelsPerGen; model += 2)
			{
				SceneManager.UnloadSceneAsync(MAIN_SCENE_INDEX);
				while (SceneManager.GetSceneByBuildIndex(MAIN_SCENE_INDEX).isLoaded)
				{
					yield return null;
				}
				SceneManager.LoadScene(MAIN_SCENE_INDEX, LoadSceneMode.Additive);
				yield return null;
				SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(MAIN_SCENE_INDEX));

				if (!GameController.instance.setupHasFinished)
				{
					Debug.Log("Waiting for setup");
					bool setupFinished = false;
					GameController.instance.OnSetupComplete += (() => setupFinished = true);
					while (setupFinished == false)
					{
						// TODO timeout
						yield return null;
					}
				}
				Debug.Log("Setup finished.");

				// Wait a frame for scene to load
				yield return null;

				float[] model1LastInput = new float[inputNodeCount];
				float[] model2LastInput = new float[inputNodeCount];

				// Play through a game for every two models
				while (GameController.instance.gameEnded == false)
				{
					// Set weights depending on whose turn it is
					if (TurnHandler.CurrentTurn % 2 == 1) // Turn is odd (turns start on 1)
					{
						// Set NN to have weights for first model
						nn.SetWeights(genWeights[model]);
					}
					else // Turn is even
					{
						// Set NN to have weights for second model
						nn.SetWeights(genWeights[model + 1]);
					}

					float[] input = GatherInputs();
					float[] output = nn.Calculate(input);


					while (true) // Repeat until turn end
					{
						// If this model is facing the same input that it was last time it had a turn,
						// make skipping the turn undesirable
						if (TurnHandler.CurrentTurn % 2 == 1)
						{
							if (Enumerable.SequenceEqual(model1LastInput, input))
							{
								output[0] -= 1000;
								Debug.Log("input is same as last time");
							}
							else
							{
								model1LastInput = input;
							}
						}
						else
						{
							if (Enumerable.SequenceEqual(model2LastInput, input))
							{
								output[0] -= 1000;
								Debug.Log("input is same as last time");
							}
							else
							{
								model2LastInput = input;
							}
						}


						// Find best starting tile
						int bestStart = 1;

						for (int i = 0; i < GameController.WORLD_X * GameController.WORLD_Y; i++)
						{
							int y = (i) / GameController.WORLD_X;
							int x = (i) % GameController.WORLD_X;

							// If this would not be a valid start tile, make it highly undesirable
							GameTile tile = GameController.instance.gameMap[new Vector2Int(x, y)];
							if (tile.owner != TurnHandler.GetCurrentPlayer() || (!InputHandler.instance.placingArmies && tile.armies == tile.expendedArmies))
							{
								output[i + 1] = -1000;
							}

							if (output[i + 1] > output[bestStart])
							{
								bestStart = i;
							}
						}

						// Find best target tile
						int offset = 1 + GameController.WORLD_X * GameController.WORLD_Y;
						int bestTarget = offset;

						for (int i = 0; i < GameController.WORLD_X * GameController.WORLD_Y; i++)
						{
							int y = (i) / GameController.WORLD_X;
							int x = (i) % GameController.WORLD_X;
							GameTile tile = GameController.instance.gameMap[new Vector2Int(x, y)];

							// Check if this tile is next to the start tile, and if not make it highly undesirable
							bool nextToStartTile = false;
							foreach (Vector2Int adjTile in GameController.instance.GetAdjacentTiles(new Vector2Int(x, y)))
							{
								if (adjTile.x == bestStart % GameController.WORLD_X && adjTile.y == bestStart / GameController.WORLD_X)
								{
									nextToStartTile = true;
								}
							}
							if (!nextToStartTile)
							{
								output[i + offset] = -1000;
							}

							if (output[i + offset] > output[bestTarget])
							{
								bestTarget = i;
								if (nextToStartTile)
									Debug.Log("best move is next to start");
							}
						}

						if (output[0] > output[bestStart + 1])
						{
							// Agent decided end turn is best move
							InputHandler.instance.OnEndTurnButton();
							yield return null;
						}
						else
						{
							// Input move
							int startY = bestStart / GameController.WORLD_X;
							int startX = bestStart % GameController.WORLD_X;
							int targetY = bestTarget / GameController.WORLD_X;
							int targetX = bestTarget % GameController.WORLD_X;

							Debug.Log(startX + ", " + startY + " to " + targetX + ", " + targetY);

							InputHandler.instance.RegisterClickOnTile(new Vector2Int(startX, startY));
							yield return null;

							if (!InputHandler.instance.placingArmies) // Only input the second move if we're not placing armies
							{
								Debug.Log("Target selected");
								InputHandler.instance.RegisterClickOnTile(new Vector2Int(targetX, targetY));
								yield return null;
							}
						}

						input = GatherInputs();
						if (!Enumerable.SequenceEqual(input, GatherInputs()))
						{
							Debug.Log(input[1]);
							Debug.Log(GatherInputs()[1]);
							Debug.LogError("input gathering is not deterministic");
						}
						output = nn.Calculate(input);
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



