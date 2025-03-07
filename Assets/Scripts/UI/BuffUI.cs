using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class BuffUI : MonoBehaviour
{
    public Image buffIcon;        // ���� ������ (������ ������ Ȱ��)
    public Text durationText;     // ���� �ð� ǥ��

    public void SetBuff(Sprite icon, float duration)
    {
        buffIcon.sprite = icon;
        buffIcon.fillAmount = 1f; // ó������ �������� �� ä������ ����
        StartCoroutine(UpdateBuffTime(duration));
    }

    private IEnumerator UpdateBuffTime(float duration)
    {
        float remainingTime = duration;
        while (remainingTime > 0)
        {
            durationText.text = $"{remainingTime:F1}s";  // �Ҽ��� �� �ڸ����� ǥ��
            buffIcon.fillAmount = remainingTime / duration; // ���� �ð��� ���� ������ ä���
            yield return new WaitForSeconds(0.1f);
            remainingTime -= 0.1f;
        }

        // ������ ������ UI ����
        Destroy(gameObject);
    }
}
