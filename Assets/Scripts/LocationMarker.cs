using UnityEngine;
using UnityEngine.UI;

public class LocationMarker : MonoBehaviour {
    public WorldSpaceUIElement worldSpaceUIElement;
    public GeoCoord coord;
    public SLayout layout;
    public TMPro.TextMeshProUGUI label;
    public Image image;
    public bool _showing;
    public bool showing {
        get {
            return _showing;
        } set {
            if(_showing == value) return;
            _showing = value;
			layout.Animate(0.2f, () => {
				layout.groupAlpha = _showing.ToInt();
			});
        }
    }
    public enum RollMode {
        A,
        B,
        C
    }
    public static RollMode rollMode;

    void OnEnable () {
        layout.groupAlpha = _showing.ToInt();
    }

    void Update () {
        label.text = coord.name;
        var pointOnGlobe = LongLatUtils.LLHtoECEF(coord.latitude, coord.longitude, coord.alt) * Globe.Instance.radius;
        worldSpaceUIElement.worldPosition = pointOnGlobe;
        if(LocationServiceEnabler.Instance.ready) {
            layout.rotation = Main.Instance.uiRotation;
        }
        // transform.rotation = Quaternion.LookRotation(pointOnGlobe.normalized, Vector3.up);
    }
	void OnDrawGizmos() {
        // if(showing)
		    Gizmos.DrawSphere(LongLatUtils.LLHtoECEF(coord.latitude, coord.longitude, coord.alt) * Globe.Instance.radius, 10);
	}
}
