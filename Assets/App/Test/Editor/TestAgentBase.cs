﻿/*
 * using Dekuple.Agent;
using NUnit.Framework;

namespace App.Agent.Test
{
    using App;
    using Mock;
    using Mock.Agent;

    /// <summary>
    /// Common for all tests that use agents.
    ///
    /// Based on the common test base for models
    /// </summary>
    class TestAgentBase<TWhite, TBlack>
        : Model.Test.TestBaseModel
        where TBlack : IBlackPlayerAgent
        where TWhite : IWhitePlayerAgent
    {
        protected TestAgentBase()
        {
            LogSubject = this;
            LogPrefix = "AgentTest";
        }

        protected override void SetupTest()
        {
            base.SetupTest();

            _agency = new AgentRegistry();
            _agency.Bind<IBoardAgent, BoardAgent>(new BoardAgent(_board));
            _agency.Bind<IArbiterAgent, ArbiterAgent>(new ArbiterAgent(_arbiter));
            _agency.Bind<ICardAgent, CardAgent>();
            _agency.Bind<IDeckAgent, DeckAgent>();
            _agency.Bind<IHandAgent, HandAgent>();
            _agency.Bind<IPieceAgent, PieceAgent>();
            _agency.Bind<IWhitePlayerAgent, TWhite>();
            _agency.Bind<IBlackPlayerAgent, TBlack>();
            _agency.Bind<IEndTurnButtonAgent, EndTurnButtonAgent>();
            _agency.Resolve();

            _boardAgent = _agency.New<IBoardAgent>();
            _arbiterAgent = _agency.New<IArbiterAgent>();
            _whiteAgent = _agency.New<IWhitePlayerAgent>(_white);
            _blackAgent = _agency.New<IBlackPlayerAgent>(_black);
        }

        protected override void TearDownTest()
        {
            _boardAgent.Destroy();
            _arbiterAgent.Destroy();
            _blackAgent.Destroy();
            _whiteAgent.Destroy();

            base.TearDownTest();
        }

        protected AgentRegistry _agency;
        protected IBoardAgent _boardAgent;
        protected IPlayerAgent _whiteAgent;
        protected IPlayerAgent _blackAgent;
        protected IArbiterAgent _arbiterAgent;
    }
}
*/