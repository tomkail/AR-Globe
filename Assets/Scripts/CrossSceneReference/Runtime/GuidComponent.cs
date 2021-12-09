using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Experimental.SceneManagement;
#endif

// This component gives a GameObject a stable, non-replicatable Globally Unique IDentifier.
// It can be used to reference a specific instance of an object no matter where it is.
// This can also be used for other systems, such as Save/Load game
[ExecuteInEditMode, DisallowMultipleComponent]
public class GuidComponent : MonoBehaviour, ISerializationCallbackReceiver
{
    // This event fires once ever, even after compilation or after reloading scenes. 
    public System.Action OnSetGUID;
    [System.NonSerialized]
    public bool didJustAssignNewGUID;
    

    // System guid we use for comparison and generation
    System.Guid guid = System.Guid.Empty;

    // Unity's serialization system doesn't know about System.Guid, so we convert to a byte array
    // Fun fact, we tried using strings at first, but that allocated memory and was twice as slow
    [SerializeField, HideInInspector]
    private byte[] serializedGuid;


    // Never return an invalid GUID
    public System.Guid GetGuid()
    {
        if (guid == System.Guid.Empty && serializedGuid != null && serializedGuid.Length == 16)
        {
            guid = new System.Guid(serializedGuid);
        }

        return guid;
    }

    void OnDisable () {
        #if UNITY_EDITOR
        if(didJustAssignNewGUID) {
            didJustAssignNewGUID = false;
        }
        #endif
    }


    // When de-serializing or creating this component, we want to either restore our serialized GUID
    // or create a new one.
    void CreateGuid()
    {
        var createdNew = false;
        // if our serialized data is invalid, then we are a new object and need a new GUID
        if (serializedGuid == null || serializedGuid.Length != 16)
        {
#if UNITY_EDITOR
            // if in editor, make sure we aren't a prefab of some kind
            if (IsAssetOnDisk())
            {
                return;
            }
            Undo.RecordObject(this, "Added GUID");
#endif
            guid = System.Guid.NewGuid();
            serializedGuid = guid.ToByteArray();
            createdNew = true;
#if UNITY_EDITOR
            // If we are creating a new GUID for a prefab instance of a prefab, but we have somehow lost our prefab connection
            // force a save of the modified prefab instance properties
            if (PrefabUtility.IsPartOfNonAssetPrefabInstance(this))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(this);
            }
#endif
        }
        else if (guid == System.Guid.Empty)
        {
            // otherwise, we should set our system guid to our serialized guid
            guid = new System.Guid(serializedGuid);
        }

        // register with the GUID Manager so that other components can access this
        if (guid != System.Guid.Empty)
        {
            if (!GuidManager.Add(this))
            {
                // if registration fails, we probably have a duplicate or invalid GUID, get us a new one.
                serializedGuid = null;
                guid = System.Guid.Empty;
                CreateGuid();
            } else if(createdNew) {
                didJustAssignNewGUID = true;
                #if UNITY_EDITOR
                EditorApplication.delayCall += () => {
                    if(this == null) return;
                    // This works too, but is less efficient, and because we need to wait a frame either way we might as well rely on the subscriber.
                    // SendMessage("OnSetGUID", SendMessageOptions.RequireReceiver);
                    if(OnSetGUID != null) OnSetGUID();
                    didJustAssignNewGUID = false;
                };
                #endif
            }
        }
    }

#if UNITY_EDITOR
    private bool IsEditingInPrefabMode()
    {
        if (EditorUtility.IsPersistent(this))
        {
            // if the game object is stored on disk, it is a prefab of some kind, despite not returning true for IsPartOfPrefabAsset =/
            return true;
        }
        else
        {
            // If the GameObject is not persistent let's determine which stage we are in first because getting Prefab info depends on it
            var mainStage = StageUtility.GetMainStageHandle();
            var currentStage = StageUtility.GetStageHandle(gameObject);
            if (currentStage != mainStage)
            {
                var prefabStage = PrefabStageUtility.GetPrefabStage(gameObject);
                if (prefabStage != null)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private bool IsAssetOnDisk()
    {
        return PrefabUtility.IsPartOfPrefabAsset(this) || IsEditingInPrefabMode();
    }
#endif

    // We cannot allow a GUID to be saved into a prefab, and we need to convert to byte[]
    public void OnBeforeSerialize()
    {
#if UNITY_EDITOR
        // This lets us detect if we are a prefab instance or a prefab asset.
        // A prefab asset cannot contain a GUID since it would then be duplicated when instanced.
        if (IsAssetOnDisk())
        {
            serializedGuid = null;
            guid = System.Guid.Empty;
        }
        else
#endif
        {
            if (guid != System.Guid.Empty)
            {
                serializedGuid = guid.ToByteArray();
            }
        }
    }

    // On load, we can go head a restore our system guid for later use
    public void OnAfterDeserialize()
    {
        if (serializedGuid != null && serializedGuid.Length == 16)
        {
            guid = new System.Guid(serializedGuid);
        }
    }

    void Awake() {
        ManualAwake();
    }

    void OnValidate() {
        ManualValidate();
    }

    void ManualAwake () {
        CreateGuid();
    }
    void ManualValidate () {
        #if UNITY_EDITOR
        // similar to on Serialize, but gets called on Copying a Component or Applying a Prefab
        // at a time that lets us detect what we are
        if (IsAssetOnDisk())
        {
            serializedGuid = null;
            guid = System.Guid.Empty;
        }
        else
        #endif
        {
            CreateGuid();
        }
    }

    // let the manager know we are gone, so other objects no longer find this
    void OnDestroy()
    {
        GuidManager.Remove(guid);
    }
}
