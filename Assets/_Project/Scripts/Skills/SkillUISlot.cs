using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

public class SkillUISlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI References")]
    public Image skillSliderBGImage;
    public Image skillIconImage;
    public Slider slider;
    public CanvasGroup activeBorderCanvasGroup;
    public CanvasGroup cooldownOverlayCanvasGroup;
    public GameObject switchToManulUIRoot;
    public GameObject cancelManualUIRoot;


    [Header("Skill Data")]
    public SkillData currentSkill;
    public bool IsSkillRunning = false;
    public bool IsAutoFireEnabled = true;



    public void Init(SkillData skillData)
    {
        currentSkill = skillData;
        skillIconImage.sprite = skillData.icon;
        skillIconImage.enabled = true;
        skillSliderBGImage.color = GlobalDataStore.Instance.SkillElementLibrary.GetElementColor(skillData.element);
        slider.value = slider.maxValue; // Assume skill is ready at start
        activeBorderCanvasGroup.alpha = 0f;
        cooldownOverlayCanvasGroup.alpha = 1f;
        IsAutoFireEnabled = true;
        switchToManulUIRoot.SetActive(false);
        cancelManualUIRoot.SetActive(false);
    }
    

    void Update()
    {
        if (GameManager.Instance.GamePaused) return;
        TickCooldown();
        HandleAutoFire();
        cooldownOverlayCanvasGroup.alpha = IsReadyToFire() ? 1f : .9f;
    }

    public void ClearSlot()
    {
        currentSkill = null;
        skillIconImage.sprite = null;
    }


    public void RefreshElementColor()
    {
        if (currentSkill == null || skillSliderBGImage == null)
            return;

        skillSliderBGImage.color =
            GlobalDataStore.Instance.SkillElementLibrary.GetElementColor(currentSkill.element);
    }


    public void SetFireMode(bool isAuto)
    {
        if (GameManager.Instance.GamePaused) return;

        IsAutoFireEnabled = isAuto;

        if (isAuto)
        {
            activeBorderCanvasGroup.DOFade(0f, 0.2f);
            cancelManualUIRoot.SetActive(false);
            switchToManulUIRoot.SetActive(false);
        }
        else
        {
            activeBorderCanvasGroup.DOFade(1f, 0.2f);
        }
    }


    private void TickCooldown()
    {
        if (IsSkillRunning || currentSkill == null) return;
        if (GameManager.Instance.GamePaused) return;
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
        || !GlobalDataStore.Instance.SkillCaster.HasTarget()
        || !IsAutoFireEnabled
        || GameManager.Instance.GamePaused)
        {
            return;
        }

        StartCoroutine(TriggerSkill());
    }

    public IEnumerator TriggerSkill(bool overrideAutoFire = false)
    {
        if (
            currentSkill == null
            || IsSkillRunning
            || GameManager.Instance.GamePaused
        )
        {
            Debug.LogWarning("No skill assigned to this slot! OR skill is already running.");
            yield break;
        }


        IsSkillRunning = true;
        slider.value = 0f; // Reset cooldown


        if (currentSkill.IsProjectileSkill)
        {
            // Get the correct target
            Transform currentTarget = overrideAutoFire
                ? WorldCursor.instance.GetTarget()
                : GlobalDataStore.Instance.SkillCaster.ClosestTarget;


            gameObject.transform.DOPunchScale(Vector3.one * 0.3f, 0.2f, 1, 0.5f);

            GlobalDataStore.Instance.SkillCaster.FireProjectile(
                currentSkill,
                currentTarget
            );
        }
        else
        {
            Debug.Log("Trigger NON-projectile skill effect here.");
        }

        yield return new WaitForSeconds(currentSkill.cooldownRestartDelay);
        IsSkillRunning = false;
    }

    

    #region Pointer Event Handlers

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (GameManager.Instance.GamePaused) return;

        if (GlobalDataStore.Instance.SkillCaster.GetActiveManualSkillData() == currentSkill)
        {
            // Show turn off auto-fire hint
            cancelManualUIRoot.SetActive(true);
        }
        else
        {
            // Show turn on auto-fire hint
            switchToManulUIRoot.SetActive(true);
            cancelManualUIRoot.SetActive(false);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        switchToManulUIRoot.SetActive(false);
        cancelManualUIRoot.SetActive(false);
    }



    public void OnPointerClick(PointerEventData eventData)
    {
        if (GameManager.Instance.GamePaused) return;

        if (GlobalDataStore.Instance.SkillCaster.GetActiveManualSkillData() == currentSkill)
        {
            // Clear active manual skill
            GlobalDataStore.Instance.SkillCaster.ClearActiveManualSkill();
            cancelManualUIRoot.SetActive(false);
            return;
        }

        // Set this skill as the active manual skill
        GlobalDataStore.Instance.SkillCaster.AssignActiveManualSkill(this);
        cancelManualUIRoot.SetActive(false);
    }

    #endregion



    public void ForceCooldownReset()
    {
        slider.value = slider.maxValue;

        // Reset UI
        cooldownOverlayCanvasGroup.alpha = 1f;
        cancelManualUIRoot.SetActive(false);
        switchToManulUIRoot.SetActive(false);
    }
}