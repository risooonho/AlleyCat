using System;
using EnsureThat;
using Godot;
using Microsoft.Extensions.Logging;

namespace AlleyCat.Animation
{
    public class AnimationGraphContext
    {
        public AnimationPlayer Player { get; }

        public AnimationTree AnimationTree { get; }

        public IObservable<float> OnAdvance { get; }

        public IAnimationGraphFactory GraphFactory { get; }

        public IAnimationControlFactory ControlFactory { get; }

        public ILoggerFactory LoggerFactory { get; }

        public AnimationGraphContext(
            AnimationPlayer player,
            AnimationTree animationTree,
            IObservable<float> onAdvance,
            IAnimationGraphFactory graphFactory,
            IAnimationControlFactory controlFactory,
            ILoggerFactory loggerFactory)
        {
            Ensure.That(player, nameof(player)).IsNotNull();
            Ensure.That(animationTree, nameof(animationTree)).IsNotNull();
            Ensure.That(onAdvance, nameof(onAdvance)).IsNotNull();
            Ensure.That(graphFactory, nameof(graphFactory)).IsNotNull();
            Ensure.That(controlFactory, nameof(controlFactory)).IsNotNull();
            Ensure.That(loggerFactory, nameof(loggerFactory)).IsNotNull();

            Player = player;
            AnimationTree = animationTree;
            OnAdvance = onAdvance;
            GraphFactory = graphFactory;
            ControlFactory = controlFactory;
            LoggerFactory = loggerFactory;
        }
    }
}
