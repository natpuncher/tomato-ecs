![](https://img.shields.io/badge/unity-2021%20or%20later-green)
[![](https://img.shields.io/github/license/natpuncher/tomato-ecs)](https://github.com/natpuncher/tomato-ecs/blob/master/LICENSE)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-blue.svg?style=flat-square)](https://makeapullrequest.com)

# tomato-ecs

Ecs framework for Unity mostly for tomato games.

* [Installation](#installation)
* [Usage](#usage)

## Installation
* In **Package Manager** press `+`, select `Add package from git URL` and paste `https://github.com/natpuncher/tomato-ecs.git`
* Or find the `manifest.json` file in the `Packages` folder of your project and add the following line to dependencies section:
```json
{
 "dependencies": {
    "com.npg.tomato-ecs": "https://github.com/natpuncher/tomato-ecs.git",
 },
}
```

## Usage

```c#
public struct DamageComponent
{
    public int Amount;
    public uint Source;
    public uint Target;
}
```
```c#
public sealed class DamageApplySystem : IInitializeSystem, IExecuteSystem
{
	private readonly BattleContext _battleContext;
	private Components<HealthComponent> _health;
	private Components<DeadComponent> _dead;
	private Components<DamageComponent> _damage;
	private Components<DamageDealtComponent> _damageDealt;
	private Components<CleanupComponent> _cleanup;

	public DamageApplySystem(BattleContext battleContext)
	{
		_battleContext = battleContext;
	}

	public void Initialize()
	{
		_damage = _battleContext.GetComponents<DamageComponent>();

		_health = _battleContext.GetComponents<HealthComponent>();
		_dead = _battleContext.GetComponents<DeadComponent>();
		_damageDealt = _battleContext.GetComponents<DamageDealtComponent>();
		_cleanup = _battleContext.GetComponents<CleanupComponent>();
	}

	public void Execute()
	{
		foreach (var damageEntity in _damage.GetEntities())
		{
			ref var damageComponent = ref damageEntity.GetComponent(_damage);
			var targetEntity = _battleContext.GetEntity(damageComponent.Target);

			if (!IsValidTarget(targetEntity))
			{
				continue;
			}

			ref var targetHealthComponent = ref targetEntity.GetComponent(_health);
			targetHealthComponent.Current -= damageComponent.Amount;

			CreateDamageDealtEvent(damageComponent);
		}
	}

	private void CreateDamageDealtEvent(DamageComponent damageComponent)
	{
		var damageDealtEntity = _battleContext.CreateEntity();
		damageDealtEntity.AddComponent(_cleanup);
		ref var damageDealtComponent = ref damageDealtEntity.AddComponent(_damageDealt);
		damageDealtComponent.Source = damageComponent.Source;
		damageDealtComponent.Target = damageComponent.Target;
		damageDealtComponent.Amount = damageComponent.Amount;
	}

	private bool IsValidTarget(Entity entity)
	{
		return entity.HasComponent(_health) && !entity.HasComponent(_dead);
	}
}
```
```c#
public void UpdateEcs(double delta)
{
	_battleContext.DeltaTime = delta;
	_battleContext.UpdateReactiveComponents();
	_battleEcsRunner.Execute();
}

public class UpdateUnitHealthSystem : IInitializeSystem, IExecuteSystem
{
	private readonly BattleContext _battleContext;
	private Components<UnitViewComponent> _unitView;
	private ReactiveComponents<HealthComponent> _health;

	public UpdateUnitHealthSystem(BattleContext battleContext)
	{
		_battleContext = battleContext;
	}

	public void Initialize()
	{
		_unitView = _battleContext.GetComponents<UnitViewComponent>();
		_health = _battleContext.GetReactiveComponents<HealthComponent>();
	}

	public void Execute()
	{
		foreach (var entity in _health.Changed())
		{
			ref var unitViewComponent = ref entity.GetComponent(_unitView);
			ref var healthComponent = ref entity.GetComponent(_health);
			unitViewComponent.Value.SetHealth(healthComponent.Current * 1f / healthComponent.Max);
		}
	}
}
```
```c#
public sealed class ReadyToAttackSystem : IInitializeSystem, IExecuteSystem
{
	private readonly BattleContext _battleContext;
	
	private Group _expiredActionCooldowns;

	public ReadyToAttackSystem(BattleContext battleContext)
	{
		_battleContext = battleContext;
	}

	public void Initialize()
	{
		_expiredActionCooldowns = _battleContext.GetGroup<UnitActionCooldownComponent>().Include<TimerExpiredComponent>().Build();
	}

	public void Execute()
	{
		foreach (var cooldownEntity in _expiredActionCooldowns.GetEntities())
		{
		}
	}
}
```
```c#
var unitEntity = context.CreateEntity();
var abilityEntity = context.CreateEntity();
abilityEntity.AddComponent<EvadeAbilityComponent>();
unitEntity.Link(abilityEntity);

var evade = context.GetComponents<EvadeAbilityComponent>();
// get all evade abilities linked to this unit
foreach(var evadeAbilityEntity in evade.GetLinkedEntities(unitEntity))
{
    // evadeAbilityEntity == abilityEntity 
}
```
```c#
	public void Initialize()
	{
		_damageDealt = _battleContext.GetComponents<DamageDealtComponent>();
	}

	public void Execute()
	{
		foreach (var damageDealt in _damageDealt)
		{
			CreateView(damageDealt.Source, damageDealt.Target, damageDealt.Amount);
		}
	}


```

```c#
	public void Initialize()
	{
		_damage = _battleContext.GetComponents<DamageComponent>();

		_armor = _battleContext.GetComponents<ArmorStatComponent>();
	}

	public void Execute()
	{
		foreach (var damageEntity in _damage.GetEntities())
		{
			ref var damageComponent = ref damageEntity.GetComponent(_damage);
			var targetEntity = _battleContext.GetEntity(damageComponent.Target);

			var armor = 0;
			foreach (var armorComponent in _armor.Linked(targetEntity))
			{
				armor += armorComponent.Amount;
			}
		}
	}
```
