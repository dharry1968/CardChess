﻿using App.Agent;
using App.Common;
using App.Model;
using UnityEngine;

namespace App.View
{
    using Registry;

    public class ViewRegistry
        : Registry<IViewBase>
        , IViewRegistry
    {
        public override string ToString()
        {
            return base.ToString();
        }

        public override IViewBase Prepare(IViewBase view)
        {
            return base.Prepare(view);
        }

        public TIView FromPrefab<TIView>(Object prefab)
            where TIView : class, IViewBase
        {
            Assert.IsNotNull(prefab);
            var view = Object.Instantiate(prefab) as TIView;
            Assert.IsNotNull(view);
            return Prepare(Prepare(typeof(TIView), view)) as TIView;
        }

        public TIView FromPrefab<TIView, TIAgent, TModel>(IPlayerView player, Object prefab, TModel model)
            where TIView : class , IViewBase
            where TIAgent : class, IAgent, IHasDestroyHandler<IAgent>
            where TModel : IModel
        {
            var view = FromPrefab<TIView>(prefab);
            view.SetAgent(player, player.Agent.Registry.New<TIAgent>(model));
            Assert.IsTrue(view.IsValid);
            return view;
        }
    }
}
