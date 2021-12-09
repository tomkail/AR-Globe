using UnityEngine;

public class Globe : MonoSingleton<Globe> {
    public float radius = 1000;
    public const float radiusOfEarth = 6378137f;


    public float rotationSpeed = 10;
    Vector3 screenCenter;
    Vector3 screenEdge;
    void Update () {
        transform.localScale = Vector3.one * radius * 2f;
        
        // if(Input.GetMouseButton(0)) {
        //     screenCenter = Camera.main.WorldToScreenPoint(Globe.Instance.transform.position);
        //     screenEdge = Camera.main.WorldToScreenPoint(Globe.Instance.transform.position+ Camera.main.transform.rotation * new Vector3(1,1,0).normalized * radius);
        //     var screenVector = new Vector2(Mathf.Abs(screenEdge.x-screenCenter.x), Mathf.Abs(screenEdge.y-screenCenter.y));
            
        //     var rotY = -InputX.Instance.mouseInput.deltaPosition.x / screenVector.x;
        //     var rotX = InputX.Instance.mouseInput.deltaPosition.y / screenVector.y;

        //     transform.Rotate(new Vector3(rotX, rotY, 0) * rotationSpeed * Time.deltaTime, Space.World);
        // }
    }

    public static Quaternion GetRotationOnGlobe (Vector3 pointOnGlobe) {
        return Quaternion.LookRotation(pointOnGlobe.normalized) * Quaternion.Euler(-90,180,0);    
    }

    // void OnGUI () {
    //     Debug.Log(screenCenter+" "+screenEdge);
    //     GUI.Box(RectX.CreateFromCenter(screenCenter, Vector2.one * 100), "");
    //     GUI.Box(RectX.CreateFromCenter(screenEdge, Vector2.one * 100), "");
    // }
}