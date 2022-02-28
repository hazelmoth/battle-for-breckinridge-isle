using UnityEngine;

namespace ML
{
	public class Perceptron
	{
		public Perceptron (float[] weights)
		{
			this.Weights = weights;
		}

		public float[] Weights { get; set; }

		public float Activate (float[] input)
		{
			return (float)System.Math.Tanh(WeightedSum(input, Weights));
		}
		public void RandomizeWeights ()
		{
			for (int i = 0; i < Weights.Length; i++)
			{
				Weights[i] = Random.Range(-1f, 1f);
			}
		}

		private float WeightedSum (float[] inputs, float[] weights)
		{
			if (inputs.Length != weights.Length)
			{
				Debug.LogError("Number of weights doesn't match number of inputs!");
				return 0;
			}
			float result = 0;
			for (int i = 0; i < inputs.Length; i++)
			{
				result += inputs[i] * weights[i];
			}

			return result;
		}

	}
}
