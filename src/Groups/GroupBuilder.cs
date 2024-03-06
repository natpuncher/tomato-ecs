namespace npg.tomatoecs.Groups
{
	public sealed class GroupBuilder
	{
		private readonly Context _context;
		private readonly GroupProvider _groupProvider;

		private Matcher _matcher;

		internal GroupBuilder(Context context, GroupProvider groupProvider)
		{
			_context = context;
			_groupProvider = groupProvider;
		}

		internal void Initialize(Matcher matcher)
		{
			_matcher = matcher;
		}

		public GroupBuilder All<TComponent>() where TComponent : struct
		{
			_matcher.All(_context.GetComponents<TComponent>().Id);
			return this;
		}

		public GroupBuilder Any<TComponent>() where TComponent : struct
		{
			_matcher.Any(_context.GetComponents<TComponent>().Id);
			return this;
		}

		public GroupBuilder None<TComponent>() where TComponent : struct
		{
			_matcher.None(_context.GetComponents<TComponent>().Id);
			return this;
		}

		public Group Build()
		{
			var group = _groupProvider.GetGroup(_matcher);
			_groupProvider.ReturnGroupBuilder(this);
			_matcher = null;
			return group;
		}
	}
}