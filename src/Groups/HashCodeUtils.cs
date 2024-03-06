using System.Collections.Generic;

namespace npg.tomatoecs.Groups
{
	public static class HashCodeUtils
	{
		public static int Calculate(List<int> all, List<int> any, List<int> none)
		{
			all.Sort();
			any.Sort();
			none.Sort();

			unchecked
			{
				var result = all.Count * 17 + any.Count * 23 + none.Count * 31;
				foreach (var id in all)
				{
					result = result * 393241 + id;
				}

				foreach (var id in any)
				{
					result = result * 393241 + id;
				}

				foreach (var id in none)
				{
					result = result * 393241 + id;
				}

				return result;
			}
		}
	}
}