using UnityEngine;

public class PlanetValues : MonoBehaviour
{
    [SerializeField] private GameObject _planet;
    public Vector3 center;
    public float radius;

    private void Awake()
    {
        center = _planet.transform.position;
        radius = _planet.transform.localScale.x / 2;
    }
}
