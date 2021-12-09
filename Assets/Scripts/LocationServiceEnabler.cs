using System;
using System.Collections;
using UnityEngine;

public class LocationServiceEnabler : MonoSingleton<LocationServiceEnabler> {
    public bool ready;
    public Action OnReady;

    private IEnumerator Start() {
        var timeElapsed = 0f;
        var userEnableTime = 20f;

        while (!Input.location.isEnabledByUser && timeElapsed < userEnableTime) {
            timeElapsed += Time.deltaTime;
            yield return new WaitForSeconds(Time.deltaTime);
        }

        if (timeElapsed >= userEnableTime) {
            // applicationLogText = permissionDeniedMsg;
            yield break;
        }

        Input.location.Start();

        timeElapsed = 0f;
        while (Input.location.status == LocationServiceStatus.Initializing && timeElapsed < userEnableTime) {
            timeElapsed += Time.deltaTime;
            yield return new WaitForSeconds(Time.deltaTime);
        }

        if (timeElapsed >= userEnableTime) {
            // applicationLogText = locationTimeoutMsg;
            yield break;
        }

        Input.compass.enabled = true;

        if (SystemInfo.supportsAccelerometer) {
            Input.gyro.enabled = true;
        }
        Debug.Log(Input.compass.timestamp);
        // This seems enough time for the compass to turn on
        yield return new WaitForSeconds(1);
        Debug.Log(Input.compass.timestamp);
        ready = true;
        if(OnReady != null) OnReady();
    }
}
