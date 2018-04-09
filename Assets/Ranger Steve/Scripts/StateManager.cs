using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateManager : MonoBehaviour
{
    public string nickname;

    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
    }

    public void HandleSetNickname(string newNickname)
    {
        nickname = newNickname;
    }
}
