using System.Collections.Generic;

namespace npg.tomatoecs.Entities
{
	internal class EntityBuffer
	{
		private readonly Entities _entities;
		private readonly List<Entity> _buffer;

		private bool _isValid;

		internal List<Entity> Buffer => _buffer;
		internal bool IsValid => _isValid;

		internal EntityBuffer(Entities entities, int capacity)
		{
			_entities = entities;
			_buffer = new List<Entity>(capacity);
		}

		internal void Add(uint entityId)
		{
			_buffer.Add(_entities.GetEntity(entityId));
		}

		internal void Update()
		{
			_isValid = true;
			if (_buffer.Count == 0)
			{
				return;
			}

			_buffer.Clear();
		}

		internal void Invalidate()
		{
			_isValid = false;
		}
	}
}