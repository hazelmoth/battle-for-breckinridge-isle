using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Perceptron
{
	public Perceptron (float[] weights)
	{
		this.weights = weights;
	}

	float[] weights;

	public float Activate (float[] input)
	{
		return (float)System.Math.Tanh(WeightedSum(input, weights));
	}

	float WeightedSum (float[] inputs, float[] weights)
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
