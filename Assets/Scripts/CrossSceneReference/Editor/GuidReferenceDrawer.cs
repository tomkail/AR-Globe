using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

// Using a property drawer to allow any class to have a field of type GuidRefernce and still get good UX
// If you are writing your own inspector for a class that uses a GuidReference, drawing it with
// EditorLayout.PropertyField(prop) or similar will get this to show up automatically
[CustomPropertyDrawer(typeof(GuidReference))]
public class GuidReferenceDrawer : PropertyDrawer
{
    SerializedProperty guidProp;
    SerializedProperty sceneProp;
    SerializedProperty nameProp;

    // cache off GUI content to avoid creating garbage every frame in editor
    GUIContent sceneLabel = new GUIContent("Containing Scene", "The target object is expected in this scene asset.");
    GUIContent clearButtonGUI = new GUIContent("Clear", "Remove Cross Scene Reference");

    // add an extra line to display source scene for targets
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return base.GetPropertyHeight(property, label) + EditorGUIUtility.singleLineHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
       
        guidProp = property.FindPropertyRelative("serializedGuid");
        nameProp = property.FindPropertyRelative("cachedName");
        sceneProp = property.FindPropertyRelative("cachedScene");

        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);

        position.height = EditorGUIUtility.singleLineHeight;

        // Draw prefix label, returning the new rect we can draw in
        var guidCompPosition = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        System.Guid currentGuid;
        GameObject currentGO = null;

        // working with array properties is a bit unwieldy
        // you have to get the property at each index manually
        byte[] byteArray = new byte[16];
        int arraySize = guidProp.arraySize;
        for( int i = 0; i < arraySize; ++i )
        {
            var byteProp = guidProp.GetArrayElementAtIndex(i);
            byteArray[i] = (byte)byteProp.intValue;
        }

        currentGuid = new System.Guid(byteArray);
        currentGO = GuidManager.ResolveGuid(currentGuid);
        GuidComponent currentGuidComponent = currentGO != null ? currentGO.GetComponent<GuidComponent>() : null;

        GuidComponent component = null;

        if (currentGuid != System.Guid.Empty && currentGuidComponent == null)
        {
            // if our reference is set, but the target isn't loaded, we display the target and the scene it is in, and provide a way to clear the reference
            float buttonWidth = 55.0f;
            
            guidCompPosition.xMax -= buttonWidth;

            bool guiEnabled = GUI.enabled;
            GUI.enabled = false;
            EditorGUI.LabelField(guidCompPosition, new GUIContent(nameProp.stringValue, "Target GameObject is not currently loaded."), EditorStyles.objectField);
            GUI.enabled = guiEnabled;

            Rect clearButtonRect = new Rect(guidCompPosition);
            clearButtonRect.xMin = guidCompPosition.xMax;
            clearButtonRect.xMax += buttonWidth;

            if (GUI.Button(clearButtonRect, clearButtonGUI, EditorStyles.miniButton))
            {
                ClearPreviousGuid(property);
            }
        }
        else
        {
            // if our object is loaded, we can simply use an object field directly
            component = EditorGUI.ObjectField(guidCompPosition, currentGuidComponent, typeof(GuidComponent), true) as GuidComponent;
        }

        if (currentGuidComponent != null && component == null)
        {
            ClearPreviousGuid(property);
        }

        // if we have a valid reference, draw the scene name of the scene it lives in so users can find it
        if (component != null)
        {
            nameProp.stringValue = component.name;
            string scenePath = component.gameObject.scene.path;
            sceneProp.objectReferenceValue = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);

            // only update the GUID Prop if something changed. This fixes multi-edit on GUID References
            if (component != currentGuidComponent)
            {
                byteArray = component.GetGuid().ToByteArray();
                arraySize = guidProp.arraySize;
                for (int i = 0; i < arraySize; ++i)
                {
                    var byteProp = guidProp.GetArrayElementAtIndex(i);
                    byteProp.intValue = byteArray[i];
                }

                var guidReference = GetSerializedValue<GuidReference>(property);
                MethodInfo dynMethod = guidReference.GetType().GetMethod("GuidAdded", 
                BindingFlags.NonPublic | BindingFlags.Instance);
                dynMethod.Invoke(guidReference, new object[] {component.gameObject});
            }
        }

        EditorGUI.indentLevel++;
        position.y += EditorGUIUtility.singleLineHeight;
        bool cachedGUIState = GUI.enabled;
        GUI.enabled = false;
        EditorGUI.ObjectField(position, sceneLabel, sceneProp.objectReferenceValue, typeof(SceneAsset), false);
        GUI.enabled = cachedGUIState;
        EditorGUI.indentLevel--;
       
        EditorGUI.EndProperty();
    }

    void ClearPreviousGuid(SerializedProperty property)
    {
        nameProp.stringValue = string.Empty;
        sceneProp.objectReferenceValue = null;

        int arraySize = guidProp.arraySize;
        for (int i = 0; i < arraySize; ++i)
        {
            var byteProp = guidProp.GetArrayElementAtIndex(i);
            byteProp.intValue = 0;
        }

        var guidReference = GetSerializedValue<GuidReference>(property);
        MethodInfo dynMethod = guidReference.GetType().GetMethod("GuidRemoved", 
        BindingFlags.NonPublic | BindingFlags.Instance);
        dynMethod.Invoke(guidReference, null);
    }

    public static T GetSerializedValue<T>(SerializedProperty property) {
        object @object = property.serializedObject.targetObject;
        string[] propertyNames = property.propertyPath.Split('.');
 
        // Clear the property path from "Array" and "data[i]".
        if (propertyNames.Length >= 3 && propertyNames[propertyNames.Length - 2] == "Array")
            propertyNames = propertyNames.Take(propertyNames.Length - 2).ToArray();
 
        // Get the last object of the property path.
        foreach (string path in propertyNames)
        {
            @object = @object.GetType()
                .GetField(path, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                .GetValue(@object);
        }
 
        if (@object.GetType().GetInterfaces().Contains(typeof(IList<T>)))
        {
            int propertyIndex = int.Parse(property.propertyPath[property.propertyPath.Length - 2].ToString());
 
            return ((IList<T>) @object)[propertyIndex];
        }
        else return (T) @object;
    }
}