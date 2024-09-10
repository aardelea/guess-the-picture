using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    private BannerAd bannerAd;

    public static GameController Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            audioSource = GetComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private DataController dataController;
    public Image pictureImage;
    public Transform lettersContainer;
    public GameObject letterButtonPrefab;
    public Transform answerContainer;
    public GameObject answerSlotPrefab;
    public Text levelText;
    public Text coinsText;
    public float maxImageHeight = 512;
    public int coins;
    public Button buyCoinsButton;
    public GameObject buyCoinsConfirmationPanel;
    public Button coinsConfirmButton;
    public Button coinsCancelButton;
    public Button exchangeButton;

    public Button refreshButton;
    private bool isRefreshing = false;

    public Text hintsText;
    public int initialHints = 2;
    private int hints;
    private Color hintLetterColor = Color.grey;
    private Dictionary<char, int> hintLettersCount = new Dictionary<char, int>();
    public Button hintsButton;
    public GameObject coinsUsagePanel;
    public GameObject noMoreCoinsPanel;

    private List<GameObject> answerSlots = new List<GameObject>();
    private List<char> currentAnswer = new List<char>();
    private string correctAnswer;
    public int currentLevel = 1;

    public GameObject correctPanel;
    public float moveDistance = 50f;
    public float duration = 1f;
    private Vector2 originalPositionCorrectMessage;
    private Vector2 originalPositionCoinsUsageMsg;
    private Vector2 originalPositionNoMoreCoinsMsg;

    public Image wrongAnswerImage;
    public float wrongAnswerDuration= 0.5f;

    public CanvasGroup mainCanvasGroup;
    
    public GameObject endOfGamePopup;

    private AudioSource audioSource;
    public AudioClip buttonPressClip;

    public Button backToMenuButton;

    public void BackToMenu()
    {
        SavePlayerState();
        SceneManager.LoadScene("MenuScreen");
    }

    void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        StartCoroutine(InitializeGame());
        startAdBanner();
    }

    void InitializeUI()
    {
        refreshButton.onClick.AddListener(() => { PlayButtonSound(); OnRefreshButtonClicked(); });
        buyCoinsButton.onClick.AddListener(() => { PlayButtonSound(); OnBuyCoinsButtonClicked(); });
        coinsConfirmButton.onClick.AddListener(() => { PlayButtonSound(); OnConfirmPurchase(); });
        coinsCancelButton.onClick.AddListener(() => { PlayButtonSound(); OnCancelPurchase(); });
        exchangeButton.onClick.AddListener(() => { PlayButtonSound(); ExchangeCoinsForHint(); });
        hintsButton.onClick.AddListener(() => { PlayButtonSound(); UseHint(); });
        backToMenuButton.onClick.AddListener(() => { PlayButtonSound(); BackToMenu(); });

        originalPositionCorrectMessage = correctPanel.GetComponent<RectTransform>().anchoredPosition;
        originalPositionCoinsUsageMsg = coinsUsagePanel.GetComponent<RectTransform>().anchoredPosition;
        originalPositionNoMoreCoinsMsg = noMoreCoinsPanel.GetComponent<RectTransform>().anchoredPosition;

        CheckDailyHint();

        if (endOfGamePopup != null)
        {
            endOfGamePopup.SetActive(false);
        }
    }

    void OnRefreshButtonClicked()
    {
        isRefreshing = true;
        LoadLevel(currentLevel);
    }

    void startAdBanner()
    {
        bannerAd = FindObjectOfType<BannerAd>();
        if (bannerAd != null)
        {
            bannerAd.CreateBannerView();
            bannerAd.LoadAd();
        }
    }

    IEnumerator InitializeGame()
    {

        while (dataController == null)
        {
            dataController = FindObjectOfType<DataController>();
            yield return null;
        }

        LoadPlayerState(); // Load player state after dataController is initialized
        InitializeUI(); // Initialize UI elements and listeners
        LoadLevel(currentLevel); // Load the level data after player state is loaded
    }

    void ResetLettersContainer()
    {
        foreach (Transform child in lettersContainer)
        {
            Destroy(child.gameObject);
        }
    }

    void LoadLevel(int levelNumber)
    {
        LevelData levelData = dataController.GetCurrentLevelData(levelNumber);
        if (levelData == null)
        {
            Debug.LogError("Level data not found for level: " + levelNumber);
            return;
        }

        correctAnswer = levelData.answer;

        if (pictureImage == null || lettersContainer == null || letterButtonPrefab == null || answerContainer == null || answerSlotPrefab == null || levelText == null || coinsText == null)
        {
            Debug.LogError("One or more UI elements are not assigned!");
            return;
        }

        levelText.text = "Level " + levelNumber;
        UpdateCoinsHintsUI();

        pictureImage.sprite = levelData.picture;
        SetImageSize(pictureImage);

        ResetLettersContainer();

        List<Color> existingColors = new List<Color>();
        foreach (GameObject slot in answerSlots)
        {
            Image slotImage = slot.GetComponent<Image>();
            if (slotImage != null)
            {
                existingColors.Add(slotImage.color);
            }
            else
            {
                existingColors.Add(Color.clear);
            }
        }

        ResetAnswer();

        hintLettersCount.Clear();

        // Calculate scale factor based on answer length
        float scaleFactor = Mathf.Clamp(10.0f / levelData.answer.Length, 0.5f, 1.5f);

        List<char> existingAnswer = new List<char>(currentAnswer);
        currentAnswer.Clear();

        for (int i = 0; i < levelData.answer.Length; i++)
        {
            GameObject slot = Instantiate(answerSlotPrefab, answerContainer);
            Text slotText = slot.GetComponentInChildren<Text>();

            if (isRefreshing && existingAnswer.Count > i && existingColors[i] == hintLetterColor)
            {
                currentAnswer.Add(existingAnswer[i]);
                slotText.text = existingAnswer[i].ToString();

                Image slotImage = slot.GetComponent<Image>();
                if (slotImage != null)
                {
                    slotImage.enabled = true;
                    slotImage.color = hintLetterColor;
                }

                IncrementHintLetterCount(existingAnswer[i]);
            }
            else if (!isRefreshing && existingAnswer.Count > i && existingAnswer[i] != '_')
            {
                currentAnswer.Add(existingAnswer[i]);
                slotText.text = existingAnswer[i].ToString();
            }
            else
            {
                currentAnswer.Add('_');
                slotText.text = "_";
            }

            slotText.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor); // Scale the letters
            answerSlots.Add(slot);
        }

        char[] availableLetters = GetShuffledLetters(levelData.answer.ToCharArray(), levelData.answer);
        foreach (char letter in availableLetters)
        {
            GameObject letterButton = Instantiate(letterButtonPrefab, lettersContainer);
            Text letterText = letterButton.GetComponentInChildren<Text>();
            letterText.text = letter.ToString();
            letterText.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor); // Scale the letters
            letterButton.GetComponent<Button>().onClick.AddListener(() => { PlayButtonSound(); OnLetterClicked(letterButton, letter); });

            if (hintLettersCount.ContainsKey(letter) && hintLettersCount[letter] > 0)
            {
                letterButton.GetComponent<Button>().interactable = false;
                hintLettersCount[letter]--;
            }
        }

        isRefreshing = false;
    }

    void ResetAnswer()
    {
        foreach (Transform child in answerContainer)
        {
            Destroy(child.gameObject);
        }
        answerSlots.Clear();
    }

    void SetImageSize(Image image)
    {
        image.SetNativeSize();
        float originalWidth = image.rectTransform.rect.width;
        float originalHeight = image.rectTransform.rect.height;

        if (originalHeight > maxImageHeight)
        {
            float scaleFactor = maxImageHeight / originalHeight;
            image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, maxImageHeight);
            image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, originalWidth * scaleFactor);
        }
    }

    char[] GetShuffledLetters(char[] realLetters, string answer)
    {
        int answerLength = answer.Length;
        int minFakeLetters = Mathf.Max(1, 6 - answerLength);
        int maxFakeLetters = Mathf.Max(3, 6 - answerLength); 

        List<char> letters = new List<char>(realLetters);
        int numFakeLetters = Random.Range(minFakeLetters, maxFakeLetters + 1);
        for (int i = 0; i < numFakeLetters; i++)
        {
            char fakeLetter;
            do
            {
                fakeLetter = (char)Random.Range('A', 'Z' + 1);
            } while (letters.Contains(fakeLetter));
            letters.Add(fakeLetter);
        }

        for (int i = 0; i < letters.Count; i++)
        {
            int randomIndex = Random.Range(0, letters.Count);
            char temp = letters[i];
            letters[i] = letters[randomIndex];
            letters[randomIndex] = temp;
        }

        return letters.ToArray();
    }

    void OnLetterClicked(GameObject letterButton, char letter)
    {
        for (int i = 0; i < currentAnswer.Count; i++)
        {
            if (currentAnswer[i] == '_')
            {
                currentAnswer[i] = letter;
                answerSlots[i].GetComponentInChildren<Text>().text = letter.ToString();
                letterButton.GetComponent<Button>().interactable = false; // Disable interaction instead of hiding
                GameObject answerLetter = answerSlots[i];
                answerLetter.GetComponent<Button>().onClick.RemoveAllListeners();
                answerLetter.GetComponent<Button>().onClick.AddListener(() => OnAnswerLetterClicked(i, letter, letterButton));
                CheckAnswer();
                return;
            }
        }
    }

    void OnAnswerLetterClicked(int index, char letter, GameObject letterButton)
    {
        if (answerSlots[index].GetComponentInChildren<Text>().color == hintLetterColor)
        {
            return; // Prevent removing letters placed by hints
        }

        currentAnswer[index] = '_';
        answerSlots[index].GetComponentInChildren<Text>().text = "_";

        // Check if the letter was part of the hint
        if (!hintLettersCount.ContainsKey(letter) || hintLettersCount[letter] == 0)
        {
            letterButton.GetComponent<Button>().interactable = true; // Re-enable interaction
        }
        else
        {
            // Decrease the count of hint letters
            hintLettersCount[letter]--;
        }

        answerSlots[index].GetComponent<Button>().onClick.RemoveAllListeners(); // Remove the listener after clicking
    }

    public void UseHint()
    {
        if (hints > 0)
        {
            hints--;
            UpdateCoinsHintsUI();
            PlaceRandomCorrectLetter();
        }
    }

    void PlaceRandomCorrectLetter()
    {
        List<int> availableIndexes = new List<int>();

        for (int i = 0; i < correctAnswer.Length; i++)
        {
            if (currentAnswer[i] == '_' && correctAnswer[i] != '_')
            {
                availableIndexes.Add(i);
            }
        }

        if (availableIndexes.Count > 0)
        {
            int randomIndex = availableIndexes[Random.Range(0, availableIndexes.Count)];
            char letter = correctAnswer[randomIndex];
            currentAnswer[randomIndex] = letter;
            answerSlots[randomIndex].GetComponentInChildren<Text>().text = letter.ToString();

            Image slotImage = answerSlots[randomIndex].GetComponent<Image>();
            if (slotImage != null)
            {
                slotImage.enabled = true;
                slotImage.color = hintLetterColor;
            }

            IncrementHintLetterCount(letter);

            foreach (Transform child in lettersContainer)
            {
                Button button = child.GetComponent<Button>();
                if (button.GetComponentInChildren<Text>().text == letter.ToString() && button.interactable)
                {
                    button.interactable = false;
                    break;
                }
            }
        }

        CheckAnswer();
    }

    void CheckAnswer()
    {
        if (currentAnswer.Contains('_'))
        {
            return;
        }

        if (new string(currentAnswer.ToArray()) == correctAnswer)
        {
            currentLevel++;
            if (currentLevel <= dataController.allLevels.Length)
            {
                ShowCorrectMessage();
            }
            else
            {
                ShowGameCompletedPopup();
            }
        }
        else
        {
            IncorrectAnswer();
        }
    }

    public void ShowCorrectMessage()
    {
        mainCanvasGroup.interactable = false; // Disable interactions
        mainCanvasGroup.blocksRaycasts = false; // Block raycasts to prevent clicks
        correctPanel.SetActive(true);
        StartCoroutine(MoveAndHideCorrectMessage());
    }

    IEnumerator MoveAndHideCorrectMessage()
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            Vector2 newPosition = originalPositionCorrectMessage + Vector2.up * Mathf.Lerp(0, moveDistance, t);
            correctPanel.GetComponent<RectTransform>().anchoredPosition = newPosition;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        correctPanel.GetComponent<RectTransform>().anchoredPosition = originalPositionCorrectMessage;
        correctPanel.SetActive(false);
        mainCanvasGroup.interactable = true; // Re-enable interactions
        mainCanvasGroup.blocksRaycasts = true; // Allow raycasts to pass through
        coins += 10;
        ResetGameVariables();
        LoadLevel(currentLevel);
    }

    void ResetGameVariables()
    {
        currentAnswer.Clear();
        hintLettersCount.Clear();
        ResetLettersContainer();
        ResetAnswer();
    }

    public void IncorrectAnswer()
    {
        wrongAnswerImage.color = new Color(1f, 0f, 0f, 0.1f);
        wrongAnswerImage.gameObject.SetActive(true);
        StartCoroutine(FadeOutRedOverlay());
    }

    IEnumerator FadeOutRedOverlay()
    {
        float elapsedTime = 0f;

        while (elapsedTime < wrongAnswerDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / wrongAnswerDuration);
            wrongAnswerImage.color = new Color(1f, 0f, 0f, alpha);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        wrongAnswerImage.color = new Color(1f, 0f, 0f, 0f);
        wrongAnswerImage.gameObject.SetActive(false);
    }

    void CheckDailyHint()
    {
        string lastHintDate = PlayerPrefs.GetString("LastHintDate", "");
        string currentDate = System.DateTime.Now.ToString("yyyy-MM-dd");

        if (lastHintDate != currentDate)
        {
            hints++;
            UpdateCoinsHintsUI();
            PlayerPrefs.SetString("LastHintDate", currentDate);
        }
    }

    void UpdateCoinsHintsUI()
    {
        coinsText.text = "Coins: " + coins;
        hintsText.text = "Hints: " + hints;
    }

    public void ShowGameCompletedPopup()
    {
        if (endOfGamePopup != null)
        {
            endOfGamePopup.SetActive(true);
            ResetGameState();
        }
    }

    public void ExchangeCoinsForHint()
    {
        if (coinsUsagePanel != null) coinsUsagePanel.SetActive(false);
        if (noMoreCoinsPanel != null) noMoreCoinsPanel.SetActive(false);

        if (coins >= 10)
        {
            coins -= 10;
            hints++;
            ShowCoinsUsageMessage();
            UpdateCoinsHintsUI();
        }
        else
        {
            ShowNoMoreCoinsMessage();
        }
    }

    public void ShowCoinsUsageMessage()
    {
        mainCanvasGroup.interactable = false; // Disable interactions
        mainCanvasGroup.blocksRaycasts = false; // Block raycasts to prevent clicks
        coinsUsagePanel.SetActive(true);
        StartCoroutine(MoveAndHideCoinsUsageMessage());
    }

    void IncrementHintLetterCount(char letter)
    {
        if (!hintLettersCount.ContainsKey(letter))
        {
            hintLettersCount[letter] = 0;
        }
        hintLettersCount[letter]++;
    }

    public void ShowNoMoreCoinsMessage()
    {
        mainCanvasGroup.interactable = false; // Disable interactions
        mainCanvasGroup.blocksRaycasts = false; // Block raycasts to prevent clicks
        noMoreCoinsPanel.SetActive(true);
        StartCoroutine(MoveAndHideNoMoreCoinsMessage());
    }

    IEnumerator MoveAndHideCoinsUsageMessage()
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            Vector2 newPosition = originalPositionCoinsUsageMsg - Vector2.up * Mathf.Lerp(0, moveDistance, t);
            coinsUsagePanel.GetComponent<RectTransform>().anchoredPosition = newPosition;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        coinsUsagePanel.GetComponent<RectTransform>().anchoredPosition = originalPositionCoinsUsageMsg;
        coinsUsagePanel.SetActive(false);
        mainCanvasGroup.interactable = true; // Re-enable interactions
        mainCanvasGroup.blocksRaycasts = true; // Allow raycasts to pass through
    }

    IEnumerator MoveAndHideNoMoreCoinsMessage()
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            Vector2 newPosition = originalPositionNoMoreCoinsMsg - Vector2.up * Mathf.Lerp(0, moveDistance, t);
            noMoreCoinsPanel.GetComponent<RectTransform>().anchoredPosition = newPosition;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        noMoreCoinsPanel.SetActive(false);
        noMoreCoinsPanel.GetComponent<RectTransform>().anchoredPosition = originalPositionNoMoreCoinsMsg;
        mainCanvasGroup.interactable = true; // Re-enable interactions
        mainCanvasGroup.blocksRaycasts = true; // Allow raycasts to pass through
    }

    void OnBuyCoinsButtonClicked()
    {
        if (IAPManager.Instance == null)
        {
            Debug.LogError("IAPManager.Instance is null");
        }
        else
        {
            if (coins <= 0)
            {
                ShowConfirmationDialog();
            }
        }
    }

    void ShowConfirmationDialog()
    {
        mainCanvasGroup.interactable = false;
        buyCoinsConfirmationPanel.SetActive(true);
        coinsConfirmButton.interactable = true;
        coinsCancelButton.interactable = true;
    }

    public void OnConfirmPurchase()
    {
        OnCancelPurchase();
        IAPManager.Instance.BuyCoinPack50();
    }

    public void OnCancelPurchase()
    {
        buyCoinsConfirmationPanel.SetActive(false);
        mainCanvasGroup.interactable = true;
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        UpdateCoinsHintsUI();
    }

    void SavePlayerState()
    {
        PlayerPrefs.SetInt("CurrentLevel", currentLevel);
        PlayerPrefs.SetInt("Coins", coins);
        PlayerPrefs.SetInt("Hints", hints);

        // Save currentAnswer as a string
        string currentAnswerString = new string(currentAnswer.ToArray());
        PlayerPrefs.SetString("CurrentAnswer", currentAnswerString);

        string hintColors = "";
        foreach (GameObject slot in answerSlots)
        {
            Image slotImage = slot.GetComponent<Image>();
            if (slotImage != null && slotImage.color == hintLetterColor)
            {
                hintColors += "1"; // 1 indicates it's a hint letter
            }
            else
            {
                hintColors += "0"; // 0 indicates it's not a hint letter
            }
        }
        PlayerPrefs.SetString("HintColors", hintColors);

        PlayerPrefs.Save();
    }

    void LoadPlayerState()
    {
        if (PlayerPrefs.HasKey("CurrentLevel"))
        {
            currentLevel = PlayerPrefs.GetInt("CurrentLevel");
            coins = PlayerPrefs.GetInt("Coins");
            hints = PlayerPrefs.GetInt("Hints");

            // Load currentAnswer
            LevelData levelData = dataController.GetCurrentLevelData(currentLevel);
            if (levelData != null)
            {
                string savedAnswer = PlayerPrefs.GetString("CurrentAnswer", new string('_', levelData.answer.Length));
                currentAnswer = new List<char>(savedAnswer.ToCharArray());
                isRefreshing = true;

                ResetAnswer();

                string hintColors = PlayerPrefs.GetString("HintColors", new string('0', currentAnswer.Count));

                for (int i = 0; i < currentAnswer.Count; i++)
                {
                    GameObject slot = Instantiate(answerSlotPrefab, answerContainer);
                    Text slotText = slot.GetComponentInChildren<Text>();
                    slotText.text = currentAnswer[i] != '_' ? currentAnswer[i].ToString() : "_";

                    Image slotImage = slot.GetComponent<Image>();
                    if (hintColors[i] == '1' && slotImage != null)
                    {
                        slotImage.color = hintLetterColor;
                    }

                    answerSlots.Add(slot);
                }

            }
            else
            {
                Debug.LogError("Level data not found for the current level.");
                currentAnswer = new List<char>();
                isRefreshing = false;
            }

        }
        else
        {
            // Initialize defaults if no saved state
            currentLevel = 1;
            coins = 10;
            hints = 2;
            currentAnswer = new List<char>();
            isRefreshing = false;
        }

        dataController.InitializeLevels();
        LoadLevel(currentLevel);

        // Update UI

        UpdateCoinsHintsUI();
        levelText.text = "Level " + currentLevel;
    }

    void OnApplicationQuit()
    {
        SavePlayerState();
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SavePlayerState();
        }
    }

    void PlayButtonSound()
    {
        if (audioSource != null && buttonPressClip != null)
        {
            audioSource.PlayOneShot(buttonPressClip);
        }
    }
    void OnDestroy()
    {
        if (bannerAd != null)
        {
            bannerAd.DestroyAd();
        }
    }

    private void ResetGameState()
    {
        // Reset player progress
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        // Set default values
        currentLevel = 1;
        coins = 10;
        hints = 2;
        currentAnswer.Clear();
        isRefreshing = false;

        // Ensure levels are initialized properly
        dataController.InitializeLevels();
        LoadLevel(currentLevel);
        UpdateCoinsHintsUI();
    }
}
