using System;
using System.Collections.Generic;
using System.Linq;
using AlleyCat.Autowire;
using AlleyCat.Common;
using AlleyCat.Common.Generic;
using EnsureThat;
using Godot;
using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using static LanguageExt.Prelude;

namespace AlleyCat.Item
{
    [AutowireContext]
    public class EquipmentFactory : RigidBody, IGameObjectFactory<Equipment>
    {
        [Export]
        public string Key { get; set; }

        [Export]
        public string DisplayName { get; set; }

        [Export]
        public string Description { get; set; }

        [Export]
        public EquipmentType EquipmentType { get; set; }

        [Export]
        public Mesh ItemMesh { get; set; }

        [Service]
        public Option<MeshInstance> Mesh { get; set; }

        [Service]
        public Option<CollisionShape> Shape { get; set; }

        [Service]
        public IEnumerable<EquipmentConfiguration> Configurations { get; set; } = Seq<EquipmentConfiguration>();

        [Service(local: true)]
        public IEnumerable<Marker> Markers { get; set; } = Seq<Marker>();

        public virtual IEnumerable<Type> ProvidedTypes => TypeUtils.FindInjectableTypes<Equipment>();

        public Validation<string, Equipment> Service { get; private set; } =
            Fail<string, Equipment>("The factory has not been initialized yet.");

        Validation<string, object> IGameObjectFactory.Service => Service.Map(v => (object) v);

        protected Validation<string, Equipment> CreateService()
        {
            var key = Key.TrimToOption().IfNone(GetName);
            var displayName = DisplayName.TrimToOption().Map(Tr).IfNone(key);
            var description = Description.TrimToOption().Map(Tr);

            return
                from mesh in Mesh
                    .ToValidation("Failed to find the mesh instance.")
                from shape in Shape
                    .ToValidation("Failed to find the collision shape.")
                from itemMesh in Optional(ItemMesh)
                    .ToValidation("Failed to find the item mesh.")
                from configurations in Optional(Configurations.Freeze()).Filter(c => c.Any())
                    .ToValidation("Failed to find equipment configuration.")
                select new Equipment(
                    key,
                    displayName,
                    description,
                    EquipmentType,
                    configurations,
                    this,
                    shape,
                    mesh,
                    itemMesh,
                    Markers);
        }

        public void AddServices(IServiceCollection collection)
        {
            Ensure.That(collection, nameof(collection)).IsNotNull();

            if (Service.IsSuccess)
            {
                throw new InvalidOperationException("The service has been already created.");
            }

            (Service = CreateService()).BiIter(
                service => ProvidedTypes.Iter(type => collection.AddSingleton(type, service)),
                error => GD.Print(error) // TODO Need a better way to handle errors (i.e. using a logger)
            );
        }

        public override void _Ready()
        {
            base._Ready();

            this.Autowire();
        }

        [PostConstruct]
        protected virtual void PostConstruct() => Service.SuccessAsEnumerable().Iter(s => s.Initialize());

        protected override void Dispose(bool disposing)
        {
            Service.SuccessAsEnumerable().Iter(s => s.DisposeQuietly());
            Service = Fail<string, Equipment>("The factory has been disposed.");

            base.Dispose(disposing);
        }
    }
}