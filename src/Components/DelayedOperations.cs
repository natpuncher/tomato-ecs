using System;
using System.Collections.Generic;

namespace npg.tomatoecs.Components
{
	internal class DelayedOperations<TComponent> : IDisposable where TComponent : struct
	{
		private readonly List<uint> _toAdd = new();
		private readonly List<uint> _toRemove = new();
		private readonly Components<TComponent> _components;

		internal int AddedCount => _toAdd.Count;

		internal DelayedOperations(Components<TComponent> components)
		{
			_components = components;
		}

		internal void Add(uint entityId)
		{
			_toAdd.Add(entityId);
		}

		internal void Remove(uint entityId)
		{
			_toRemove.Add(entityId);
		}

		internal void Apply()
		{
			foreach (var entityId in _toAdd)
			{
				_components.InvokeAdded(entityId);
			}

			_toAdd.Clear();

			foreach (var entityId in _toRemove)
			{
				_components.RemoveComponent(entityId);
			}

			_toRemove.Clear();
		}

		public void Dispose()
		{
			_toAdd.Clear();
			_toRemove.Clear();
		}
	}
}