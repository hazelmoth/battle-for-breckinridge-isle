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
        public BasicAIPlayer(string nationName, Color color, float armyProdMultiplier)
            : base(nationName, color, false, armyProdMultiplier) { }

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
                GameController.instance.PlaceArmyIfAvailable(this, heldTiles.WeightedRandom(evaluateTile));
                yield return new WaitForSeconds(TimePerAction);
            }

            // For each held tile with multiple armies, find a weaker enemy tile to attack
            foreach (Vector2Int heldTile in heldTiles)
            {
                if (GameController.instance.gameMap[heldTile].armies == 1) continue;
                if (GameController.instance.gameMap[heldTile].AvailableArmies == 0) continue;

                List<Vector2Int> vulnerableEnemyTiles = GameController.instance.GetAdjacentTiles(heldTile)
                    .Where(
                        t => GameController.instance.gameMap[t].owner != null
                            && GameController.instance.gameMap[t].owner != this
                            && GameController.instance.gameMap[t].armies > 0
                            && (GameController.instance.gameMap[t].armies
                                < GameController.instance.gameMap[heldTile].armies
                            || GameController.instance.gameMap[t].type.id == "water"))
                    .ToList();
                if (vulnerableEnemyTiles.Count > 0)
                {
                    // Attack tile with fewest armies
                    Vector2Int targetTile =
                        vulnerableEnemyTiles.OrderBy(t => GameController.instance.gameMap[t].armies).First();
                    // As long as we have at more than 1 army and the target is alive, attack
                    while (GameController.instance.gameMap[heldTile].AvailableArmies > 1
                        && GameController.instance.gameMap[targetTile].armies > 0)
                    {
                        GameController.instance.LaunchAttack(heldTile, targetTile);
                        yield return new WaitForSeconds(TimePerAction);
                    }
                }
                else
                {
                    // No adjacent enemies

                    // If there are no adjacent enemy tiles, and some adjacent friendly
                    // tiles are vulnerable to adjacent enemy tiles, move to defend
                    List<Vector2Int> adjacentVulnerableTiles = GameController.instance.GetAdjacentTiles(heldTile)
                        .Where(t => GameController.instance.gameMap[t].owner == this
                            && GameController.instance.GetAdjacentTiles(t)
                                .Any(t2 => GameController.instance.gameMap[t2].owner != null
                                    && GameController.instance.gameMap[t2].owner != this
                                    && GameController.instance.gameMap[t2].armies > 0
                                    && (GameController.instance.gameMap[t2].armies
                                        > GameController.instance.gameMap[t].armies
                                    || GameController.instance.gameMap[t].type.id == "water")))
                        .ToList();
                    while (adjacentVulnerableTiles.Count > 0 && GameController.instance.gameMap[heldTile].AvailableArmies > 1)
                    {
                        // Move to the weakest adjacent tile
                        Vector2Int targetTile =
                            adjacentVulnerableTiles.OrderBy(t => GameController.instance.gameMap[t].armies).First();
                        GameController.instance.MoveArmy(heldTile, targetTile);
                        yield return new WaitForSeconds(TimePerAction);
                    }

                    if (GameController.instance.gameMap[heldTile].AvailableArmies == 0) continue;

                    // If this tile has no adjacent enemy tiles and some adjacent tiles
                    // are free, distribute all but one army to free adjacent tiles
                    List<Vector2Int> freeAdjacentTiles = GameController.instance.GetAdjacentTiles(heldTile)
                        .Where(
                            t => GameController.instance.gameMap[t].owner == null
                                && GameController.instance.gameMap[t].type.id != "water")
                        .ToList();

                    if (freeAdjacentTiles.Count > 0)
                        while (GameController.instance.gameMap[heldTile].AvailableArmies > 1) {
                            GameController.instance.MoveArmy(
                                heldTile,
                                freeAdjacentTiles.WeightedRandom(evaluateTile));
                            yield return new WaitForSeconds(TimePerAction);
                        }
                }
            }

            onTurnEnd();
        }

        private float evaluateTile(Vector2Int tile)
        {
            return 2 * GameController.instance.gameMap[tile].type.armyProduction
                + GameController.instance.gameMap[tile].type.attackBonus
                + GameController.instance.gameMap[tile].type.defenseBonus;
        }
    }
}
