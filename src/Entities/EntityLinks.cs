using System;

namespace npg.tomatoecs.Entities
{
	public class EntityLinks
	{
		private bool[] _links;

		internal EntityLinks(int capacity)
		{
			_links = new bool[capacity];
		}

		internal void Add(uint id)
		{
			Resize(id);
			_links[id] = true;
		}

		internal void Remove(uint id)
		{
			if (id >= _links.Length)
			{
				return;
			}

			_links[id] = false;
		}

		internal bool Has(uint id)
		{
			if (id >= _links.Length)
			{
				return false;
			}

			return _links[id];
		}

		internal void Clear()
		{
			_links.Clear();
		}

		internal bool[] Raw()
		{
			return _links;
		}

		private void Resize(uint id)
		{
			if (id < _links.Length)
			{
				return;
			}

			var newSize = _links.Length << 1;
			while (id >= newSize)
			{
				newSize <<= 1;
			}

			Array.Resize(ref _links, newSize);
		}
	}
}