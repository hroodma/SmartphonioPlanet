using System;
using UnityEditor;
using UnityEngine;

public class UnitFactory : MonoBehaviour, IUnitFactory
{
    [Header("Animal prefabs")]
    [SerializeField] private GameObject animalPrefab;

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
        return data.Id;
    }

    public void SpawnWave()
    {
        SpawnAnimal(animalPrefab, animalSpawnpoint);
    }

    private void SpawnAnimal(GameObject prefab, Transform point)
    {
        if (prefab == null || point == null)
            return;

        GameObject go = Instantiate(prefab, point.position, point.rotation);

        // Префабы животных не трогаем - адаптеры навешиваем здесь, на границе.
        UnitBody body = go.GetComponent<UnitBody>();
        if (body == null)
            body = go.AddComponent<UnitBody>();

        if (go.GetComponent<UnitView>() == null)
            go.AddComponent<UnitView>();

        RegisterBody(body, animalStats, UnitKind.Animal);
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
        };
    }
}
