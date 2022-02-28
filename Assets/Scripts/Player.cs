using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Player
{
	protected Player(string nationName, Color color, bool allowHumanInput, float armyProductionMultiplier)
	{
		NationName = nationName;
		Color = color;
		AllowHumanInput = allowHumanInput;
		ArmyProductionMultiplier = armyProductionMultiplier;
	}

	public string NationName { get; set; }

	public Color Color { get; set; }

	public bool AllowHumanInput { get; }

	public float ArmyProductionMultiplier { get; }

	/// How many armies this player currently has available to place
	public int ArmiesToPlace { get; set; }

	public abstract void BeginTurn(Action onTurnEnd);

	public List<Vector2Int> GetHeldTiles()
	{
		return GameController.instance.gameMap
			.Where(pair => this.Equals(pair.Value.owner))
			.Select(pair => pair.Key)
			.ToList();
	}
}
