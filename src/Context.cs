using System;
using System.Collections.Generic;
using npg.tomatoecs.Components;
using npg.tomatoecs.Entities;
using npg.tomatoecs.Groups;

namespace npg.tomatoecs
{
	public class Context : IDisposable
	{
		private const int DefaultCapacity = 32;

		private readonly Entities.Entities _entities;
		private readonly List<InternalComponents> _components;
		private readonly Dictionary<Type, InternalComponents> _componentsMap;
		private readonly List<InternalReactiveComponents> _reactiveComponents;
		private readonly Dictionary<Type, InternalReactiveComponents> _reactiveComponentsMap;
		private readonly GroupProvider _groupProvider;
		private readonly EntityLinker _entityLinker;

		public Context(int entityCapacity = DefaultCapacity, int componentCapacity = DefaultCapacity)
		{
			_entities = new Entities.Entities(entityCapacity);

			_components = new List<InternalComponents>(componentCapacity);
			_componentsMap = new Dictionary<Type, InternalComponents>(componentCapacity);
			_reactiveComponents = new List<InternalReactiveComponents>(componentCapacity);
			_reactiveComponentsMap = new Dictionary<Type, InternalReactiveComponents>(componentCapacity);
			_entityLinker = new EntityLinker(entityCapacity);
			_groupProvider = new GroupProvider(_entities, this, _entityLinker);
		}

		public Entity CreateEntity()
		{
			return _entities.CreateEntity(this);
		}

		public Entity GetEntity(uint id)
		{
			return _entities.GetEntity(id);
		}

		public Entity[] GetAllEntities()
		{
			return _entities.Raw;
		}
	
		public Components<TComponent> GetComponents<TComponent>() where TComponent : struct
		{
			var type = typeof(TComponent);
			if (_componentsMap.TryGetValue(type, out var components))
			{
				return (Components<TComponent>)components;
			}

			var newComponents = new Components<TComponent>(_entities, _entityLinker, _components.Count, _entities.Capacity);
			_componentsMap.Add(type, newComponents);
			_components.Add(newComponents);
			return newComponents;
		}

		public ReactiveComponents<TComponent> GetReactiveComponents<TComponent>() where TComponent : struct, IEquatable<TComponent>
		{
			var type = typeof(TComponent);
			if (_reactiveComponentsMap.TryGetValue(type, out var reactiveComponents))
			{
				return (ReactiveComponents<TComponent>)reactiveComponents;
			}

			var newComponents = new ReactiveComponents<TComponent>(GetComponents<TComponent>(), _entities, _entities.Capacity);
			_reactiveComponentsMap.Add(type, newComponents);
			_reactiveComponents.Add(newComponents);
			return newComponents;
		}

		public void UpdateReactiveComponents()
		{
			for (var i = 0; i < _reactiveComponents.Count; i++)
			{
				_reactiveComponents[i].Update();
			}
		}

		public GroupBuilder GetGroup<TComponent>() where TComponent : struct
		{
			return _groupProvider.GetGroupBuilder<TComponent>();
		}

		public void Dispose()
		{
			_entities.Dispose();
			_groupProvider.Dispose();
			for (var i = 0; i < _components.Count; i++)
			{
				_components[i].Dispose();
			}

			_entityLinker.Dispose();
		}

		internal void Link(uint id1, uint id2)
		{
			_entityLinker.Link(id1, id2);
		}

		internal void Unlink(uint id1, uint id2)
		{
			_entityLinker.Unlink(id1, id2);
		}

		internal void UnlinkAll(uint id)
		{
			_entityLinker.UnlinkAll(id);
		}

		internal bool HasLink(uint id1, uint id2)
		{
			return _entityLinker.HasLink(id1, id2);
		}

		internal InternalComponents GetComponents(int id)
		{
			return _components[id];
		}

		internal void DestroyEntity(uint entityId)
		{
			_entityLinker.UnlinkAll(entityId);
			var componentsCount = _components.Count;
			for (var index = 0; index < componentsCount; index++)
			{
				_components[index].RemoveComponent(entityId);
			}

			_entities.RemoveEntity(entityId);
		}

		internal bool HasComponent<TComponent>(uint entityId) where TComponent : struct
		{
			return GetComponents<TComponent>().HasComponent(entityId);
		}

		internal ref TComponent AddComponent<TComponent>(uint entityId) where TComponent : struct
		{
			return ref GetComponents<TComponent>().AddComponent(entityId);
		}

		internal void RemoveComponent<TComponent>(uint entityId) where TComponent : struct
		{
			GetComponents<TComponent>().RemoveComponent(entityId);
		}
	}
}