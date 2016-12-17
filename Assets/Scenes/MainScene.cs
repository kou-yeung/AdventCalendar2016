//=========================
// 簡単な確認用シーンです
// ユーザがページ開いたら、プレイヤーオブジェクトが出現し
// ArrowKey (←↑↓→) で移動できます。
// これ以上のことができません！
//=========================
using UnityEngine;
using System.Collections.Generic;
using Milkcocoa;
using System;

// プレイヤー出現
class Bron
{
    public string guid;
}
// 座標更新
class Position
{
    public string guid;
    public Vector3 pos;
}
// 生存確認
class Alive
{
    public string guid;
    public Vector3 pos;
}

class TypedDataStore<T>
{
    DataStore dataStore;
    public TypedDataStore(DataStore dataStore, Action<T> OnSend)
    {
        this.dataStore = dataStore;
        this.dataStore.OnSend(json =>
        {
            if(OnSend != null) OnSend(JsonUtility.FromJson<T>(json));
        });
    }

    public void Send(T data)
    {
        dataStore.Send(JsonUtility.ToJson(data));
    }
}

public class MainScene : MonoBehaviour
{
    static readonly float MaxAliveTimeout = 5.0f;   // 五秒以上返答なかったら死亡とする
    public GameObject playerDummy;

    MilkCocoa milkcocoa = new MilkCocoa("Your API Key");

    string myGUID = Guid.NewGuid().ToString();
    GameObject me;

    // 機能ごとにストアを用意する
    TypedDataStore<Position> positionStore;
    TypedDataStore<Bron> bronStore;
    TypedDataStore<Alive> aliveStore;

    float lastAlive;

    // 全プレイヤーのタイムスタンプ
    Dictionary<string, float> playersTimestamp = new Dictionary<string, float>();

    void Start()
    {
        // だれか生まれたときのイベント
        bronStore = new TypedDataStore<Bron>(milkcocoa.DataStore("Bron"), OnBron);

        // 座標更新イベント
        positionStore = new TypedDataStore<Position>(milkcocoa.DataStore("Position"), OnPosition);

        // 生存確認
        aliveStore = new TypedDataStore<Alive>(milkcocoa.DataStore("Alive"), OnAlive);

        // 自分のプレイヤーを生成する
        bronStore.Send(new Bron { guid = myGUID });
    }
    void Update()
    {
        CheckAlive();
        UpdateMe();
        // 生存していることを通知する:時間制限
        if (Time.realtimeSinceStartup - lastAlive >= 3)
        {
            aliveStore.Send(new Alive { guid = myGUID, pos = me.transform.position });
            lastAlive = Time.realtimeSinceStartup;
        }
    }

    // GUIDを指定してユーザを生成する
    GameObject CreatePlayer(string guid)
    {
        return CreatePlayer(guid, Vector3.zero);
    }
    GameObject CreatePlayer(string guid, Vector3 pos)
    {
        var go = GameObject.Instantiate(playerDummy);
        go.name = guid;
        go.transform.position = pos;
        playersTimestamp.Add(guid, Time.realtimeSinceStartup);
        return go;
    }

    void CheckAlive()
    {
        var deadList = new List<string>();
        foreach (var timestamp in playersTimestamp)
        {
            if ((Time.realtimeSinceStartup - timestamp.Value) >= MaxAliveTimeout)
            {
                deadList.Add(timestamp.Key);
            }
        }

        deadList.Remove(myGUID);    // 自分だけは削除しない！

        foreach (var dead in deadList)
        {
            var go = GameObject.Find(dead);
            if(go != null) GameObject.Destroy(go);
            playersTimestamp.Remove(dead);
        }
    }
    void UpdateMe()
    {
        if (me == null) return;
        var transform = me.transform;
        var curPos = transform.position;

        if (Input.GetKey("up")) curPos += new Vector3(0, 0.5f, 0);
        if (Input.GetKey("down")) curPos += new Vector3(0, -0.5f, 0);
        if (Input.GetKey("left")) curPos += new Vector3(-0.5f, 0, 0);
        if (Input.GetKey("right")) curPos += new Vector3(0.5f, 0, 0);

        if (transform.position != curPos)
        {
            // 自分のプレイヤーの座標が更新しました
            positionStore.Send(new Position { guid = myGUID, pos = curPos });
        }
    }

    void OnPosition(Position data)
    {
        var go = GameObject.Find(data.guid);
        go.transform.position = data.pos;
    }

    void OnBron(Bron data)
    {
        // 表示用オブジェクト生成
        var go = CreatePlayer(data.guid);

        if (data.guid == myGUID)
        {
            me = go;
            var renderer = me.GetComponent<Renderer>();
            renderer.material.color = Color.red;
        }
    }

    void OnAlive(Alive data)
    {
        // 未登録の場合、登録します。基本は自分より古いプレイヤーです
        if (!playersTimestamp.ContainsKey(data.guid))
        {
            var go = CreatePlayer(data.guid, data.pos);
        }
        playersTimestamp[data.guid] = Time.realtimeSinceStartup;
    }
}
