﻿using UniRx;

namespace App.Model
{
    using Common;
    using Common.Message;
    using Registry;

    /// <summary>
    /// Model of a piece on the board.
    /// </summary>
    public class PieceModel
        : PlayerOwnedModelBase
        , IPieceModel
    {
        public ICardModel Card { get; }
        public EPieceType PieceType => Card.PieceType;
        public IReactiveProperty<Coord> Coord => _coord;
        public IReadOnlyReactiveProperty<int> Power => Card.Power;
        public IReadOnlyReactiveProperty<int> Health => Card.Health;
        public IReactiveProperty<bool> Dead => _dead;
        [Inject] public IBoardModel Board { get; set; }

        public PieceModel(IPlayerModel player, ICardModel card)
            : base(player)
        {
            Card = card;
            Health.Subscribe(h => Dead.Value = h <= 0).AddTo(this);
            Dead.Subscribe(dead => { if (dead) Died(); }).AddTo(this);
        }

        void Died()
        {
            Info($"{this} died");
            Board.Remove(this);
        }

        public IResponse Attack(IPieceModel defender)
        {
            var attack = defender.TakeDamage(this);
            if (attack.Failed)
                return attack;
            var defend = TakeDamage(defender);
            if (defend.Failed)
                return defend;
            if (defender.Dead.Value && !Dead.Value)
            {
                return Board.TryMovePiece(
                    new MovePiece(Player, this, defender.Coord.Value));
            }

            return Response.Ok;
        }

        public IResponse TakeDamage(IPieceModel attacker)
        {
            return Card.TakeDamage(attacker.Card);
        }

        public override string ToString()
        {
            return $"{Player}'s {PieceType} @{Coord} with {Power}/{Health}";
        }

        private readonly ReactiveProperty<Coord> _coord = new ReactiveProperty<Coord>();
        private readonly BoolReactiveProperty _dead = new BoolReactiveProperty(false);
    }
}
