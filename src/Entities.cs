using System;
using System.Collections.Generic;

namespace npg.tomato_ecs
{
	internal sealed class Entities : IDisposable
	{
		private Entity[] _entities;
		private uint _entityCount;

		private uint[] _removedEntities;
		private int _removedEntitiesCount;

		internal int Capacity { get; private set; }

		internal IReadOnlyCollection<Entity> RawEntities => _entities;

		internal Entities(int capacity)
		{
			Capacity = capacity;
			_entities = new Entity[Capacity];
			_removedEntities = new uint[Capacity];
		}

		internal Entity CreateEntity(Context context)
		{
			var entityIndex = _entityCount;
			if (_removedEntitiesCount > 0)
			{
				_removedEntitiesCount--;
				entityIndex = _removedEntities[_removedEntitiesCount];
			}

			if (_entityCount == _entities.Length)
			{
				Resize();
			}

			_entityCount++;

			ref var entity = ref _entities[entityIndex];
			entity.Context = context;
			entity.Id = entityIndex;
			return entity;
		}

		internal Entity GetEntity(uint entityId)
		{
			return _entities[entityId];
		}

		internal void RemoveEntity(uint entityId)
		{
			_entityCount--;
			if (_removedEntitiesCount == _removedEntities.Length)
			{
				Array.Resize(ref _removedEntities, _removedEntitiesCount << 1);
			}

			_removedEntities[_removedEntitiesCount] = entityId;
			_removedEntitiesCount++;
			_entities[entityId] = default;
		}

		public void Dispose()
		{
			_entities.Clear();
			_entityCount = 0;
			_removedEntities.Clear();
			_removedEntitiesCount = 0;
		}

		private void Resize()
		{
			Capacity <<= 1;
			Array.Resize(ref _entities, Capacity);
		}
	}
}