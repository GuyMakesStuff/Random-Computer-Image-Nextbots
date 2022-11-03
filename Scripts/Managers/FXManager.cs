using System;
using TMPro;
using UnityEngine;

namespace Experiments.Global.Managers
{
    public class FXManager : Manager<FXManager>
    {
        [Space]
        public Transform EffectsContainer;
        [System.Serializable]
        public class Effect
        {
            public string Name;
            public GameObject Prefab;
        }
        public Effect[] Effects;
        public GameObject TextPrefab;

        void Start()
        {
            Init(this);

            if(EffectsContainer == null) { EffectsContainer = new GameObject("FX").transform; }
        }

        public void SpawnFX(string EffectName, Vector3 Position)
        {
            Effect effect = Array.Find(Effects, Effect => Effect.Name == EffectName);
            if(effect == null)
            {
                Debug.LogError("Effect " + EffectName + " Not Found!");
                return;
            }

            GameObject NewEffect = Instantiate(effect.Prefab, Position, effect.Prefab.transform.rotation, EffectsContainer);
            Destroy(NewEffect, NewEffect.GetComponent<ParticleSystem>().main.duration);
        }
        public void SpawnText(Vector3 Position, string Text, Color color, float Size, bool PhysicsBased, float UpDir, float LastTime)
        {
            TMP_Text Txt = Instantiate(TextPrefab, Position, Quaternion.identity, EffectsContainer).GetComponent<TextMeshPro>();
            Txt.text = Text;
            Txt.color = color;
            Txt.fontSize = Size;
            TextPopUp PopUp = Txt.GetComponent<TextPopUp>();
            PopUp.LastTime = LastTime;
            PopUp.UsePhysics = PhysicsBased;
            PopUp.UpDirection = UpDir;
        }
    }
}