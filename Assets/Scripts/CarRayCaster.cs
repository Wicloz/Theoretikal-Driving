using System.Collections.Generic;
using UnityEngine;

public class CarRayCaster : MonoBehaviour {
    public static CarRayCaster _static = null;

    void Awake () {
        if (_static == null)
            _static = this;
    }

    public GameObject TileStraightBack () {
        RaycastHit hit;
        GameObject parent = null;
        float length = 999;

        if (Physics.Raycast(transform.position, transform.forward * -1, out hit)) {
            length = hit.distance;
            try {
                GameObject thisParent = hit.collider.transform.parent.gameObject;
                if (thisParent.tag == "Road Tile")
                    parent = thisParent;
            }
            catch {}
        }

        Debug.DrawRay(transform.position, transform.forward * -1 * length);

        return parent;
    }

    public List<GameObject> TilesMultiFront (bool addSky = false) {
        List<GameObject> hits = new List<GameObject>();

        for (float i = -1.5f; i <= 1.5f; i += 0.05f) {
            Vector3 direction = transform.forward.RotateAroundY(i);
            RaycastHit hit;
            float length = 999;

            if (Physics.Raycast(transform.position, direction, out hit)) {
                length = hit.distance;
                try {
                    GameObject parent = hit.collider.transform.parent.gameObject;
                    if (parent.tag == "Road Tile")
                        hits.Add(parent);
                }
                catch {}
            }
            else if (addSky) {
                hits.Add(null);
            }

            Debug.DrawRay(transform.position, direction * length);
        }

        return hits;
    }

    public bool FrontViewFull () {
        return !TilesMultiFront(true).Contains(null);
    }
}
