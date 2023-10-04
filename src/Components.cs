using System;
using System.Collections.Generic;

namespace npg.tomato_ecs
{
	public sealed class Components<TComponent> : InternalComponents where TComponent : struct
	{
		private readonly EntityLinker _entityLinker;
		internal readonly int Id;
		private readonly int _componentsCapacity;

		private TComponent[] _components;
		private uint[] _componentToEntityId;
		private int[] _entityIdToComponent;
		private int _componentCount;

		private readonly EntityBuffer _entityBuffer;
		private readonly EntityBuffer _linkedBuffer;
		internal int Version { get; private set; }

		public int Count => _componentCount;

		internal Components(Entities entities, EntityLinker entityLinker, int id, int capacity)
		{
			Id = id;
			_entityLinker = entityLinker;
			_entityIdToComponent = new int[capacity];
			_entityBuffer = new EntityBuffer(entities, capacity);
			_linkedBuffer = new EntityBuffer(entities, capacity);
			_componentsCapacity = capacity;
		}

		public List<Entity> GetEntities()
		{
			if (_entityBuffer.IsValid)
			{
				return _entityBuffer.Buffer;
			}

			_entityBuffer.Update();
			for (var i = 0; i < _componentCount; i++)
			{
				_entityBuffer.Add(_componentToEntityId[i] - 1);
			}

			return _entityBuffer.Buffer;
		}

		public List<Entity> GetLinkedEntities(Entity entity)
		{
			_linkedBuffer.Update();
			var links = _entityLinker.GetLinks(entity.Id);
			for (var i = 0; i < _componentCount; i++)
			{
				var entityId = _componentToEntityId[i] - 1;
				if (!links.Has(entityId))
				{
					continue;
				}

				_linkedBuffer.Add(entityId);
			}

			return _linkedBuffer.Buffer;
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
			_entityBuffer.Invalidate();
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
			_entityBuffer.Invalidate();

			SetComponent(entityId, componentId);
			InvokeAdded(entityId);
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

			_componentCount--;
			_entityBuffer.Invalidate();

			MoveComponent(_componentCount, componentId);
			ClearComponent(_componentCount, entityId);
			InvokeRemoved(entityId);
		}

		internal ref TComponent GetComponent(uint entityId)
		{
			Version++;
			return ref _components[GetComponentId(entityId)];
		}

		internal override bool HasComponent(uint entityId)
		{
			return GetComponentId(entityId) >= 0;
		}

		internal void FillComponents(ref TComponent[] components, ref uint[] entityIds, ref int[] componentIndexes)
		{
			Version = 0;

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

			var sourceEntityId = _componentToEntityId[sourceComponentId] - 1;
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