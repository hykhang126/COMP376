using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;


#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[DisallowMultipleComponent]
public class InteractableComponent : MonoBehaviour
{
    enum InteractableType { BOX, SPHERE, CAPSULE }
    public UnityEvent interactionTriggered = new UnityEvent();

    [SerializeField] public float cooldown = 1.0f;
    [SerializeField] public bool isOneShot = false;
    [SerializeField] private InteractableType interactableType = InteractableType.BOX;


    private bool isCoolingDown = false;
    [SerializeField, HideInInspector] private Collider _collider;
    [SerializeField, HideInInspector] private int _colliderID;

#if UNITY_EDITOR
    [NonSerialized] private bool _isValidating;
#endif

    
    void Awake()
    {
        if (_collider == null)
            ResetCollider_Runtime();
    }

    void Start()
    {
        interactionTriggered.AddListener(OnInteractionTriggered);
    }

    public void AttempyTriggerInteraction()
    {
        if (isCoolingDown)
        {
            return;
        }
        else
        {
            interactionTriggered.Invoke();
            StartCoroutine(CooldownCoroutine());
        }
    

    }
    void OnInteractionTriggered()
    {
        //If interaction is one-shot, detach all listeners 
        if (isOneShot)
        {
            interactionTriggered.RemoveAllListeners();
        }
    }

    private IEnumerator CooldownCoroutine()
    {
        isCoolingDown = true;
        float timer = cooldown;
        while (timer > 0.0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }
        isCoolingDown = false;

    }
    void ResetCollider_Runtime()
    {
        // Safe at runtime
        if (_collider)
            Destroy(_collider);

        _collider = (Collider)gameObject.AddComponent(DesiredType());
        _collider.isTrigger = true;
        _collider.includeLayers = LayerMask.GetMask("Interact");

        #if UNITY_EDITOR
        _colliderID = _collider.GetInstanceID();
        #endif
    }

    // Editor Mode Sections
#if UNITY_EDITOR
    void OnValidate()
    {
        // Fix the missing braces and guard re-entrancy
        if (_isValidating) return;

        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            // In play mode we can do it right away
            ResetCollider_Runtime();
            return;
        }

        _isValidating = true;
        int selfId = GetInstanceID(); // capture in case of domain reloads

        // Defer add/remove until AFTER OnValidate
        EditorApplication.delayCall += () =>
        {
            var self = EditorUtility.InstanceIDToObject(selfId) as InteractableComponent;
            if (!self) { _isValidating = false; return; }

            self.ResetCollider_Editor();
            _isValidating = false;
        };
    }

    void RehydrateEditorRef()
    {
        if (_collider == null && _colliderID != 0)
        {
            var obj = EditorUtility.InstanceIDToObject(_colliderID) as Collider;
            if (obj) _collider = obj;
        }
    }

    Type DesiredType() =>
        interactableType == InteractableType.BOX    ? typeof(BoxCollider) :
        interactableType == InteractableType.SPHERE ? typeof(SphereCollider) :
                                                      typeof(CapsuleCollider);

    void ResetCollider_Editor()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) { ResetCollider_Runtime(); return; }

        RehydrateEditorRef();

        // Remove ONLY the collider we previously added
        if (_collider)
            Undo.DestroyObjectImmediate(_collider);

        // Add the desired collider with Undo support
        _collider = (Collider)Undo.AddComponent(gameObject, DesiredType());
        _collider.isTrigger = true;
        _collider.includeLayers = LayerMask.GetMask("Interact");

        _colliderID = _collider.GetInstanceID();
        EditorUtility.SetDirty(this);
        EditorSceneManager.MarkSceneDirty(gameObject.scene);
    }

#endif
}
