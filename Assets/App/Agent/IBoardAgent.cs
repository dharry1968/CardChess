﻿namespace App.Agent
{
    using Dekuple;
    using Dekuple.Agent;
    using UniRx;
    using Flow;
    using Common;
    using Model;

    /// <summary>
    /// An agent that acts on behalf of a model board.
    /// </summary>
    public interface IBoardAgent
        : IGameAgent<IBoardModel>
        , IPrintable
    {
        IReadOnlyReactiveProperty<int> Width { get; }
        IReadOnlyReactiveProperty<int> Height { get; }
        IReadOnlyReactiveCollection<IPieceAgent> Pieces { get; }

        ITransient PerformNewGame();
        IPieceAgent At(Coord coord);

        IResponse Add(IPieceAgent agent);
        IResponse Move(IPieceAgent agent, Coord coord);
        IResponse Remove(IPieceAgent agent);
    }
}
