using UnityEngine;

public class UnitView : MonoBehaviour, IUnitView
{
    [SerializeField] private Animator _animator;

    private void Awake()
    {
        if (_animator == null)
            _animator = GetComponentInChildren<Animator>();
    }

    public void Render(bool hasSpeed, float normalizedSpeed, bool shouldPlayInteract)
    {
        if (_animator == null)
            return;

        if (hasSpeed)
            _animator.SetFloat("Speed", normalizedSpeed);

        if (shouldPlayInteract)
        {
            _animator.ResetTrigger("Interact");
            _animator.SetTrigger("Interact");
        }
    }
}