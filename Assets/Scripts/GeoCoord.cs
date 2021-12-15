using UnityEngine;

[System.Serializable]
public struct GeoCoord
{
    public string name;
    public float latitude;
    public float longitude;
    public float alt;

    public static GeoCoord deviceLocation {
        get {
            var _deviceLocation = new GeoCoord();
            _deviceLocation.name = "Device Location";
            _deviceLocation.latitude = Input.location.lastData.latitude;
            _deviceLocation.longitude = Input.location.lastData.longitude;
            _deviceLocation.alt = Input.location.lastData.altitude;
            return _deviceLocation;
        }
    }

    public Vector3 PolarToCartesian() {
        //an origin vector, representing lat,lon of 0,0. 
        Vector3 origin = new Vector3(0, 0, 1);
        //build a quaternion using euler angles for lat,lon
        Quaternion rotation = Quaternion.Euler(-latitude, -longitude, 0);
        Vector3 point = rotation * origin;

            // LongLatUtils.LLHtoECEF(coord.latitude, coord.longitude, coord.alt) * Globe.Instance.radius
        return point;
    }
}
