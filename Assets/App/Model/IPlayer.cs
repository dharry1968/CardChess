﻿using System;
using System.Collections.Generic;

namespace App.Model
{
    /// <summary>
    /// Player in the game.
    /// Hopefully, these could be bots, or remote players as well
    /// as simple hotseat players at the same local device.
    /// </summary>
    public interface IPlayer : IModel, IOwner
    {
        #region Properties
        EColor Color { get; }
        int MaxMana { get; }
        int Mana { get; }
        int Health { get; }
        IHand Hand { get; }
        IDeck Deck { get; }
        ICardInstance King { get; }
        IEnumerable<ICardInstance> CardsOnBoard { get; }
        IEnumerable<ICardInstance> CardsInGraveyard { get; }
        #endregion

        #region Methods
        void NewGame();
        void ChangeMaxMana(int mana);
        void ChangeMana(int mana);
        void MockMakeHand();
        #endregion
    }
}
