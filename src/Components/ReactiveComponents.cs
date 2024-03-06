using System;
using npg.tomatoecs.Entities;

namespace npg.tomatoecs.Components
{
	public sealed class ReactiveComponents<TComponent> : InternalReactiveComponents, IDisposable
		where TComponent : struct, IReactiveComponent<TComponent>
	{
		private readonly ComponentComparer<TComponent> _componentComparer = new();
		private readonly Components<TComponent> _actualComponents;
		private readonly Entities.Entities _entities;

		private TComponent[] _previousComponents;
		private uint[] _componentToEntityId;
		private int[] _entityIdToComponent;
		private int _componentsCount;

		internal Components<TComponent> ActualComponents => _actualComponents;

		internal ReactiveComponents(Components<TComponent> actualComponents, Entities.Entities entities)
		{
			_actualComponents = actualComponents;
			_entities = entities;

			Update();
		}

		public ChangedEnumerator Changed()
		{
			return new ChangedEnumerator(this, _entities);
		}

		public void Dispose()
		{
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
			_componentsCount = _actualComponents.Count;
			if (_componentsCount == 0)
			{
				return;
			}

			_actualComponents.CopyTo(ref _previousComponents, ref _componentToEntityId, ref _entityIdToComponent);
		}

		private bool HasChanges(int index)
		{
			if (!WasComponent(index))
			{
				return false;
			}

			var entityId = _componentToEntityId[index] - 1;
			if (!_actualComponents.HasComponent(entityId))
			{
				return false;
			}

			return _componentComparer.HasChanges(_previousComponents[index], _actualComponents.GetComponent(entityId));
		}

		private uint GetEntityId(int index)
		{
			return _componentToEntityId[index] - 1;
		}

		private void Lock()
		{
			_actualComponents.Lock();
		}

		private void Unlock()
		{
			_actualComponents.Unlock();
		}

		private int GetComponentId(uint entityId)
		{
			return _entityIdToComponent[entityId] - 1;
		}

		private bool WasComponent(int index)
		{
			return _componentToEntityId[index] > 0;
		}

		public readonly struct ChangedEnumerator
		{
			private readonly ReactiveComponents<TComponent> _reactiveComponents;
			private readonly Entities.Entities _entities;

			internal ChangedEnumerator(ReactiveComponents<TComponent> reactiveComponents, Entities.Entities entities)
			{
				_reactiveComponents = reactiveComponents;
				_entities = entities;
			}

			public Enumerator GetEnumerator()
			{
				return new Enumerator(_reactiveComponents, _entities);
			}

			public struct Enumerator : IDisposable
			{
				private readonly ReactiveComponents<TComponent> _reactiveComponents;
				private readonly Entities.Entities _entities;
				private readonly int _componentCount;
				private int _index;

				internal Enumerator(ReactiveComponents<TComponent> reactiveComponents, Entities.Entities entities)
				{
					_reactiveComponents = reactiveComponents;
					_entities = entities;
					_index = -1;
					_reactiveComponents.Lock();
					_componentCount = _reactiveComponents._componentsCount;
				}

				public Entity Current => _entities.GetEntity(_reactiveComponents.GetEntityId(_index));

				public bool MoveNext()
				{
					while (++_index < _componentCount)
					{
						if (_reactiveComponents.HasChanges(_index))
						{
							break;
						}
					}

					return _index < _componentCount;
				}

				public void Dispose()
				{
					_reactiveComponents.Unlock();
				}
			}
		}

		private class ComponentComparer<TComponent> where TComponent : struct, IReactiveComponent<TComponent>
		{
			public bool HasChanges(TComponent first, TComponent second)
			{
				return !first.Equals(second);
			}
		}
	}
}