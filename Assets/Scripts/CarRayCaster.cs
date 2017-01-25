using System.Collections;
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
        Debug.DrawRay(transform.position, transform.forward * -999);
        if (Physics.Raycast(transform.position, transform.forward * -1, out hit)) {
            try {
                GameObject parent = hit.collider.transform.parent.gameObject;
                if (parent.tag == "Road Tile")
                    return parent;
            }
            catch {}
        }
        return null;
    }

    public List<GameObject> TilesMultiFront (bool addSky = false) {
        List<GameObject> hits = new List<GameObject>();

        for (float i = -1; i <= 1; i += 0.05f) {
            Vector3 direction = transform.forward.RotateAroundY(i);
            RaycastHit hit;
            Debug.DrawRay(transform.position, direction * 999);

            if (Physics.Raycast(transform.position, direction, out hit)) {
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
        }

        return hits;
    }

    public bool FrontViewFull () {
        return !TilesMultiFront(true).Contains(null);
    }
}
