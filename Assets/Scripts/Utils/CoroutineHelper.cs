using UnityEngine;
using System.Collections;

/// <summary>
/// 协程辅助器：让非 MonoBehaviour 类也能启动协程
/// </summary>
public class CoroutineHelper : MonoBehaviour
{
    private static CoroutineHelper _instance;

    public static CoroutineHelper Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject obj = new GameObject("[CoroutineHelper]");
                _instance = obj.AddComponent<CoroutineHelper>();
                DontDestroyOnLoad(obj);
            }
            return _instance;
        }
    }

    /// <summary>
    /// 启动一个协程
    /// </summary>
    public Coroutine StartRoutine(IEnumerator routine)
    {
        return StartCoroutine(routine);
    }

    /// <summary>
    /// 停止一个协程
    /// </summary>
    public void StopRoutine(Coroutine routine)
    {
        if (routine != null)
            StopCoroutine(routine);
    }

    /// <summary>
    /// 停止所有通过此 Helper 启动的协程
    /// </summary>
    public void StopAllRoutines()
    {
        StopAllCoroutines();
    }
}