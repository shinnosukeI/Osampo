using UnityEngine;
using System.Collections;

/// <summary>
/// HeartRateManagerとStageHeartRateManagerの共通基底クラス
/// </summary>
public abstract class BaseHeartRateManager : MonoBehaviour
{
    [Header("Base System")]
    [SerializeField] protected DataRecorder dataRecorder;

    // 内部変数
    protected int currentBPM = 60;
    protected bool isSensorActive = false;

    protected virtual void Start()
    {
        if (dataRecorder == null) dataRecorder = GetComponent<DataRecorder>();
    }

    protected virtual void Update()
    {
        // 共通のUpdate処理があればここに記述
    }

    // ▼▼▼ EventReceiverから呼ばれる関数 (共通処理) ▼▼▼
    public virtual void OnIntEvent(int value)
    {
        // 30未満はノイズまたは未装着とみなすフィルタ
        if (value >= 30)
        {
            currentBPM = value;
            isSensorActive = true;
        }
        else
        {
            isSensorActive = false;
        }
    }
}
