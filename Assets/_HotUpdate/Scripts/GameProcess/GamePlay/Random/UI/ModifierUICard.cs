using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ModifierUICard : MonoBehaviour
{
    [Header("UI 绑定")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI tagsText;
    public Image iconImage;
    public Button cardButton;

    private string currentModifierId;
    private Action<string> onSelectedCallback;

    /// <summary>
    /// 初始化这张卡牌的数据
    /// </summary>
    public void SetupCard(ModifierDataSO data, Action<string> callback)
    {
        currentModifierId = data.modifierId;
        onSelectedCallback = callback;

        // 填充视觉表现
        nameText.text = data.modifierName;
        descriptionText.text = data.description;

        if (data.icon != null)
        {
            iconImage.sprite = data.icon;
            iconImage.enabled = true;
        }
        else
        {
            iconImage.enabled = false;
        }

        // 把 Tags 拼成一个好看的字符串，比如 "[Rapid] [Fire]"
        if (data.tags != null && data.tags.Count > 0)
        {
            tagsText.text = "[" + string.Join("] [", data.tags) + "]";
        }
        else
        {
            tagsText.text = "";
        }

        // 绑定点击事件 (先清空防重复绑定)
        cardButton.onClick.RemoveAllListeners();
        cardButton.onClick.AddListener(OnCardClicked);
    }

    private void OnCardClicked()
    {
        // 触发回调，把自己的 ID 传出去
        onSelectedCallback?.Invoke(currentModifierId);
    }
}