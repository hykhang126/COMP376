using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class Transition
{
    [Tooltip("state event that will trigger this Transition. Can initiate from StateMachine.InvokeStateEvent()")]
    public string triggerEvent;
    [Tooltip("Destination State for this Transition. Must match the stateName of the target State exactly.")]
    public string toState;
}

[System.Serializable]
public class State
{
    public String stateName;
    // Each state now owns its outgoing transitions
    [Header("Transitions")]
    [Tooltip("List of Transitions starting from this State")]
    public List<Transition> transitions;
    // Efficient lookup for this state's transitions: triggerEvent -> toStateName
    [NonSerialized]
    public Dictionary<string, string> transitionDictionary;
    [Header("Callbacks")]
    [Tooltip("UnityEvent invoked after every transition to a new State")]
    public UnityEvent stateEnter;
    [Tooltip("UnityEvent invoked before every transition to a new State on the current State")]
    public UnityEvent stateExit;
    [Tooltip("UnityEvent invoked every Update() frame for the current State")]
    public UnityEvent stateUpdate;
    [Tooltip("UnityEvent invoked every FixedUpdate() frame for the current State")]
    public UnityEvent stateFixedUpdate;

    // Build the fast lookup dictionary (call after states list is available)
    public void BuildTransitionDictionary()
    {
        transitionDictionary = new Dictionary<string, string>();
        if (transitions == null)
            return;

        foreach (var t in transitions)
        {
            if (t == null || string.IsNullOrEmpty(t.triggerEvent) || string.IsNullOrEmpty(t.toState))
                continue;

            // Overwrite duplicates with the last defined transition
            transitionDictionary[t.triggerEvent] = t.toState;
        }
    }
}

public class StateMachine : MonoBehaviour
{
    [SerializeField, Tooltip("List of states for this StateMachine. First State assumed to be initial State.")]
    public List<State> states;

    [SerializeField, Tooltip("List of transitions that can be executed from any State")]
    public List<Transition> globalTransitions;

    private State currentState;

    // GlobalTransitions Dictionary for efficient lookup using state event
    private Dictionary<string, State> globalTransitionsDictionary;

    //Reference to State prior to latest Transition
    private State previousState;

    void Awake()
    {
        // Safety checks
        if (states == null || states.Count == 0)
        {
            Debug.LogError("StateMachine has no states defined.");
            return;
        }


        // Initialize dictionary for global transitions
        globalTransitionsDictionary = new Dictionary<string, State>();

        //initial state defaults to first state in list
        currentState = states[0];

        // Populate globalTransitionsDictionary from the serialized globalTransitions list
        if (globalTransitions != null)
        {
            foreach (Transition t in globalTransitions)
            {
                if (t == null)
                    continue;

                State toState = states.Find(s => s != null && s.stateName == t.toState);
                if (toState == null)
                {
                    Debug.LogError($"Could not find state '{t.toState}' in StateMachine States list.");
                    continue;
                }

                if (string.IsNullOrEmpty(t.triggerEvent))
                {
                    Debug.LogWarning($"Global transition to '{t.toState}' has empty triggerEvent, skipping.");
                    continue;
                }

                if (globalTransitionsDictionary.ContainsKey(t.triggerEvent))
                {
                    Debug.LogWarning($"Duplicate global transition trigger '{t.triggerEvent}' found. Overwriting.");
                    globalTransitionsDictionary[t.triggerEvent] = toState;
                }
                else
                {
                    globalTransitionsDictionary.Add(t.triggerEvent, toState);
                }
            }
        }

        // Build per-state transition dictionaries for fast lookup
        foreach (var s in states)
        {
            s.BuildTransitionDictionary();
        }

    }

    void Start()
    {
        currentState?.stateEnter?.Invoke();
    }

    void Update()
    {
        currentState?.stateUpdate?.Invoke();
    }

    void FixedUpdate()
    {
        currentState?.stateFixedUpdate?.Invoke();
    }

    private void Transition(State newState)
    {
        if (currentState != null)
        {
            currentState.stateExit?.Invoke();
        }
        previousState = currentState;
        currentState = newState;
        currentState?.stateEnter?.Invoke();
    }

    public void InvokeStateEvent(String eventName)
    {
        if (string.IsNullOrEmpty(eventName))
            return;

        State next = GetDestinationState(eventName);
        if (next != null)
        {
            Transition(next);
        }
    }

    public State GetDestinationState(String eventName)
    {
        if (string.IsNullOrEmpty(eventName))
            return null;

        // Prioritize GlobalTransition before State Transition
        if (globalTransitionsDictionary != null && globalTransitionsDictionary.TryGetValue(eventName, out var globalState))
            return globalState;

        // Get State Transition if no matching GlobalTransition
        if (currentState != null)
        {
            if (currentState.transitionDictionary != null && currentState.transitionDictionary.TryGetValue(eventName, out var toStateName))
            {
                State toState = states.Find(s => s != null && s.stateName == toStateName);
                if (toState == null)
                {
                    Debug.LogError($"Could not find state '{toStateName}' referenced by transition '{eventName}' in state '{currentState.stateName}'.");
                    return null;
                }

                return toState;
            }

        }

        return null;
    }

    public State GetPreviousState()
    {
        return previousState;
    }

}
