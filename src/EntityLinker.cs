using System;

namespace npg.tomato_ecs
{
	internal sealed class EntityLinker : IDisposable
	{
		private readonly int _capacity;

		private EntityLinks[] _links;

		public EntityLinker(int capacity)
		{
			_capacity = capacity;
			_links = new EntityLinks[capacity];
		}

		internal void Link(uint id1, uint id2)
		{
			var links = GetLinks(id1);
			links.Add(id2);
			links = GetLinks(id2);
			links.Add(id1);
		}

		internal void Unlink(uint id1, uint id2)
		{
			var links = GetLinks(id1);
			if (links.Has(id2))
			{
				links.Remove(id2);
			}

			links = GetLinks(id2);
			if (links.Has(id1))
			{
				links.Remove(id1);
			}
		}

		internal void UnlinkAll(uint id)
		{
			var links = GetLinks(id);
			var raw = links.Raw();
			for (var index = 0u; index < raw.Length; index++)
			{
				var hasLink = raw[index];
				if (!hasLink)
				{
					continue;
				}
				var internalLink = GetLinks(index);
				if (internalLink.Has(id))
				{
					internalLink.Remove(id);
				}
			}

			links.Clear();
		}

		internal EntityLinks GetLinks(uint id)
		{
			Resize(id);
			if (_links[id] == null)
			{
				_links[id] = new EntityLinks(_capacity);
			}

			return _links[id];
		}

		internal bool HasLink(uint id1, uint id2)
		{
			return GetLinks(id1).Has(id2);
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

		public void Dispose()
		{
			foreach (var links in _links)
			{
				links?.Clear();
			}
			_links.Clear();
		}
	}
}