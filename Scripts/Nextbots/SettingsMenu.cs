using TMPro;
using System;
using UnityEngine.Events;
using System.Collections.Generic;
using Experiments.Global.Audio;
using UnityEngine.UI;
using UnityEngine;

public class SettingsMenu : MonoBehaviour
{
    GameObject ButtonInstance;
    public RectTransform SettingsContainer;
    public RectTransform CategoryButtonsContainer;
    public float CategoryButtonWidth;
    [System.Serializable]
    public class SettingsCategory
    {
        public string CategoryName;
        public GameObject ExtraContents;
        [System.Serializable]
        public class Setting
        {
            public string SettingLabel;
            public enum SettingTypes { Slider, Dropdown, Toggle }
            public SettingTypes SettingType;
            public string PropertyName;

            GameObject SettingObject;
            RectTransform rectTransform;
            TMP_Text SettingLabelText;

            Slider slider;
            NextbotSettingsManager.SliderSetting sliderSetting;
            TMP_Dropdown dropdown;
            NextbotSettingsManager.DropdownSetting dropdownSetting;
            Toggle toggle;
            NextbotSettingsManager.ToggleSetting toggleSetting;

            public void Construct(GameObject[] Widgets, SettingsCategory category)
            {
                SettingObject = Instantiate(Widgets[(int)SettingType], Vector3.zero, Quaternion.identity, category.rectTransform);
                SettingObject.SetActive(true);
                rectTransform = SettingObject.GetComponent<RectTransform>();
                rectTransform.anchorMin = Vector2.up;
                rectTransform.anchorMax = Vector2.one;

                SettingLabelText = rectTransform.Find("SettingLabel").GetComponent<TextMeshProUGUI>();
            }

            public void InitSetting()
            {
                switch (SettingType)
                {
                    case SettingTypes.Slider:
                    {
                        slider = SettingObject.transform.Find("ValueSlider").GetComponent<Slider>();
                        sliderSetting = NextbotSettingsManager.Instance.FindSliderSetting(PropertyName);

                        slider.minValue = sliderSetting.MinValue;
                        slider.maxValue = sliderSetting.MaxValue;
                        slider.value = sliderSetting.Value;
                        break;
                    }
                    case SettingTypes.Dropdown:
                    {
                        dropdown = SettingObject.transform.Find("ValueDropdown").GetComponent<TMP_Dropdown>();
                        dropdownSetting = NextbotSettingsManager.Instance.FindDropdownSetting(PropertyName);

                        dropdown.ClearOptions();
                        dropdown.AddOptions(dropdownSetting.Options);
                        dropdown.value = dropdownSetting.Value;
                        dropdown.RefreshShownValue();
                        dropdown.onValueChanged.AddListener(new UnityAction<int>(delegate { AudioManager.Instance.PlaySelectSound(); }));
                        break;
                    }
                    case SettingTypes.Toggle:
                    {
                        toggle = SettingObject.transform.Find("ValueToggle").GetComponent<Toggle>();
                        toggleSetting = NextbotSettingsManager.Instance.FindToggleSetting(PropertyName);

                        toggle.isOn = toggleSetting.Value;
                        toggle.onValueChanged.AddListener(new UnityAction<bool>(delegate { AudioManager.Instance.PlaySelectSound(); }));
                        break;
                    }
                }
            }
            public void UpdateSetting(bool Init)
            {
                SettingLabelText.text = SettingLabel;

                if(!Init)
                {
                    switch (SettingType)
                    {
                        case SettingTypes.Slider:
                        {
                            sliderSetting.Value = slider.value;
                            break;
                        }
                        case SettingTypes.Dropdown:
                        {
                            dropdownSetting.Value = dropdown.value;
                            break;
                        }
                        case SettingTypes.Toggle:
                        {
                            toggleSetting.Value = toggle.isOn;
                            break;
                        }
                    }
                }
            }

            public void SetValue(float SliderValue, int DropdownValue, bool ToggleValue)
            {
                switch (SettingType)
                {
                    case SettingTypes.Slider:
                    {
                        slider.value = SliderValue;
                        break;
                    }
                    case SettingTypes.Dropdown:
                    {
                        dropdown.value = DropdownValue;
                        break;
                    }
                    case SettingTypes.Toggle:
                    {
                        toggle.isOn = ToggleValue;
                        break;
                    }
                }

                UpdateSetting(false);
            }

            public void PlaceObject(int Index, float StartHeight, float HeightIntervals)
            {
                rectTransform.sizeDelta = Vector2.up * HeightIntervals;
                float YPos = StartHeight - (HeightIntervals * Index);
                rectTransform.anchoredPosition = Vector2.up * YPos;
            }
        }
        public Setting[] Settings;

        [HideInInspector]
        public GameObject CategoryObject;
        [HideInInspector]
        public RectTransform rectTransform;
        [HideInInspector]
        public GameObject ExtraContentsObject;
        RectTransform ExtraContentsRectTrans;
        RectTransform Container;
        float WidgetGaps;

        public void Construct(GameObject[] Instances, float WidgetHeight, RectTransform container)
        {
            Container = container;
            WidgetGaps = WidgetHeight;

            // Category Object.
            CategoryObject = new GameObject(CategoryName + "Settings");
            rectTransform = CategoryObject.AddComponent<RectTransform>();
            rectTransform.SetParent(Container);
            rectTransform.anchorMin = Vector2.up;
            rectTransform.anchorMax = Vector2.one;

            // Additional Contents.
            if(ExtraContents != null)
            {
                ExtraContentsObject = Instantiate(ExtraContents, Vector3.zero, Quaternion.identity, rectTransform);
                ExtraContentsObject.SetActive(true);
                ExtraContentsObject.name = "Additional Contents";
                ExtraContentsRectTrans = ExtraContentsObject.GetComponent<RectTransform>();
                ExtraContentsRectTrans.anchorMin = Vector2.up;
                ExtraContentsRectTrans.anchorMax = Vector2.one;
            }

            // Settings Widgets.
            for (int S = 0; S < Settings.Length; S++)
            {
                Settings[S].Construct(Instances, this);
            }

            UpdateCategory(true);
        }

        public void UpdateCategory(bool Init)
        {
            MoveSettings();

            foreach (Setting S in Settings)
            {
                S.UpdateSetting(Init);
            }
        }

        public void MoveSettings()
        {
            float ExtraContentsHeight = 0f;
            if(ExtraContentsRectTrans != null)
            {
                ExtraContentsHeight = ExtraContentsRectTrans.sizeDelta.y;
                ExtraContentsRectTrans.sizeDelta = Vector2.up * ExtraContentsHeight;
                ExtraContentsRectTrans.anchoredPosition = Vector2.down * (ExtraContentsHeight / 2f);
            }

            float StartYPos = -ExtraContentsHeight - (WidgetGaps / 2f);
            for (int S = 0; S < Settings.Length; S++)
            {
                Settings[S].PlaceObject(S, StartYPos, WidgetGaps);
            }

            float TotalCategorySize = ExtraContentsHeight + (WidgetGaps * Settings.Length);
            rectTransform.sizeDelta = Vector2.up * TotalCategorySize;
            rectTransform.anchoredPosition = Vector2.down * (TotalCategorySize / 2f); 
        }

        public void Init()
        {
            foreach (Setting S in Settings)
            {
                S.InitSetting();
            }
        }
    }
    public SettingsCategory[] SettingsCategories;
    public RectTransform CategoryContainer;
    public GameObject[] SettingsInstances;
    public float SettingsWidgetsHeight;
    List<Button> CategoryButtons;
    int CurCategoryIndex;


    // Start is called before the first frame update
    void Awake()
    {
        ButtonInstance = NextbotsMenuManager.Instance.ButtonInstance;
        int SettingsCategoryCount = SettingsCategories.Length;
        float StartButtonXPos = 0f - ((CategoryButtonWidth / 2f) * (SettingsCategoryCount - 1));
        Vector2 ButtonAnchor = new Vector2(0.5f, 1f);
        CategoryButtons = new List<Button>(SettingsCategoryCount);
        for (int SC = 0; SC < SettingsCategoryCount; SC++)
        {
            float ButtonXPos = StartButtonXPos + (CategoryButtonWidth * SC);
            int CategoryIndex = SC;
            GameObject NewSettingButton = Instantiate(ButtonInstance, Vector3.zero, Quaternion.identity, CategoryButtonsContainer);
            NewSettingButton.SetActive(true);
            NewSettingButton.name = SettingsCategories[SC].CategoryName + "Button";
            RectTransform rectTransform = NewSettingButton.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(ButtonXPos, 0f);
            rectTransform.sizeDelta = new Vector2(CategoryButtonWidth, rectTransform.sizeDelta.y);
            NewSettingButton.GetComponent<TextMeshProUGUI>().text = SettingsCategories[SC].CategoryName;
            Button button = NewSettingButton.GetComponent<Button>();
            button.interactable = true;
            ColorBlock colorBlock = button.colors;
            colorBlock.disabledColor = Color.green;
            button.colors = colorBlock;
            button.onClick.AddListener(new UnityAction( delegate { SetCategory(CategoryIndex); } ));
            CategoryButtons.Add(button);

            SettingsCategories[SC].Construct(SettingsInstances, SettingsWidgetsHeight, CategoryContainer);
        }

        CurCategoryIndex = -1;
        SetCategory(0);
    }
    void Start()
    {
        ResetValues();
    }

    // Update is called once per frame
    void Update()
    {
        SettingsContainer.sizeDelta = SettingsCategories[CurCategoryIndex].rectTransform.sizeDelta;
        for (int SC = 0; SC < SettingsCategories.Length; SC++)
        {
            if(SC == CurCategoryIndex)
            {
                SettingsCategories[SC].UpdateCategory(false);
            }
        }
    }

    public void SetCategory(int CategoryIndex)
    {
        if(CategoryIndex == CurCategoryIndex) { return; }

        for (int SC = 0; SC < SettingsCategories.Length; SC++)
        {
            SettingsCategories[SC].CategoryObject.SetActive(SC == CategoryIndex);
            CategoryButtons[SC].interactable = SC != CategoryIndex;
            if(SC == CategoryIndex)
            {
                CurCategoryIndex = SC;
                if(AudioManager.IsInstanced) { AudioManager.Instance.PlaySelectSound(); }
            }
        }
    }
    public SettingsCategory GetCategory(string CategoryName)
    {
        return Array.Find(SettingsCategories, SettingsCategory => SettingsCategory.CategoryName == CategoryName);
    }

    public SettingsCategory.Setting GetSetting(string Category, string SettingName)
    {
        return Array.Find(GetCategory(Category).Settings, Setting => Setting.PropertyName == SettingName);
    }

    public void ResetValues()
    {
        foreach (SettingsCategory SC in SettingsCategories)
        {
            SC.Init();
        }
    }
}
