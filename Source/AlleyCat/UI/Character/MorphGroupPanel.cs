using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AlleyCat.Autowire;
using AlleyCat.Character.Morph;
using AlleyCat.Character.Morph.Generic;
using EnsureThat;
using Godot;
using LanguageExt;
using static LanguageExt.Prelude;

namespace AlleyCat.UI.Character
{
    public class MorphGroupPanel : ScrollContainer
    {
        public IMorphGroup Group => _group.Head();

        public IEnumerable<IMorph> Morphs { get; private set; } = Enumerable.Empty<IMorph>();

        [Node]
        protected Container MorphsPanel { get; private set; }

        [Export] private PackedScene _colorMorphPanelScene;

        [Export] private PackedScene _rangedMorphPanelScene;

        private Option<IMorphGroup> _group;

        [PostConstruct]
        protected virtual void OnInitialize()
        {
            Morphs
                .Bind(m => CreateMorphPanel(m).AsEnumerable())
                .Iter(p => MorphsPanel.AddChild(p));
        }

        public void LoadGroup(IMorphGroup group, IMorphSet morphSet)
        {
            Ensure.That(group, nameof(group)).IsNotNull();
            Ensure.That(morphSet, nameof(morphSet)).IsNotNull();

            _group = Some(group);

            Morphs = morphSet.GetMorphs(group);
        }

        protected virtual Option<MorphPanel> CreateMorphPanel(IMorph morph)
        {
            Ensure.That(morph, nameof(morph)).IsNotNull();

            Debug.Assert(_colorMorphPanelScene != null, "_colorMorphPanelScene != null");
            Debug.Assert(_rangedMorphPanelScene != null, "_rangedMorphPanelScene != null");

            MorphPanel panel = null;

            switch (morph)
            {
                case IMorph<float, RangedMorphDefinition> _:
                    panel = (MorphPanel) _rangedMorphPanelScene.Instance();
                    break;
                case IMorph<Color, ColorMorphDefinition> _:
                    panel = (MorphPanel) _colorMorphPanelScene.Instance();
                    break;
            }

            panel?.LoadMorph(morph);

            return panel;
        }

        public override void _Ready()
        {
            base._Ready();

            this.Autowire();
        }
    }
}
