using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public struct LevelInfo
{
    public int starCnt;
    public bool isUnlocked;
}

[System.Serializable]
public struct LevelDataArray
{
    public LevelInfo[] levels;
}

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    public int curLevel;
    public int curStar;

    public GameObject nextOrRestartPanelPrefab;

    LevelDataArray levelDataArray;
    public int levelNum;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        LoadLevelData();

        
    }

    private void OnApplicationQuit()
    {
        SaveLevelData();
    }

    void InitLevel(int level)
    {
        curLevel = level;
        curStar = 0;
        SetGameTimeScale(1f);
    }

    public void OpenLevel(int level)
    {
        if (level <= levelNum)
        {
            InitLevel(level);
            SceneManager.LoadScene("level_" + level);
        }
    }

    public void GoHome()
    {
        curLevel = 0;
        curStar = 0;
        SetGameTimeScale(1f);
        SceneManager.LoadScene(0);
    }

    public void ClearLevel()
    {
        bool haveToSave = false;

        if (curStar > levelDataArray.levels[curLevel - 1].starCnt)
        {
            levelDataArray.levels[curLevel - 1].starCnt = curStar;
            haveToSave = true;
        }
        
        if (curLevel < levelNum && levelDataArray.levels[curLevel].isUnlocked == false)
        {
            levelDataArray.levels[curLevel].isUnlocked = true;
            haveToSave = true;
        }

        if (haveToSave)
        {
            SaveLevelData();
        }

        if (curLevel < levelNum)
        {
            OpenNextOrRestartPanel();
        }
        else
        {
            SceneManager.LoadScene(0);
        }
    }

    private void OpenNextOrRestartPanel()
    {
        Canvas currentCanvas = FindAnyObjectByType<Canvas>();
        SetGameTimeScale(0.0f);
        Instantiate(nextOrRestartPanelPrefab, currentCanvas.transform);
    }

    public void RestartLevel()
    {
        OpenLevel(curLevel);
    }

    public void NextLevel()
    {
        OpenLevel(curLevel + 1);
    }

    public void SetGameTimeScale(float newTimeScale)
    {
        Time.timeScale = newTimeScale;

        if (newTimeScale == 0.0f)
            AudioManager.Instance.SetBGMStop(true);
        else
            AudioManager.Instance.SetBGMStop(false);
    }

    public void SaveLevelData()
    {
        string json = JsonUtility.ToJson(levelDataArray);
        File.WriteAllText(Application.persistentDataPath + "/leveldata.json", json);
    }

    public void LoadLevelData()
    {
        string path = Application.persistentDataPath + "/leveldata.json";

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            levelDataArray = JsonUtility.FromJson<LevelDataArray>(json);

            if (levelDataArray.levels.Length < levelNum)
            {
                LevelDataArray tempLevelData = levelDataArray;

                levelDataArray = tempLevelData;

                levelDataArray.levels = new LevelInfo[levelNum];

                for (int i = 0; i < tempLevelData.levels.Length; i++)
                {
                    levelDataArray.levels[i] = tempLevelData.levels[i];
                }

                for (int i = tempLevelData.levels.Length; i < levelNum; i++)
                {
                    levelDataArray.levels[i].starCnt = 0;
                    levelDataArray.levels[i].isUnlocked = false;
                }
            }
        }
        else
        {
            levelDataArray = new LevelDataArray();
            levelDataArray.levels = new LevelInfo[levelNum];
            for (int i = 0; i < levelNum; i++)
            {
                levelDataArray.levels[i].starCnt = 0;
                levelDataArray.levels[i].isUnlocked = false;
            }

            levelDataArray.levels[0].isUnlocked = true;
        }
    }

    public bool IsLevelUnlocked(int level)
    {
        if (level > levelDataArray.levels.Length)
        {
            return false;
        }

        return levelDataArray.levels[level - 1].isUnlocked;
    }

    public int GetLevelStartCnt(int level)
    {
        if (level > levelDataArray.levels.Length)
        {
            return 0;
        }

        return levelDataArray.levels[level - 1].starCnt;
    }

    public bool InRangeLevel(int level)
    {
        return levelNum >= level;
    }

    public IEnumerator GameOver()
    {
        AudioManager.Instance.PlaySFX("Fail");

        yield return new WaitForSeconds(1f);

        RestartLevel();
    }

    public void GetStar()
    {
        curStar = Mathf.Clamp(curStar + 1, 0, 3);
    }
}
