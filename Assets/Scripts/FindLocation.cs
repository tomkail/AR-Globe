    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using TMPro;
    using UnityEngine;
 
    [Serializable]
    public struct GeoLocation
    {
        [SerializeField] private float latitude;
        [SerializeField] private float longitude;
        [SerializeField] private float horizontalAccuracy;
        [SerializeField] private double timeStamp;
 
        public float Latitude { get { return latitude; } }
        public float Longitude { get { return longitude; } }
        public float HorizontalAccuracy { get { return horizontalAccuracy; } }
        public double TimeStamp { get { return timeStamp; } }
 
        public GeoLocation(float latitude, float longitude, float horizontalAccuracy, double timeStamp)
        {
            this.latitude = latitude;
            this.longitude = longitude;
            this.horizontalAccuracy = horizontalAccuracy;
            this.timeStamp = timeStamp;
        }
    }
 
    /// <summary>
    /// This class is the core of the application. It positions the target, the pointers, and handles rotation.
    /// </summary>
    public class FindLocation : MonoBehaviour
    {
        public const float UserEnableTime = 20;
        public const int LatLongInMetres = 111111;
        public const float MaxHeadings = 24;
     
        [SerializeField] private Transform firstPersonUser, topDownUser;
        [SerializeField] private Transform targetMarker;
        [SerializeField] private string applicationLogText;
        [SerializeField] private GameObject arrowNorth;
        [SerializeField] private GeoLocation targetGeoData = new GeoLocation(52.162608f, -1.924693f, 0f, 0);
        [SerializeField] private GeoLocation lastLocation = new GeoLocation();
        [SerializeField] private List<Vector3> compassHeadings = new List<Vector3>();
        private string initializePendingMsg = "Initialising Location Service";
        private string permissionDeniedMsg = "User did not enable location services on device. Please go to System->Apps->Locator and enable location service.";
        private string locationTimeoutMsg = "Location service timed out before being able to update application. Please contact the developer.";
 
        private float compassCalibrations = 0;
        private float lastAcceleration;
 
        public bool Ready { get; private set; } = false;
     
        /// <summary>
        /// Setup.
        /// </summary>
        /// <returns></returns>
        private IEnumerator Start()
        {
            applicationLogText = initializePendingMsg;
 
            var waiter = new WaitForSeconds(0.5f);
            var timeElapsed = 0f;
 
            while (!Input.location.isEnabledByUser && timeElapsed < UserEnableTime)
            {
                timeElapsed += 0.5f;
                yield return waiter;
            }
 
            if (timeElapsed >= UserEnableTime)
            {
                applicationLogText = permissionDeniedMsg;
                yield break;
            }
 
            Input.location.Start();
 
            timeElapsed = 0f;
            while (Input.location.status == LocationServiceStatus.Initializing && timeElapsed < UserEnableTime)
            {
                timeElapsed += 0.5f;
                yield return waiter;
            }
 
            if (timeElapsed >= UserEnableTime)
            {
                applicationLogText = locationTimeoutMsg;
                yield break;
            }
 
            Input.compass.enabled = true;
 
            if (SystemInfo.supportsAccelerometer)
            {
                Input.gyro.enabled = true;
            }
 
            // Calibrate real world rotation.
            for (int i = 0; i < MaxHeadings; i++)
            {
                UpdateCompassHeading(true);
                yield return null;
            }
 
            Ready = true;
        }
 
        /// <summary>
        /// Main loop.
        /// </summary>
        private void Update()
        {
            if (Ready)
            {
                UpdatePosition();
                UpdateRotation();
                UpdateApplicationLog();
            }
        }
 
        private void UpdatePosition()
        {
            if (GeoLocationUpdateCheck())
            {
                // float x = (lastLocation.Latitude - targetGeoData.Latitude) * LatLongInMetres;
                // float y = -0.7f;
                // float z = (lastLocation.Longitude - targetGeoData.Longitude) * LatLongInMetres;
 
                // targetMarker.position = new Vector3(x, y, z);
            }
            else
            {
            //     float x = Input.gyro.userAcceleration.x * Time.deltaTime;
            //     float y = 0f;
            //     float z = Input.gyro.userAcceleration.z * Time.deltaTime;
 
            //     targetMarker.Translate(new Vector3(x, y, z));
            }
        }
 
        private void UpdateRotation()
        {
            // float deltaX = (Mathf.Rad2Deg * -Input.gyro.rotationRateUnbiased.x) * Time.deltaTime;
            // float deltaY = (Mathf.Rad2Deg * -Input.gyro.rotationRateUnbiased.y) * Time.deltaTime;
            // float deltaZ = (Mathf.Rad2Deg * -Input.gyro.rotationRateUnbiased.z) * Time.deltaTime;
 
            // Vector3 firstPersonRot = firstPersonUser.rotation.eulerAngles;
            // Vector3 topDownRot = topDownUser.rotation.eulerAngles;
 
            // firstPersonRot += new Vector3(0f, deltaY, 0f);
            // topDownRot += new Vector3(0f, deltaY, 0f);
 
            // firstPersonUser.rotation = Quaternion.Euler(firstPersonRot);
            // topDownUser.rotation = Quaternion.Euler(topDownRot);
 
            UpdateCompassHeading();
        }
 
        private void UpdateCompassHeading(bool rotatePlayer = false)
        {
            compassHeadings.Add(new Vector3(0f, Input.compass.trueHeading, 0f));
 
            if(compassHeadings.Count > MaxHeadings)
            {
                compassHeadings.RemoveAt(0);
            }
 
            float meanCurrentRotation = compassHeadings.Sum(vector => vector.y);
            float meanAverage = meanCurrentRotation / compassHeadings.Count;
            float northOffset = (365 - meanAverage);
 
            if (rotatePlayer)
            {
                // firstPersonUser.rotation = Quaternion.Euler(new Vector3(0f, meanAverage, 0f));
                // topDownUser.rotation = Quaternion.Euler(new Vector3(topDownUser.rotation.eulerAngles.x, meanAverage, topDownUser.rotation.eulerAngles.z));
            }
         
            // arrowNorth.transform.rotation = Quaternion.Euler(new Vector3(0f, meanAverage + northOffset, 0f));
        }

        // void OnGUI () {
        //     var matrix = GUI.matrix;
        //     var guiScale = Mathf.Max(Screen.width/600f, Screen.height/800f);
        //     GUI.matrix = Matrix4x4.Scale(guiScale*Vector3.one);
            
        //     var str = "Headings\n";
        //     compassHeadings.ForEach(x => str+="\n"+(x.ToString()));
        //     GUILayout.Label(str);
        //     GUILayout.Label(applicationLogText);
            
        //     GUI.matrix = matrix;
        // }
 
        private bool GeoLocationUpdateCheck()
        {
            if (lastLocation.TimeStamp != Input.location.lastData.timestamp)
            {
                lastLocation = new GeoLocation(
                Input.location.lastData.latitude,
                Input.location.lastData.longitude,
                Input.location.lastData.horizontalAccuracy,
                Input.location.lastData.timestamp
                );
                return true;
            }
            return false;
        }
 
        public static Quaternion FindRealWorldRotation()
        {
            return Quaternion.Euler(FindRealWorldEulerAngles());
        }
        public static Vector3 FindRealWorldEulerAngles()
        {
            // Input.compass.rawVector
            // This does not handle the phone being upside down when started.
            float deviceX = Input.acceleration.z * 90f;
            float deviceY = Input.compass.trueHeading;
            float deviceZ = Input.acceleration.x * 90f;
            return new Vector3(deviceX, deviceY, deviceZ);
        }
 
        private void UpdateApplicationLog()
        {
            // float distance = Vector3.Distance(firstPersonUser.position, targetMarker.position);
            // string output = distance.ToString("F1") + "m";
            // applicationLogText = "Distance " + output;
            // distanceText.text = output;
        }
    }