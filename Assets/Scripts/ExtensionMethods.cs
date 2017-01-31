using UnityEngine;

public static class ExtensionMethods {
    public static Vector3 RotateAroundY (this Vector3 input, float radians) {
        return new Vector3(
            input.x * Mathf.Cos(radians) + input.z * Mathf.Sin(radians),
            input.y,
            -1 * input.x * Mathf.Sin(radians) + input.z * Mathf.Cos(radians)
        );
    }

    public static Vector3 RotateOffset (this Vector3 input, Vector3 rotation) {
        float radians = Mathf.Deg2Rad * (rotation.y + rotation.z);
        return input.RotateAroundY(radians);
    }

    public static int MaxSpeed (this trafficzone trafficZone) {
        switch (trafficZone) {
            case trafficzone.woonwijk:
                return 15;
            case trafficzone.dertig:
                return 30;
            case trafficzone.bebouwd:
                return 50;
            case trafficzone.onbebouwd:
                return 80;
            default:
                return -1;
        }
    }
}
