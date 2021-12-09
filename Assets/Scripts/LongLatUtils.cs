using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongLatUtils : MonoBehaviour
{
    // https://stackoverflow.com/questions/10473852/convert-latitude-and-longitude-to-point-in-3d-space
    public static Vector3 LLHtoECEF(float lat, float lon, float alt) {
        lat = Mathf.Deg2Rad * lat;
        lon = Mathf.Deg2Rad * lon;
        // see http://www.mathworks.de/help/toolbox/aeroblks/llatoecefposition.html

        // var rad = 6378137f;        // Radius of the Earth (in meters)
        // var f = 1f/298.257223563f;  // Flattening factor WGS84 Model
        // var cosLat = Mathf.Cos(lat);
        // var sinLat = Mathf.Sin(lat);
        // var FF     = (1f-f)*2;
        // var C      = 1f/Mathf.Sqrt(cosLat*2 + FF * sinLat*2);
        // var S      = C * FF;

        // var x = (rad * C + alt)*cosLat * Mathf.Cos(lon);
        // var y = (rad * C + alt)*cosLat * Mathf.Sin(lon);
        // var z = (rad * S + alt)*sinLat;

        // return new Vector3(x, y, z);

        // see: http://www.mathworks.de/help/toolbox/aeroblks/llatoecefposition.html
        // var f  = 0f;                              // flattening
        // var ls = Mathf.Atan((1 - f)*2f * Mathf.Tan(lat));    // lambda

        // var x = rad * Mathf.Cos(ls) * Mathf.Cos(lon) + alt * Mathf.Cos(lat) * Mathf.Cos(lon);
        // var y = rad * Mathf.Cos(ls) * Mathf.Sin(lon) + alt * Mathf.Cos(lat) * Mathf.Sin(lon);
        // var z = rad * Mathf.Sin(ls) + alt * Mathf.Sin(lat);

        // return new Vector3(x, y, z);

        //flips the Y axis
        lat = Mathf.PI / 2f - lat;

        //distribute to sphere
        var pos = new Vector3(
                    Mathf.Sin( lat ) * Mathf.Sin( -lon ),
                    Mathf.Cos( lat ),
                    Mathf.Sin( lat ) * Mathf.Cos( -lon )
        );
        return Globe.Instance.transform.TransformDirection(pos);
        // pos = (Quaternion.LookRotation(pos, Vector3.up) * Globe.Instance.transform.rotation) * Vector3.forward;
        // return pos;
    }
}
