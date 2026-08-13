using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

public interface IEntity
{
    UnitData Data { get; }
}

public interface IMoveable : IEntity
{
    MovementData Movement { get; set; }
}

public interface IInteractable : IEntity { }

public interface IBooster : IInteractable, IMoveable
{
    float Value { get; set; }
    bool IsTaken {  get; set; }
}

public interface ISpeedBooster : IBooster
{
    float Duration { get; set; }
}

public interface ICaughtable : IEntity
{
    bool IsCaughted { get; set; }
}

public interface IPlayer : IMoveable
{
    bool IsInteract { get; set; }
    IInteractable Interactable { get; set; }
}

public interface IAnimal : IInteractable, ICaughtable, IMoveable
{
    float UnitBonusTime { get; set; }

    float DetectionDistance { get; set; }
    float MinDirectionDistance { get; set; }
    float MaxDirectionDistance { get; set; }
    float CurrentWalkDistance { get; set; }
    float TargetWalkDistance { get; set; }
    Vector3 TargetForward { get; set; }

    bool IsTurning { get; set; }
}

public enum UnitKind { Player, Animal, Booster}
public enum AnimalTag { None, Rabbit, Cow, Pig, Sheep, Horse }

public sealed class UnitData
{
    public int Id;
    public UnitKind Kind;

    public bool Alive = true;        
}

public sealed class PlayerData : IPlayer
{
    public UnitData Data { get; set; }
    public MovementData Movement { get; set; }

    public float SumBonusTime;
    public int CaughtAnimals;

    public float MoveInput;
    public float TurnInput;
    public float InteractionRadius;

    public bool IsInteract { get; set; }
    public IInteractable Interactable { get; set; }

    public bool PlayPickupSound { get; set; }
}

public sealed class AnimalData : IAnimal
{
    public UnitData Data { get; set; }
    public MovementData Movement { get; set; }

    public AnimalTag Tag;

    public float DetectionDistance { get; set; }
    public float MinDirectionDistance { get; set; }
    public float MaxDirectionDistance { get; set; }
    public float CurrentWalkDistance { get; set; }
    public float TargetWalkDistance { get; set; }
    public Vector3 TargetForward { get; set; }

    public float UnitBonusTime { get; set; }

    public bool IsTurning { get; set; }
    public bool IsCaughted { get; set; }
}

public sealed class SpeedBoosterData : ISpeedBooster
{
    public UnitData Data { get; set; }
    public MovementData Movement { get; set; }

    public float Value { get; set; }
    public float Duration { get; set; }
    public bool IsTaken { get; set; }
}

public sealed class MovementData
{
    public Vector3 Position;
    public Vector3 HorizontalVelocity;
    public Vector3 VerticalVelocity;
    public Vector3 DesiredVelocity;

    public Vector3 Forward;
    public Vector3 Right;

    public float MaxSpeed;
    public float DefaultSpeed;
    public float CurrentSpeed;
    public float Acceleration;
    public Vector3 MoveDirection;
    public Vector3 UpDirection;

    public float TurnSpeed = 60f;
}

public sealed class PlanetData
{
    public Vector3 Center;
    public float GravityStrength;
    public float Radius;
}

public sealed class MatchState
{
    public bool Over;
    public float Timer;
}

public sealed class World
{
    public readonly Dictionary<int, IEntity> Entities = new();

    public readonly Dictionary<int, AnimalData> Animals = new();
    public readonly Dictionary<int, SpeedBoosterData> SpeedBoosters = new();
    public PlayerData Player;

    public readonly PlanetData Planet = new();
    public readonly MatchState Match = new();

    private int nextId = 1;

    public T Add<T>(T entity) where T : IEntity
    {
        entity.Data.Id = nextId++;
        Entities[entity.Data.Id] = entity;

        switch (entity)
        {
            case PlayerData playerData:
                Player = playerData;
                break;
            case AnimalData animalData:
                Animals[animalData.Data.Id] = animalData;
                break;
            case SpeedBoosterData speedBoosterData:
                SpeedBoosters[speedBoosterData.Data.Id] = speedBoosterData;
                break;
        }
        return entity;
    }

    public void Remove(int id)
    {
        if (Entities.TryGetValue(id, out IEntity entity))
        {
            Entities.Remove(id);

            switch (entity)
            {
                case PlayerData:
                    Player = null;
                    break;

                case AnimalData:
                    Animals.Remove(id);
                    break;

                case SpeedBoosterData:
                    SpeedBoosters.Remove(id);
                    break;
            }
        }
    }
}

public sealed class Bindings
{
    public readonly Dictionary<int, IBody> Bodies = new Dictionary<int, IBody>();
    public readonly Dictionary<int, IUnitView> Views = new Dictionary<int, IUnitView>();
    public readonly Dictionary<int, IUnitSound> Sounds = new Dictionary<int, IUnitSound>();
    public IPlayerUI PlayerUI;
    public IPlayerInput PlayerInput;
    public int PlayerId;
}

public struct PlayerCommand
{
    public bool Move;
    public Vector2 MoveAxis;
} 