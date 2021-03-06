﻿namespace App.Agent
{
    using System.Linq;
    using System.Text;
    using UniRx;
    using Flow;
    using Dekuple;
    using Dekuple.Agent;
    using Common;
    using Model;

    /// <summary>
    /// Active agent for dealing with the BoardModel. This will be eventually networked,
    /// and the Agent will be local and the Model remote (on server).
    /// </summary>
    public class BoardAgent
        : AgentBaseCoro<IBoardModel>
        , IBoardAgent
    {
        public IReadOnlyReactiveProperty<int> Width => _width;
        public IReadOnlyReactiveProperty<int> Height => _height;
        public IReadOnlyReactiveCollection<IPieceAgent> Pieces => _pieces;

        private readonly ReactiveCollection<IPieceAgent> _pieces = new ReactiveCollection<IPieceAgent>();
        private readonly IntReactiveProperty _width = new IntReactiveProperty(8);
        private readonly IntReactiveProperty _height = new IntReactiveProperty(8);

        public override bool IsValid
        {
            get
            {
                Verbose(10, "Test Valid BoardAgent");
                if (!base.IsValid)
                    return false;
                Assert.AreEqual(_pieces.Count, Model.Pieces.Count);
                var n = 0;
                foreach (var p in _pieces)
                {
                    Assert.AreSame(p.Model, Model.Pieces.ElementAt(n));
                    ++n;
                }
                return true;
            }
        }

        public BoardAgent(IBoardModel model)
            : base(model)
        {
            Assert.IsNotNull(model);
            model.Pieces.ObserveAdd().Subscribe(PieceAdded);
            model.Pieces.ObserveRemove().Subscribe(PieceRemoved);
        }

        public string Print()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"BoardAgent: {_pieces.Count} pieces:");
            foreach (var p in _pieces)
            {
                sb.AppendLine($"\t{p.Coord.Value} -> {p.Model}");
            }
            return sb.ToString();
        }

        public IBoardModel TypedModel => Model as IBoardModel;

        public void StartGame()
        {
            Model.StartGame();
            _pieces.Clear();
        }

        public void EndGame()
        {
            Info($"BoardAgent EndGame");
            _pieces.Clear();
        }

        public IResponse Move(IPieceAgent agent, Coord coord)
            => Model.Move(agent.Model, coord);

        public IResponse Remove(IPieceAgent agent)
            => Model.Remove(agent.Model);

        public IResponse Add(IPieceAgent agent) 
            => Model.Add(agent.Model);

        public IPieceAgent At(Coord coord)
            => _pieces.FirstOrDefault(p => p.Coord.Value == coord);

        public ITransient PerformNewGame()
        {
            _pieces.Clear();
            return null;
        }

        private void PieceAdded(CollectionAddEvent<IPieceModel> add)
        {
            var pieceAgent = Registry.Get<IPieceAgent>(add.Value);
            pieceAgent.SetOwner(add.Value.Owner.Value);
            _pieces.Insert(add.Index, pieceAgent);
        }

        private void PieceRemoved(CollectionRemoveEvent<IPieceModel> remove)
            => _pieces.RemoveAt(remove.Index);
    }
}

