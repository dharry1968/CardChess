﻿using System;

namespace App.Model.Test
{
    using Common;
	using Model;
    
    public class MockDeck 
		: DeckModel
    {
        public MockDeck(Guid a0, IOwner owner)
            : base(a0, owner)
        {
        }
    }
}