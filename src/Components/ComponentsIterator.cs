using System;
using npg.tomatoecs.Entities;

namespace npg.tomatoecs.Components
{
	public class ComponentsIterator<TComponent> : IDisposable where TComponent : struct
	{
		private readonly Components<TComponent> _components;
		private readonly DelayedOperations<TComponent> _delayedOperations;
		private readonly EntityLinker _entityLinker;
		private readonly Entities.Entities _entities;
		private int _lock;

		internal bool IsLocked => _lock > 0;

		internal ComponentsIterator(Components<TComponent> components,
			DelayedOperations<TComponent> delayedOperations,
			EntityLinker entityLinker,
			Entities.Entities entities)
		{
			_components = components;
			_delayedOperations = delayedOperations;
			_entityLinker = entityLinker;
			_entities = entities;
		}

		internal ComponentEnumerator GetComponentEnumerator()
		{
			return new ComponentEnumerator(_components);
		}

		internal LinkedComponentEnumerator GetLinkedComponentEnumerator(uint entityId)
		{
			return new LinkedComponentEnumerator(_components, _entityLinker.GetLinks(entityId));
		}

		internal EntityEnumerator GetEntityEnumerator()
		{
			return new EntityEnumerator(_components, _entities);
		}

		internal LinkedEntityEnumerator GetLinkedEntityEnumerator(uint entityId)
		{
			return new LinkedEntityEnumerator(_components, _entities, _entityLinker.GetLinks(entityId));
		}

		internal void Lock()
		{
			_lock++;
		}

		internal void Unlock()
		{
			_lock--;
			if (_lock != 0)
			{
				return;
			}

			_delayedOperations.Apply();
		}

		public void Dispose()
		{
			_lock = 0;
		}

		public struct ComponentEnumerator : IDisposable
		{
			private readonly Components<TComponent> _components;
			private readonly int _componentCount;
			private int _index;

			public ComponentEnumerator(Components<TComponent> components)
			{
				_components = components;
				_components.Lock();
				_componentCount = components.Count;
				_index = -1;
			}

			public ref TComponent Current => ref _components[_index];

			public bool MoveNext()
			{
				return ++_index < _componentCount;
			}

			public void Dispose()
			{
				_components.Unlock();
			}
		}

		public readonly struct LinkedComponentEnumerator
		{
			private readonly Components<TComponent> _components;
			private readonly EntityLinks _links;

			internal LinkedComponentEnumerator(Components<TComponent> components, EntityLinks links)
			{
				_components = components;
				_links = links;
			}

			public Enumerator GetEnumerator()
			{
				return new Enumerator(_components, _links, _components.Count);
			}

			public struct Enumerator : IDisposable
			{
				private readonly Components<TComponent> _components;
				private readonly EntityLinks _links;
				private readonly int _count;
				private int _index;

				internal Enumerator(Components<TComponent> components, EntityLinks links, int componentCount)
				{
					_components = components;
					_components.Lock();
					_links = links;
					_count = componentCount;
					_index = -1;
				}

				public ref TComponent Current => ref _components[_index];

				public bool MoveNext()
				{
					while (++_index < _count)
					{
						if (_links.Has(_components.GetEntityId(_index)))
						{
							break;
						}
					}

					return _index < _count;
				}

				public void Dispose()
				{
					_components.Unlock();
				}
			}
		}

		public readonly struct EntityEnumerator
		{
			private readonly Components<TComponent> _components;
			private readonly Entities.Entities _entities;

			internal EntityEnumerator(Components<TComponent> components, Entities.Entities entities)
			{
				_components = components;
				_entities = entities;
			}

			public Enumerator GetEnumerator()
			{
				return new Enumerator(_components, _entities);
			}

			public struct Enumerator : IDisposable
			{
				private readonly Components<TComponent> _components;
				private readonly Entities.Entities _entities;
				private readonly int _count;
				private int _index;

				internal Enumerator(Components<TComponent> components, Entities.Entities entities)
				{
					_components = components;
					_components.Lock();
					_entities = entities;
					_index = -1;
					_count = components.Count;
				}

				public Entity Current => _entities.GetEntity(_components.GetEntityId(_index));

				public bool MoveNext()
				{
					return ++_index < _count;
				}

				public void Dispose()
				{
					_components.Unlock();
				}
			}
		}

		public readonly struct LinkedEntityEnumerator
		{
			private readonly Components<TComponent> _components;
			private readonly Entities.Entities _entities;
			private readonly EntityLinks _links;

			internal LinkedEntityEnumerator(Components<TComponent> components,
				Entities.Entities entities,
				EntityLinks links)
			{
				_components = components;
				_entities = entities;
				_links = links;
			}

			public Enumerator GetEnumerator()
			{
				return new Enumerator(_components, _entities, _links, _components.Count);
			}

			public struct Enumerator : IDisposable
			{
				private readonly Components<TComponent> _components;
				private readonly Entities.Entities _entities;
				private readonly EntityLinks _links;
				private readonly int _componentCount;
				private int _index;

				internal Enumerator(Components<TComponent> components,
					Entities.Entities entities,
					EntityLinks links,
					int componentCount)
				{
					_components = components;
					_components.Lock();
					_entities = entities;
					_links = links;
					_componentCount = componentCount;
					_index = -1;
				}

				public Entity Current => _entities.GetEntity(_components.GetEntityId(_index));

				public bool MoveNext()
				{
					while (++_index < _componentCount)
					{
						if (_links.Has(_components.GetEntityId(_index)))
						{
							break;
						}
					}

					return _index < _componentCount;
				}

				public void Dispose()
				{
					_components.Unlock();
				}
			}
		}
	}
}