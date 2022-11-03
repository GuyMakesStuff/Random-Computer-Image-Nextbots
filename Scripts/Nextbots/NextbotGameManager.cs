using EasyPlayerController;
using System.Collections.Generic;
using Experiments.Global.Managers;
using Experiments.Global.Interface;
using Experiments.Global.Audio;
using Experiments.Global.IO;
using UnityEngine;

public class NextbotGameManager : Manager<NextbotGameManager>
{
    [System.Serializable]
    public class Progress : SaveFile
    {
        [Space]
        public int BestKills;
        public int BestTime;
        [System.Serializable]
        public class DiscoverableImage
        {
            public string ImageName;
            public int DiscoverCount;
        }
        public List<DiscoverableImage> ImagesDiscovered;
    }

    [Space]
    public Progress progress;
    public GameStates GameState;
    public CCPlayerController Player;
    public ShootyShootyBangBang Gun;
    public GameObject GunObject;
    
    [Header("Cult")]
    public GameObject CultContainer;
    public GameObject CultMemberPrefab;
    public int CultCount;

    [Header("UI")]
    public GameObject MainPanel;
    public GameObject CrossairSprite;
    public Counter KillsCounter;
    public Counter TimeCounter;
    public float GameOverFadeTime;
    [HideInInspector]
    public int Kills;
    [HideInInspector]
    public float PlayTime;
    NextbotsMenuManager.Menu MiniMenu;
    NextbotsMenuManager.Menu.Option EnemyNameText;
    NextbotsMenuManager.Menu.Option FinalScoreText;
    NextbotsMenuManager.Menu.Option FinalTimeText;
    NextbotsMenuManager.Menu.Option ResumeButton;
    NextbotsMenuManager.Menu.Option DiscoveredText;
    public enum GameStates { MainMenu, Playing, Paused, GameOver }
    float GameOverTime;
    int ImagesDiscovered;
    [HideInInspector]
    public string KilledBy;
    [HideInInspector]
    public bool InitPhase;
    public delegate void InitEvent();
    public static event InitEvent InitFinished;
    
    // Start is called before the first frame update
    void Awake()
    {
        Init(this);

        Progress LoadedProgress = Saver.Load(progress) as Progress;
        if(LoadedProgress != null) { progress = LoadedProgress; }
        else { ResetProgress(); }

        KillsCounter.ResetNewHI(progress.BestKills);
        TimeCounter.ResetNewHI(progress.BestTime);
        KillsCounter.ValueName = "Kills";
        TimeCounter.ValueName = "Time";

        InitPhase = true;
    }
    public void WakeUp()
    {
        InitPhase = false;

        MiniMenu = NextbotsMenuManager.Instance.GetMenu("Mini");
        EnemyNameText = MiniMenu.GetOption("Kill Text");
        FinalScoreText = MiniMenu.GetOption("Final Score");
        FinalTimeText = MiniMenu.GetOption("Final Time");
        ResumeButton = MiniMenu.GetOption("Resume");
        DiscoveredText = NextbotsMenuManager.Instance.GetMenu("Gallery").GetOption("DiscoveredCount");

        CreateCult();
        Menu();

        InitFinished();
    }
    void CreateCult()
    {
        if(CultContainer.transform.childCount > 0)
        {
            for (int C = 0; C < CultContainer.transform.childCount; C++)
            {
                Destroy(CultContainer.transform.GetChild(C).gameObject);
            }
        }

        const float CultRadius = 5f;
        float CultScope = (CultRadius * 2f) * Mathf.PI;
        float CultMemberGaps = CultScope / CultCount;
        float AngleGaps = (360f / CultCount);
        for (int F = 0; F < CultCount; F++)
        {
            float Angle = AngleGaps * F;
            Vector2 OnUnitCircle = ArenaCreator.UnitCircle(Angle);
            Nextbot CultBot = Instantiate(CultMemberPrefab, Vector3.zero, Quaternion.Euler(0f, Angle, 0f), CultContainer.transform).GetComponent<Nextbot>();
            CultBot.gameObject.name = "Cult Member_" + (F + 1).ToString();
            CultBot.transform.localPosition = new Vector3(OnUnitCircle.x * CultRadius, 0.25f, OnUnitCircle.y * CultRadius);
            CultBot.Dummy = true;
            CultBot.Image = NextbotSpawner.Images[Random.Range(0, NextbotSpawner.Images.Count)];
            CultBot.AssignImage();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(IsInGame()) { PlayTime += Time.deltaTime; }

        KillsCounter.Value = Kills;
        TimeCounter.Value = (int)PlayTime;

        if(Input.GetKeyDown(KeyCode.Escape) && IsInGame()) { GameState = GameStates.Paused; PlaySelectSound(); }
        MainPanel.SetActive((GameState == GameStates.Playing || GameState == GameStates.Paused) && !InitPhase);
        Time.timeScale = (GameState == GameStates.Paused) ? 0f : 1f;
        Player.InputReciever.LockMouse = (GameState == GameStates.Playing && NextbotSettingsManager.Instance.HideMouse);

        SoundEffectBehaviour behaviour = (GameState == GameStates.Paused) ? SoundEffectBehaviour.Pause : SoundEffectBehaviour.Resume;
        AudioManager.Instance.InteractWithAllSFX(behaviour);
        AudioManager.Instance.InteractWithMusic(behaviour);
        Player.PlaySFX = IsInGame();

        if(!InitPhase)
        {
            MiniMenu.ActiveSelf = (GameState == GameStates.Paused || GameState == GameStates.GameOver);
            CrossairSprite.SetActive(!MiniMenu.ActiveSelf);
            ResumeButton.Active = (GameState == GameStates.Paused);
            EnemyNameText.Text = "You Got Killed By " + KilledBy + "!";
            EnemyNameText.Active = (GameState == GameStates.GameOver);
            FinalScoreText.Text = "Kills:" + Kills.ToString() + "   ---   Best Kills:" + KillsCounter.HighValue.ToString();
            FinalScoreText.Active = (GameState == GameStates.GameOver);
            FinalTimeText.Text = "Time:" + TimeCounter.Value.ToString() + "   ---   Best Time:" + TimeCounter.HighValue.ToString();
            FinalTimeText.Active = (GameState == GameStates.GameOver);

            if(GameState == GameStates.GameOver && GameOverTime < GameOverFadeTime) { GameOverTime += Time.deltaTime; }
            MiniMenu.ActiveElements = (GameState != GameStates.GameOver ^ GameOverTime >= GameOverFadeTime);
            MiniMenu.Title = (GameState == GameStates.GameOver) ? "Game Over!" : "Paused";
            MiniMenu.TitleColor = (GameState == GameStates.GameOver) ? Color.red : Color.white;
            MiniMenu.BackgorundColor = (GameState == GameStates.GameOver) ? new Color(1f, 0f, 0f, (GameOverTime / GameOverFadeTime) / 2f) : new Color(0f, 0f, 0f, 0.5f);

            DiscoveredText.Text = "Images Discovered: " + ImagesDiscovered.ToString() + "/" + NextbotSpawner.Images.Count;
        }

        GunObject.SetActive(!IsInMenu());
        Player.CanMove = IsInGame() && !InitPhase;
        Gun.enabled = !IsInMenu() && !InitPhase;
        CultContainer.SetActive(IsInMenu());
        if(IsInMenu() || InitPhase)
        { 
            Player.ResetOrientation(false);
            Player.Teleport(Vector3.up * 1.5f, true);; 
        }

        progress.BestKills = KillsCounter.HighValue;
        progress.BestTime = TimeCounter.HighValue;
        progress.Save();
    }

    public void Resume()
    {
        GameState = GameStates.Playing;
        NextbotsMenuManager.Instance.SetMenu("");
        PlaySelectSound();
    }
    public void Retry()
    {
        Player.ResetOrientation(true);
        Player.Teleport(Vector3.up * 5f, true);
        Gun.ResetAmmo();
        NextbotSpawner.Instance.ResetValues();

        KillsCounter.ResetNewHI(progress.BestKills);
        TimeCounter.ResetNewHI(progress.BestTime);

        Kills = 0;
        PlayTime = 0f;
        GameOverTime = 0f;
        GameState = GameStates.Playing;
        NextbotsMenuManager.Instance.SetMenu("");
        AudioManager.Instance.SetMusicTrack("Arena");

        PlaySelectSound();
    }
    public void Menu()
    {
        Gun.ResetAmmo();
        NextbotSpawner.Instance.ResetValues();

        KillsCounter.ResetNewHI(progress.BestKills);
        TimeCounter.ResetNewHI(progress.BestTime);

        Kills = 0;
        PlayTime = 0f;
        GameOverTime = 0f;
        GameState = GameStates.MainMenu;
        NextbotsMenuManager.Instance.SetMenu("Main");

        PlaySelectSound();
        AudioManager.Instance.SetMusicTrack("Cult");
    }

    public bool IsInGame()
    {
        return GameState == GameStates.Playing;
    }
    public bool IsInMenu()
    {
        return GameState == GameStates.MainMenu;
    }

    public void DiscoverImage(string imageName)
    {
        Progress.DiscoverableImage ImageID = progress.ImagesDiscovered.Find(DiscoverableImage => DiscoverableImage.ImageName == imageName);
        if(ImageID == null)
        {
            ImageID = new Progress.DiscoverableImage();
            ImageID.ImageName = imageName;
            progress.ImagesDiscovered.Add(ImageID);
        }

        ImageID.DiscoverCount++;
    }
    public void GetDiscoveredImages()
    {
        int TotalDiscovered = 0;
        for (int I = 0; I < progress.ImagesDiscovered.Count; I++)
        {
            if(NextbotSpawner.Images.Exists(Texture2D => Texture2D.name == progress.ImagesDiscovered[I].ImageName))
            {
                TotalDiscovered++;
            }
        }

        ImagesDiscovered = TotalDiscovered;
    }
    public void ResetProgress()
    {
        progress.BestKills = 0;
        progress.BestTime = 0;
        progress.ImagesDiscovered = new List<Progress.DiscoverableImage>();

        progress.Save();

        KillsCounter.ResetNewHI(progress.BestKills);
        TimeCounter.ResetNewHI(progress.BestTime);
    }
}
