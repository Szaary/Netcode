using UnityEngine;

public abstract class TransportAchievements : MonoBehaviour
{
    [SerializeField] protected TransportFacade facade;
    
    public abstract void Trigger(string aName);
    public abstract void WipeAchievements();
}