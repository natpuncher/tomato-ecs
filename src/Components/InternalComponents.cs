using System;

namespace npg.tomatoecs.Components
{
	public abstract class InternalComponents : IDisposable
	{
		internal event Action<uint> OnAdded;
		internal event Action<uint> OnRemoved;

		internal abstract bool HasComponent(uint entityId);
		internal abstract void RemoveComponent(uint entityId);

		internal void InvokeAdded(uint entityId)
		{
			OnAdded?.Invoke(entityId);
		}

		internal void InvokeRemoved(uint entityId)
		{
			OnRemoved?.Invoke(entityId);
		}

		internal abstract void Update();
		public abstract void Dispose();
		internal abstract void Lock();
		internal abstract void Unlock();
	}
}