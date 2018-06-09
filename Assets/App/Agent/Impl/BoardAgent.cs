﻿using System;
using System.Collections;
using System.Collections.Generic;

using Flow;

namespace App.Agent
{
    using Common;
    using Model;

    public class BoardAgent
        : AgentBaseCoro<IBoardModel>
        , IBoardAgent
    {
        public BoardAgent(IBoardModel model)
            : base(model)
        {
        }

        protected override IEnumerator Next(IGenerator self)
        {
            yield break;
        }

        public void NewGame()
        {
            Model.NewGame();
            _idToPiece.Clear();
        }

        public void EndGame()
        {
        }

        public IFuture<IPieceAgent> At(Coord coord)
        {
            return New.Future(GetAgent(Model.At(coord)));
        }

        public ITransient NewGameAction()
        {
            // move all pieces back to deck, shuffle
            return null;
        }

        private IPieceAgent GetAgent(IPieceModel model)
        {
            if (model == null)
                return null;
            IPieceAgent piece;
            if (_idToPiece.TryGetValue(model.Id, out piece))
                return piece;
            return _idToPiece[model.Id] = Registry.New<IPieceAgent>(model);
        }

        private readonly Dictionary<Guid, IPieceAgent> _idToPiece = new Dictionary<Guid, IPieceAgent>();
    }
}
