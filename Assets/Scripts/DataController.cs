using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class DataController : MonoBehaviour
{
    public LevelData[] allLevels;

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        InitializeLevels();
        SceneManager.LoadScene("MenuScreen");
    }

    public void InitializeLevels()
    {
        // Load all sprites from the Resources folder
        Sprite[] sprites = Resources.LoadAll<Sprite>("Levels");

        // Initialize the allLevels array
        allLevels = new LevelData[sprites.Length];

        for (int i = 0; i < sprites.Length; i++)
        {
            string spriteName = sprites[i].name;

            // Extract the level number and answer from the sprite name
            string[] parts = spriteName.Split('_');
            if (parts.Length < 2)
            {
                Debug.LogError($"Invalid sprite name format: {spriteName}. Expected format: 'levelNumber_answer'");
                continue;
            }
            if (!int.TryParse(parts[0], out int levelNumber))
            {
                Debug.LogError($"Invalid level number in sprite name: {spriteName}. '{parts[0]}' is not a valid integer.");
                continue;
            }
            string answer = parts[1].ToUpper();

            allLevels[i] = new LevelData
            {
                levelNumber = levelNumber,
                picture = sprites[i],
                answer = answer
            };
        }

        // Sort levels by level number
        allLevels = allLevels.Where(level => level != null).OrderBy(level => level.levelNumber).ToArray();
    }

    public LevelData GetCurrentLevelData(int levelNumber)
    {
        if (levelNumber <= 0 || levelNumber > allLevels.Length)
        {
            Debug.LogError($"Level number {levelNumber} is out of bounds.");
            return null;
        }
        return allLevels[levelNumber - 1];
    }

}