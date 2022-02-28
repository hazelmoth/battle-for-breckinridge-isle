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
            HUDManager.HidePlaceArmyText();
            GameController.instance.StartCoroutine(TurnCoroutine(onTurnEnd));
        }

        private IEnumerator TurnCoroutine(Action onTurnEnd)
        {
            // Distribute armies, prioritizing most vulnerable tiles
            List<Vector2Int> heldTiles = GetHeldTiles();
            while (ArmiesToPlace > 0)
            {
                // Prioritize first defense, then expansion, then building the front
                GameController.instance.PlaceArmyIfAvailable(
                    this,
                    heldTiles
                        .OrderBy(i => -EvaluateVulnerability(i))
                        .ThenBy(i => -EvaluateExpansionOpportunities(i))
                        .ThenBy(i => -CountAdjacentEnemies(i))
                        .First());

                yield return new WaitForSeconds(TimePerAction);
            }

            // For each held tile with multiple armies, find a weaker enemy tile to attack
            foreach (Vector2Int heldTile in heldTiles)
            {
                if (GameController.instance.gameMap[heldTile].armies == 1) continue;
                if (GameController.instance.gameMap[heldTile].AvailableArmies == 0) continue;

                // This tile has armies available to move.

                // If an adjacent friendly tile is more vulnerable than this tile, move
                // armies to that tile until it is no more vulnerable than this tile.
                List<Vector2Int> adjacentFriendlyTiles =
                    GameController.instance.GetAdjacentTiles(heldTile)
                    .Where(t => GameController.instance.gameMap[t].owner == this)
                    .ToList();
                if (adjacentFriendlyTiles.Count > 0)
                {
                    Vector2Int weakestFriendlyTile = adjacentFriendlyTiles.OrderBy(t => -EvaluateVulnerability(t)).First();
                    while (EvaluateVulnerability(weakestFriendlyTile) > EvaluateVulnerability(heldTile) + 1
                           && GameController.instance.gameMap[heldTile].AvailableArmies > 0
                           && GetArmies(heldTile) > 1)
                    {
                        GameController.instance.MoveArmy(heldTile, weakestFriendlyTile);
                        yield return new WaitForSeconds(TimePerAction);
                    }
                }


                // If this tile has extra armies and we can attack an enemy tile, attack.

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
                    // As long as we have excess armies and the target is alive, attack
                    while (EvaluateSurplus(heldTile) > 1
                        && GetAvailableArmies(heldTile) > 1
                        && GetArmies(targetTile) > 0)
                    {
                        GameController.instance.LaunchAttack(heldTile, targetTile);
                        yield return new WaitForSeconds(TimePerAction);
                    }
                }

                if (EvaluateSurplus(heldTile) > 0)
                {
                    // We still have extra armies; let's look for new territory

                    if (GameController.instance.gameMap[heldTile].AvailableArmies == 0) continue;

                    // If this tile has no adjacent enemy tiles and some adjacent tiles
                    // are free, distribute excess to free adjacent tiles.
                    List<Vector2Int> freeAdjacentTiles = GameController.instance.GetAdjacentTiles(heldTile)
                        .Where(
                            t => GameController.instance.gameMap[t].owner == null
                                && GameController.instance.gameMap[t].type.id != "water")
                        .OrderBy(i => -EvaluateTile(i))
                        .ToList();

                    // Move into as many adjacent tiles as we can defend before running
                    // out of surplus.
                    foreach (Vector2Int adjacent in freeAdjacentTiles)
                    {
                        while (GetAvailableArmies(heldTile) > 0
                            && GetArmies(heldTile) > 1
                            && EvaluateSurplus(heldTile) > EvaluateSurplus(adjacent))
                        {
                            GameController.instance.MoveArmy(heldTile, adjacent);
                            yield return new WaitForSeconds(TimePerAction);
                            if (EvaluateSurplus(heldTile) == 0) break;
                        }
                    }
                }
            }
            onTurnEnd();
        }

        /// Returns the number of armies on the given tile.
        private int GetArmies(Vector2Int tile)
        {
            return GameController.instance.gameMap[tile].armies;
        }

        /// Returns the number of available armies on the tile.
        private int GetAvailableArmies(Vector2Int tile)
        {
            return GameController.instance.gameMap[tile].AvailableArmies;
        }

        private float EvaluateTile(Vector2Int tile)
        {
            if (GameController.instance.gameMap[tile].type.id == "water") return 0.1f;

            float result = 2 * GameController.instance.gameMap[tile].type.armyProduction
                + GameController.instance.gameMap[tile].type.attackBonus
                + GameController.instance.gameMap[tile].type.defenseBonus;
            return Math.Max(result, 0.01f);
        }

        /// Returns an estimate of how many armies the given tile needs to be defended.
        /// Always returns at least 1.
        private float EvaluateNeededArmies(Vector2Int tile)
        {
            if (!GameController.instance
                .GetAdjacentTiles(tile).Any(
                    t => GameController.instance.gameMap[t].owner != null
                        && GameController.instance.gameMap[t].owner != this))
            {
                // No enemy adjacent tiles.
                // If there are adjacent empty tiles, return the value of the tile.
                if (GameController.instance.GetAdjacentTiles(tile)
                    .Any(t => GameController.instance.gameMap[t].owner == null))
                {
                    return Math.Max(EvaluateTile(tile), 1);
                }

                // Otherwise, no need to defend.
                return 1;
            }

            // Number of adjacent enemy armies times 0.8 (times 1 + attack bonus)
            // plus this tile's production, minus the tile's defense bonus times its armies
            // divided by 2, plus 1 if there are any adjacent non-friendly tiles.
            float result = GameController.instance.GetAdjacentTiles(tile)
                .Where(t => GameController.instance.gameMap[t].owner != null
                    && GameController.instance.gameMap[t].owner != this
                    && GameController.instance.gameMap[t].armies > 0)
                .Sum(t => GameController.instance.gameMap[t].armies * 0.8f
                    * (1 + GameController.instance.gameMap[t].type.attackBonus))
                + GameController.instance.gameMap[tile].type.armyProduction
                - GameController.instance.gameMap[tile].type.defenseBonus
                * GameController.instance.gameMap[tile].armies
                / 2
                + (GameController.instance
                    .GetAdjacentTiles(tile).Any(t => GameController.instance.gameMap[t].owner != this) ? 1 : 0);

            return Math.Max(result, 1);
        }

        /// How many more armies the given tile needs to be properly defended.
        /// Zero if the tile has surplus armies.
        private float EvaluateVulnerability(Vector2Int tile)
        {
            return Math.Max(EvaluateNeededArmies(tile) - GetArmies(tile), 0);
        }

        /// How many more armies the given tile has than it needs to be properly defended.
        /// Returns 0 if the tile is vulnerable.
        private float EvaluateSurplus(Vector2Int tile)
        {
            if (GameController.instance.gameMap[tile].armies <= 1) return 0;
            return Math.Max(GameController.instance.gameMap[tile].armies - EvaluateNeededArmies(tile), 0);
        }

        /// Returns the number of non-friendly armies adjacent to the given tile.
        private int CountAdjacentEnemies(Vector2Int tile)
        {
            return GameController.instance.GetAdjacentTiles(tile)
                .Where(t => GameController.instance.gameMap[t].owner != null
                    && GameController.instance.gameMap[t].owner != this)
                .Sum(t => GameController.instance.gameMap[t].armies);
        }

        /// Returns the sum of the values of all adjacent uncontrolled non-water tiles,
        /// for which there is not yet an army to move there.
        private float EvaluateExpansionOpportunities(Vector2Int tile)
        {
            List<Vector2Int> tiles = GameController.instance.GetAdjacentTiles(tile)
                .Where(t => GameController.instance.gameMap[t].owner == null)
                .Where(t => GameController.instance.gameMap[t].type.id != "water")
                .ToList();
            int alreadyAvailable = Mathf.FloorToInt(EvaluateSurplus(tile));
            if (alreadyAvailable >= tiles.Count) return 0;
            return tiles.GetRange(alreadyAvailable, tiles.Count - alreadyAvailable).Sum(EvaluateTile);
        }
    }
}
