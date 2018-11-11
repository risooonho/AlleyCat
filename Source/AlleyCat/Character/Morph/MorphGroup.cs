using System.Collections.Generic;
using EnsureThat;

namespace AlleyCat.Character.Morph
{
    public class MorphGroup : IMorphGroup
    {
        public string Key { get; }

        public virtual string DisplayName { get; }

        public IEnumerable<IMorphDefinition> Definitions { get; }

        public MorphGroup(
            string key, 
            string displayName,
            IEnumerable<IMorphDefinition> definitions)
        {
            Ensure.That(key, nameof(key)).IsNotNullOrEmpty();
            Ensure.That(displayName, nameof(displayName)).IsNotNullOrEmpty();
            Ensure.That(definitions, nameof(definitions)).IsNotNull();

            Key = key;
            DisplayName = displayName;
            Definitions = definitions.Freeze();
        }
    }
}
