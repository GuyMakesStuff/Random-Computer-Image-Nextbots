using System.Collections;
using TMPro;
using UnityEngine;
using Experiments.Global.Audio;

namespace Experiments.Global.Interface
{
    public class Counter : MonoBehaviour
    {
        public string ValueName;
        public int Value;
        public TMP_Text ValueText;
        public int HighValue;
        public TMP_Text HighValueText;
        public TMP_Text NewHIText;
        bool IsNewHI;
        int CurNewHIColorIndex;
        Color[] NewHICols = new Color[4]
        {
            Color.magenta,
            Color.yellow,
            Color.green,
            new Color(0, 1, 1, 1)
        };
        float ColorChangeTimer;
        const float ColChangeIntervals = 0.25f;

        void Start()
        {
            ColorChangeTimer = ColChangeIntervals;
        }

        void Update()
        {
            ValueText.text = ValueName + ":" + Value.ToString();
            HighValueText.text = "High " + ValueName + ":" + HighValue.ToString();

            ColorChangeTimer -= Time.deltaTime;
            if(ColorChangeTimer <= 0f)
            {
                ColorChangeTimer = ColChangeIntervals;
                CurNewHIColorIndex++;
                if(CurNewHIColorIndex >= NewHICols.Length) { CurNewHIColorIndex = 0; }
            }
            NewHIText.enabled = IsNewHI;
            NewHIText.text = "New High " + ValueName + "!";
            NewHIText.color = NewHICols[CurNewHIColorIndex];

            if (Value > HighValue)
            {
                HighValue = Value;
                if(IsNewHI == false)
                {
                    AudioManager.Instance.InteractWithSFX("New HI", SoundEffectBehaviour.Play);
                    IsNewHI = true;
                }
            }
        }

        public void ResetNewHI(int NewHighValue)
        {
            IsNewHI = false;
            HighValue = NewHighValue;
        }
    }
}