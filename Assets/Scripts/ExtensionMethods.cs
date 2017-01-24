using UnityEngine;

public static class ExtensionMethods {
    public static Vector3 RotateAroundY (this Vector3 offset, float radians) {
        return new Vector3(
            offset.x * Mathf.Cos(radians) + offset.z * Mathf.Sin(radians),
            offset.y,
            -1 * offset.x * Mathf.Sin(radians) + offset.z * Mathf.Cos(radians)
        );
    }
}
