﻿using NUnit.Framework;

namespace App.Model.Test
{
    [TestFixture]
    class TestBoard : TestBaseModel
    {
        [Test]
        public void TestBoardCreation()
        {
            Assert.IsNotNull(_board);
            Assert.IsTrue(_board.Width == 8);
            Assert.IsTrue(_board.Height == 8);

            Assert.IsNotNull(_arbiter);
            Assert.AreSame(_reg.Get<IBoardModel>(), _board);
            Assert.AreSame(_reg.Get<IArbiterModel>(), _board.Arbiter);
            Assert.AreSame(_reg.Get<IArbiterModel>(), _arbiter);

            Assert.IsNotNull(_white);
            Assert.IsNotNull(_black);

            Assert.AreSame(_white.Arbiter, _black.Arbiter);
            Assert.AreSame(_white.Board, _black.Board);
            Assert.AreSame(_white.Board.Arbiter.CurrentPlayer, _black.Board.Arbiter.Board.Arbiter.CurrentPlayer);
        }

        [Test]
        public void TestBasicGameModels()
        {
            _arbiter.PrepareGame(_white, _black);
            _arbiter.StartGame();
            _white.Mana.Value = _white.MaxMana.Value = 10;
            _black.Mana.Value = _black.MaxMana.Value = 10;
            for (var n = 0; n < 5; ++n)
            {
                Assert.IsTrue(_arbiter.Arbitrate(_white.NextAction()).Success);
                Assert.IsTrue(_arbiter.Arbitrate(_black.NextAction()).Success);
                _arbiter.Info(_board.Print());
            }
        }
    }
}
