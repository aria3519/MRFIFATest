// @ Author : Jisu Jeon
// @ Update : 2023.03.03
// @ Brief : 안드로이드 플랫폼의 PlayerSetting에 Project Keystore를 자동으로 삽입하는 스크립트

#if UNITY_EDITOR
using UnityEditor;

namespace Appnori.Utils
{
    [InitializeOnLoad]
    public class AutoAndroidKeySetting
    {
        private static readonly string keyStore = "Appnori.keystore";
        private static readonly string keyAlias = "appnori";

        private static readonly string password = "nori7321";

        static AutoAndroidKeySetting()
        {
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            {
                PlayerSettings.Android.useCustomKeystore = true;

                PlayerSettings.Android.keystoreName = keyStore;
                PlayerSettings.Android.keystorePass = password;

                PlayerSettings.Android.keyaliasName = keyAlias;
                PlayerSettings.Android.keyaliasPass = password;
            }
        }
    }
}
#endif