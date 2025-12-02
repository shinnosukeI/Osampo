using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ClickableLabel : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Toggle targetToggle;
    private Transform targetTransform;
    private Vector3 originalScale;
    private bool isSetup = false;
    private Coroutine scaleCoroutine;

    public void Setup(Toggle toggle)
    {
        targetToggle = toggle;
        
        // 親オブジェクト（テキスト）を操作対象とする
        targetTransform = transform.parent;
        if (targetTransform != null)
        {
            originalScale = targetTransform.localScale;
            isSetup = true;
        }

        // Raycast Targetが見つからない・設定されていない場合のために強制的にONにする
        Graphic graphic = GetComponent<Graphic>();
        if (graphic != null)
        {
            graphic.raycastTarget = true;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (targetToggle != null && targetToggle.interactable)
        {
            targetToggle.isOn = true;
            // ToggleGroup will handle deselecting others
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isSetup && targetToggle != null && targetToggle.interactable)
        {
            if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
            scaleCoroutine = StartCoroutine(ScaleTo(originalScale * 1.02f));
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isSetup)
        {
            if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
            scaleCoroutine = StartCoroutine(ScaleTo(originalScale));
        }
    }

    private System.Collections.IEnumerator ScaleTo(Vector3 target)
    {
        float duration = 0.2f; // アニメーション時間
        float time = 0;
        Vector3 start = targetTransform.localScale;

        while (time < duration)
        {
            time += Time.deltaTime;
            targetTransform.localScale = Vector3.Lerp(start, target, time / duration);
            yield return null;
        }
        targetTransform.localScale = target;
    }

    private void OnDisable()
    {
        // 無効化されたときにサイズを戻す
        if (isSetup && targetTransform != null)
        {
            targetTransform.localScale = originalScale;
        }
    }
}
