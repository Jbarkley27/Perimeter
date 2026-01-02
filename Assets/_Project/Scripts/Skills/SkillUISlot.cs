using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SkillUISlot : MonoBehaviour
{
    public Image skillSliderBGImage;
    public Image skillIconImage;
    public Slider slider;
    private SkillData currentSkill;
    public CanvasGroup activeBorderCanvasGroup;
    public float cooldownTimer;   // Runtime cooldown
    public bool IsReady => cooldownTimer <= 0;
    public bool IsSkillRunning = false;
    public CanvasGroup cooldownOverlayCanvasGroup;
    public ParticleSystem SkillTriggeredVFX;

    public void Init(SkillData skillData)
    {
        currentSkill = skillData;
        skillIconImage.sprite = skillData.icon;
        skillIconImage.enabled = true;
        skillSliderBGImage.color = GlobalDataStore.Instance.SkillElementLibrary.GetElementColor(skillData.element);
        slider.value = slider.maxValue; // Assume skill is ready at start
        activeBorderCanvasGroup.alpha = 0f;
        cooldownOverlayCanvasGroup.alpha = 1f;
    }

    void Update()
    {
        TickCooldown();
        HandleAutoFire();
        cooldownOverlayCanvasGroup.alpha = IsReadyToFire() ? 1f : .9f;
    }

    public void ClearSlot()
    {
        currentSkill = null;
        skillIconImage.sprite = null;
    }


    private void TickCooldown()
    {
        if (IsSkillRunning) return;
        if (!IsReadyToFire())
        {
            // Lerp/Slerp the slider value towards max
            slider.value = Mathf.MoveTowards(slider.value, slider.maxValue, Time.deltaTime * currentSkill.cooldownRate);
        }
    }

    public bool IsReadyToFire()
    {
        return slider.value >= slider.maxValue;
    }

    private void HandleAutoFire()
    {
        if (!IsReadyToFire()
        || !GlobalDataStore.Instance.SkillCaster.HasTarget())
        {
            Debug.Log("Skill not ready or no target available.");
            return;
        }
        StartCoroutine(TriggerSkill());
    }

    public IEnumerator TriggerSkill()
    {
        if (
            currentSkill == null
            || IsSkillRunning
        )
        {
            Debug.LogWarning("No skill assigned to this slot! OR skill is already running.");
            yield return null;
        }


        IsSkillRunning = true;
        slider.value = 0f; // Reset cooldown

        Debug.Log($"<color=cyan>FIRE:</color> {currentSkill.skillName} | " +
                  $"Damage:{currentSkill.damage} | Cooldown:{currentSkill.cooldownRate}");

        if (currentSkill.IsProjectileSkill)
        {
            // SkillTriggeredVFX.Play();
            gameObject.transform.DOPunchScale(Vector3.one * 0.3f, 0.2f, 1, 0.5f);
            Debug.Log($"Spawn projectile: {currentSkill.projectilePrefab}");

            GlobalDataStore.Instance.SkillCaster.FireProjectile(
                currentSkill,
                GlobalDataStore.Instance.SkillCaster.ClosestTarget
            );
        }
        else
        {
            Debug.Log("Trigger NON-projectile skill effect here.");
        }

        yield return new WaitForSeconds(currentSkill.cooldownRestartDelay);
        IsSkillRunning = false;
    }

    
}