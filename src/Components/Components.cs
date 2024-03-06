using System;
using System.Collections.Generic;
using npg.tomatoecs.Entities;

namespace npg.tomatoecs.Components
{
	public sealed class Components<TComponent> : InternalComponents where TComponent : struct
	{
		private readonly ComponentsIterator<TComponent> _iterator;
		private readonly DelayedOperations<TComponent> _delayedOperations;
		internal readonly int Id;
		private readonly int _componentsCapacity;

		private TComponent[] _components;
		private uint[] _componentToEntityId;
		private int[] _entityIdToComponent;
		private int _componentCount;

		private readonly EntityBuffer _added;
		private readonly EntityBuffer _removed;

		public int Count => _componentCount - _delayedOperations.AddedCount;

		internal Components(Entities.Entities entities, EntityLinker entityLinker, int id, int capacity)
		{
			Id = id;
			_delayedOperations = new DelayedOperations<TComponent>(this);
			_iterator = new ComponentsIterator<TComponent>(this, _delayedOperations, entityLinker, entities);
			_entityIdToComponent = new int[capacity];
			_added = new EntityBuffer(entities, capacity);
			_removed = new EntityBuffer(entities, capacity);
			_componentsCapacity = capacity;
		}

		public ComponentsIterator<TComponent>.ComponentEnumerator GetEnumerator()
		{
			return _iterator.GetComponentEnumerator();
		}

		public ComponentsIterator<TComponent>.LinkedComponentEnumerator LinkedTo(Entity entity)
		{
			return _iterator.GetLinkedComponentEnumerator(entity.Id);
		}

		public ComponentsIterator<TComponent>.EntityEnumerator GetEntities()
		{
			return _iterator.GetEntityEnumerator();
		}

		public ComponentsIterator<TComponent>.LinkedEntityEnumerator GetLinkedEntities(Entity entity)
		{
			return _iterator.GetLinkedEntityEnumerator(entity.Id);
		}

		public List<Entity> Added()
		{
			return _added.Buffer;
		}

		public List<Entity> Removed()
		{
			return _removed.Buffer;
		}

		public ref TComponent this[int index] => ref _components[index];

		internal uint GetEntityId(int index)
		{
			return _componentToEntityId[index] - 1;
		}

		internal override void Update()
		{
			_added.Update();
			_removed.Update();
		}

		public override void Dispose()
		{
			if (_components != null)
			{
				_components.Clear();
				_componentToEntityId.Clear();
			}

			_entityIdToComponent.Clear();
			_componentCount = 0;
			_added.Update();
			_removed.Update();
			_iterator.Dispose();
			_delayedOperations.Dispose();
		}

		internal override void Lock()
		{
			_iterator.Lock();
		}

		internal override void Unlock()
		{
			_iterator.Unlock();
		}

		internal ref TComponent AddComponent(uint entityId)
		{
			if (HasComponent(entityId))
			{
				return ref GetComponent(entityId);
			}

			var componentId = _componentCount;
			if (_components == null)
			{
				_components = new TComponent[_componentsCapacity];
				_componentToEntityId = new uint[_componentsCapacity];
			}

			if (componentId >= _components.Length)
			{
				var newSize = _components.Length << 1;
				Array.Resize(ref _components, newSize);
				Array.Resize(ref _componentToEntityId, newSize);
			}

			_componentCount++;

			SetComponent(entityId, componentId);
			_added.Add(entityId);
			if (_iterator.IsLocked)
			{
				_delayedOperations.Add(entityId);
			}
			else
			{
				InvokeAdded(entityId);
			}

			return ref _components[componentId];
		}

		internal override void RemoveComponent(uint entityId)
		{
			var componentId = GetComponentId(entityId);
			var hasComponent = componentId >= 0;
			if (!hasComponent)
			{
				return;
			}

			if (_iterator.IsLocked)
			{
				_delayedOperations.Remove(entityId);
				return;
			}

			_componentCount--;

			MoveComponent(_componentCount, componentId);
			ClearComponent(_componentCount, entityId);
			_removed.Add(entityId);
			InvokeRemoved(entityId);
		}

		internal ref TComponent GetComponent(uint entityId)
		{
			return ref _components[GetComponentId(entityId)];
		}

		internal override bool HasComponent(uint entityId)
		{
			return GetComponentId(entityId) >= 0;
		}

		internal void CopyTo(ref TComponent[] components, ref uint[] entityIds, ref int[] componentIndexes)
		{
			var componentsLength = _components.Length;
			if (components == null || componentsLength != components.Length)
			{
				Array.Resize(ref components, componentsLength);
				Array.Resize(ref entityIds, componentsLength);
			}

			Array.Copy(_components, components, componentsLength);
			Array.Copy(_componentToEntityId, entityIds, componentsLength);

			var indexesLength = _entityIdToComponent.Length;
			if (componentIndexes == null || indexesLength != componentIndexes.Length)
			{
				Array.Resize(ref componentIndexes, indexesLength);
			}

			Array.Copy(_entityIdToComponent, componentIndexes, indexesLength);
		}

		private int GetComponentId(uint entityId)
		{
			Resize(entityId);
			return _entityIdToComponent[entityId] - 1;
		}

		private void SetComponent(uint entityId, int componentId)
		{
			_entityIdToComponent[entityId] = componentId + 1;
			_componentToEntityId[componentId] = entityId + 1;
		}

		private void MoveComponent(int sourceComponentId, int targetComponentId)
		{
			_components[targetComponentId] = _components[sourceComponentId];

			var sourceEntityId = GetEntityId(sourceComponentId);
			SetComponent(sourceEntityId, targetComponentId);
		}

		private void ClearComponent(int componentId, uint entityId)
		{
			_components[componentId] = default;
			_componentToEntityId[componentId] = 0;
			_entityIdToComponent[entityId] = 0;
		}

		private void Resize(uint id)
		{
			if (id < _entityIdToComponent.Length)
			{
				return;
			}

			var newSize = _entityIdToComponent.Length << 1;
			while (id >= newSize)
			{
				newSize <<= 1;
			}

			Array.Resize(ref _entityIdToComponent, newSize);
		}
	}
}