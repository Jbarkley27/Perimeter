using System.Collections.Generic;
using UnityEngine;

public class SkillElementLibrary : MonoBehaviour
{
    public List<SkillElementData> elements;

    public Color GetElementColor(Element element)
    {
        foreach (var elemData in elements)
        {
            if (elemData.element == element)
                return elemData.color;
        }

        return Color.white; // Default color if element not found
    }


    public Sprite GetElementIcon(Element element)
    {
        foreach (var elemData in elements)
        {
            if (elemData.element == element)
                return elemData.icon;
        }

        return null; // Default icon if element not found
    }
}

public enum Element
{
    Kinetic,
    Scorch,
    Acid,
    Gravity,

    // Other types
    Barrier,
    Healing,
    Critical
}

public enum SkillType
{
    Projectile,
    Instant,
    Shield,
    Buff,
    Debuff,
    Area
}

[System.Serializable]
public struct SkillElementData
{
    public Element element;
    public Color color;
    public Sprite icon;
}