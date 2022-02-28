using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Players
{
    public class BasicAIPlayer : Player
    {
        private const float TimePerAction = 0.3f;
        public BasicAIPlayer(string nationName, Color color) : base(nationName, color, false) { }

        public override void BeginTurn(Action onTurnEnd)
        {
            GameController.instance.StartCoroutine(TurnCoroutine(onTurnEnd));
        }

        private IEnumerator TurnCoroutine(Action onTurnEnd)
        {
            // Randomly distribute armies
            List<Vector2Int> heldTiles = GetHeldTiles();
            while (ArmiesToPlace > 0)
            {
                GameController.instance.PlaceArmyIfAvailable(this, heldTiles.PickRandom());
                yield return new WaitForSeconds(TimePerAction);
            }

            // For each held tile with multiple armies, find a weaker enemy tile to attack
            foreach (Vector2Int heldTile in heldTiles)
            {
                if (GameController.instance.gameMap[heldTile].armies == 1) continue;

                List<Vector2Int> vulnerableEnemyTiles = GameController.instance.GetAdjacentTiles(heldTile)
                    .Where(
                        t => GameController.instance.gameMap[t].owner != null
                            && GameController.instance.gameMap[t].owner != this
                            && GameController.instance.gameMap[t].armies > 0
                            && GameController.instance.gameMap[t].armies
                                < GameController.instance.gameMap[heldTile].armies)
                    .ToList();
                if (vulnerableEnemyTiles.Count > 0)
                {
                    // Attack tile with fewest armies
                    Vector2Int targetTile =
                        vulnerableEnemyTiles.OrderBy(t => GameController.instance.gameMap[t].armies).First();
                    // As long as we have at more than 1 army and the target is alive, attack
                    while (GameController.instance.gameMap[heldTile].armies > 1 && GameController.instance.gameMap[targetTile].armies > 0)
                    {
                        GameController.instance.LaunchAttack(heldTile, targetTile);
                        yield return new WaitForSeconds(TimePerAction);
                    }
                }
                else
                {
                    // todo move armies towards vulnerable fronts

                    // If this tile has no adjacent enemy tiles and some adjacent tiles
                    // are free, distribute all but one army to free adjacent tiles
                    List<Vector2Int> freeAdjacentTiles = GameController.instance.GetAdjacentTiles(heldTile)
                        .Where(
                            t => GameController.instance.gameMap[t].owner == null
                                && GameController.instance.gameMap[t].type.id != "water")
                        .ToList();

                    if (freeAdjacentTiles.Count > 0)
                        while (GameController.instance.gameMap[heldTile].armies > 1) {
                            GameController.instance.MoveArmies(heldTile, freeAdjacentTiles.PickRandom());
                            yield return new WaitForSeconds(TimePerAction);
                        }
                }
            }

            onTurnEnd();
        }
    }
}
