using System;
using System.Collections.Generic;

namespace npg.tomatoecs.Groups
{
	internal sealed class Matcher : IDisposable
	{
		private const int DefaultCapacity = 4;

		private readonly List<int> _all = new(DefaultCapacity);
		private readonly List<int> _any = new(DefaultCapacity);
		private readonly List<int> _none = new(DefaultCapacity);

		private readonly Context _context;

		private int _hash;
		private bool _hashInitialized;

		internal event Action<uint> OnEntityAdded;
		internal event Action<uint> OnEntityRemoved;

		internal Matcher(Context context)
		{
			_context = context;
		}

		internal void All(int componentId)
		{
			_all.Add(componentId);
			_hashInitialized = false;
		}

		internal void Any(int componentId)
		{
			_any.Add(componentId);
			_hashInitialized = false;
		}

		internal void None(int componentId)
		{
			_none.Add(componentId);
			_hashInitialized = false;
		}

		internal void Build()
		{
			GetHashCode();
			Subscribe();
		}

		internal bool Match(uint entityId)
		{
			return IsIncluded(entityId) && !IsExcluded(entityId);
		}

		private bool IsIncluded(uint entityId)
		{
			return CheckAll(entityId) && CheckAny(entityId);
		}

		private bool CheckAll(uint entityId)
		{
			for (var index = 0; index < _all.Count; index++)
			{
				var components = _context.GetComponents(_all[index]);
				if (!components.HasComponent(entityId))
				{
					return false;
				}
			}

			return true;
		}

		private bool CheckAny(uint entityId)
		{
			if (_any.Count == 0)
			{
				return true;
			}

			for (var index = 0; index < _any.Count; index++)
			{
				var components = _context.GetComponents(_any[index]);
				if (components.HasComponent(entityId))
				{
					return true;
				}
			}

			return false;
		}

		private bool IsExcluded(uint entityId)
		{
			for (var index = 0; index < _none.Count; index++)
			{
				var components = _context.GetComponents(_none[index]);
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
			_all.Clear();
			_none.Clear();
			_hashInitialized = false;
		}

		private void Subscribe()
		{
			var add = new HashSet<int>();
			var remove = new HashSet<int>();
			foreach (var componentId in _all)
			{
				var components = _context.GetComponents(componentId);
				if (!add.Contains(componentId))
				{
					add.Add(componentId);
					components.OnAdded += CheckEntityAdd;
				}

				if (!remove.Contains(componentId))
				{
					remove.Add(componentId);
					components.OnRemoved += CheckEntityRemove;
				}
			}

			foreach (var componentId in _any)
			{
				var components = _context.GetComponents(componentId);
				if (!add.Contains(componentId))
				{
					add.Add(componentId);
					components.OnAdded += CheckEntityAdd;
				}

				if (!remove.Contains(componentId))
				{
					remove.Add(componentId);
					components.OnRemoved += CheckEntityRemove;
				}
			}

			foreach (var componentId in _none)
			{
				var components = _context.GetComponents(componentId);
				if (!add.Contains(componentId))
				{
					add.Add(componentId);
					components.OnRemoved += CheckEntityAdd;
				}

				if (!remove.Contains(componentId))
				{
					remove.Add(componentId);
					components.OnAdded += CheckEntityRemove;
				}
			}
		}

		public override int GetHashCode()
		{
			if (_hashInitialized)
			{
				return _hash;
			}

			_hash = HashCodeUtils.Calculate(_all, _any, _none);
			_hashInitialized = true;
			return _hash;
		}

		public override bool Equals(object obj)
		{
			var matcher = obj as Matcher;
			if (matcher == null)
			{
				return false;
			}

			return EqualIndexes(_all, matcher._all) && EqualIndexes(_any, matcher._any) && EqualIndexes(_none, matcher._none);
		}

		internal void Lock()
		{
			Lock(_all);
			Lock(_any);
			Lock(_none);
		}

		internal void Unlock()
		{
			Unlock(_all);
			Unlock(_any);
			Unlock(_none);
		}

		private void Lock(List<int> componentIds)
		{
			foreach (var componentId in componentIds)
			{
				var components = _context.GetComponents(componentId);
				components.Lock();
			}
		}

		private void Unlock(List<int> componentIds)
		{
			foreach (var componentId in componentIds)
			{
				var components = _context.GetComponents(componentId);
				components.Unlock();
			}
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