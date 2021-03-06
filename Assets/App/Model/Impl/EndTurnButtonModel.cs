﻿namespace App.Model.Impl
{
    using System.Linq;
    using Dekuple;
    using Dekuple.Model;
    using UniRx;
    using Model;

    /// <summary>
    /// Logic for state of 'end-turn' button
    /// </summary>
    public class EndTurnButtonModel
        : ModelBase
        , IEndTurnButtonModel
    {
        public IReadOnlyReactiveProperty<bool> Interactive => _isInteractive;
        public IReadOnlyReactiveProperty<bool> PlayerHasOptions => _playerHasOptions;
        public IPlayerModel PlayerModel => Owner.Value as IPlayerModel;

        [Inject] public IBoardModel _board;
        [Inject] public IArbiterModel _arbiter;

        private readonly BoolReactiveProperty _isInteractive = new BoolReactiveProperty();
        private readonly BoolReactiveProperty _playerHasOptions = new BoolReactiveProperty();

        public EndTurnButtonModel(IOwner owner) : base(owner) { }

        public override void PrepareModels()
        {
            base.PrepareModels();
            
            _arbiter.LastResponse.CombineLatest(PlayerModel.Mana, (p, m) =>
            {
                if (_arbiter.CurrentPlayer.Value != PlayerModel)
                    return false;
                var canPlace = PlayerModel.Hand.Cards.Any(c => c.ManaCost.Value <= m);
                if (canPlace)
                    return true;
                return m > 0 && _board.Pieces.Where(SameOwner).Any(_board.CanMoveOrAttack);
            })
            .Subscribe(h => _playerHasOptions.Value = h).AddTo(this);

            _arbiter.CurrentPlayer.Subscribe(p => _isInteractive.Value = p == PlayerModel).AddTo(this);
        }
    }
}
