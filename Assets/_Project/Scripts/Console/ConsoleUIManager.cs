using UnityEngine.UI;
using UnityEngine;

public class ConsoleUIManager : MonoBehaviour
{
    public enum ConsoleUIScreenState
    {
        MINING = 0,
        SKILL_TREE = 1,
        PRESTIGE = 2
    }

    public ConsoleUIScreenState CurrentScreenState = ConsoleUIScreenState.SKILL_TREE;


    [Header("Mining Screen UI Elements")]
    public GameObject miningScreenUIRoot;
    public Image miningNavElementActiveIndicator;
    

    [Header("Skill Tree Screen UI Elements")]
    public GameObject skillTreeScreenUIRoot;
    public Image skillTreeNavElementActiveIndicator;


    [Header("Prestige Screen UI Elements")]
    public GameObject prestigeScreenUIRoot;
    public Image prestigeNavElementActiveIndicator;




    public void SetScreenState(int newState)
    {
        CurrentScreenState = (ConsoleUIScreenState)newState;
        UpdateScreenUI();
    }


    public void UpdateScreenUI()
    {
        miningScreenUIRoot.SetActive(CurrentScreenState == ConsoleUIScreenState.MINING);
        skillTreeScreenUIRoot.SetActive(CurrentScreenState == ConsoleUIScreenState.SKILL_TREE);
        prestigeScreenUIRoot.SetActive(CurrentScreenState == ConsoleUIScreenState.PRESTIGE);

        // Update active screen indicator
        switch (CurrentScreenState)
        {
            case ConsoleUIScreenState.MINING:
                miningNavElementActiveIndicator.enabled = true;
                skillTreeNavElementActiveIndicator.enabled = false;
                prestigeNavElementActiveIndicator.enabled = false;
                break;

            case ConsoleUIScreenState.SKILL_TREE:
                miningNavElementActiveIndicator.enabled = false;
                skillTreeNavElementActiveIndicator.enabled = true;
                prestigeNavElementActiveIndicator.enabled = false;
                SkillTreeUIManager.Instance.CenterUIOnScreen();
                break;

            case ConsoleUIScreenState.PRESTIGE:
                miningNavElementActiveIndicator.enabled = false;
                skillTreeNavElementActiveIndicator.enabled = false;
                prestigeNavElementActiveIndicator.enabled = true;
                break;
        }
    }
}
