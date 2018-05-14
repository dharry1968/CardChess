﻿using System;
using System.Collections;
using System.Linq;
using App.Model;
using Flow;
using UnityEngine.Assertions;

namespace App.Main
{
    using Agent;
    using Action;

    /// <inheritdoc />
    /// <summary>
    /// The 'umpire' of the game: enforces all the rules.
    /// </summary>
    public class Arbiter : App.Logger
    {
        public static Arbiter Instance;
        public static IKernel Kernel;
        public static INode Root => Kernel.Root;
        public static IFactory New => Kernel.Factory;

        public IPlayer WhitePlayer => _players[0];
        public IPlayer BlackPlayer => _players[1];

        public IPlayer CurrentPlayer => _players[_currentPlayer];
        public IBoard Board => _board;

        public Arbiter()
        {
            //Assert.IsNull(Instance);
            Instance = this;
            Kernel = Create.Kernel();
        }

        public IEntity<TModel, TAgent> NewEntity<TModel, A0, A1, TAgent>(A0 a0, A1 a1)
            where TModel : class, Model.IModel, ICreateWith<A0, A1>, new()
            where TAgent : class, Agent.IAgent<TModel>, new()
        {
            var model = NewModel<TModel, A0, A1>(a0, a1);
            var agent = NewAgent<TAgent, TModel>(model);
            var entity = new Entity<TModel, TAgent>();
            entity.Create(model, agent);
            return entity;
        }

        public TModel NewModel<TModel>()
            where TModel : class, ICreateWith, new()
        {
            var model = new TModel();
            if (!model.Create())
            {
                Error("Failed to create Model {0}", typeof(TModel));
                return null;
            }
            return model;
        }

        public TModel NewModel<TModel, A0>(A0 a0)
            where TModel : class, ICreateWith<A0>, new()
        {
            var model = new TModel();
            if (!model.Create(a0))
            {
                Error("Failed to create Model {0} with arg {1}", typeof(TModel), a0);
                return null;
            }
            return model;
        }

        public TModel NewModel<TModel, A0, A1>(A0 a0, A1 a1)
            where TModel: class, ICreateWith<A0, A1>, new()
        {
            var model = new TModel();
            if (!model.Create(a0, a1))
            {
                Error("Failed to create Model {0} with args {1}, {2}", typeof(TModel), a0, a1);
                return null;
            }

            return model;
        }

        /// <summary>
        /// Make a new Agent that represents a Model.
        /// </summary>
        public  TAgent NewAgent<TAgent, TModel>(TModel model)
            where TModel : class, Model.IModel
            where TAgent : class, Agent.IAgent<TModel>, new()
        {
            var agent = new TAgent();
            if (!agent.Create(model))
            {
                Error("Failed to create Agent {0} for Model {1}", typeof(TAgent), typeof(TModel));
                return null;
            }
            agent.Arbiter = this;

            return agent;
        }

        public void SetPlayers(IPlayer p0, IPlayer p1)
        {
            Assert.IsNotNull(p0);
            Assert.IsNotNull(p1);
            _players[0] = p0;
            _players[1] = p1;
        }

        public void Setup(IBoard board, IPlayer p0, IPlayer p1)
        {
            _board = board;
            SetPlayers(p0, p1);
            NewGame();
        }

        public void NewGame()
        {
            Assert.IsNotNull(Board);
            Assert.IsNotNull(WhitePlayer);
            Assert.IsNotNull(BlackPlayer);

            Info("New Game");

            _currentPlayer = 0;
            _turnNumber = 0;

            Board.NewGame();
            foreach (var player in _players)
            {
                Assert.IsNotNull(player);
                player.NewGame();
            }
        }

        public void Step()
        {
            Info($"Step: {Kernel.StepNumber}");
            Kernel.Step();
        }

        public void StartGame()
        {
            Info("Game Started");

            Root.Add(
                New.Sequence(
                    New.Coroutine(StartGame).SetName("StartGame"),
                    New.Log("Main game sequence begins"),
                    New.While(() => !_gameOver),
                        New.Coroutine(PlayerTurn).SetName($"Turn {_turnNumber}"),
                    New.Coroutine(EndGame).SetName("EndGame")
                )
            );

            Info(Root);
        }

        IEnumerator StartGame(IGenerator self)
        {
            yield return self.After(
                New.Sequence(
                    New.Barrier(
                        WhitePlayer.StartGame(),
                        BlackPlayer.StartGame()
                    ),
                    New.Barrier(
                        WhitePlayer.DeliverCards(),
                        BlackPlayer.DeliverCards()
                    ),
                    New.Barrier(
                        New.TimedBarrier(
                            TimeSpan.FromSeconds(20),
                            WhitePlayer.HasAcceptedCards(),
                            BlackPlayer.HasAcceptedCards()
                        ),
                        New.TimedBarrier(
                            TimeSpan.FromSeconds(20),
                            WhitePlayer.HasPlacedKing(),
                            BlackPlayer.HasPlacedKing()
                        )
                    )
                )
            );
        }

        IEnumerator EndGame(IGenerator self)
        {
            Info("Game Ended");
            yield break;
        }

        IEnumerator PlayerTurn(IGenerator self)
        {
            yield return null;
            ++_turnNumber;
            var player = CurrentPlayer;
            Info($"Start turn {_turnNumber} for {player}");

            yield return self.After(player.ChangeMaxMana(1));

            SaveState();
            var timeOut = 60;

            while (true)
            {
                // the options a player has
                var playCard = player.PlayCard();
                var movePiece = player.MovePiece();
                var pass = player.Pass();

                var trigger = New.TimedTrigger(
                    TimeSpan.FromSeconds(timeOut),
                    playCard,
                    movePiece,
                    pass);

                trigger.TimedOut += timed =>
                {
                    Info($"Player {CurrentPlayer.Color} timed out. Should do something");
                    throw new NotImplementedException();
                };

                // wait for player to make a move
                yield return self.After(trigger);
                if (trigger.HasTimedOut || trigger.Reason == pass)
                {
                    // timed out, next player's turn
                    yield return self.After(PlayerTimedOut(player));
                    break;
                }

                SaveState();
                if (trigger.Reason == playCard)
                {
                    var canPlay = New.Future<bool>();
                    yield return self.After(TestCanPlayCard(player, playCard.Value, canPlay));
                    if (canPlay.Available && canPlay.Value)
                    {
                        yield return self.After(PerformPlayCard(playCard.Value));
                        break;
                    }
                    timeOut = trigger.Timer.TimeRemaining.Seconds;
                    continue;
                }

                if (trigger.Reason == movePiece)
                {
                    var canPlay = New.Future<bool>();
                    yield return self.After(TestCanMovePiece(player, movePiece.Value, canPlay));
                    if (canPlay.Available && canPlay.Value)
                    {
                        yield return self.After(PerformMovePiece(movePiece.Value));
                        continue;
                    }
                    timeOut = trigger.Timer.TimeRemaining.Seconds;
                    continue;
                }
            }

            _gameOver = CurrentPlayer.Health <= 0;
        }

        void SaveState()
        {
            // TODO: Save state of the board, and both players' hands
            Info("SaveState");
        }

        void RestoreState()
        {
            Info("RestoreState");
            // TODO: Restore state of the board, and both players' hands
        }

        bool CanPlayCard(IPlayer player, PlayCard playCard)
        {
            // TODO: Determine if the given action is valid
            return false;
        }

        bool CanMovePiece(IPlayer player, MovePiece move)
        {
            // TODO: determine if player can perform the given move
            return false;
        }

        // wrappers to create coroutines
        IGenerator PlayerLost(IPlayer player) { return New.Coroutine(PlayerLostCoro, player); }
        IGenerator PlayerTimedOut(IPlayer player) { return New.Coroutine(PlayerTimedOutCoro, player); }
        IGenerator TestCanPlayCard(IPlayer player, PlayCard play, IFuture<bool> future) { return New.Coroutine(TestCanPlayCardCoro, player, play, future); }
        IGenerator TestCanMovePiece(IPlayer player, MovePiece move, IFuture<bool> future) { return New.Coroutine(TestCanMovePieceCoro, player, move, future); }
        IGenerator PerformPlayCard(PlayCard playCard) { return New.Coroutine(PlayCardCoro, playCard); }
        IGenerator PerformMovePiece(MovePiece move) { return New.Coroutine(MovePieceCoro, move); }

        IEnumerator PlayerLostCoro(IGenerator self, IPlayer loser)
        {
            // TODO: show player lost sequence
            Info("Player {0} lost", loser);
            //yield return self.After(_view.PlayerLost(loser));
            yield break;
        }

        IEnumerator PlayerTimedOutCoro(IGenerator self, IPlayer player)
        {
            Info("Player {0} timedout", player);
            //yield return self.After(_view.PlayerTimedOut(player));
            yield break;
        }

        IEnumerator TestCanPlayCardCoro(IGenerator self, IPlayer player,
            PlayCard playCard, IFuture<bool> canPlay)
        {
            var current = Board.Model.At(playCard.Coord);
            if (current == null)
            {
                Info("Card play {0} for player {1} is VALID", playCard, player.Color);
                canPlay.Value = true;
                yield break;
            }

            Info("Card play {0} for player {1} is INVALID", playCard, player.Color);
            canPlay.Value = false;
        }

        IEnumerator TestCanMovePieceCoro(IGenerator self, IPlayer player, MovePiece move, IFuture<bool> canMove)
        {
            // TODO: if card can't be moved, show why
            Info("Move {0} is invalid");
            //yield return self.After(_view.InvalidMove(move));
            canMove.Value = false;
            yield break;
        }

        IEnumerator PlayCardCoro(IGenerator self, PlayCard playCard)
        {
            // TODO: play the card
            Info("PlayCard: {0}", playCard);
            //yield return self.After(_view.PlayCard(playCard));
            yield break;
        }

        IEnumerator MovePieceCoro(IGenerator self, MovePiece move)
        {
            Info("Move: {0}", move);
            //yield return self.After(_view.MovePiece(move));
            yield break;
        }

        public ICardInstance NewCardAgent(Model.ICardTemplate template, Model.IOwner owner)
        {
            var cardInstance = Database.CardTemplates.New(template.Id, owner);
            return NewAgent<CardInstance, Model.ICardInstance>(cardInstance);
        }

        public Agent.ICardInstance NewCardAgent(Model.ECardType type, Model.IOwner owner)
        {
            var template = Database.CardTemplates.OfType(type).FirstOrDefault();
            return template == null ? null : NewCardAgent(template, owner);
        }

        public Model.ICardInstance NewCardModel(Model.ICardTemplate tmpl, IOwner owner)
        {
            return new Model.CardInstance(tmpl, owner);
        }

        #region Private Fields

        private readonly Agent.IPlayer[] _players = new Agent.IPlayer[2];
        private int _currentPlayer;
        private ICoroutine _playerTimerCountdown;
        private Agent.IBoard _board;
        private int _turnNumber;
        private bool _gameOver;

        #endregion

        public static bool CanPlaceKing(Player player, Coord coord)
        {
            return true;
        }
    }
}

