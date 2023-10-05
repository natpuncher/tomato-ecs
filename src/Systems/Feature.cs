using System.Collections.Generic;

namespace npg.tomatoecs.Systems
{
	public class Feature : IInitializeSystem, IExecuteSystem, ITeardownSystem
	{
		private const int DefaultSystemCapacity = 16;

		private readonly List<IInitializeSystem> _initializeSystems = new(DefaultSystemCapacity);
		private readonly List<IExecuteSystem> _executeSystems = new(DefaultSystemCapacity);
		private readonly List<ITeardownSystem> _teardownSystems = new(DefaultSystemCapacity);

		protected void Add(ISystem system)
		{
			if (system is IInitializeSystem initializeSystem)
			{
				_initializeSystems.Add(initializeSystem);
			}

			if (system is IExecuteSystem executeSystem)
			{
				_executeSystems.Add(executeSystem);
			}

			if (system is ITeardownSystem teardownSystem)
			{
				_teardownSystems.Add(teardownSystem);
			}
		}

		public void Initialize()
		{
			for (var i = 0; i < _initializeSystems.Count; i++)
			{
				_initializeSystems[i].Initialize();
			}
		}

		public void Execute()
		{
			for (var i = 0; i < _executeSystems.Count; i++)
			{
				_executeSystems[i].Execute();
			}
		}

		public void Teardown()
		{
			for (var i = 0; i < _teardownSystems.Count; i++)
			{
				_teardownSystems[i].Teardown();
			}
		}
	}
}