//============================
// UnityWebGL用 MilkCocoaプラグインの実装ファイルです
//
// 現在は以下の機能を提供しています
// ■ MilkCocoa のインスタンス生成する
// ■ MilkCocoa のインスタンスから DataStore を生成する
// ■ DataStoreを使用して Send(...) でデータを送信する
// ■ DataStoreを使用して OnSend()イベントを監視してデータを受信する
//============================

using AOT;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Milkcocoa
{
    public class MilkCocoa
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        static extern int MilkcocoaCreate(string host);
        [DllImport("__Internal")]
        static extern void MilkcocoaDestroy(int id);
        [DllImport("__Internal")]
        static extern int MilkcocoaDataStore(int id, string path);
#else
        static int MilkcocoaCreate(string host) { return -1; }
        static void MilkcocoaDestroy(int id) { }

        static HashSet<string> dataStorePath = new HashSet<string>();
        static int MilkcocoaDataStore(int id, string path)
        {
            dataStorePath.Add(path);
            return dataStorePath.Count;
        }
#endif
        int id;

        public MilkCocoa(string appId)
        {
            var host = String.Format("{0}.mlkcca.com", appId);
            Debug.Log("MilkCocoa : " + host);
            id = MilkcocoaCreate(host);
        }
        public DataStore DataStore(string path)
        {
            return new DataStore(MilkcocoaDataStore(id, path));
        }
        public void Logout()
        {
            MilkcocoaDestroy(id);
        }
    }

    public class DataStore : IDisposable
    {
        static Dictionary<int, Action<string>> SendEvents = new Dictionary<int, Action<string>>();

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        static extern void DataStoreSend(int id, string json);
        [DllImport("__Internal")]
        static extern void DataStoreAddSendEvent(int id, Action<int, string> OnSend);
#else
        static void DataStoreAddSendEvent(int id, Action<int, string> OnSend) { }
        static void DataStoreSend(int id, string json)
        {
            SendEventFromJS(id, json);  // 受信したこととします
        }
#endif
        int id;
        
        public DataStore(int id)
        {
            this.id = id;
            DataStoreAddSendEvent(id, SendEventFromJS);
        }

        // データストアをにデータを保存しないデータの送信を行うことが出来ます。
        public void Send(string json)
        {
            DataStoreSend(id, json);
        }
        // データストアのsendイベントを監視します
        // Action<string value>
        public void OnSend(Action<string> cb)
        {
            if (SendEvents.ContainsKey(id)) SendEvents[id] += cb;
            else SendEvents.Add(id, cb);
        }

        public void Dispose()
        {
            SendEvents.Remove(id);
        }

        [MonoPInvokeCallback(typeof(Action<int, string>))]
        static void SendEventFromJS(int instanceId, string json)
        {
            if (SendEvents.ContainsKey(instanceId))
            {
                SendEvents[instanceId](json);
            }
        }
    }
}
