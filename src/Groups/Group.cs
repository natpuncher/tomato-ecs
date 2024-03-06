using System;
using System.Collections.Generic;
using System.Linq;
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

		public int Count => _entityIds.Count;

		internal Group(Matcher matcher, Entities.Entities entities, EntityLinker entityLinker)
		{
			_entityIds = new HashSet<uint>(DefaultGroupCapacity);

			_entities = entities;
			_entityLinker = entityLinker;
			_matcher = matcher;
			_matcher.Build();
			_matcher.OnEntityAdded += EntityAdded;
			_matcher.OnEntityRemoved += EntityRemoved;

			Initialize();
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(_entityIds, _entities, _matcher);
		}

		public LinkedEntityEnumerator GetLinkedEntities(Entity entity)
		{
			return new LinkedEntityEnumerator(_entityIds, _entityLinker.GetLinks(entity.Id), _entities, _matcher);
		}

		public Entity this[int index] => _entities.GetEntity(_entityIds.ElementAt(index));

		public bool Contains(Entity entity)
		{
			return Contains(entity.Id);
		}

		private bool Contains(uint entityId)
		{
			return _entityIds.Contains(entityId);
		}

		public void Dispose()
		{
			_entityIds.Clear();
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
			_entityIds.Add(entityId);
		}

		private void EntityRemoved(uint entityId)
		{
			_entityIds.Remove(entityId);
		}

		public struct Enumerator : IDisposable
		{
			private readonly Entities.Entities _entities;
			private readonly Matcher _matcher;
			private HashSet<uint>.Enumerator _enumerator;

			internal Enumerator(HashSet<uint> entityIds, Entities.Entities entities, Matcher matcher)
			{
				_entities = entities;
				_matcher = matcher;
				_matcher.Lock();
				_enumerator = entityIds.GetEnumerator();
			}

			public Entity Current => _entities.GetEntity(_enumerator.Current);

			public bool MoveNext()
			{
				return _enumerator.MoveNext();
			}

			public void Dispose()
			{
				_enumerator.Dispose();
				_matcher.Unlock();
			}
		}

		public readonly struct LinkedEntityEnumerator
		{
			private readonly HashSet<uint> _entityIds;
			private readonly EntityLinks _links;
			private readonly Entities.Entities _entities;
			private readonly Matcher _matcher;

			internal LinkedEntityEnumerator(HashSet<uint> entityIds, EntityLinks links, Entities.Entities entities, Matcher matcher)
			{
				_entityIds = entityIds;
				_links = links;
				_entities = entities;
				_matcher = matcher;
			}

			public Enumerator GetEnumerator()
			{
				return new Enumerator(_entityIds, _links, _entities, _matcher);
			}

			public struct Enumerator : IDisposable
			{
				private readonly EntityLinks _links;
				private readonly Entities.Entities _entities;
				private readonly Matcher _matcher;
				private HashSet<uint>.Enumerator _enumerator;

				internal Enumerator(HashSet<uint> entityIds, EntityLinks links, Entities.Entities entities, Matcher matcher)
				{
					_matcher = matcher;
					_matcher.Lock();
					_enumerator = entityIds.GetEnumerator();
					_links = links;
					_entities = entities;
				}

				public Entity Current => _entities.GetEntity(_enumerator.Current);

				public bool MoveNext()
				{
					var found = false;
					while (_enumerator.MoveNext())
					{
						if (_links.Has(_enumerator.Current))
						{
							found = true;
							break;
						}
					}

					return found;
				}

				public void Dispose()
				{
					_enumerator.Dispose();
					_matcher.Unlock();
				}
			}
		}
	}
}