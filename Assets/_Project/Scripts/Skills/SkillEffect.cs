using UnityEngine;

public abstract class SkillEffect : ScriptableObject
{
    public int priority = 0; // higher = apply later
    public abstract void Apply();

    
    public virtual string GetDescription()
    {
        return name;
    }

    public virtual string GetDescriptionWithValue()
    {
        return GetDescription();
    }
}
