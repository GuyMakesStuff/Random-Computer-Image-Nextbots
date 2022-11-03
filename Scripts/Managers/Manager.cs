using System.Collections;
using Experiments.Global.Audio;
using UnityEngine;

namespace Experiments.Global.Managers
{
    public class Manager<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance { get; private set; }
        public static bool IsInstanced
        {
            get
            {
                return Instance != null;
            }
        }
        public bool IsGlobal;

        protected void Init(T OBJ)
        {
            if(!IsInstanced)
            {
                Instance = OBJ;
                if(IsGlobal)
                {
                    transform.SetParent(null);
                    DontDestroyOnLoad(gameObject);
                }
            }
            else { Destroy(OBJ.gameObject); }
        }

        public void PlaySelectSound()
        {
            AudioManager.Instance.InteractWithSFX("Select", SoundEffectBehaviour.Play);
        }

        private void OnApplicationQuit() {
            Instance = null;
        }
    }
}