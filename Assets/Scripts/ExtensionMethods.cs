using UnityEngine;

public static class ExtensionMethods {
    public static Vector3 RotateAroundY (this Vector3 offset, float radians) {
        return new Vector3(
            offset.x * Mathf.Cos(radians) + offset.z * Mathf.Sin(radians),
            offset.y,
            -1 * offset.x * Mathf.Sin(radians) + offset.z * Mathf.Cos(radians)
        );
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
            case trafficzone.snelweg:
                return 130;
            default:
                return -1;
        }
    }
}
