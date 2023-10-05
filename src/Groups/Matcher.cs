using System;
using System.Collections.Generic;

namespace npg.tomatoecs.Groups
{
	internal sealed class Matcher : IDisposable
	{
		private const int DefaultCapacity = 4;

		private readonly List<int> _include = new(DefaultCapacity);
		private readonly List<int> _exclude = new(DefaultCapacity);

		private readonly Context _context;

		private int _hash;
		private bool _hashInitialized;

		internal event Action<uint> OnEntityAdded;
		internal event Action<uint> OnEntityRemoved;

		public Matcher(Context context)
		{
			_context = context;
		}

		internal void Include(int componentId)
		{
			_include.Add(componentId);
			_hashInitialized = false;
		}

		internal void Exclude(int componentId)
		{
			_exclude.Add(componentId);
			_hashInitialized = false;
		}

		internal void Build()
		{
			GetHashCode();
			foreach (var componentId in _include)
			{
				var components = _context.GetComponents(componentId);
				components.OnAdded += CheckEntityAdd;
				components.OnRemoved += CheckEntityRemove;
			}

			foreach (var componentId in _exclude)
			{
				var components = _context.GetComponents(componentId);
				components.OnAdded += CheckEntityRemove;
				components.OnRemoved += CheckEntityAdd;
			}
		}

		internal bool Match(uint entityId)
		{
			return IsIncluded(entityId) && !IsExcluded(entityId);
		}

		private bool IsIncluded(uint entityId)
		{
			for (var index = 0; index < _include.Count; index++)
			{
				var components = _context.GetComponents(_include[index]);
				if (!components.HasComponent(entityId))
				{
					return false;
				}
			}

			return true;
		}

		private bool IsExcluded(uint entityId)
		{
			for (var index = 0; index < _exclude.Count; index++)
			{
				var components = _context.GetComponents(_exclude[index]);
				if (components.HasComponent(entityId))
				{
					return true;
				}
			}

			return false;
		}

		private void CheckEntityAdd(uint entityId)
		{
			if (!Match(entityId))
			{
				return;
			}

			OnEntityAdded?.Invoke(entityId);
		}

		private void CheckEntityRemove(uint entityId)
		{
			if (Match(entityId))
			{
				return;
			}

			OnEntityRemoved?.Invoke(entityId);
		}

		public void Dispose()
		{
			_include.Clear();
			_exclude.Clear();
			_hashInitialized = false;
		}

		public override int GetHashCode()
		{
			if (_hashInitialized)
			{
				return _hash;
			}

			_include.Sort();
			_exclude.Sort();

			unchecked
			{
				var result = _include.Count * 17 + _exclude.Count * 23;
				foreach (var id in _include)
				{
					result = result * 393241 + id;
				}

				foreach (var id in _exclude)
				{
					result = result * 393241 - id;
				}

				_hash = result;
				_hashInitialized = true;
				return result;
			}
		}

		public override bool Equals(object obj)
		{
			if (obj == null || obj.GetType() != GetType() || obj.GetHashCode() != GetHashCode())
			{
				return false;
			}

			var matcher = (Matcher)obj;
			return EqualIndexes(_include, matcher._include) && EqualIndexes(_exclude, matcher._exclude);
		}

		private static bool EqualIndexes(List<int> source, List<int> target)
		{
			if ((source == null) != (target == null))
			{
				return false;
			}

			if (source == null)
			{
				return true;
			}

			var sourceCount = source.Count;
			if (sourceCount != target.Count)
			{
				return false;
			}

			for (var i = 0; i < sourceCount; i++)
			{
				if (source[i] != target[i])
				{
					return false;
				}
			}

			return true;
		}
	}
}