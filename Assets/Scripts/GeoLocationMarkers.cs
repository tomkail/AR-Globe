using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class GeoLocationMarkers : MonoBehaviour {
	public GeoCoordLibrary coordLibrary;
	public LocationMarker locationMarkerPrefab;
	public List<LocationMarker> locationMarkers;

	void OnEnable () {
		locationMarkerPrefab.gameObject.SetActive(true);
		foreach(var coord in coordLibrary.coords) {
			var locationMarker = Object.Instantiate<LocationMarker>(locationMarkerPrefab, Vector3.zero, Quaternion.identity, transform);
			locationMarker.gameObject.name = coord.name;
			locationMarker.coord = coord;
			locationMarker.gameObject.SetActive(true);
			locationMarkers.Add(locationMarker);
		}
	}
	
	void OnDisable () {
		foreach(var locationMarker in locationMarkers) {
			Destroy(locationMarker.gameObject);
		}
		locationMarkers.Clear();

		// locationMarkers.
	}

	void Update () {
		SortChildOrderByDistance();
	}

	void SortChildOrderByDistance () {
		var sortedLocations = locationMarkers.OrderBy(x => Vector3X.SignedDistanceInDirection(Camera.main.transform.position, x.worldSpaceUIElement.worldPosition, Camera.main.transform.forward));
		foreach(var sortedLocation in sortedLocations) {
			sortedLocation.transform.SetAsFirstSibling();
		}
		List<Rect> blockedRects = new List<Rect>();
		foreach(var sortedLocation in sortedLocations) {
			var isBlocked = false;
			var rect = sortedLocation.layout.rectTransform.GetScreenRect(sortedLocation.layout.canvas);
            for (int i = 0; i < blockedRects.Count; i++) {
                Rect blockedRect = blockedRects[i];
                if (RectX.Intersects(blockedRect, rect)) {
					blockedRects.Add(rect);
					isBlocked = true;
					break;
				}
			}
			if(!isBlocked) blockedRects.Add(rect);
			sortedLocation.showing = !isBlocked;
		}
	}

	// void OnGUI () {
	//     foreach(var coord in coords) {
	//         var position = LongLatUtils.LLHtoECEF(coord.lat, coord.lon, coord.alt) * World.scaleFactor;
	//         var screenPoint = Camera.current.WorldToScreenPoint(position);
	//         GUI.Label(new Rect(screenPoint.x, Screen.height-screenPoint.y, 100,30), coord.name, GUI.skin.box);
	//     }
	// }
}
