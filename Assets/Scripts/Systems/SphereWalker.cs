using UnityEngine;

public sealed class SphereWalker : MonoBehaviour
{
    [SerializeField] private Transform planet;      // перетащи сюда планету
    [SerializeField] private float radius = 5f;      // радиус планеты
    [SerializeField] private float speed = 5f;       // скорость
    [SerializeField] private float gravity = 20f;    // сила гравитации

    private Rigidbody rb;
    private Vector3 verticalVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;  // выключаем МИРОВУЮ гравитацию
        rb.freezeRotation = true;  // физика не крутит нас
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void FixedUpdate()
    {
        // 1. Локальный "верх" = от центра планеты к нам
        Vector3 up = (transform.position - planet.position).normalized;

        // 2. Поворачиваем капсулу ногами к планете
        transform.rotation = Quaternion.FromToRotation(transform.up, up) * transform.rotation;

        // 3. Читаем ввод (WASD)
        float h = Input.GetAxis("Horizontal");  // A/D
        float v = Input.GetAxis("Vertical");    // W/S

        // 4. Направление движения ПО ЛОКАЛЬНЫМ ОСЯМ персонажа
        Vector3 moveDir = transform.forward * v + transform.right * h;

        // 5. ПРОЕКЦИЯ на касательную плоскость (убираем компоненту вдоль "верха")
        moveDir = Vector3.ProjectOnPlane(moveDir, up);

        // 6. Горизонтальная скорость (то, что мы хотим)
        Vector3 horizontalVelocity = moveDir.normalized * speed;
        if (moveDir.sqrMagnitude < 0.01f)
            horizontalVelocity = Vector3.zero;

        // 7. Гравитация — отдельный вектор к центру планеты
        verticalVelocity = -up * gravity * Time.fixedDeltaTime;

        // 8. Если мы на поверхности — сбрасываем падающую вертикаль
        float distanceFromCenter = Vector3.Distance(transform.position, planet.position);
        if (distanceFromCenter <= radius + 0.1f)
        {
            // Убираем компоненту скорости, направленную К центру
            float fallSpeed = Vector3.Dot(verticalVelocity, -up);
            if (fallSpeed > 0)
                verticalVelocity += up * fallSpeed;
        }

        // 9. Итоговая скорость = горизонталь + вертикаль
        rb.linearVelocity = horizontalVelocity + verticalVelocity;
    }
}