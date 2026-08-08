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

        //Vector3 v = data.DesiredVelocity / data.MoveSpeed;
        //v.y = 0f;
        //_animator.SetFloat("Speed", v.magnitude);

        // ✅ Для сферы: убираем компоненту вдоль UpDirection
        Vector3 horizontal = Vector3.ProjectOnPlane(data.DesiredVelocity, data.UpDirection) / data.MoveSpeed;
        _animator.SetFloat("Speed", horizontal.magnitude);

        Debug.Log($"Скорость для анимации: {horizontal.magnitude}");
    }
}