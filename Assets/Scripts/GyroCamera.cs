using UnityEngine;
using System.Collections;

public class GyroCamera : MonoBehaviour
{
    public GeoCoord coord;

    // STATE
    private float _initialYAngle = 0f;
    private float _appliedGyroYAngle = 0f;
    private float _calibrationYAngle = 0f;
    private Quaternion _axis;
    private Transform _rawGyroRotation;
    private float _tempSmoothing;

    // SETTINGS
    [Range(0.01f,1f)]
    public float _smoothing = 0.1f;
    
    public float distanceFromGlobe = 1;

    public bool useAxis;
    public bool useGyro;

    void OnEnable () {
        LocationServiceEnabler.Instance.OnReady += OnReady;
        _rawGyroRotation = new GameObject("GyroRaw").transform;
        _rawGyroRotation.position = transform.position;
        _rawGyroRotation.rotation = transform.rotation;
    }

    void OnReady () {
        Calibrate();
    }

    void Calibrate () {
        // Debug.Log(Input.compass.trueHeading);
        coord = GeoCoord.deviceLocation;
        _initialYAngle = Input.compass.trueHeading;


        var pointOnGlobe = LongLatUtils.LLHtoECEF(coord.latitude, coord.longitude, 1);
        _axis = Globe.GetRotationOnGlobe(pointOnGlobe);

        ApplyGyroRotation();
        _calibrationYAngle = _appliedGyroYAngle - _initialYAngle;

        Update();
    }

    private void Update() {
        if(LocationServiceEnabler.Instance.ready) {
            coord = GeoCoord.deviceLocation;
            ApplyGyroRotation();
            ApplyCalibration();
        }

        var pointOnGlobe = LongLatUtils.LLHtoECEF(coord.latitude, coord.longitude, coord.alt).normalized;
        if(!LocationServiceEnabler.Instance.ready) {
            transform.rotation = Globe.GetRotationOnGlobe(pointOnGlobe);
        } else {
            var targetRotation = _rawGyroRotation.rotation.Difference(_axis);
            if(useAxis) targetRotation = _axis;
            if(useGyro) targetRotation = _rawGyroRotation.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _smoothing);
        }
        transform.position = pointOnGlobe * Globe.Instance.radius;


        // Camera.main.transform.position = pointOnGlobe * Globe.Instance.radius * 3;
        // Camera.main.transform.rotation = Quaternion.LookRotation((pointOnGlobe-Camera.main.transform.position).normalized);
        
        // Globe.GetRotationOnGlobe(pointOnGlobe);
    }

    private void ApplyGyroRotation()
    {
        _rawGyroRotation.rotation = Input.gyro.attitude;
        _rawGyroRotation.Rotate(0f, 0f, 180f, Space.Self); // Swap "handedness" of quaternion from gyro.
        _rawGyroRotation.Rotate(90f, 180f, 0f, Space.World); // Rotate to make sense as a camera pointing out the back of your device.
        _appliedGyroYAngle = _rawGyroRotation.eulerAngles.y; // Save the angle around y axis for use in calibration.
    }

    private void ApplyCalibration()
    {
        _rawGyroRotation.Rotate(0f, -_calibrationYAngle, 0f, Space.World); // Rotates y angle back however much it deviated when calibrationYAngle was saved.
    }

    private static Quaternion GyroToUnity(Quaternion q) {
        return new Quaternion(q.x, q.y, -q.z, -q.w);
    }


    void OnGUI () {
        var matrix = GUI.matrix;
        var guiScale = Mathf.Max(Screen.width/600f, Screen.height/800f);
        GUI.matrix = Matrix4x4.Scale(guiScale*Vector3.one);
        
        GUILayout.Label("latitude:"+coord.latitude);
        GUILayout.Label("longitude:"+coord.longitude);
        GUILayout.Label("alt:"+coord.alt);

        GUILayout.Space(20);

        GUILayout.Label("True Heading:"+Input.compass.trueHeading);
        
        GUILayout.Space(20);

        // GUILayout.Label("Acceleration X:"+Input.acceleration.x);
        // GUILayout.Label("Acceleration Y:"+Input.acceleration.y);
        // GUILayout.Label("Acceleration Z:"+Input.acceleration.z);

        // GUILayout.Space(20);

        // GUILayout.Label("Gryo Attitude X:"+Input.gyro.attitude.x);
        // GUILayout.Label("Gryo Attitude Y:"+Input.gyro.attitude.y);
        // GUILayout.Label("Gryo Attitude Z:"+Input.gyro.attitude.z);
        
        // GUILayout.Space(20);

    
    
    
    

        GUILayout.Label("_initialYAngle: "+_initialYAngle);
        GUILayout.Label("_appliedGyroYAngle: "+_appliedGyroYAngle);
        GUILayout.Label("_calibrationYAngle: "+_calibrationYAngle);
        GUILayout.Label("gyro+offset: "+_appliedGyroYAngle+" "+_calibrationYAngle);
        
        GUILayout.Space(20);

        _calibrationYAngle = GUILayout.HorizontalSlider(_calibrationYAngle, -180, 180);
        
        distanceFromGlobe = GUILayout.HorizontalSlider(distanceFromGlobe, 0.5f, 2f);
        
        if (GUILayout.Button ("Set Axis to globe forward")) {
            var pointOnGlobe = LongLatUtils.LLHtoECEF(coord.latitude, coord.longitude, 1);
            _axis = Globe.GetRotationOnGlobe(pointOnGlobe);
        }
        useAxis = GUILayout.Toggle (useAxis, "useAxis");
        useGyro = GUILayout.Toggle (useGyro, "useGyro");
        if (GUILayout.Button ("Calibrate")) {
            Calibrate();
        }
        GUI.matrix = matrix;
    }
}