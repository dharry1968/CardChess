﻿namespace Dekuple.Common
{
    public interface IHasId
    {
        System.Guid Id { get; /* private */ set; }
    }
}