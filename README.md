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
public void UpdateEcs(double delta)
{
	_battleContext.DeltaTime = delta;
	_battleContext.Update();
	_battleEcsRunner.Execute();
}
```
```c#
public struct DamageComponent
{
    public int Amount;
    public uint Source;
    public uint Target;
}
```
```c#
public class TimerSystem : IInitializeSystem, IExecuteSystem
{
	private readonly BattleContext _battleContext;

	private Components<TimerComponent> _timer;
	private Components<TimerExpiredComponent> _timerExpired;
	private Components<CleanupAfterTimerComponent> _cleanupAfterTimer;
	private Components<CleanupComponent> _cleanup;

	public TimerSystem(BattleContext battleContext)
	{
		_battleContext = battleContext;
	}

	public void Initialize()
	{
		_timer = _battleContext.GetComponents<TimerComponent>();
		_timerExpired = _battleContext.GetComponents<TimerExpiredComponent>();
		_cleanupAfterTimer = _battleContext.GetComponents<CleanupAfterTimerComponent>();
		_cleanup = _battleContext.GetComponents<CleanupComponent>();
	}

	public void Execute()
	{
		foreach (var timerEntity in _timer.GetEntities())
		{
			ref var timer = ref timerEntity.GetComponent(_timer);
			timer.Current += _battleContext.DeltaTime;
			if (timer.Current < timer.Max)
			{
				continue;
			}

			timerEntity.RemoveComponent(_timer);
			timerEntity.AddComponent(_timerExpired);
			if (timerEntity.HasComponent(_cleanupAfterTimer))
			{
				timerEntity.AddComponent(_cleanup);
			}
		}
	}
}
```
```c#
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
	private Components<ReadyToAttackComponent> _readyToAttack;
	private Components<AttackDamageComponent> _attackDelay;
	
	private Group _expiredActionCooldowns;
    
	public ReadyToAttackSystem(BattleContext battleContext)
	{
		_battleContext = battleContext;
	}

	public void Initialize()
	{
		_expiredActionCooldowns = _battleContext.GetGroup().
		All<UnitActionCooldownComponent>().
		All<TimerExpiredComponent>().
		Build();
        
		_attackDelay = _battleContext.GetComponents<AttackDamageComponent>();
		_readyToAttack = _battleContext.GetComponents<ReadyToAttackComponent>();
	}

	public void Execute()
	{
		foreach (var cooldownEntity in _expiredActionCooldowns)
		{
			foreach (var unitEntity in _attackDelay.GetLinkedEntities(cooldownEntity))
			{
				unitEntity.AddComponent(_readyToAttack);
			}

			cooldownEntity.Destroy();
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
		foreach (ref var damageDealt in _damageDealt)
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
		foreach (ref var damageComponent in _damage)
		{
			var targetEntity = _battleContext.GetEntity(damageComponent.Target);

			var armor = 0;
			foreach (ref var armorComponent in _armor.LinkedTo(targetEntity))
			{
				armor += armorComponent.Amount;
			}

			if (armor == 0)
			{
				continue;
			}
			
			var reducedAmount = (int)(BattleMath.ArmorDamageReduction(armor) * damageComponent.Amount);
			damageComponent.Amount -= reducedAmount;
			if (damageComponent.Amount < 0)
			{
				damageComponent.Amount = 0;
			}
		}
	}
```
