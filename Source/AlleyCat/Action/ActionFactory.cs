using AlleyCat.Common;
using AlleyCat.Game;
using Godot;
using LanguageExt;
using Microsoft.Extensions.Logging;

namespace AlleyCat.Action
{
    public abstract class ActionFactory<T> : GameNodeFactory<T> where T : Action
    {
        [Export]
        public bool Active { get; set; } = true;

        [Export]
        public string Key { get; set; }

        [Export]
        public string DisplayName { get; set; }

        protected override Validation<string, T> CreateService(ILoggerFactory loggerFactory)
        {
            var key = Key.TrimToOption().IfNone(() => Name);
            var displayName = DisplayName.TrimToOption().Map(Tr).IfNone(key);

            return CreateService(key, displayName, loggerFactory);
        }

        protected abstract Validation<string, T> CreateService(
            string key, string displayName, ILoggerFactory loggerFactory);
    }
}
