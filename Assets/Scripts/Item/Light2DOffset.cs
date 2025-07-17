using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
public class Light2DOffset : MonoBehaviour
{
    public Vector3 offset = Vector3.zero;

    private Vector3 originalLocalPos;
    private bool initialized = false;

    void OnEnable()
    {
        CacheOriginalPosition();
        ApplyOffset();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        // 允许你在编辑器实时更新 offset
        ApplyOffset();
    }
#endif

    void Update()
    {
        if (!initialized)
        {
            CacheOriginalPosition();
        }

        ApplyOffset();
    }

    private void CacheOriginalPosition()
    {
        originalLocalPos = transform.localPosition - offset;
        initialized = true;
    }

    private void ApplyOffset()
    {
        transform.localPosition = originalLocalPos + offset;
    }
}
