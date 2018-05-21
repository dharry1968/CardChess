﻿using System.Collections.Generic;

namespace App.Model
{
    using Common;

    /// <summary>
    /// A Persistent user.
    /// </summary>
    public interface IUserModel : IModel
    {
        #region Properties
        string Handle { get; }
        string Email { get; }

        IDictionary<CardCollectionDesc, IList<ICardTemplate>> Decks { get; }
        IEnumerable<ICardTemplate> AllCards { get; }
        IGameHistory GameHistory { get; }
        #endregion
    }
}