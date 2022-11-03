using Experiments.Global.Managers;
using Experiments.Global.Audio;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;

public class NextbotsMenuManager : Manager<NextbotsMenuManager>
{
    [System.Serializable]
    public class Menu
    {
        [HideInInspector]
        public bool ActiveSelf;
        [HideInInspector]
        public bool ActiveElements;
        public string MenuName;
        public string Title;
        public Color TitleColor;
        public Color BackgorundColor;
        public GameObject ExtraContents;
        [System.Serializable]
        public class Option
        {
            [HideInInspector]
            public bool Active;
            public string OptionName;
            public string Text;
            public float YPosition;
            public bool AutoAlign;
            public bool Clickable;
            public Vector2 AnchorMin;
            public Vector2 AnchorMax;
            public Button.ButtonClickedEvent OnClick;

            TMP_Text OptionText;
            Button OptionButton;
            GameObject OptionObject;
            RectTransform OptionRect;

            public void Construct(GameObject buttonInstance, RectTransform menu, int Index)
            {
                OptionObject = Instantiate(buttonInstance, Vector3.zero, Quaternion.identity, menu);
                OptionRect = OptionObject.GetComponent<RectTransform>();
                OptionRect.anchorMin = AnchorMin;
                OptionRect.anchorMax = AnchorMax;
                OptionRect.anchoredPosition = Vector2.up * YPosition;
                OptionObject.name = OptionName + "Option_" + Index;
                OptionObject.SetActive(true);
                OptionText = OptionObject.GetComponent<TextMeshProUGUI>();
                OptionButton = OptionObject.GetComponent<Button>();

                Active = true;
                OptionText.text = Text;
                OptionButton.interactable = Clickable;
                OptionButton.onClick = OnClick;
            }

            public void UpdateOption(bool MenuActive)
            {
                OptionObject.SetActive(Active && MenuActive);
                OptionText.text = Text;
            }
        }
        public Option[] Options;
        public bool BackButtonEnabled;
        public bool BelowMain;
        const float ButtonHeight = 70f;

        TMP_Text TitleText;
        Image BackgroundImage;
        GameObject BackButton;
        GameObject MenuObject;
        [HideInInspector]
        public GameObject ContentsObject;

        public void Construct(GameObject menuInstance, GameObject buttonInstance, RectTransform canvas, float ButtonCenterY)
        {
            Vector3 ScreenCenter = new Vector3(canvas.rect.width / 2f, canvas.rect.height / 2f);

            MenuObject = Instantiate(menuInstance, ScreenCenter, Quaternion.identity, canvas);
            MenuObject.transform.SetSiblingIndex((!BelowMain) ? canvas.childCount - 3 : 2);
            MenuObject.name = MenuName + "Menu";
            MenuObject.SetActive(true);
            TitleText = MenuObject.transform.Find("MenuTitle").GetComponent<TextMeshProUGUI>();
            BackgroundImage = MenuObject.transform.Find("MenuBackground").GetComponent<Image>();
            if(ExtraContents != null)
            {
                ContentsObject = Instantiate(ExtraContents, ScreenCenter, Quaternion.identity, MenuObject.transform);
                ContentsObject.name = "Additional Contents";
                ContentsObject.SetActive(true);
            }
            BackButton = MenuObject.transform.Find("BackButton").gameObject;

            ActiveElements = true;
            TitleText.text = Title;
            TitleText.color = TitleColor;
            BackgroundImage.color = BackgorundColor;
            BackButton.SetActive(BackButtonEnabled);

            float StartAutoButtonYPos = ButtonCenterY + ((ButtonHeight / 2f) * (Options.Length - 1));
            for (int B = 0; B < Options.Length; B++)
            {
                float AutoButtonYPos = StartAutoButtonYPos - (ButtonHeight * B);
                if(Options[B].AutoAlign) { Options[B].YPosition = AutoButtonYPos; }
                Options[B].Construct(buttonInstance, MenuObject.GetComponent<RectTransform>(), B + 1);
            }
        }

        public void UpdateMenu()
        {
            TitleText.text = Title;
            TitleText.color = TitleColor;
            BackgroundImage.color = BackgorundColor;

            MenuObject.SetActive(ActiveSelf);
            TitleText.gameObject.SetActive(ActiveElements);
            BackButton.SetActive(ActiveElements && BackButtonEnabled);

            for (int B = 0; B < Options.Length; B++)
            {
                Options[B].UpdateOption(ActiveElements);
            }
        }

        public Option GetOption(string OptionName)
        {
            return Array.Find(Options, Option => Option.OptionName == OptionName);
        }
    }
    [Space]
    public Menu[] Menus;
    public GameObject MenuInstance;
    public GameObject ButtonInstance;
    public RectTransform MenuCanvas;
    public float StartMenuButtonY;

    [Header("Dialog")]
    public GameObject DialogObject;
    public TMP_Text DialogText;
    public GameObject DialogOKButton;
    public GameObject DialogYesNoButtons;
    UnityEvent DialogAcceptEvent;
    public static bool DialogOpen = false;
    public static UnityEvent DialogClose;

    // Start is called before the first frame update
    void Awake()
    {
        Init(this);

        DialogClose = new UnityEvent();
        DialogClose.AddListener(CloseDialog);

        for (int M = 0; M < Menus.Length; M++)
        {
            Menus[M].Construct(MenuInstance, ButtonInstance, MenuCanvas, StartMenuButtonY);
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int M = 0; M < Menus.Length; M++)
        {
            Menus[M].UpdateMenu();
        }
    }

    public Menu GetMenu(string MenuName)
    {
        return Array.Find(Menus, Menu => Menu.MenuName == MenuName);
    }
    public void SetMenu(string MenuName)
    {
        for (int M = 0; M < Menus.Length; M++)
        {
            Menus[M].ActiveSelf = (Menus[M].MenuName == MenuName);
        }
    }

    public void ShowDialog(string Dialog, bool OneWay, UnityEvent DialogEvent)
    {
        DialogOpen = true;
        DialogObject.SetActive(true);
        DialogText.text = Dialog;
        DialogOKButton.SetActive(OneWay);
        DialogYesNoButtons.SetActive(!OneWay);
        DialogAcceptEvent = DialogEvent;

        PlaySelectSound();
    }
    public void CloseDialog()
    {
        DialogOpen = false;
        DialogObject.SetActive(false);

        PlaySelectSound();
    }
    public void AcceptDialog()
    {
        DialogAcceptEvent.Invoke();
        PlaySelectSound();
    }

    public void Back()
    {
        if(!NextbotSettingsManager.Instance.CheckImageSettingsChange(false))
        {
            SetMenu("Main");
            PlaySelectSound();
        }
    }

    public void TryReset(bool Settings)
    {
        string Message = (Settings) ? "Restore Default Settings?" : "Reset Your HighScores And Discovered Images?";
        bool settings = Settings;
        UnityEvent DialogEvent = new UnityEvent();
        DialogEvent.AddListener(new UnityAction(delegate
        {
            if(settings) { NextbotSettingsManager.Instance.RestoreDefaults(); }
            else { NextbotGameManager.Instance.ResetProgress(); }

            AudioManager.Instance.InteractWithSFX("Reset", SoundEffectBehaviour.Play);
            CloseDialog();
        }));

        ShowDialog(Message, false, DialogEvent);
    }

    public void TryQuit()
    {
        UnityEvent QuitEvent = new UnityEvent();
        QuitEvent.AddListener(new UnityAction(QuitGame));
        ShowDialog("Are You Sure You Want To Exit The Game?", false, QuitEvent);
    }
    public void QuitGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
        #endif
    }
}
