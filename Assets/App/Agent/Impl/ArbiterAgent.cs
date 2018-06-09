﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Flow;
using UniRx;

namespace App
{
    using Common.Message;
    using Common;
    using Registry;
    using Agent;
    using Model;

    /// <summary>
    /// The 'Adjudicator' of the game: controls the sequencing of the events
    /// but not all the rules.
    ///
    /// Responsiblity for enforcing the rules of the game are shared with
    /// the Board- and Card-Agents and Models.
    /// </summary>
    public class ArbiterAgent
        : AgentBaseCoro<IArbiterModel>
        , IArbiterAgent
    {
        public IReadOnlyReactiveProperty<IPlayerAgent> PlayerAgent => _playerAgent;
        public IReadOnlyReactiveProperty<IPlayerModel> PlayerModel { get; private set; }
        [Inject] public IBoardAgent BoardAgent { get; set; }
        public IPlayerAgent WhitePlayerAgent => _players[0];
        public IPlayerAgent BlackPlayerAgent => _players[1];

        public ArbiterAgent(IArbiterModel model)
            : base(model)
        {
            Verbose(50, $"{this} created");
        }

        public void Step()
        {
            Verbose(20, $"Step: {Kernel.StepNumber}");
            Kernel.Step();
        }

        public ITransient PrepareGame(IPlayerAgent p0, IPlayerAgent p1)
        {
            Assert.IsNotNull(p0);
            Assert.IsNotNull(p1);

            Model.PrepareGame(p0.Model, p1.Model);

            _players = new List<IPlayerAgent> {p0, p1};
            _currentPlayerIndex.Subscribe(n => _playerAgent.Value = _players[n]);
            _currentPlayerIndex.Value = 0;
            PlayerModel = PlayerAgent.Select(x => x.Model).DistinctUntilChanged().ToReactiveProperty();

            // TODO: do some animations etc
            return null;
        }

        private ITransient NewGameWork()
        {
            return null;
        }

        public void StartGame()
        {
            Info($"{this} StartGame");
            _Node.Add(GameLoop());
            //_Node.Add(GameLoop());
            //_Node.Add(
            //    New.Sequence(
            //        New.Barrier(
            //            NewGameWork(),
            //            Board.NewGameAction()
            //        ).Named("NewGame"),
            //        New.Barrier(
            //            _players.Select(p => p.StartGame())
            //        ).Named("PlayersStartGame")
            //    ).Named("Setup"),
            //    New.Log("Entering main game loop..."),
            //    GameLoop()
            //);
        }

        private IGenerator StartGameCoro()
        {
            var rejectTimeOut = TimeSpan.FromSeconds(Parameters.MulliganTimer);
            var kingPlaceTimeOut = TimeSpan.FromSeconds(Parameters.PlaceKingTimer);

            return New.Barrier(
                New.Sequence(
                    New.Barrier(
                        _players.Select(p => p.StartGame())
                    ).Named("InitGame"),
                    New.Barrier(
                        _players.Select(p => p.DrawInitialCards())
                    ).Named("DealCards")
                ),
                ArbitrateFutures(
                    rejectTimeOut,
                    _players.Select(p => p.Mulligan())
                ).Named("Mulligan"),
                ArbitrateFutures(
                    kingPlaceTimeOut,
                    _players.Select(p => p.PlaceKing())
                ).Named("PlaceKings")
            ).Named("StartGame");
        }

        public ITransient GameLoop()
        {
            return New.Sequence(
                //StartGameCoro(),
                New.While(() => Model.GameState.Value != EGameState.Completed,
                    New.Coroutine(PlayerTurn).Named("Turn")
                ).Named("GameLoop"),
                New.Coroutine(EndGame).Named("EndGame")
            ).Named("Done");
        }

        private IEnumerator PlayerTurn(IGenerator self)
        {
            Assert.AreSame(PlayerModel.Value, Model.CurrentPlayer.Value);

            PlayerModel.Value.StartTurn();

            var timeOut = Parameters.GameTurnTimer;
            var timeStart = Kernel.Time.Now;

            // player can make as many valid actions as he can during his turn
            while (true)
            {
                var request = PlayerAgent.Value.NextRequest(timeOut);
                if (request == null)
                {
                    Warn($"{PlayerModel} passed");
                    break;
                }
                yield return self.After(request);

                if (request.HasTimedOut)
                {
                    Warn($"{PlayerModel} timed-out");
                    yield return self.After(New.Coroutine(PlayerTimedOut));
                    break;
                }
                if (!request.Available)
                    Warn($"{PlayerModel} didn't make a request");

                // do the arbitration before we test for time out
                var response = Model.Arbitrate(request.Value);
                if (response.Failed)
                    Warn($"Request {request.Value} failed for {PlayerModel}");

                var now = Kernel.Time.Now;
                var dt = (float)(now - timeStart).TotalSeconds;
                if (dt < 1.0f/60.0f)    // give them a 60Hz frame of grace
                {
                    Warn($"{PlayerModel} ran out of time for turn");
                    break;
                }

                timeStart = now;
                timeOut -= dt;
            }
        }

        private IEnumerator PlayerTimedOut(IGenerator arg)
        {
            Warn($"{PlayerModel} TimedOut");
            yield break;
        }

        private IEnumerator EndGame(IGenerator self)
        {
            Info("Game Ended");
            yield break;
        }

        /// <summary>
        /// Make a TimedBarrier that contains a collection of futures.
        /// When the barrier is completed, perform an act on each future that
        /// was in the barrier.
        /// </summary>
        private IGenerator TimedBarrierOfFutures<T>(
            TimeSpan timeOut,
            IEnumerable<IFuture<T>> futures,
            Action<IFuture<T>> act)
        {
            return New.TimedBarrier(timeOut, futures).ForEach(act);
        }

        /// <summary>
        /// Make a TimedBarrier that contains a collection of future IRequests.
        /// When the barrier is completed, pass the value of each available request to
        /// the Arbiter.
        /// </summary>
        private IGenerator ArbitrateFutures<T>(
            TimeSpan timeOut,
            IEnumerable<IFuture<T>> futures,
            Action<IFuture<T>> onUnavailable = null)
            where T : IRequest
        {
            return TimedBarrierOfFutures(
                timeOut,
                futures,
                f =>
                {
                    if (f.Available)
                        Model.Arbitrate(f.Value);
                    else
                        onUnavailable?.Invoke(f);
                }
            );
        }

        protected override IEnumerator Next(IGenerator self)
        {
            // TODO: general game backround animation, music loops etc
            yield return null;
        }

        private List<IPlayerAgent> _players = new List<IPlayerAgent>();
        private readonly IntReactiveProperty _currentPlayerIndex = new IntReactiveProperty(0);
        private readonly ReactiveProperty<IPlayerAgent> _playerAgent = new ReactiveProperty<IPlayerAgent>();
    }
}
