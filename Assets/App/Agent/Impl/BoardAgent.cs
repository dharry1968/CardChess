﻿using System;
using System.Collections;
using App.Model;
using Flow;

namespace App.Agent
{
    using Common;

    public class BoardAgent
        : AgentBaseCoro<Model.IBoardModel>
        , IBoardAgent
    {
        public BoardAgent(IBoardModel model) : base(model)
        {
        }

        protected override IEnumerator Next(IGenerator self)
        {
            yield break;
        }

        public void NewGame()
        {
            Model.NewGame();
        }

        public IPieceAgent At(Coord coord)
        {
            return null;
            //return Model.At(coord);
        }

    }
}
