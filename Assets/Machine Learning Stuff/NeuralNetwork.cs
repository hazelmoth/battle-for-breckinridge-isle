using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNetwork
{
	int numInputNodes;
	int numOutputNodes;
	List<PerceptronLayer> layers;
	
	// Takes a number of input nodes and an array containing
	// a number of perceptrons for each layer.
	public NeuralNetwork(int inputNodes, int outputNodes, int[] layers)
	{
		this.numInputNodes = inputNodes;
		this.numOutputNodes = outputNodes;
		this.layers = new List<PerceptronLayer>();

		for (int i = 0; i < layers.Length; i++)
		{
			if (i > 0)
				this.layers.Add(new PerceptronLayer(layers[i], layers[i - 1]));
			else
				this.layers.Add(new PerceptronLayer(layers[i], inputNodes));
		}
	}

	public float[] Calculate (float[] inputs)
	{
		if (inputs.Length != numInputNodes)
		{
			Debug.LogError("Num inputs doesn't match num input nodes!");
		}

		float[] currentInput = inputs;
		for (int i = 0; i < layers.Count; i++)
		{
			currentInput = layers[i].Calculate(inputs);

		}
		return currentInput;
	}

	public float[][][] GetWeights ()
	{
		float[][][] result = new float[layers.Count][][];
		for (int l = 0; l < layers.Count; l++)
		{
			result[l] = new float[layers[l].Perceptrons.Count][];
			for (int p = 0; p < layers[l].Perceptrons.Count; l++)
			{
				result[l][p] = new float[layers[l].Perceptrons[p].Weights.Length];
				for (int w = 0; w < layers[l].Perceptrons[p].Weights.Length; w++)
				{
					result[l][p][w] = layers[l].Perceptrons[p].Weights[w];
				}
			}
		}
		return result;
	}
	public void SetWeights(float[][][] weights)
	{
		for (int l = 0; l < layers.Count; l++)
		{
			for (int p = 0; p < layers[l].Perceptrons.Count; l++)
			{
				layers[l].Perceptrons[p].Weights = weights[l][p];
			}
		}
	}

	class PerceptronLayer
	{
		public List<Perceptron> Perceptrons { get; private set; }

		public PerceptronLayer (int perceptronCount, int numInputs)
		{
			Perceptrons = new List<Perceptron>();
			for (int i = 0; i < perceptronCount; i++)
			{
				Perceptron newPercep = new Perceptron(new float[numInputs]);
				newPercep.RandomizeWeights();
				Perceptrons.Add(newPercep);
			}
		}

		// Takes an array of floats to be input into every perceptron and returns
		// a float for each perceptron's output
		public float[] Calculate(float[] inputs)
		{
			float[] results = new float[Perceptrons.Count];

			for (int i = 0; i < Perceptrons.Count; i++)
			{
				float output = Perceptrons[i].Activate(inputs);
				results[i] = output;
			}

			return results;
		}
	}
}
