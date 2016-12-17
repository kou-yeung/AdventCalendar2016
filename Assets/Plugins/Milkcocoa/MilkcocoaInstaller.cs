//=======================
// Milkcocoa のスクリプトをインストールに使ってください。
// html に <script src="https://cdn.mlkcca.com/v0.6.0/milkcocoa.js"></script> を記載する場合、
// このMilkcocoaInstallerを使わなくてもいいです。
// 
// プラグインの実装は 動的<script>タグを生成してロードするだけ。。
// <script>のロードは非同期なのでロード完了まで MilkCocoa のインスタンスが生成できないです
// ロード完了かどうかは MilkcocoaInstaller.Installed() で確認できます。
//=======================
using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

namespace Milkcocoa
{
    public class MilkcocoaInstaller : MonoBehaviour
    {
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    static extern void MilkcocoaInstall();
    [DllImport("__Internal")]
    static extern bool MilkcocoaInstalled();
#else
        static void MilkcocoaInstall() { }
        static bool MilkcocoaInstalled() { return true; }
#endif
        [SerializeField]
        private bool dontDestroyOnLoad = true;

        void Awake()
        {
            MilkcocoaInstall();
            if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);
        }

        // Milkcocoa をインストールします。
        // インストール完了まで待つ
        IEnumerable Start()
        {
            while (!Installed()) yield return null;
        }

        static public bool Installed()
        {
            return MilkcocoaInstalled();
        }
    }
}
