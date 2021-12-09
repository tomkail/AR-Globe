using UnityEngine;
using System.Collections;


public class Player : MonoBehaviour {
    public GeoCoord coord;
    public float pitch;
    public float yaw;
    public float roll;

    float pitchVelocity;
    float yawVelocity;
    float rollVelocity;
    
    Quaternion origin;
    
    void OnEnable () {
        LocationServiceEnabler.Instance.OnReady += OnReady;
    }

    void OnReady () {
        origin = GyroToUnity(Input.gyro.attitude);
    }

    void Update () {
        if(!LocationServiceEnabler.Instance.ready) return;

        #if !UNITY_EDITOR
        coord.latitude = Input.location.lastData.latitude;
        coord.longitude = Input.location.lastData.longitude;
        #endif

        var pointOnGlobe = LongLatUtils.LLHtoECEF(coord.latitude, coord.longitude, coord.alt) * Globe.Instance.radius;
        // transform.position = pointOnGlobe;
        // transform.rotation = Quaternion.LookRotation(Vector3.forward, pointOnGlobe) * Input.gyro.attitude;

        #if !UNITY_EDITOR
        // roll = 1 + (-Input.acceleration.z);
        // while(roll < 1) roll += 2;
        // while(roll > 1) roll -= 2;
        // Debug.Log(roll);
        // transform.rotation = Quaternion.LookRotation(pointOnGlobe.normalized, Vector3.up) * Quaternion.Euler(-90,180,0) * Quaternion.Euler((roll)*90,Input.compass.trueHeading,0);
        var smoothDamp = 0.1f;
        var targetPitch = 90+(Mathf.Atan2(Input.acceleration.y, Input.acceleration.z) * 180/Mathf.PI);
        pitch = Mathf.SmoothDampAngle(pitch, targetPitch, ref pitchVelocity, smoothDamp, Mathf.Infinity, Time.deltaTime);
        yaw = Mathf.SmoothDampAngle(yaw, Input.compass.trueHeading, ref yawVelocity, smoothDamp, Mathf.Infinity, Time.deltaTime);
        var targetRoll = Mathf.Atan2(-Input.acceleration.x, Mathf.Sqrt(Input.acceleration.y*Input.acceleration.y + Input.acceleration.z*Input.acceleration.z)) * 180/Mathf.PI;
        roll = Mathf.SmoothDampAngle(roll, targetRoll, ref rollVelocity, smoothDamp, Mathf.Infinity, Time.deltaTime);

        // transform.rotation = Quaternion.LookRotation(pointOnGlobe.normalized, Vector3.up) * Quaternion.Euler(-90,180,0) * Quaternion.Euler(FindLocation.FindRealWorldEulerAngles());
        #endif

        // transform.rotation = Quaternion.LookRotation(pointOnGlobe.normalized, Vector3.up) * Quaternion.Euler(-90,180,0) * Quaternion.Euler(pitch,yaw,roll);
        transform.rotation = Quaternion.Euler(-pitch,yaw,roll);
        transform.rotation = Quaternion.Inverse(origin) * GyroToUnity(Input.gyro.attitude);
        // Debug.Log(Input.acceleration.x+" "+Input.acceleration.y+" "+Input.acceleration.z);
        // transform.rotation = Quaternion.LookRotation(Vector3.up, pointOnGlobe.normalized) * FindLocation.FindRealWorldRotation();
    }

    public static Quaternion GyroToUnity(Quaternion q) {
        return new Quaternion(q.x, q.y, -q.z, -q.w);
    }

    void OnGUI () {
        var matrix = GUI.matrix;
        var guiScale = Mathf.Max(Screen.width/600f, Screen.height/800f);
        GUI.matrix = Matrix4x4.Scale(guiScale*Vector3.one);
        
        GUILayout.Label("X:"+Input.acceleration.x);
        GUILayout.Label("Y:"+Input.acceleration.y);
        GUILayout.Label("Z:"+Input.acceleration.z);
        GUILayout.Label("Roll:"+roll);
        GUILayout.Label("Yaw:"+yaw);
        GUILayout.Label("Pitch:"+pitch);
        
        if (GUILayout.Button ("Recenter View")) {
            //RECENTER THE CAMERA VIEW
            origin = GyroToUnity(Input.gyro.attitude);
        }
        // if (GUILayout.Button ("Recenter Position")) {
            //RECENTER THE CAMERA VIEW
            // PortalPlayer.Instance.currentWorldPositionOffset = Vector3.zero;
        // }
        
        GUI.matrix = matrix;
    }
}
