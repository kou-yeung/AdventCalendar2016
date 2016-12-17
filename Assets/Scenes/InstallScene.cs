//====================
// このシーンは Milkcocoa API をインストールします
// HTML に直接 install script を書いた場合、このシーンを飛ばしてもいい
//====================
using UnityEngine;
using UnityEngine.SceneManagement;
using Milkcocoa;
using System;
public class InstallScene : MonoBehaviour {

    MilkCocoa milkcocoa;
    DataStore dataStore;

    [Serializable]
    struct Data
    {
        public string content;
    }

    // Update is called once per frame
    void Update () {
        Debug.Log(MilkcocoaInstaller.Installed());
        if (MilkcocoaInstaller.Installed())
        {
            Debug.Log(JsonUtility.ToJson(Vector3.up));
            SceneManager.LoadScene(1);
        }
    }
}
