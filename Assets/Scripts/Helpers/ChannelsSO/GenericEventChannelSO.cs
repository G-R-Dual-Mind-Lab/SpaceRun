using UnityEngine; 
using UnityEngine.Events;

public abstract class GenericEventChannelSO<T>: ScriptableObject 
{ 
    public UnityAction<T> OnEventRaised; 
    public void RaiseEvent(T parameter) 
    {
        if (OnEventRaised == null) return; 
    
        OnEventRaised.Invoke(parameter);
    } 
}