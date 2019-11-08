using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathHelper
{
	public static float WeightedSum(float[] inputs, float[] weights)
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

    public static float HyperbolicTangent (float input)
	{
		//Debug.Log();
		return (float)System.Math.Tanh(input);
	}

	public static int Vector2IntCompare(Vector2Int value1, Vector2Int value2)
	{
		if (value1.x < value2.x)
		{
			return -1;
		}
		else if (value1.x == value2.x)
		{
			if (value1.y < value2.y)
			{
				return -1;
			}
			else if (value1.y == value2.y)
			{
				return 0;
			}
			else
			{
				return 1;
			}
		}
		else
		{
			return 1;
		}
	}
}
