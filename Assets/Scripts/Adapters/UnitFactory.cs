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

    // Усыновляет объект с UnitBody (игрок, зверь) как юнит данных + тело + вью.
    public int RegisterBody(UnitBody body, UnitStats stats, UnitKind kind)
    {
        UnitData data = MakeData(stats, kind, body.Position);
        world.Add(data);

        Tag(body.gameObject, data.Id);
        bindings.Bodies[data.Id] = body;

        UnitView view = body.GetComponent<UnitView>();
        if (view != null)
            bindings.Views[data.Id] = view;

        UnitSound sound = body.GetComponent<UnitSound>();
        if (sound != null)
            bindings.Sounds[data.Id] = sound;

        return data.Id;
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
        if (!world.Units.TryGetValue(id, out UnitData u))
            return;

        u.Alive = true;
        u.DesiredVelocity = Vector3.zero;

        if (bindings.Bodies.TryGetValue(id, out IBody body) && body is Component c)
        {
            Vector3 newRandomPoint = RandomPointOnSphere();

            c.transform.position = newRandomPoint;
            u.Position = newRandomPoint;
        }
    }

    private Vector3 RandomPointOnSphere()
    {
        if (!world.Units.TryGetValue(bindings.PlayerId, out UnitData player))
            return default;

        Vector3 playerPos = player.Position;
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

    // Метка id на GameObject юнита
    private static void Tag(GameObject go, int id)
    {
        UnitRef unit = go.GetComponent<UnitRef>();
        if (unit == null)
            unit = go.AddComponent<UnitRef>();
        unit.Id = id;
    }

    private static UnitData MakeData(UnitStats s, UnitKind kind, Vector3 pos)
    {
        UnitData newUnitData = new UnitData();

        newUnitData.Kind = kind;
        newUnitData.Position = pos;
        newUnitData.MaxSpeed = s.maxSpeed;
        newUnitData.MoveSpeed = s.moveSpeed;
        newUnitData.Acceleration = s.acceleration;
        newUnitData.InteractionRadius = s.interactionRadius;
        newUnitData.Tag = AnimalTag.None;

        switch (s)
        {
            case AnimalStats animal:
                newUnitData.UnitBonusTime = animal.unitBunusTime;
                newUnitData.DetecionDistance = animal.detectionDistance;
                newUnitData.MinDirectionDistance = animal.minDirectionDistance;
                newUnitData.MaxDirectionDistance = animal.maxDirectionDistance;
                newUnitData.IsTurning = false;
                newUnitData.Tag = animal.tag;
                break;

            default:
                Debug.LogWarning($"Неизвестный тип UnitStats: {s.GetType()}");
                break;
        }

        return newUnitData;
    }
}
