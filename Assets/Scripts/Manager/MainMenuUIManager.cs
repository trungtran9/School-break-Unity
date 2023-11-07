using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;

public class MainMenuUIManager : MonoBehaviour
{
    [SerializeField] public RectTransform settings, characters, mainMenu, credits;
    [SerializeField] GameObject buttonPanels;

    private void Start()
    {
        mainMenu.DOAnchorPos(Vector2.zero, 0.25f);
    }

    public void CharacterPress()
    {
        characters.DOAnchorPos(Vector2.zero, 0.25f).SetEase(Ease.OutSine);
        StartCoroutine(ToggleButtonPanels());
    }

    public void CloseCharacterPress()
    {
        characters.DOAnchorPos(new Vector2(850, 0), 0.25f);
        StartCoroutine(ToggleButtonPanels());
    }

    public void SettingsPress()
    {
        settings.DOAnchorPos(Vector2.zero, 0.25f).SetEase(Ease.OutSine);
        StartCoroutine(ToggleButtonPanels());
    }

    public void CloseSettingsPress()
    {
        settings.DOAnchorPos(new Vector2(0, 1612), 0.25f);
        StartCoroutine(ToggleButtonPanels());
    }

    public void CreditPress() {
        credits.DOAnchorPos(Vector2.zero, 0.25f).SetEase(Ease.OutSine);
        StartCoroutine(ToggleButtonPanels());
    }

    public void CloseCreditPress() {
        credits.DOAnchorPos(new Vector2(0, 1612), 0.25f);
        StartCoroutine(ToggleButtonPanels());
    }

    IEnumerator ToggleButtonPanels()
    {
        foreach (Transform child in buttonPanels.transform)
        {
            child.GetComponent<Button>().interactable = false;
        }
        yield return new WaitForSeconds(0.5f);
        foreach (Transform child in buttonPanels.transform)
        {
            child.GetComponent<Button>().interactable = true;
        }
    }
}
