using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class BuffUI : MonoBehaviour
{
    public Image buffIcon;        // 버프 아이콘 (아이템 아이콘 활용)
    public Text durationText;     // 남은 시간 표시

    public void SetBuff(Sprite icon, float duration)
    {
        buffIcon.sprite = icon;
        buffIcon.fillAmount = 1f; // 처음에는 아이콘을 다 채워놓고 시작
        StartCoroutine(UpdateBuffTime(duration));
    }

    private IEnumerator UpdateBuffTime(float duration)
    {
        float remainingTime = duration;
        while (remainingTime > 0)
        {
            durationText.text = $"{remainingTime:F1}s";  // 소수점 한 자리까지 표시
            buffIcon.fillAmount = remainingTime / duration; // 남은 시간에 따라 아이콘 채우기
            yield return new WaitForSeconds(0.1f);
            remainingTime -= 0.1f;
        }

        // 버프가 끝나면 UI 제거
        Destroy(gameObject);
    }
}
