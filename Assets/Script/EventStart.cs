using UnityEngine;

public class EventForceStarter : MonoBehaviour
{
    [Header("強制スタートさせたいスクリプト")]
    public MonoBehaviour targetScript;

    [Header("呼び出したいメソッド名")]
    public string methodName = "StartFalling";

    public void ForceStart()
    {
        if (targetScript == null)
        {
            Debug.LogWarning("⚠ 強制スタート対象が設定されていません");
            return;
        }

        var method = targetScript.GetType().GetMethod(methodName);
        if (method == null)
        {
            Debug.LogError($"❌ メソッド '{methodName}' が {targetScript.GetType().Name} に存在しません");
            return;
        }

        Debug.Log($"▶ 強制スタート実行：{targetScript.GetType().Name}.{methodName}()");
        method.Invoke(targetScript, null);
    }
}