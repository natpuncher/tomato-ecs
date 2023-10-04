namespace npg.tomato_ecs.Groups
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
	
		public GroupBuilder Include<TComponent>() where TComponent : struct
		{
			_matcher.Include(_context.GetComponents<TComponent>().Id);
			return this;
		}

		public GroupBuilder Exclude<TComponent>() where TComponent : struct
		{
			_matcher.Exclude(_context.GetComponents<TComponent>().Id);
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