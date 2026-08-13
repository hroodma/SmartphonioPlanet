using UnityEditor;
using UnityEngine;

public class UnitFactory : MonoBehaviour, IUnitFactory, IUnitSink
{
    [Header("Wave count")]
    [SerializeField] private int animalEveryoneCount;
    [SerializeField] private int speedBoosterCount;

    [Header("Animal prefabs")]
    [SerializeField] private GameObject rabbitPrefab;
    [SerializeField] private GameObject sheepPrefab;
    [SerializeField] private GameObject horsePrefab;
    [SerializeField] private GameObject cowPrefab;
    [SerializeField] private GameObject pigPrefab;

    [Header("Animal Animal stats (ScriptableObject)")]
    [SerializeField] private AnimalStats rabbitStats;
    [SerializeField] private AnimalStats sheepStats;
    [SerializeField] private AnimalStats horseStats;
    [SerializeField] private AnimalStats cowStats;
    [SerializeField] private AnimalStats pigStats;

    [Header("Booster prefabs")]
    [SerializeField] private GameObject speedBoosterPrefab;

    [Header("Booster stats (ScriptableObject)")]
    [SerializeField] private SpeedBoosterStats speedBoosterStats;

    private World world;
    private Bindings bindings;

    public void Init(World world, Bindings bindings)
    {
        this.world = world;
        this.bindings = bindings;
    }

    public int RegisterBody(UnitBody body, UnitStats stats, UnitKind kind)
    {
        IEntity entity = MakeEntity(stats, kind, body.Position, world.Planet);
        world.Add(entity);

        Tag(body.gameObject, entity.Data.Id);
        bindings.Bodies[entity.Data.Id] = body;

        UnitView view = body.GetComponent<UnitView>();
        if (view != null)
            bindings.Views[entity.Data.Id] = view;

        UnitSound sound = body.GetComponent<UnitSound>();
        if (sound != null)
            bindings.Sounds[entity.Data.Id] = sound;

        return entity.Data.Id;
    }

    public void SpawnWave()
    {
        for (int i = 0; i < animalEveryoneCount; i++)
        {
            SpawnAnimal(rabbitPrefab, RandomPointOnSphere(), rabbitStats);
            SpawnAnimal(sheepPrefab, RandomPointOnSphere(), sheepStats);
            SpawnAnimal(horsePrefab, RandomPointOnSphere(), horseStats);
            SpawnAnimal(cowPrefab, RandomPointOnSphere(), cowStats);
            SpawnAnimal(pigPrefab, RandomPointOnSphere(), pigStats);            
        }

        for (int i = 0; i < speedBoosterCount; i++)
        {
            SpawnBooster(speedBoosterPrefab, RandomPointOnSphere(), speedBoosterStats);
        }        
    }

    private void SpawnAnimal(GameObject prefab, Vector3 point, AnimalStats stats)
    {
        if (prefab == null || point == null)
            return;

        Vector3 upDir = (point - world.Planet.Center).normalized;
        Vector3 randomForward = Vector3.ProjectOnPlane(UnityEngine.Random.onUnitSphere, upDir).normalized;

        if (randomForward.sqrMagnitude < 0.001f)
            randomForward = Vector3.Cross(upDir, Vector3.right).normalized;

        Quaternion startRotation = Quaternion.LookRotation(randomForward, upDir);

        GameObject go = Instantiate(prefab, point, startRotation);

        UnitBody body = go.GetComponent<UnitBody>();
        if (body == null)
            body = go.AddComponent<UnitBody>();

        if (go.GetComponent<UnitView>() == null)
            go.AddComponent<UnitView>();

        RegisterBody(body, stats, UnitKind.Animal);
    }

    private void SpawnBooster(GameObject prefab, Vector3 point, SpeedBoosterStats stats)
    {
        if (prefab == null || point == null)
            return;

        Vector3 upDir = (point - world.Planet.Center).normalized;
        Vector3 randomForward = Vector3.ProjectOnPlane(Random.onUnitSphere, upDir).normalized;

        if (randomForward.sqrMagnitude < 0.001f)
            randomForward = Vector3.Cross(upDir, Vector3.right).normalized;

        Quaternion startRotation = Quaternion.LookRotation(randomForward, upDir);

        GameObject go = Instantiate(prefab, point, startRotation);

        UnitBody body = go.GetComponent<UnitBody>();
        if (body == null)
            body = go.AddComponent<UnitBody>();

        if (go.GetComponent<UnitView>() == null)
            go.AddComponent<UnitView>();

        RegisterBody(body, stats, UnitKind.Booster);
    }

    public void Respawn(int id)
    {
        if (!world.Entities.TryGetValue(id, out IEntity e))
            return;

        if (e is IMoveable moveable)
        {
            moveable.Data.Alive = true;
            moveable.Movement.DesiredVelocity = Vector3.zero;

            MoveOnOtherPosition(moveable);
        }

        if (e is IBooster booster)
        {
            booster.IsTaken = false;
            MoveOnOtherPosition(booster);
        }
    }

    public void Uncaught(int id)
    {
        if (world.Entities.TryGetValue(id, out IEntity e) && e is ICaughtable caughtable)
        {
            caughtable.IsCaughted = false;

            if (caughtable is IMoveable moveable)
                MoveOnOtherPosition(moveable);
        }
    }

    private void MoveOnOtherPosition(IMoveable moveable)
    {
        if (bindings.Bodies.TryGetValue(moveable.Data.Id, out IBody body) && body is Component c)
        {
            Vector3 newRandomPoint = RandomPointOnSphere();

            c.transform.position = newRandomPoint;
            moveable.Movement.Position = newRandomPoint;
        }
    }

    private Vector3 RandomPointOnSphere()
    {
        if (world.Player == null)
            return default;

        Vector3 playerPos = world.Player.Movement.Position;
        float radius = world.Planet.Radius;
        Vector3 center = world.Planet.Center;

        const float requiredDistanceToPlayer = 20f;
        const float requiredDistanceToOtherAnimal = 2f;

        Vector3 point;
        int attempts = 0;
        const int maxAttempts = 100;
        Collider[] otherInteractable = new Collider[1];

        do
        {
            Vector3 direction = Random.onUnitSphere;

            point = center + direction.normalized * radius;

            int count = Physics.OverlapSphereNonAlloc(
                point,
                requiredDistanceToOtherAnimal,
                otherInteractable,
                LayerMask.GetMask("Interactable")
            );

            attempts++;

            if (attempts >= maxAttempts)
            {
                break;
            }

            if (Vector3.Distance(point, playerPos) > requiredDistanceToPlayer && count == 0)
                break;
        }
        while (true);

        if (attempts == maxAttempts)
            Debug.Log($"Использовано максимальное количество попыток, поэтому точка поставилась не совсем там где надо: {point}");

        return point;
    }

    private static void Tag(GameObject go, int id)
    {
        UnitRef unit = go.GetComponent<UnitRef>();
        if (unit == null)
            unit = go.AddComponent<UnitRef>();
        unit.Id = id;
    }

    private static IEntity MakeEntity(UnitStats s, UnitKind kind, Vector3 pos, PlanetData planet)
    {
        UnitData baseData = new UnitData
        {
            Kind = kind
        };
        MovementData newMovementData = new();
        
        if (s is MovementStats movement)
        {
            Vector3 upDir = (pos - planet.Center).normalized;
            Vector3 randomDir = Random.onUnitSphere;
            Vector3 forwardDir = Vector3.ProjectOnPlane(randomDir, upDir).normalized;
            if (forwardDir.sqrMagnitude < 0.001f)
            {
                forwardDir = Vector3.Cross(upDir, Vector3.right).normalized;
            }

            Vector3 rightDir = Vector3.Cross(upDir, forwardDir).normalized;

            MovementData movementData = new MovementData
            {
                Position = pos,
                MaxSpeed = movement.maxSpeed,
                DefaultSpeed = movement.defaultSpeed,
                CurrentSpeed = movement.defaultSpeed,
                Acceleration = movement.acceleration,
                UpDirection = upDir,
                Forward = forwardDir,
                Right = rightDir
            };

            newMovementData = movementData;
        }
        
        switch (s)
        {
            case AnimalStats animalStats:
                return new AnimalData
                {
                    Data = baseData,
                    Movement = newMovementData,

                    Tag = animalStats.tag,
                    UnitBonusTime = animalStats.unitBunusTime,
                    DetectionDistance = animalStats.detectionDistance,
                    MinDirectionDistance = animalStats.minDirectionDistance,
                    MaxDirectionDistance = animalStats.maxDirectionDistance,

                    IsTurning = false,
                    CurrentWalkDistance = 0f,
                    TargetWalkDistance = Random.Range(animalStats.minDirectionDistance, animalStats.maxDirectionDistance)
                };

            case PlayerStats playerStats:
                return new PlayerData
                {
                    Data = baseData,
                    Movement = newMovementData,

                    InteractionRadius = playerStats.interactionRadius,
                    CaughtAnimals = 0,
                    SumBonusTime = 0f
                };

            case SpeedBoosterStats speedBoosterStats:
                return new SpeedBoosterData
                {
                    Data = baseData,
                    Movement = newMovementData,

                    Value = speedBoosterStats.value,
                    Duration = speedBoosterStats.duration,
                    IsTaken = false
                };

            default:
                Debug.LogError($"Неизвестный тип UnitStats при создании сущности: {s.GetType()}");
                return null;
        }
    }
}
