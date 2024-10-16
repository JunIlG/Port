using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectionMenu : MonoBehaviour
{
    [Header("View")]
    [SerializeField] private GameObject LevelSelectionPanel;

    public void OpenLevelSelectionPanel()
    {
        LevelSelectionPanel.SetActive(true);
    }

    public void CloseLevelSelectionPanel()
    {
        LevelSelectionPanel.SetActive(false);
    }
}
