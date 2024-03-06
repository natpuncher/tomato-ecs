using System;
using System.Collections.Generic;
using npg.tomatoecs.Entities;

namespace npg.tomatoecs.Groups
{
	internal sealed class GroupProvider : IDisposable
	{
		private const int DefaultCapacity = 4;

		private readonly Entities.Entities _entities;
		private readonly Context _context;
		private readonly EntityLinker _entityLinker;
		private readonly Stack<GroupBuilder> _groupBuilders;
		private readonly Stack<Matcher> _freeMatchers;
		private readonly Dictionary<Matcher, Group> _groups;

		internal GroupProvider(Entities.Entities entities, Context context, EntityLinker entityLinker)
		{
			_entities = entities;
			_context = context;
			_entityLinker = entityLinker;

			_groupBuilders = new Stack<GroupBuilder>(DefaultCapacity);
			_freeMatchers = new Stack<Matcher>(DefaultCapacity);
			_groups = new Dictionary<Matcher, Group>(DefaultCapacity);
		}

		internal GroupBuilder GetGroupBuilder()
		{
			if (!_groupBuilders.TryPop(out var groupBuilder))
			{
				groupBuilder = new GroupBuilder(_context, this);
			}

			groupBuilder.Initialize(GetMatcher());
			return groupBuilder;
		}

		internal void ReturnGroupBuilder(GroupBuilder groupBuilder)
		{
			_groupBuilders.Push(groupBuilder);
		}

		internal Group GetGroup(Matcher matcher)
		{
			if (_groups.TryGetValue(matcher, out var group))
			{
				matcher.Dispose();
				_freeMatchers.Push(matcher);
				return group;
			}

			group = new Group(matcher, _entities, _entityLinker);
			_groups.Add(matcher, group);
			return group;
		}

		public void Dispose()
		{
			foreach (var group in _groups.Values)
			{
				group.Dispose();
			}
		}

		private Matcher GetMatcher()
		{
			if (!_freeMatchers.TryPop(out var mather))
			{
				mather = new Matcher(_context);
			}

			return mather;
		}
	}
}