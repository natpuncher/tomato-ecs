using System;

namespace npg.tomatoecs.Components
{
	public abstract class InternalComponents : IDisposable
	{
		internal event Action<uint> OnAdded;
		internal event Action<uint> OnRemoved;

		internal abstract bool HasComponent(uint entityId);
		internal abstract void RemoveComponent(uint entityId);

		protected void InvokeAdded(uint entityId)
		{
			OnAdded?.Invoke(entityId);
		}

		protected void InvokeRemoved(uint entityId)
		{
			OnRemoved?.Invoke(entityId);
		}

		public abstract void Dispose();
	}
}