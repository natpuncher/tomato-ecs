using System;

namespace npg.tomato_ecs
{
	public struct Entity : IEquatable<Entity>
	{
		public uint Id { get; internal set; }
		public bool IsActive => Context != null;

		internal Context Context;

		public ref TComponent AddComponent<TComponent>() where TComponent : struct
		{
			return ref Context.AddComponent<TComponent>(Id);
		}

		public ref TComponent AddComponent<TComponent>(Components<TComponent> components)
			where TComponent : struct
		{
			return ref components.AddComponent(Id);
		}

		public bool HasComponent<TComponent>() where TComponent : struct
		{
			return Context.HasComponent<TComponent>(Id);
		}

		public bool HasComponent<TComponent>(Components<TComponent> components) where TComponent : struct
		{
			return components.HasComponent(Id);
		}

		public ref TComponent GetComponent<TComponent>() where TComponent : struct
		{
			return ref Context.GetComponents<TComponent>().GetComponent(Id);
		}

		public ref TComponent GetComponent<TComponent>(Components<TComponent> components) where TComponent : struct
		{
			return ref components.GetComponent(Id);
		}

		public ref TComponent GetComponent<TComponent>(ReactiveComponents<TComponent> reactiveComponents)
			where TComponent : struct, IEquatable<TComponent>
		{
			return ref reactiveComponents.ActualComponents.GetComponent(Id);
		}

		public void RemoveComponent<TComponent>() where TComponent : struct
		{
			Context.RemoveComponent<TComponent>(Id);
		}

		public void RemoveComponent<TComponent>(Components<TComponent> components) where TComponent : struct
		{
			components.RemoveComponent(Id);
		}

		public void RemoveComponent<TComponent>(ReactiveComponents<TComponent> reactiveComponents)
			where TComponent : struct, IEquatable<TComponent>
		{
			reactiveComponents.ActualComponents.RemoveComponent(Id);
		}

		public ref TComponent GetPreviousComponent<TComponent>(ReactiveComponents<TComponent> reactiveComponents)
			where TComponent : struct, IEquatable<TComponent>
		{
			return ref reactiveComponents.GetPreviousComponent(Id);
		}

		public void Link(Entity entity)
		{
			Context.Link(Id, entity.Id);
		}

		public bool HasLink(Entity entity)
		{
			return Context.HasLink(Id, entity.Id);
		}

		public void Unlink(Entity entity)
		{
			Context.Unlink(Id, entity.Id);
		}

		public void UnlinkAll()
		{
			Context.UnlinkAll(Id);
		}

		public void Destroy()
		{
			Context.DestroyEntity(Id);
			Context = null;
		}

		public override string ToString()
		{
			return $"[ecs] entity [{Id}]";
		}

		public bool Equals(Entity other)
		{
			return Id == other.Id && Equals(Context, other.Context);
		}

		public override bool Equals(object obj)
		{
			return obj is Entity other && Equals(other);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Context, Id);
		}
	}
}