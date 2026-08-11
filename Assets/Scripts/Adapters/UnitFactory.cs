using UnityEditor;
using UnityEngine;

public class UnitFactory : MonoBehaviour, IUnitFactory, IUnitSink
{
    [Header("Wave count")]
    [SerializeField] private int waveCount;

    [Header("Animal prefabs")]
    [SerializeField] private GameObject rabbitPrefab;

    [Header("Animal spawn points")]
    [SerializeField] private Transform animalSpawnpoint;

    [Header("Animal stats (ScriptableObject)")]
    [SerializeField] private UnitStats animalStats;

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
            SpawnAnimal(rabbitPrefab, RandomPointOnSphere());
        }
    }

    private void SpawnAnimal(GameObject prefab, Vector3 point)
    {
        if (prefab == null || point == null)
            return;

        GameObject go = Instantiate(prefab, point, Quaternion.identity);

        UnitBody body = go.GetComponent<UnitBody>();
        if (body == null)
            body = go.AddComponent<UnitBody>();

        if (go.GetComponent<UnitView>() == null)
            go.AddComponent<UnitView>();

        RegisterBody(body, animalStats, UnitKind.Animal);
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
        return new UnitData
        {
            Kind = kind,
            Position = pos,
            MoveSpeed = s.moveSpeed,
            Acceleration = s.acceleration,
            InteractionRadius = s.interactionRadius,
            UnitBonusTime = s.unitBunusTime
        };
    }
}
