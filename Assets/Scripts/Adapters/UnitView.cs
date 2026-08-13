using UnityEngine;

public class UnitView : MonoBehaviour, IUnitView
{
    [SerializeField] private Animator _animator;

    private void Awake()
    {
        if (_animator == null)
            _animator = GetComponentInChildren<Animator>();
    }

    public void Render(IEntity entity)
    {
        if (_animator == null)
            return;

        switch (entity)
        {
            case IMoveable moveable:
                Vector3 horizontal = Vector3.ProjectOnPlane(moveable.Movement.DesiredVelocity, moveable.Movement.UpDirection) / moveable.Movement.MaxSpeed;
                float speed = horizontal.magnitude;

                if (speed < 0.01f)
                    speed = 0f;

                _animator.SetFloat("Speed", speed);
                break;
        }
    }
}