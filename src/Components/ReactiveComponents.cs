using System;
using System.Collections.Generic;
using npg.tomatoecs.Entities;

namespace npg.tomatoecs.Components
{
	public sealed class ReactiveComponents<TComponent> : InternalReactiveComponents, IDisposable
		where TComponent : struct, IEquatable<TComponent>
	{
		private readonly EqualityComparer<TComponent> _equalityComparer = EqualityComparer<TComponent>.Default;
		private readonly Components<TComponent> _actualComponents;
		private readonly Entities.Entities _entities;

		private TComponent[] _previousComponents;
		private uint[] _componentToEntityId;
		private int[] _entityIdToComponent;
		private int _componentsCount;

		private readonly List<Entity> _added;
		private readonly List<Entity> _changed;
		private readonly List<Entity> _removed;

		private int _version;

		internal Components<TComponent> ActualComponents => _actualComponents;

		internal ReactiveComponents(Components<TComponent> actualComponents, Entities.Entities entities, int capacity)
		{
			_actualComponents = actualComponents;
			_entities = entities;
			_added = new List<Entity>(capacity);
			_changed = new List<Entity>(capacity);
			_removed = new List<Entity>(capacity);

			_actualComponents.OnAdded += OnAdded;
			_actualComponents.OnRemoved += OnRemoved;

			Update();
		}

		public List<Entity> Added()
		{
			return _added;
		}

		public List<Entity> Changed()
		{
			if (_version == _actualComponents.Version)
			{
				return _changed;
			}

			_version = _actualComponents.Version;

			_changed.Clear();
			for (var i = 0; i < _componentsCount; i++)
			{
				if (!WasComponent(i))
				{
					continue;
				}

				var entityId = _componentToEntityId[i] - 1;
				if (!_actualComponents.HasComponent(entityId))
				{
					continue;
				}

				if (_equalityComparer.Equals(_previousComponents[i], _actualComponents.GetComponent(entityId)))
				{
					continue;
				}

				_changed.Add(_entities.GetEntity(entityId));
			}

			return _changed;
		}

		public List<Entity> Removed()
		{
			return _removed;
		}

		public void Dispose()
		{
			Clear();
			_previousComponents.Clear();
			_componentToEntityId.Clear();
			_entityIdToComponent.Clear();
		}

		internal ref TComponent GetPreviousComponent(uint entityId)
		{
			return ref _previousComponents[GetComponentId(entityId)];
		}

		internal override void Update()
		{
			Clear();
			_componentsCount = _actualComponents.Count;
			if (_componentsCount == 0)
			{
				return;
			}

			_actualComponents.FillComponents(ref _previousComponents, ref _componentToEntityId, ref _entityIdToComponent);
		}

		private void Clear()
		{
			_version = -1;
			_added.Clear();
			_changed.Clear();
			_removed.Clear();
		}

		private int GetComponentId(uint entityId)
		{
			return _entityIdToComponent[entityId] - 1;
		}

		private bool WasComponent(int index)
		{
			return _componentToEntityId[index] > 0;
		}

		private void OnAdded(uint entityId)
		{
			_added.Add(_entities.GetEntity(entityId));
		}

		private void OnRemoved(uint entityId)
		{
			_removed.Add(_entities.GetEntity(entityId));
		}
	}
}