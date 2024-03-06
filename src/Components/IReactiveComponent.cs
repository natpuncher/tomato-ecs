namespace npg.tomatoecs.Components
{
	public interface IReactiveComponent<TComponent> where TComponent : struct
	{
		bool Equals(TComponent component);
	}
}