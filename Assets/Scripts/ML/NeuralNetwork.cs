using System.Collections.Generic;
using UnityEngine;

namespace ML
{
	public class NeuralNetwork
	{
		private int _numInputNodes;
		private int _numOutputNodes;
		private List<PerceptronLayer> _layers;
	
		// Takes a number of input nodes and an array containing
		// a number of perceptrons for each layer.
		public NeuralNetwork(int inputNodes, int outputNodes, int[] layers)
		{
			this._numInputNodes = inputNodes;
			this._numOutputNodes = outputNodes;
			this._layers = new List<PerceptronLayer>();

			for (int i = 0; i < layers.Length; i++)
			{
				if (i > 0)
					this._layers.Add(new PerceptronLayer(layers[i], layers[i - 1]));
				else
					this._layers.Add(new PerceptronLayer(layers[i], inputNodes));
			}
		}

		public float[] Calculate (float[] inputs)
		{
			if (inputs.Length != _numInputNodes)
			{
				Debug.LogError("Num inputs doesn't match num input nodes!");
			}

			float[] currentInput = inputs;
			for (int i = 0; i < _layers.Count; i++)
			{
				currentInput = _layers[i].Calculate(inputs);

			}
			return currentInput;
		}

		public float[][][] GetWeights ()
		{
			float[][][] result = new float[_layers.Count][][];
			for (int l = 0; l < _layers.Count; l++)
			{
				result[l] = new float[_layers[l].Perceptrons.Count][];
				for (int p = 0; p < _layers[l].Perceptrons.Count; l++)
				{
					result[l][p] = new float[_layers[l].Perceptrons[p].Weights.Length];
					for (int w = 0; w < _layers[l].Perceptrons[p].Weights.Length; w++)
					{
						result[l][p][w] = _layers[l].Perceptrons[p].Weights[w];
					}
				}
			}
			return result;
		}
		public void SetWeights(float[][][] weights)
		{
			for (int l = 0; l < _layers.Count; l++)
			{
				for (int p = 0; p < _layers[l].Perceptrons.Count; p++)
				{
					_layers[l].Perceptrons[p].Weights = weights[l][p];
				}
			}
		}
		public int GetInputCount(int layer, int perceptron)
		{
			return _layers[layer].Perceptrons[perceptron].Weights.Length;
		}

		private class PerceptronLayer
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
}
