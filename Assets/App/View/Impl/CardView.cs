﻿using App.Database;
using App.Service.Impl;

namespace App.View.Impl
{
    using UnityEngine;
    using UniRx;
    using Dekuple;
    using Dekuple.Agent;
    using Common;
    using Common.Message;
    using Agent;
    using Model;

    /// <inheritdoc cref="ICardView" />
    /// <summary>
    /// View of a card that is <b>not</b> on the Board. This includes the Hand, Deck, Graveyard.
    /// A view of a Card on the Board is a <see cref="IPieceView"/>.
    /// </summary>
    public class CardView
        : Draggable<ICardAgent>
        , ICardView
    {
        public TMPro.TextMeshProUGUI Mana;
        public TMPro.TextMeshProUGUI Health;
        public TMPro.TextMeshProUGUI Power;
        public AudioClip LeaveHandClip;
        public IPlayerView PlayerView { get; private set; }
        public IPlayerModel PlayerModel => PlayerView.Agent.Model;
        public new IReadOnlyReactiveProperty<ICardView> MouseOver => _mouseOver;

        public override bool IsValid
        {
            get
            {
                if (!base.IsValid) return false;
                if (Mana == null) return false;
                if (Health == null) return false;
                if (Power == null) return false;
                return true;
            }
        }

        private readonly ReactiveProperty<ICardView> _mouseOver = new ReactiveProperty<ICardView>();

        internal CardView()
        {
            // Verbosity = 100;
        }
        
        public override void SetAgent(IAgent agent)
        {
            var cardAgent = agent as ICardAgent;
            Assert.IsNotNull(cardAgent);
            base.SetAgent(cardAgent);

            PlayerView = ArbiterView.GetPlayerView(agent);
            AddMesh(cardAgent);
            AddCardSubscriptions();
        }

        private void AddCardSubscriptions()
        {
            MouseOver.Subscribe(v => _mouseOver.Value = v).AddTo(this);
            Agent.Power.Subscribe(p => Power.text = $"{p}").AddTo(this);
            Agent.Health.Subscribe(p => Health.text = $"{p}").AddTo(this);
            Agent.Model.ManaCost.Subscribe(p => Mana.text = $"{p}").AddTo(this);

            SquareOver.Subscribe(sq =>
            {
                if (sq != null)
                    BoardView.ShowSquares(Agent.Model, sq);
            }).AddTo(this);
        }

        private void AddMesh(ICardAgent cardAgent)
        {
            var root = Instantiate(cardAgent.Model.Template.MeshPrefab, transform);
            root.transform.SetY(0);
            var material = (Owner.Value as IPlayerModel)?.Color == EColor.Black
                ? BoardView.BlackMaterial
                : BoardView.WhiteMaterial;
            root.GetComponentInChildren<MeshRenderer>().material = material;
        }

        protected override bool MouseDown()
        {
            var inPlay = IsCurrentPlayer();
            if (inPlay)
                _AudioSource.PlayOneShot(LeaveHandClip);
            return inPlay;
        }

        protected override void MouseHover()
        {
            // TODO: popup info
        }

        protected override void MouseUp(IBoardView board, Coord coord)
        {
            Assert.IsTrue(IsValid && Agent.IsValid);
            Verbose(30, $"MouseUp: Requesting new {this} owned by {PlayerModel} @{coord}");
            PlayerAgent.PushRequest(new PlacePiece(PlayerModel, Agent.Model, coord), Response);
        }

        private void Response(IResponse response)
        {
            _Queue.RunToEnd();
            Verbose(10, $"CardViewPlaced {response.Request}, response {response.Type}:{response.Error}");
            if (response.Failed)
            {
                ReturnToStart();
                return;
            }

            Verbose(10, $"Removing {Agent.Model} from {PlayerModel.Hand}");
            PlayerModel.Hand.Remove(Agent.Model);
        }
    }
}
