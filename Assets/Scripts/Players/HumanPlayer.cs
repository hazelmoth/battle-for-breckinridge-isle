﻿using System;
using UnityEngine;

namespace Players
{
    public class HumanPlayer : Player
    {
        public HumanPlayer(string nationName, Color color) : base(nationName, color, true, 1f) { }

        public override void BeginTurn(Action onTurnEnd)
        {
            // everything done via UI
        }
    }
}
