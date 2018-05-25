﻿using System;
using System.Collections.Generic;
using App.Action;
using App.Common;

namespace App.Model.Test
{
    public interface IBlackPlayer : IPlayerModel { }
    public interface IWhitePlayer : IPlayerModel { }

    class BlackPlayer
        : MockPlayerBase, IBlackPlayer
    {
        public BlackPlayer(EColor color)
            : base(color)
        {
        }

        protected override void CreateActionList()
        {
            _requests = new List<Func<IRequest>>()
            {
                () => new AcceptCards(this),
                () => new PlayCard(this, King, new Coord(4, 5)),

                () =>
                {
                    var peon = GetACardPiece(EPieceType.Peon);
                    return new PlayCard(this, peon, new Coord(3, 3));
                },
                () => _pass,
            };
        }
    }
}
