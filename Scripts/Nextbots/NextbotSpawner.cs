using System.Collections;
using System.Collections.Generic;
using Experiments.Global.Managers;
using Experiments.Global.Audio;
using UnityEngine;
using System.IO;
using System;

public class NextbotSpawner : Manager<NextbotSpawner>
{
    [Header("General")]
    public Transform NextbotContainer;
    public GameObject NextbotPrefab;
    public int MaxDifficultySpawns;
    int SpawnedCount;

    [Header("Images")]
    public string[] Paths;
    string[] PrevPaths;
    public bool IncludeSubdirectories;
    bool PrevSubDir;
    public GameObject ImageLoadScreen;
    public TMPro.TMP_Text ImageLoadText;
    [HideInInspector]
    public static List<Texture2D> Images;
    static List<Texture2D> PrevImages;

    [Header("Spawn Settings")]
    public float SpawnRadius;
    public float SpawnHeight;
    public float StartSpawnDelay;
    public float EndSpawnDelay;
    float SpawnDelayGap;
    float SpawnDelayIntervals;
    float SpawnDelay;
    float SpawnTimer;

    [Header("Bots")]
    public float BaseBotSpeed;
    public float BaseBotAcceleration;
    public float MaxBotSpeed;
    float MaxBotAcceleration;
    float BotSpeedGap;
    float BotSpeedIntervals;
    float BotAccelerationGap;
    float BotAccelerationIntervals;

    // Start is called before the first frame update
    void Awake()
    {
        Init(this);

        Paths = null;
        Images = new List<Texture2D>();

        SpawnDelayGap = StartSpawnDelay - EndSpawnDelay;
        SpawnDelayIntervals = SpawnDelayGap / MaxDifficultySpawns;
        SpawnDelay = StartSpawnDelay;

        BotSpeedGap = MaxBotSpeed - BaseBotSpeed;
        BotSpeedIntervals = BotSpeedGap / MaxDifficultySpawns;
        float a = BaseBotSpeed / BaseBotAcceleration;
        MaxBotAcceleration = BaseBotAcceleration * a;
        BotAccelerationGap = MaxBotAcceleration - BaseBotAcceleration;
        BotAccelerationIntervals = BotAccelerationGap / MaxDifficultySpawns;

        SpawnTimer = SpawnDelay;
    }
    public void Refreash()
    {
        StartCoroutine(InitImages());
    }
    IEnumerator InitImages()
    {
        ImageLoadScreen.SetActive(true);
        NextbotGameManager.Instance.InitPhase = true;

        Images.Clear();

        for (int P = 0; P < Paths.Length; P++)
        {
            bool ValidPath;
            string[] PNGsFound = new string[0];
            string[] JPGsFound = new string[0];

            try
            {
                PNGsFound = Directory.GetFiles(Paths[P], "*.png", (IncludeSubdirectories) ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
                JPGsFound = Directory.GetFiles(Paths[P], "*.jpg", (IncludeSubdirectories) ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

                ValidPath = true;
            }
            catch (DirectoryNotFoundException)
            {
                ValidPath = false;
                NextbotsMenuManager.Instance.ShowDialog("<color=red>Invalid Directory!\n(" + Paths[P] + ")", true, NextbotsMenuManager.DialogClose);
            }
            catch (ArgumentException)
            {
                ValidPath = false;
                NextbotsMenuManager.Instance.ShowDialog("<color=red>Invalid Directory!\n(" + Paths[P] + ")", true, NextbotsMenuManager.DialogClose);
            }
            catch (UnauthorizedAccessException)
            {
                ValidPath = false;
                NextbotsMenuManager.Instance.ShowDialog("<color=red>Access Denied To Directory!\n(" + Paths[P] + ")", true, NextbotsMenuManager.DialogClose);
            }

            while(NextbotsMenuManager.DialogOpen && !ValidPath)
            {
                yield return null;
            }

            if(ValidPath)
            {
                yield return StartCoroutine(LoadImages(PNGsFound));
                yield return StartCoroutine(LoadImages(JPGsFound));
            }

            yield return null;
        }

        if(Images.Count == 0)
        {
            NextbotsMenuManager.Instance.ShowDialog("<color=red>Could Not Find/Load Any Images!", true, NextbotsMenuManager.DialogClose);
            while(NextbotsMenuManager.DialogOpen)
            {
                yield return null;
            }

            if(PrevPaths == null)
            {
                NextbotsMenuManager.Instance.QuitGame();
            }
            else
            {
                Paths = PrevPaths;
                IncludeSubdirectories = PrevSubDir;
                Images = new List<Texture2D>(PrevImages);

                NextbotSettingsManager.Instance.SetPaths(new List<string>(PrevPaths));
                NextbotSettingsManager.Instance.settingsMenu.GetSetting("Images", "Subdirectories").SetValue(0f, 0, PrevSubDir);
                ImageLoadScreen.SetActive(false);
            }

            yield break;
        }

        ImageLoadText.text = "Finishing Up...";
        AudioManager.Instance.InteractWithSFX("Finish Up", SoundEffectBehaviour.Play);
        SetPrevPaths();
        yield return null;

        NextbotGameManager.Instance.WakeUp();
        ImageLoadScreen.SetActive(false);
        AudioManager.Instance.InteractWithSFX("Finish Up", SoundEffectBehaviour.Stop);
    }
    IEnumerator LoadImages(string[] Files)
    {
        if(Files.Length == 0) { yield break; }

        for (int F = 0; F < Files.Length; F++)
        {
            bool Success = false;
            string FileName = "[NULL]";
            try
            {
                FileName = Path.GetFileName(Files[F]);
                byte[] ImageData = File.ReadAllBytes(Files[F]);
                Texture2D Image = new Texture2D(1, 1);
                bool Loaded = ImageConversion.LoadImage(Image, ImageData, false);
                if(Loaded)
                {
                    Image.name = FileName;
                    Image.wrapMode = TextureWrapMode.Clamp;
                    Images.Add(Image);

                    Success = true;
                    ImageLoadText.text = "Loading Images...\nLoaded " + Files[F];
                    AudioManager.Instance.InteractWithSFX("Add Image", SoundEffectBehaviour.Play);
                }
                else
                {
                    Success = false;
                    NextbotsMenuManager.Instance.ShowDialog("<color=red>Unable To Load File " + FileName + "!", true, NextbotsMenuManager.DialogClose);
                }
            }
            catch (FileLoadException)
            {
                Success = false;
                NextbotsMenuManager.Instance.ShowDialog("<color=red>Error Loading " + FileName + "!", true, NextbotsMenuManager.DialogClose);
            }

            while(NextbotsMenuManager.DialogOpen && !Success)
            {
                yield return null;
            }

            yield return null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(NextbotGameManager.Instance.IsInMenu()) { return; }

        SpawnTimer -= Time.deltaTime;

        if(SpawnTimer <= 0f)
        {
            if(SpawnDelay > EndSpawnDelay) { SpawnDelay -= SpawnDelayIntervals; }
            SpawnTimer = SpawnDelay;

            SpawnNextbot();
        }
    }
    void SpawnNextbot()
    {
        Vector3 UnitCircle = UnityEngine.Random.insideUnitSphere * SpawnRadius;
        Vector3 SpawnPos = new Vector3(UnitCircle.x, SpawnHeight, UnitCircle.y);

        Nextbot nextbot = Instantiate(NextbotPrefab, SpawnPos, Quaternion.identity, NextbotContainer).GetComponent<Nextbot>();
        nextbot.Image = Images[UnityEngine.Random.Range(0, Images.Count)];
        nextbot.AssignImage();
        nextbot.gameObject.name = "Nextbot_" + nextbot.Image.name + "_" + SpawnedCount.ToString();
        nextbot.AccelerationSpeed = Mathf.Clamp(BaseBotAcceleration + (BotAccelerationIntervals * SpawnedCount), BaseBotAcceleration, MaxBotAcceleration);
        nextbot.MaxAcceleration = Mathf.Clamp(BaseBotSpeed + (BotSpeedIntervals * SpawnedCount), BaseBotSpeed, MaxBotSpeed);

        SpawnedCount++;
    }

    public void SetPrevPaths()
    {
        PrevPaths = Paths;
        PrevSubDir = IncludeSubdirectories;
        PrevImages = new List<Texture2D>(Images);
    }

    public void ResetValues()
    {
        SpawnDelay = StartSpawnDelay;
        SpawnTimer = SpawnDelay;
        SpawnedCount = 0;

        Nextbot[] Bots = FindObjectsOfType<Nextbot>();
        if(Bots.Length > 0)
        {
            foreach (Nextbot Bot in Bots)
            {
                if(Bot.Dummy) { continue; }
                Destroy(Bot.gameObject);
            }
        }
    }
}