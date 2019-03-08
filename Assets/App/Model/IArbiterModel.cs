﻿using App.Common;
using Dekuple;
using Dekuple.Model;
using UniRx;

namespace App.Model
{
    /// <summary>
    /// The referee for the game.
    /// </summary>
    public interface IArbiterModel
        : IModel
    {
        IBoardModel Board { get; }
        IReactiveProperty<int> TurnNumber { get; }
        IReadOnlyReactiveProperty<EGameState> GameState { get; }
        IReadOnlyReactiveProperty<IPlayerModel> CurrentPlayer { get; }
        IReadOnlyReactiveProperty<IResponse> LastResponse { get; }

        void StartGame();
        void PrepareGame(IPlayerModel white, IPlayerModel black);
        IResponse Arbitrate(IGameRequest request);
        void EndTurn();
    }
}
