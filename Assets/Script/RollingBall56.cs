using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RollingBall : MonoBehaviour
{
    [Header("転がる強さ")]
    [SerializeField]
    private float rollForce = 5.0f; // 数字が大きいほど速く転がる

    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();

        // 生成された瞬間に「前方（青矢印の方向）」へ力を加える
        // ForceMode.Impulse は「蹴る」ような瞬間的な力を加えます
        rb.AddForce(transform.forward * rollForce, ForceMode.Impulse);

        // 少し回転（スピン）も加えて、より自然に転がり始めるようにする
        rb.AddTorque(Random.insideUnitSphere * rollForce, ForceMode.Impulse);
    }
}