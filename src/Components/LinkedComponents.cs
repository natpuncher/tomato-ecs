using npg.tomatoecs.Entities;

namespace npg.tomatoecs.Components
{
	public readonly struct LinkedComponents<TComponent>
	{
		private readonly TComponent[] _components;
		private readonly uint[] _entities;
		private readonly int _count;
		private readonly EntityLinks _links;

		internal LinkedComponents(TComponent[] components, uint[] entities, int count, EntityLinks links)
		{
			_components = components;
			_entities = entities;
			_count = count;
			_links = links;
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(_components, _entities, _count, _links);
		}

		public struct Enumerator
		{
			private readonly TComponent[] _components;
			private readonly uint[] _entities;
			private readonly int _count;
			private readonly EntityLinks _links;
			private int _index;

			public Enumerator(TComponent[] components, uint[] entities, int count, EntityLinks links)
			{
				_components = components;
				_entities = entities;
				_count = count;
				_links = links;
				_index = -1;
			}

			public TComponent Current => _components[_index];

			public bool MoveNext()
			{
				while (++_index < _count)
				{
					if (HasLink(_index))
					{
						break;
					}
				}

				return _index < _count;
			}

			private bool HasLink(int index)
			{
				return _links.Has(_entities[index]);
			}
		}
	}
}