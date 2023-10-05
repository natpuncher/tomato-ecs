using System;
using System.Collections.Generic;
using npg.tomatoecs.Entities;

namespace npg.tomatoecs.Groups
{
	public sealed class Group : IDisposable
	{
		private const int DefaultGroupCapacity = 32;

		private readonly Matcher _matcher;
		private readonly Entities.Entities _entities;
		private readonly EntityLinker _entityLinker;
		private readonly HashSet<uint> _entityIds;
		private readonly EntityBuffer _entityBuffer;
		private readonly EntityBuffer _linkedBuffer;

		public int Count => _entityIds.Count;

		internal Group(Matcher matcher, Entities.Entities entities, EntityLinker entityLinker)
		{
			_entityIds = new HashSet<uint>(DefaultGroupCapacity);
			_entityBuffer = new EntityBuffer(entities, DefaultGroupCapacity);
			_linkedBuffer = new EntityBuffer(entities, DefaultGroupCapacity);

			_entities = entities;
			_entityLinker = entityLinker;
			_matcher = matcher;
			_matcher.Build();
			_matcher.OnEntityAdded += EntityAdded;
			_matcher.OnEntityRemoved += EntityRemoved;

			Initialize();
		}

		public List<Entity> GetEntities()
		{
			if (_entityBuffer.IsValid)
			{
				return _entityBuffer.Buffer;
			}

			_entityBuffer.Update();
			if (Count == 0)
			{
				return _entityBuffer.Buffer;
			}

			foreach (var entityId in _entityIds)
			{
				_entityBuffer.Add(entityId);
			}

			return _entityBuffer.Buffer;
		}

		public List<Entity> GetLinkedEntities(Entity entity)
		{
			_linkedBuffer.Update();
			if (Count == 0)
			{
				return _linkedBuffer.Buffer;
			}

			var links = _entityLinker.GetLinks(entity.Id);
			foreach (var entityId in _entityIds)
			{
				if (!links.Has(entityId))
				{
					continue;
				}

				_linkedBuffer.Add(entityId);
			}

			return _linkedBuffer.Buffer;
		}

		public bool Contains(Entity entity)
		{
			return Contains(entity.Id);
		}

		internal bool Contains(uint entityId)
		{
			return _entityIds.Contains(entityId);
		}

		public void Dispose()
		{
			_entityIds.Clear();
			_entityBuffer.Invalidate();
		}

		private void Initialize()
		{
			foreach (var entity in _entities.Raw)
			{
				if (!entity.IsActive)
				{
					continue;
				}

				if (!_matcher.Match(entity.Id))
				{
					continue;
				}

				_entityIds.Add(entity.Id);
			}
		}

		private void EntityAdded(uint entityId)
		{
			_entityBuffer.Invalidate();
			_entityIds.Add(entityId);
		}

		private void EntityRemoved(uint entityId)
		{
			_entityBuffer.Invalidate();
			_entityIds.Remove(entityId);
		}
	}
}