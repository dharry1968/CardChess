﻿// event not used

using Dekuple;

#pragma warning disable 67

namespace App.Model
{
    using UnityEngine;
    using UniRx;
    using Dekuple.Model;
    using Common;
    using Common.Message;

    public class CardModel
        : ModelBase
        , ICardModel
    {
        public ICardTemplate Template { get; }
        public ECardType Type => Template.Type;
        public EPieceType PieceType => Template.PieceType;
        public GameObject MeshObject { get; set; }
        public string Description => Template.FlavourText;
        public EColor Color => Player.Value.Color;

        public IReactiveProperty<IPlayerModel> Player => _player;
        public IReadOnlyReactiveProperty<int> ManaCost => _manaCost;
        public IReadOnlyReactiveProperty<int> Power => _power;
        public IReadOnlyReactiveProperty<int> Health => _health;
        public IReadOnlyReactiveProperty<bool> Dead => _dead;
        public IReactiveCollection<IItemModel> Items => _items;
        public IReactiveCollection<EAbility> Abilities => _abilities;
        public IReactiveCollection<IEffectModel> Effects => _effects;

        public CardModel(IOwner owner, ICardTemplate template)
            : base(owner)
        {
            Template = template;

            _player = new ReactiveProperty<IPlayerModel>(owner as IPlayerModel);
            _power = new IntReactiveProperty(Template.Power);
            _health = new IntReactiveProperty(Template.Health);
            _manaCost = new IntReactiveProperty(Template.ManaCost);

            _items = new ReactiveCollection<IItemModel>(Template.Items);
            _abilities = new ReactiveCollection<EAbility>(Template.Abilities);
            _effects = new ReactiveCollection<IEffectModel>(Template.Effects);

            _health.Subscribe(h => { if (h <= 0) Die(); });
            _effects.ObserveAdd().Subscribe(e => Info($"Added Effect {e} from {this}")).AddTo(this);
            _items.ObserveAdd().Subscribe(e => Info($"Added Item {e} from {this}")).AddTo(this);
            _abilities.ObserveAdd().Subscribe(e => Info($"Added Ability {e} from {this}")).AddTo(this);
            _effects.ObserveRemove().Subscribe(e => Info($"Removed Effect {e} from {this}")).AddTo(this);
            _items.ObserveRemove().Subscribe(e => Info($"Removed Item {e} from {this}")).AddTo(this);
            _abilities.ObserveRemove().Subscribe(e => Info($"Removed Ability {e} from {this}")).AddTo(this);
        }

        void Die()
        {
            Info($"{this} died");
            _dead.Value = true;
        }

        public void ChangeHealth(int change)
        {
            _health.Value += change;
        }

        public void ChangeManaCost(int change)
        {
            _manaCost.Value = Math.Max(0, _manaCost.Value + change);
        }

        public void ChangePower(int change)
        {
            _power.Value = Math.Max(0, _power.Value + change);
        }

        public Response TakeDamage(ICardModel other)
        {
            ChangeHealth(-other.Power.Value);
            return Response.Ok;
        }

        public override string ToString()
        {
            return $"{Player}'s {PieceType}";
        }

        private readonly IntReactiveProperty _power;
        private readonly IntReactiveProperty _health;
        private readonly IntReactiveProperty _manaCost;
        private readonly ReactiveProperty<IPlayerModel> _player;

        private readonly ReactiveCollection<IItemModel> _items;
        private readonly ReactiveCollection<EAbility> _abilities;
        private readonly ReactiveCollection<IEffectModel> _effects;
        private readonly BoolReactiveProperty _dead = new BoolReactiveProperty(false);
    }
}
