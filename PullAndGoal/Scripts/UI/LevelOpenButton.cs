using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelOpenButton : MonoBehaviour
{
    public int level;
    [SerializeField] private Sprite colorStar;

    [Header("View")]
    [SerializeField] private Button button;
    [SerializeField] private Image leftStar;
    [SerializeField] private Image rightStar;
    [SerializeField] private Image midStar;
    [SerializeField] private TMP_Text tmp;
    void Awake()
    {
        if (LevelManager.Instance.InRangeLevel(level) == false)
        {
            gameObject.SetActive(false);
            return;
        }

        tmp.text = level.ToString();

        if (LevelManager.Instance.IsLevelUnlocked(level))
        {
            button.interactable = true;

            leftStar.gameObject.SetActive(true);
            rightStar.gameObject.SetActive(true);
            midStar.gameObject.SetActive(true);
        }

        if (LevelManager.Instance.GetLevelStartCnt(level) == 1)
        {
            leftStar.sprite = colorStar;
        }
        else if (LevelManager.Instance.GetLevelStartCnt(level) == 2)
        {
            leftStar.sprite = colorStar;
            rightStar.sprite = colorStar;
        }
        else if (LevelManager.Instance.GetLevelStartCnt(level) == 3)
        {
            leftStar.sprite = colorStar;
            rightStar.sprite = colorStar;
            midStar.sprite = colorStar;
        }
    }

    public void OnLevelButtonClick()
    {
        LevelManager.Instance.OpenLevel(level);
    }
}
