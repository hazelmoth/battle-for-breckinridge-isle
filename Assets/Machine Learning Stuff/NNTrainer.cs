using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

	public static void Train()
	{
		NeuralNetwork nn = new NeuralNetwork()
	}

	static float[] GetInputs()
	{

	}
}
