using System.Collections.Generic;
using UnityEngine;

public class Main : MonoSingleton<Main> {
    public RuntimeSceneSet sceneSet;
    
    public float uiRotation;
    float rollVelocity;
    
    protected override void Awake() {
        base.Awake();
        Application.targetFrameRate = 60;
    }

    void Start() {
        if(sceneSet != null)
            RuntimeSceneSetLoader.Instance.LoadSceneSetup(sceneSet, LoadTaskMode.LoadSingle);
    }

    public GeoCoord coord;
    public float distanceFromGlobe;

    void Update () {

        // var coord = GeoCoord.deviceLocation;
        // var pointOnGlobe = LongLatUtils.LLHtoECEF(coord.latitude, coord.longitude, coord.alt);
        // transform.position = pointOnGlobe * Globe.Instance.radius * distanceFromGlobe;
        // transform.rotation = Globe.GetRotationOnGlobe(pointOnGlobe);

        if(LocationServiceEnabler.Instance.ready) {
            // var targetUIRotation = -Mathf.Atan2(-Input.acceleration.x, Mathf.Sqrt(Input.acceleration.y*Input.acceleration.y + Input.acceleration.z*Input.acceleration.z)) * 180/Mathf.PI;
            float targetUIRotation = -GetAngleByDeviceAxis(Vector3.forward);
            targetUIRotation = 180-(Mathf.Rad2Deg * Mathf.Atan2(Input.acceleration.x, Input.acceleration.y));
            // targetUIRotation = -Mathf.Atan2(Input.acceleration.y, Input.acceleration.z) * 180f/Mathf.PI;
            // uiRotation = Mathf.SmoothDampAngle(uiRotation, targetUIRotation, ref rollVelocity, 0.05f, Mathf.Infinity);
            uiRotation = targetUIRotation;
        }
    }

    float GetAngleByDeviceAxis(Vector3 axis) {
         Quaternion deviceRotation = new Quaternion(0.5f, 0.5f, -0.5f, 0.5f) * Input.gyro.attitude * new Quaternion(0, 0, 1, 0);
         Quaternion eliminationOfOthers = Quaternion.Inverse(
             Quaternion.FromToRotation(axis, deviceRotation * axis)
         );
         Vector3 filteredEuler = (eliminationOfOthers * deviceRotation).eulerAngles;

         float result = filteredEuler.z;
         if (axis == Vector3.up) {
             result = filteredEuler.y;
         }
         if (axis == Vector3.right) {
             // incorporate different euler representations.
             result = (filteredEuler.y > 90 && filteredEuler.y < 270) ? 180 - filteredEuler.x : filteredEuler.x;
         }
         return result;
     }

    public static float GetDeviceRoll () {
        Quaternion referenceRotation = Quaternion.identity;
        Quaternion deviceRotation = new Quaternion(0.5f, 0.5f, -0.5f, 0.5f) * Input.gyro.attitude * new Quaternion(0, 0, 1, 0);
        Quaternion eliminationOfXY = Quaternion.Inverse(Quaternion.FromToRotation(referenceRotation * Vector3.forward, deviceRotation * Vector3.forward));
        Quaternion rotationZ = eliminationOfXY * deviceRotation;
        return rotationZ.eulerAngles.z;
    }
}
