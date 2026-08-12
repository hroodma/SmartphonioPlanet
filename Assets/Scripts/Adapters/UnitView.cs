using UnityEngine;

public class UnitView : MonoBehaviour, IUnitView
{
    [SerializeField] private Animator _animator;

    private void Awake()
    {
        if (_animator == null)
            _animator = GetComponentInChildren<Animator>();
    }

    public void Render(UnitData data)
    {
        if (_animator == null)
            return;

        Vector3 horizontal = Vector3.ProjectOnPlane(data.DesiredVelocity, data.UpDirection) / data.MaxSpeed;
        float speed = horizontal.magnitude;

        if (speed < 0.01f)
            speed = 0f;

        _animator.SetFloat("Speed", speed);
    }
}