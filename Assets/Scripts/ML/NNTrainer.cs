using System.Collections;
using System.Collections.Generic;
using System.Linq;
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


namespace ML
{
	public static class NnTrainer
	{
		private const int HiddenLayerCount = 12;
		private const int MAXTurnCount = 400;
		private const int MainSceneIndex = 1;
		private const float WeightInitMIN = -1f;
		private const float WeightInitMAX = 1f;
		private const float MutationChance = 0.04f;
		private const string SavePath = @"G:\Projects\Unity Projects\battle-for-breckinridge-isle\Saved Models";

		public static bool doShowMoves = true;

		public static void StartTraining (int modelsPerGen)
		{
			StartTraining(1000000, modelsPerGen);
		}
		public static void StartTraining (int numGenerations, int modelsPerGen)
		{
			SceneManager.CreateScene("Training_Scene");
			SceneManager.LoadScene("Training_Scene", LoadSceneMode.Additive);

			GameObject trainerObject = new GameObject();
			MonoBehaviour trainerBehaviour = trainerObject.AddComponent<EnumeratorRunner>();
			SceneManager.MoveGameObjectToScene(trainerObject, SceneManager.GetSceneByName("Training_Scene"));

			trainerBehaviour.StartCoroutine(Train(numGenerations, modelsPerGen));
		}

		// Creates a text file with each model in the given generation as a comma-separated line of ints
		private static void SaveGen (float[][][][] genWeights, int genNumber)
		{
			Debug.Log("Writing generation to file.");
			using (System.IO.StreamWriter file =
				new System.IO.StreamWriter(SavePath + "\\" + genNumber.ToString("D8") + ".txt", false, System.Text.Encoding.UTF8, 65536))
			{
				Debug.Log("File created");
				for (int m = 0; m < genWeights.Length; m++)
				{
					System.Text.StringBuilder line = new System.Text.StringBuilder();
					for (int l = 0; l < genWeights[m].Length; l++)
					{
						for (int p = 0; p < genWeights[m][l].Length; p++)
						{
							for (int w = 0; w < genWeights[m][l][p].Length; w++)
							{
								line.Append(genWeights[m][l][p][w]);
								line.Append(", ");
							}
						}
					}
					file.WriteLine(line.ToString());
				}
			}
			Debug.Log("Complete.");
		}

		private static IEnumerator Train (int numGenerations, int modelsPerGen)
		{
			Dictionary<Vector2Int, GameTile> gameMap = GameController.instance.gameMap;

			int inputNodeCount = gameMap.Keys.Count * 3;
			int outputNodeCount = 1 + gameMap.Keys.Count * 2;
			int nodeCountPerLayer = inputNodeCount;

			int[] perceptronArray = new int[HiddenLayerCount];
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

			// All the weights of all the models of the current generation
			float[][][][] genWeights = new float[modelsPerGen][][][];

			// Randomize weights
			for (int m = 0; m < modelsPerGen; m++)
			{
				genWeights[m] = new float[HiddenLayerCount][][];
				for (int l = 0; l < HiddenLayerCount; l++)
				{
					genWeights[m][l] = new float[nodeCountPerLayer][];
					for (int p = 0; p < nodeCountPerLayer; p++)
					{
						int weightCount = nn.GetInputCount(l, p);
						genWeights[m][l][p] = new float[weightCount];
						for (int w = 0; w < weightCount; w++)
						{
							genWeights[m][l][p][w] = Random.Range(WeightInitMIN, WeightInitMAX);
						}
					}
				}
			}

			// TRAINING PROCESS
			// We'll be testing two models at a time, pitted against each other.
			for (int gen = 0; gen < numGenerations; gen++)
			{
				// Save the current model as a text file
				SaveGen(genWeights, gen);

				// One bool for each model in the current generation, to track whether it won
				bool[] wins = new bool[modelsPerGen];

				for (int model = 0; model < modelsPerGen; model += 2)
				{
					SceneManager.UnloadSceneAsync(MainSceneIndex);
					while (SceneManager.GetSceneByBuildIndex(MainSceneIndex).isLoaded)
					{
						yield return null;
					}
					SceneManager.LoadScene(MainSceneIndex, LoadSceneMode.Additive);
					yield return null;
					SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(MainSceneIndex));

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
						if (TurnHandler.CurrentTurn > MAXTurnCount)
						{
							Debug.Log("Turn limit reached.");
							// Hit max turn count; end the game
							GameController.instance.gameEnded = true;
							break;
						}

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
							// Find best starting tile
							int bestStart = 0;

							for (int i = 0; i < GameController.WorldX * GameController.WorldY; i++)
							{
								int y = (i) / GameController.WorldX;
								int x = (i) % GameController.WorldX;

								// If this would not be a valid start tile, make it highly undesirable
								GameTile tile = GameController.instance.gameMap[new Vector2Int(x, y)];
								if (tile.owner != TurnHandler.GetCurrentPlayer() || (!InputHandler.instance.placingArmies && tile.armies == tile.expendedArmies))
								{
									output[i + 1] -= 1000;
								}

								if (output[i + 1] > output[bestStart + 1])
								{
									bestStart = i;
								}
							}

							// Find best target tile
							int offset = 1 + GameController.WorldX * GameController.WorldY;
							int bestTarget = 0;

							for (int i = 0; i < GameController.WorldX * GameController.WorldY; i++)
							{
								int y = (i) / GameController.WorldX;
								int x = (i) % GameController.WorldX;
								GameTile tile = GameController.instance.gameMap[new Vector2Int(x, y)];

								// Check if this tile is next to the start tile, and if not make it highly undesirable
								bool nextToStartTile = false;
								foreach (Vector2Int adjTile in GameController.instance.GetAdjacentTiles(new Vector2Int(x, y)))
								{
									if (adjTile.x == bestStart % GameController.WorldX && adjTile.y == bestStart / GameController.WorldX)
									{
										nextToStartTile = true;
									}
								}
								if (!nextToStartTile)
								{
									output[i + offset] -= 1000;
								}

								if (output[i + offset] > output[bestTarget + offset])
								{
									bestTarget = i;
								}
							}

							if (output[0] > output[bestStart + 1])
							{
								// Agent decided end turn is best move
								break;
							}
							else
							{
								// Input move
								int startY = bestStart / GameController.WorldX;
								int startX = bestStart % GameController.WorldX;
								int targetY = bestTarget / GameController.WorldX;
								int targetX = bestTarget % GameController.WorldX;

								InputHandler.instance.ClearTileSelection();

								InputHandler.instance.RegisterClickOnTile(new Vector2Int(startX, startY));

								if (doShowMoves)
									yield return null;

								if (!InputHandler.instance.placingArmies) // Only input the second move if we're not placing armies
								{
									InputHandler.instance.RegisterClickOnTile(new Vector2Int(targetX, targetY));
									if (doShowMoves)
										yield return null;
								}
							}

							if (GameController.instance.gameEnded)
								break;

							input = GatherInputs();
							if (!Enumerable.SequenceEqual(input, GatherInputs()))
							{
								Debug.Log(input[1]);
								Debug.Log(GatherInputs()[1]);
								Debug.LogError("input gathering is not deterministic");
							}
							output = nn.Calculate(input);
						}
						InputHandler.instance.OnEndTurnButton();
					}
					// The game is over.
					// Evaluate which player won:
					if (ScorePosition(GameController.instance.startingPlayers[0]) > ScorePosition(GameController.instance.startingPlayers[1]))
					{
						Debug.Log("Win goes to " + GameController.instance.startingPlayers[0].NationName);
						wins[model] = true;
						wins[model + 1] = false;
					}
					else if (ScorePosition(GameController.instance.startingPlayers[0]) < ScorePosition(GameController.instance.startingPlayers[1]))
					{
						Debug.Log("Win goes to " + GameController.instance.startingPlayers[1].NationName);
						wins[model] = false;
						wins[model + 1] = true;
					}
					else
					{
						Debug.Log("A perfect draw.");
						wins[model] = false;
						wins[model + 1] = false;
					}

				
				}
				// All the models in this generation have been tested. Time to breed.
				Debug.Log("Breeding winners of generation " + gen);
				List<float[][][]> breedingPool = new List<float[][][]>();
				for (int m = 0; m < modelsPerGen; m++)
				{
					if (wins[m] == true)
					{
						breedingPool.Add(genWeights[m]);
					}
				}
				if (breedingPool.Count == 0)
				{
					breedingPool.Add(genWeights[0]);
					Debug.LogError("No models in gen " + gen + " survived to breed!");
				}
				for (int m = 0; m < modelsPerGen; m++)
				{
					for (int l = 0; l < HiddenLayerCount; l++)
					{
						for (int p = 0; p < nodeCountPerLayer; p++)
						{
							for (int w = 0; w < genWeights[m][l][p].Length; w++)
							{
								if (Random.value < MutationChance)
								{
									genWeights[m][l][p][w] = Random.Range(WeightInitMIN, WeightInitMAX);
								}
								else
								{
									int i = Random.Range(0, breedingPool.Count);
									genWeights[m][l][p][w] = breedingPool[i][l][p][w];
								}
							}
						}
					}
				}
			}
		}

		private static float[] GatherInputs()
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

		private static float ScorePosition (Player player)
		{
			const float armyProductionMult = 5f;

			Dictionary<Vector2Int, GameTile> gameMap = GameController.instance.gameMap;
			float total = 0;
			foreach (Vector2Int pos in gameMap.Keys)
			{
				if (gameMap[pos].owner == player)
				{
					total += gameMap[pos].armies;
					total += gameMap[pos].type.armyProduction * armyProductionMult;
				}
			}
			return total;
		}

	}
}



