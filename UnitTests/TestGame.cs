﻿using System.Diagnostics;
using App.Action;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace App
{
    [TestClass]
    public class TestGame : TestGameBase
    {
        [TestMethod]
        public void TestPlayKings()
        {
            var arbiter = RandomBasicSetup<MockWhitePlayer, MockBlackPlayer>();

            arbiter.NewGame();

            StepArbiter(2);
            Trace.WriteLine(Arbiter.Kernel.Root);

            var w = arbiter.WhitePlayer;
            var b = arbiter.BlackPlayer;

            StepArbiter(2);
            Trace.WriteLine(Arbiter.Kernel.Root);

            w.AcceptCards();
            b.AcceptCards();

            StepArbiter(2);
            Trace.WriteLine(Arbiter.Kernel.Root);

            w.PlaceKing(new Coord(3, 1));
            b.PlaceKing(new Coord(4, 6));

            StepArbiter(2);
            Trace.WriteLine(Arbiter.Kernel.Root);

            Assert.AreEqual(Parameters.StartHandCardCount, w.Hand.Cards.Count);
            Assert.AreEqual(Parameters.StartHandCardCount, b.Hand.Cards.Count);

            Trace.WriteLine(arbiter);
        }
    }
}