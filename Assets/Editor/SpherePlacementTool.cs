using UnityEditor;
using UnityEngine;

public class SpherePlacementTool : EditorWindow
{
    private Transform planetCenter;
    private float radius = 10f;
    private float heightOffset = 0f;
    private bool mousePlacement = false;

    [MenuItem("Tools/Sphere Placement Tool")]
    private static void Open()
    {
        GetWindow<SpherePlacementTool>("Sphere Placement");
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Расстановка объектов по сфере", EditorStyles.boldLabel);

        planetCenter = (Transform)EditorGUILayout.ObjectField(
            "Центр сферы",
            planetCenter,
            typeof(Transform),
            true
        );

        radius = Mathf.Max(0.01f, EditorGUILayout.FloatField("Радиус", radius));
        heightOffset = EditorGUILayout.FloatField("Смещение вверх", heightOffset);
        mousePlacement = EditorGUILayout.Toggle("Ctrl + клик в Scene View", mousePlacement);

        EditorGUILayout.HelpBox(
            "1. Укажите объект центра сферы (обычно сама планета).\n" +
            "2. Выделите домик или несколько домиков.\n" +
            "3. Если включён Ctrl+клик, кликайте/тяните по сфере в Scene View: активный объект будет вставать низом к поверхности.\n" +
            "Кнопки ниже применяются ко всем выделенным объектам.",
            MessageType.Info
        );

        if (planetCenter == null && Selection.activeTransform != null)
        {
            if (GUILayout.Button("Использовать выделенный объект как центр сферы"))
            {
                planetCenter = Selection.activeTransform;
                TryGuessRadius();
            }
        }

        bool canWork = planetCenter != null && Selection.transforms.Length > 0;

        using (new EditorGUI.DisabledScope(!canWork))
        {
            if (GUILayout.Button("Только повернуть выделенные низом к сфере"))
            {
                Undo.RecordObjects(Selection.transforms, "Align To Sphere");

                foreach (Transform t in Selection.transforms)
                {
                    if (t == planetCenter)
                        continue;

                    AlignTransform(t, t.position, false);
                }

                SceneView.RepaintAll();
            }

            if (GUILayout.Button("Перенести на поверхность сферы и повернуть"))
            {
                Undo.RecordObjects(Selection.transforms, "Move To Sphere");

                foreach (Transform t in Selection.transforms)
                {
                    if (t == planetCenter)
                        continue;

                    AlignTransform(t, t.position, true);
                }

                SceneView.RepaintAll();
            }
        }
    }

    private void TryGuessRadius()
    {
        if (planetCenter == null)
            return;

        var sphereCollider = planetCenter.GetComponent<SphereCollider>();
        if (sphereCollider != null)
        {
            Vector3 scale = planetCenter.lossyScale;
            radius = sphereCollider.radius * Mathf.Max(
                Mathf.Abs(scale.x),
                Mathf.Abs(scale.y),
                Mathf.Abs(scale.z)
            );
            return;
        }

        var renderer = planetCenter.GetComponent<Renderer>();
        if (renderer != null)
        {
            radius = Vector3.Distance(renderer.bounds.center, renderer.bounds.max);
        }
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (!mousePlacement || planetCenter == null || Selection.activeTransform == null)
            return;

        if (Selection.activeTransform == planetCenter)
            return;

        Event e = Event.current;

        if (e.type != EventType.MouseDown && e.type != EventType.MouseDrag)
            return;

        if (e.button != 0 || !e.control)
            return;

        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

        if (TryRaySphere(ray, planetCenter.position, radius, out Vector3 hit))
        {
            Undo.RecordObject(Selection.activeTransform, "Place On Sphere");

            AlignTransform(Selection.activeTransform, hit, true, true);

            e.Use();
            sceneView.Repaint();
        }
    }

    private void AlignTransform(Transform t, Vector3 targetPosition, bool move, bool targetIsSurfacePoint = false)
    {
        if (t == null || planetCenter == null)
            return;

        Vector3 center = planetCenter.position;
        Vector3 normal = targetPosition - center;

        if (normal.sqrMagnitude < 1e-8f)
            return;

        normal.Normalize();

        Vector3 position = targetPosition;

        if (move && !targetIsSurfacePoint)
            position = center + normal * radius;

        if (move)
            position += normal * heightOffset;

        Vector3 forward = Vector3.ProjectOnPlane(t.forward, normal);

        if (forward.sqrMagnitude < 1e-6f)
            forward = Vector3.ProjectOnPlane(t.right, normal);

        if (forward.sqrMagnitude < 1e-6f)
            forward = Vector3.ProjectOnPlane(Vector3.forward, normal);

        if (forward.sqrMagnitude < 1e-6f)
            forward = Vector3.right;

        Quaternion rotation = Quaternion.LookRotation(forward, normal);

        t.SetPositionAndRotation(position, rotation);
    }

    private static bool TryRaySphere(Ray ray, Vector3 center, float radius, out Vector3 hit)
    {
        Vector3 origin = ray.origin;
        Vector3 direction = ray.direction.normalized;
        Vector3 originToCenter = origin - center;

        float b = Vector3.Dot(originToCenter, direction);
        float c = Vector3.Dot(originToCenter, originToCenter) - radius * radius;
        float discriminant = b * b - c;

        if (discriminant < 0f)
        {
            hit = default;
            return false;
        }

        float sqrtDiscriminant = Mathf.Sqrt(discriminant);
        float distance = -b - sqrtDiscriminant;

        if (distance < 0f)
            distance = -b + sqrtDiscriminant;

        if (distance < 0f)
        {
            hit = default;
            return false;
        }

        hit = origin + direction * distance;
        return true;
    }
}