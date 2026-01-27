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

    public override string GetDescription()
    {
        if (percentAmount != 0f)
            return $"+{percentAmount * 100f:0.#}% {stat}";

        if (flatAmount != 0)
            return $"+{flatAmount} {stat}";

        return $"{stat} (no change)";
    }

    public override string GetDescriptionWithValue()
    {
        if (StatsManager.Instance == null)
            return GetDescription();

        double total = StatsManager.Instance.GetStat(stat);
        return $"{GetDescription()} (Total {FormatStatValue(stat, total)})";
    }

    private string FormatStatValue(StatsManager.StatType stat, double value)
    {
        if (stat == StatsManager.StatType.CRIT_CHANCE)
            return $"{value * 100f:0.#}%";

        return $"{value:0.#}";
    }
}
