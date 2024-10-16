using UnityEngine;

public class PlayBGMObj : MonoBehaviour
{
    public string bgmName = "";

    void Start()
    {
        AudioManager.Instance.PlayMusic(bgmName);
    }

}
