﻿namespace App.View.Impl
{
    using UnityEngine;
    using UniRx;
    using CoLib;
    using Dekuple;
    using Dekuple.Agent;
    using Dekuple.View.Impl;
    using Agent;

    /// <summary>
    /// View of a player's hand in the scene.
    /// </summary>
    public class HandView
        : ViewBase<IHandAgent>
        , IHandView
    {
        public Vector3 Offset;
        public Transform CardsRoot;
        public CardView CardViewPrefab;
        public BoardOverlayView BoardOverlay;
        public AudioClip MouseOverClip;

        private float _lastClipPlayed;
        private float _minTimeBetweenClips = 0.3f;
        private readonly ReactiveCollection<ICardView> _cards = new ReactiveCollection<ICardView>();

        public override bool IsValid
        {
            get
            {
#if !UNITY_EDITOR
                Assert.IsNotNull(CardsRoot);
                Assert.IsNotNull(CardViewPrefab);
                Assert.IsNotNull(BoardOverlay);
                foreach (var c in _cards)
                    Assert.IsTrue(c.IsValid);
#endif
                return true;
            }
        }

        public override void SetAgent(IAgent agent)
        {
            var handAgent = agent as IHandAgent;
            Assert.IsNotNull(handAgent);
            base.SetAgent(agent);
            Assert.IsNotNull(CardViewPrefab);
            Assert.IsNotNull(CardsRoot);

            Clear();
            BindHand(handAgent);
            Redraw();
        }

        private void BindHand(IHandAgent agent)
        {
            foreach (var card in agent.Cards)
                _cards.Add(CreateViewFromAgent(card));

            agent.Cards.ObserveAdd().Subscribe(Add);
            agent.Cards.ObserveRemove().Subscribe(Remove);
        }

        private ICardView CreateViewFromAgent(ICardAgent agent)
        {
            var cardView = ViewRegistry.FromPrefab<ICardView>(CardViewPrefab, agent);
            cardView.MouseOver.Subscribe(CardMouseOver);
            cardView.SetAgent(agent);
            var tr = cardView.GameObject.transform;
            tr.SetParent(CardsRoot);
            tr.localScale = Vector3.one;
            tr.localPosition = new Vector3(-1, -1, 5);
            return cardView;
        }

        private void CardMouseOver(ICardView card)
        {
            if (card == null)
                return;
            
            if (Time.time - _lastClipPlayed > _minTimeBetweenClips)
            {
                _AudioSource.PlayOneShot(MouseOverClip);
                _lastClipPlayed = Time.time;
            }
            
            Verbose(20, $"MouseOver {card.Agent.Model}");
        }

        [ContextMenu("HandView-Clear")]
        public void Clear()
        {
            transform.ForEach<ICardView>(c => c.Destroy());
        }

        private void Add(CollectionAddEvent<ICardAgent> add)
        {
            Verbose(5, $"HandView: Add {add.Value} @{add.Index}");
            _cards.Insert(add.Index, CreateViewFromAgent(add.Value));
            Redraw();
        }

        private void Remove(CollectionRemoveEvent<ICardAgent> remove)
        {
            Verbose(5, $"HandView: Remove {remove.Value} @{remove.Index}");
            var view = _cards[remove.Index];
            _cards.RemoveAt(remove.Index);
            view.Destroy();
            Redraw();
        }

        [ContextMenu("Redraw")]
        private void Redraw()
        {
            _Queue.RunToEnd();

            var n = 0;
            foreach (var card in _cards)
            {
                Assert.IsTrue(card.IsValid);
                card.GameObject.name = $"{card.Agent.Model}";
                _Queue.Sequence(
                    Cmd.MoveTo(card.GameObject, n * Offset, 0.1, Ease.Smooth(), true)
                );
                ++n;
            }
        }
    }
}
