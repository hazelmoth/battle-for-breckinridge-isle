using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnHandler : MonoBehaviour
{
	public int currentTurn;
	int currentPlayerIndex;
	static TurnHandler instance;

    // Start is called before the first frame update
    void Start()
    {
		instance = this;
		currentTurn = 1;
		currentPlayerIndex = 0;

		NeuralNetwork nn = new NeuralNetwork(2, 2, new int[] { 2, 2 });
		Debug.Log(nn.Calculate(new float[] { 1f, 2f })[0]);
    }

	public static int CurrentTurn => instance.currentTurn;

	public static Player GetCurrentPlayer ()
	{
		return GameController.instance.players[instance.currentPlayerIndex];
	}
	
	public static void ProgressTurn()
	{
		GameController.instance.ResetAllExpendedArmies();
		GameController.instance.EvaluateRemainingPlayers();

		if (GameController.instance.gameEnded)
			return;

		instance.currentPlayerIndex++;
		if (instance.currentPlayerIndex >= GameController.instance.players.Count)
			instance.currentPlayerIndex = 0;
		instance.currentTurn++;

		if (instance.currentTurn > 2)
			GameController.instance.CollectArmies(GetCurrentPlayer());

		if (GetCurrentPlayer().armiesToPlace > 0)
		{
			HUDManager.ShowPlaceArmyText(GetCurrentPlayer().armiesToPlace);
			InputHandler.instance.placingArmies = true;
		}
		HUDManager.SetPlayerIndicator(GetCurrentPlayer());
	}
}
