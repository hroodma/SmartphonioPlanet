using UnityEditor;
using UnityEngine;

public class UnitFactory : MonoBehaviour, IUnitFactory, IUnitSink
{
    [Header("Wave count")]
    [SerializeField] private int waveCount;

    [Header("Animal prefabs")]
    [SerializeField] private GameObject rabbitPrefab;
    [SerializeField] private GameObject sheepPrefab;
    [SerializeField] private GameObject horsePrefab;
    [SerializeField] private GameObject cowPrefab;
    [SerializeField] private GameObject pigPrefab;

    [Header("Animal stats (ScriptableObject)")]
    [SerializeField] private AnimalStats rabbitStats;
    [SerializeField] private AnimalStats sheepStats;
    [SerializeField] private AnimalStats horseStats;
    [SerializeField] private AnimalStats cowStats;
    [SerializeField] private AnimalStats pigStats;

    private World world;
    private Bindings bindings;

    public void Init(World world, Bindings bindings)
    {
        this.world = world;
        this.bindings = bindings;
    }

    public int RegisterBody(UnitBody body, UnitStats stats, UnitKind kind)
    {
        IEntity entity = MakeEntity(stats, kind, body.Position);
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
        for (int i = 0; i < waveCount; i++)
        {
            SpawnAnimal(rabbitPrefab, RandomPointOnSphere(), rabbitStats);
            SpawnAnimal(sheepPrefab, RandomPointOnSphere(), sheepStats);
            SpawnAnimal(horsePrefab, RandomPointOnSphere(), horseStats);
            SpawnAnimal(cowPrefab, RandomPointOnSphere(), cowStats);
            SpawnAnimal(pigPrefab, RandomPointOnSphere(), pigStats);
        }
    }

    private void SpawnAnimal(GameObject prefab, Vector3 point, AnimalStats stats)
    {
        if (prefab == null || point == null)
            return;

        GameObject go = Instantiate(prefab, point, Quaternion.identity);

        UnitBody body = go.GetComponent<UnitBody>();
        if (body == null)
            body = go.AddComponent<UnitBody>();

        if (go.GetComponent<UnitView>() == null)
            go.AddComponent<UnitView>();

        RegisterBody(body, stats, UnitKind.Animal);
    }

    public void Respawn(int id)
    {
        if (!world.Entities.TryGetValue(id, out IEntity e))
            return;

        e.Data.Alive = true;
        e.Data.DesiredVelocity = Vector3.zero;

        MoveOnOtherPosition(e);
    }

    public void Uncaught(int id)
    {
        if (world.Entities.TryGetValue(id, out IEntity e) && e is ICaughtable caughtable)
        {
            caughtable.IsCaughted = false;
            MoveOnOtherPosition(caughtable);
        }
    }

    private void MoveOnOtherPosition(IEntity entity)
    {
        if (bindings.Bodies.TryGetValue(entity.Data.Id, out IBody body) && body is Component c)
        {
            Vector3 newRandomPoint = RandomPointOnSphere();

            c.transform.position = newRandomPoint;
            entity.Data.Position = newRandomPoint;
        }
    }

    private Vector3 RandomPointOnSphere()
    {
        if (world.Player == null)
            return default;

        Vector3 playerPos = world.Player.Data.Position;
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

    private static IEntity MakeEntity(UnitStats s, UnitKind kind, Vector3 pos)
    {
        Vector3 upDir = (pos - new Vector3(0, 0, 0)).normalized;

        UnitData baseData = new UnitData
        {
            Kind = kind,
            Position = pos,
            MaxSpeed = s.maxSpeed,
            MoveSpeed = s.moveSpeed,
            Acceleration = s.acceleration
        };

        switch (s)
        {
            case AnimalStats animalStats:
                return new AnimalData
                {
                    Data = baseData,

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

                    InteractionRadius = playerStats.interactionRadius,
                    CaughtAnimals = 0,
                    SumBonusTime = 0f
                };

            default:
                Debug.LogError($"Неизвестный тип UnitStats при создании сущности: {s.GetType()}");
                return null;
        }
    }
}
