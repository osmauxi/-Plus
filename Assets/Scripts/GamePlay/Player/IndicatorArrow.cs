using UnityEngine;

public class IndicatorArrow : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    // 给对象池复用时的初始化
    public void Setup(Color color)
    {
        if (spriteRenderer != null)
        {
            Color c = color;
            c.a = 0f; // 初始透明
            spriteRenderer.color = c;
        }
    }

    public void SetAlpha(float alpha)
    {
        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = alpha;
            spriteRenderer.color = c;
        }
    }
}