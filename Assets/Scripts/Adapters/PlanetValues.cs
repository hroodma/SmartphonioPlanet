using UnityEngine;

public class PlanetValues : MonoBehaviour
{
    [SerializeField] private GameObject _planet;
    public Vector3 center;
    public float radius;

    public void GetPlanetData()
    {
        if (_planet != null)
        {
            center = _planet.transform.position;
            radius = _planet.transform.localScale.x / 2f;
        }
    }
}