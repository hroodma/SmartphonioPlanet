using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public sealed class UnitBody : MonoBehaviour, IBody
{
    private Rigidbody rb;
    private Rigidbody Rb => rb != null ? rb : (rb = GetComponent<Rigidbody>());

    public Vector3 Position => Rb.position;
    public Vector3 Forward => Rb.transform.forward;
    public Vector3 Right => Rb.transform.right;

    public void Apply(Vector3 velocity, Vector3 up, Vector3 forward)
    {
        Rb.rotation = Quaternion.LookRotation(forward, up);

        Rb.linearVelocity = velocity;

        Rb.angularVelocity = Vector3.zero;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.useGravity = false;
    }
}