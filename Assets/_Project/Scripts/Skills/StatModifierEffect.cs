using UnityEngine;

[CreateAssetMenu(fileName = "StatModifierEffect", menuName = "Skills/Effects/Stat Modifier")]
public class StatModifierEffect : SkillEffect
{
    public StatsManager.StatType stat;
    public double flatAmount;
    public float percentAmount;

    public override void Apply()
    {
        if (StatsManager.Instance != null)
            StatsManager.Instance.ApplyModifier(stat, flatAmount, percentAmount);
    }
}
