using System;
using Experiments.Global.IO;
using System.Collections.Generic;
using Experiments.Global.Managers;
using Experiments.Global.Audio;
using UnityEngine;

public class NextbotSettingsManager : Manager<NextbotSettingsManager>
{
    [Serializable]
    public class SliderSetting
    {
        public string Name;
        [HideInInspector]
        public float Value;
        public float DefaultValue;
        public float MinValue;
        public float MaxValue;
    }
    [Serializable]
    public class DropdownSetting
    {
        public string Name;
        [HideInInspector]
        public int Value;
        public int DefaultValue;
        public List<string> Options;
    }
    [Serializable]
    public class ToggleSetting
    {
        public string Name;
        [HideInInspector]
        public bool Value;
        public bool DefaultValue;
    }
    [Serializable]
    public class Settings : SaveFile
    {
        [Space]
        public SliderSetting[] SliderSettings;
        public DropdownSetting[] DropdownSettings;
        public ToggleSetting[] ToggleSettings;
        public List<string> ImagePaths;
    }
    [Space]
    public Settings settings;
    [HideInInspector]
    public ItemList ImagePathList;
    [HideInInspector]
    public SettingsMenu settingsMenu;
    [HideInInspector]
    public bool HideMouse;
    int ResIndex;
    int PrevResIndex;
    Resolution[] Resolutions;
    bool SettingsLoadedSuccessfuly;
    Camera Cam;
    bool SubDir;
    List<string> Paths;

    // Start is called before the first frame update
    void Awake()
    {
        Init(this);

        settingsMenu = NextbotsMenuManager.Instance.GetMenu("Settings").ContentsObject.GetComponent<SettingsMenu>();
        ImagePathList = settingsMenu.GetCategory("Images").ExtraContentsObject.GetComponent<ItemList>();

        Settings LoadedSettings = Saver.Load(settings) as Settings;
        SettingsLoadedSuccessfuly = LoadedSettings != null;
        if(SettingsLoadedSuccessfuly) { settings = LoadedSettings; }
        else { ResetSettings(); }

        Paths = new List<string>(settings.ImagePaths);
        Cam = Camera.main;
        InitRes();

        ImagePathList.SetItems(settings.ImagePaths);
    }
    void InitRes()
    {
        // Store The Resolution Setting In A Variable
        DropdownSetting ResSetting = FindDropdownSetting("Resolution");
        // Clear All Previous Options From The Resolution Setting.
        // We Want The Options To Automatically Be All Of The Available Screen Resolutions.
        ResSetting.Options.Clear();
        // Get All Available Screen Resolutions.
        Resolutions = Screen.resolutions;
        // The Index Of The Current Screen Resolution On The Resolutions Array.
        int CurResIndex = 0;
        for (int R = 0; R < Resolutions.Length; R++)
        {
            // The Screen Resolution From The Resolutions Array At The Index R.
            Resolution Res = Resolutions[R];
            // The Current Screen Resolution.
            Resolution CurRes = Screen.currentResolution;

            // Convert The Resolution To A String Variable.
            string Res2String = Res.width + "x" + Res.height + "@" + Res.refreshRate + "Hz";
            // Add The Converted String Variable To The Options List.
            ResSetting.Options.Add(Res2String);

            // If The Resolution At Index R Is The Current Screen Resolution,
            // Set The Current Screen Resolution Index To R.
            if(Res.width == CurRes.width && Res.height == CurRes.height && Res.refreshRate == CurRes.refreshRate)
            {
                CurResIndex = R;
            }
        }

        // If The Player Has Not Set A Resolution Of Their Own,
        // Set The Value Of The Resolution Setting To The Index Of The Current Screen Resolution On The Resolutions Array.
        if(!SettingsLoadedSuccessfuly) { ResSetting.Value = CurResIndex; }
        PrevResIndex = -PrevResIndex;
    }
    void Start()
    {
        ApplySettings();
        CheckImageSettingsChange(true);
    }

    // Update is called once per frame
    void Update()
    {
        ApplySettings();
    }

    void ApplySettings()
    {
        // Apply Video Settings
        QualitySettings.SetQualityLevel(FindDropdownSetting("Quality").Value);
        Screen.fullScreen = FindToggleSetting("Fullscreen").Value;
        Application.runInBackground = FindToggleSetting("Background Run").Value;
        Cam.fieldOfView = FindSliderSetting("FOV").Value;
        // Applying Resolution Is A Little Bit Harder
        int ResIndex = FindDropdownSetting("Resolution").Value;
        if(ResIndex != PrevResIndex)
        {
            PrevResIndex = ResIndex;
            ResIndex = Mathf.Clamp(ResIndex, 0, Resolutions.Length - 1);
            Resolution Res = Resolutions[ResIndex];
            Screen.SetResolution(Res.width, Res.height, Screen.fullScreen, Res.refreshRate);
        }

        // Apply Volume Settings
        AudioManager.Instance.SetChannelVolume("Master", FindSliderSetting("Master Volume").Value);
        AudioManager.Instance.SetChannelVolume("Music", FindSliderSetting("Music Volume").Value);
        AudioManager.Instance.SetChannelVolume("SFX", FindSliderSetting("SFX Volume").Value);

        // Apply Input Settings
        HideMouse = FindToggleSetting("Mouse Lock").Value;
        NextbotGameManager.Instance.Player.InputReciever.InvertMouse = FindToggleSetting("Invert Mouse").Value;
        NextbotGameManager.Instance.Player.Sens = FindSliderSetting("Sensitivity").Value;

        // Apply Image Settings.
        Paths = ImagePathList.Items;
        SubDir = FindToggleSetting("Subdirectories").Value;

        // Save Settings.
        settings.ImagePaths = new List<string>(Paths);
        settings.Save();
    }

    public bool CheckImageSettingsChange(bool ForceReload)
    {
        bool ChangedList = false;
        int CurPathLen = 0;
        if(NextbotSpawner.Instance.Paths != null) { CurPathLen = NextbotSpawner.Instance.Paths.Length; }
        int MaxLen = Mathf.Max(CurPathLen, Paths.Count);
        for (int P = 0; P < MaxLen; P++)
        {
            if(CurPathLen == 0)
            {
                ChangedList = true;
                break;
            }

            try
            {
                if(NextbotSpawner.Instance.Paths[P] != Paths[P])
                {
                    ChangedList = true;
                    break;
                }
            }
            catch (System.IndexOutOfRangeException)
            {
                ChangedList = true;
                break;
            }
            catch (System.ArgumentOutOfRangeException)
            {
                ChangedList = true;
                break;
            }
        }

        bool MustReload = (ChangedList || SubDir != NextbotSpawner.Instance.IncludeSubdirectories) || ForceReload;
        if(MustReload)
        {
            NextbotSpawner.Instance.SetPrevPaths();
            NextbotSpawner.Instance.Paths = Paths.ToArray();
            NextbotSpawner.Instance.IncludeSubdirectories = SubDir;
            NextbotSpawner.Instance.Refreash();
        }

        return MustReload;
    }

    void ResetSettings()
    {
        foreach (SliderSetting S in settings.SliderSettings)
        {
            S.Value = S.DefaultValue;
        }
        foreach (DropdownSetting S in settings.DropdownSettings)
        {
            S.Value = S.DefaultValue;
        }
        foreach (ToggleSetting S in settings.ToggleSettings)
        {
            S.Value = S.DefaultValue;
        }

        FindDropdownSetting("Resolution").Value = Resolutions.Length - 1;

        settings.ImagePaths = new List<string>();
        settings.ImagePaths.Add(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
        settings.ImagePaths.Add(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures));
        settings.ImagePaths.Add(Environment.GetFolderPath(Environment.SpecialFolder.Personal));
    }
    public void RestoreDefaults()
    {
        ResetSettings();
        settingsMenu.ResetValues();
        ImagePathList.SetItems(settings.ImagePaths);
        ApplySettings();
    }

    public void SetPaths(List<string> NewPaths)
    {
        settings.ImagePaths = NewPaths;
        ImagePathList.SetItems(settings.ImagePaths);
    }

    public SliderSetting FindSliderSetting(string Name)
    {
        return Array.Find(settings.SliderSettings, SliderSetting => SliderSetting.Name == Name);
    }
    public DropdownSetting FindDropdownSetting(string Name)
    {
        return Array.Find(settings.DropdownSettings, DropdownSetting => DropdownSetting.Name == Name);
    }
    public ToggleSetting FindToggleSetting(string Name)
    {
        return Array.Find(settings.ToggleSettings, ToggleSetting => ToggleSetting.Name == Name);
    }
}
