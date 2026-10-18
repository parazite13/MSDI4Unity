using UnityEngine;
using System;

namespace MSDI4Unity.Register
{
    [Serializable]
    public class SceneServiceMapping
    {
        [SerializeField]
        internal string scenePath;
        public string ScenePath => scenePath;

        [SerializeField]
        internal ServiceRegisterScriptableObject serviceRegister;
        public ServiceRegisterScriptableObject ServiceRegister => serviceRegister;
    }
}