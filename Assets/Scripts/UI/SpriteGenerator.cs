using UnityEngine;

/// <summary>
/// Tự tạo tất cả sprite bằng code (Texture2D → Sprite).
/// Không cần import ảnh. Pixel art style.
/// </summary>
public static class SpriteGenerator
{
    // =====================================================
    //              PUBLIC — tạo từng sprite
    // =====================================================

    /// <summary>
    /// Khung viền board — 9-slice, viền sáng ngoài, tối trong
    /// </summary>
    public static Sprite BoardFrame()
    {
        int size = 64;
        var tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Point;

        Color outer = new Color(0.45f, 0.32f, 0.18f);
        Color inner = new Color(0.25f, 0.18f, 0.1f);
        Color highlight = new Color(0.6f, 0.45f, 0.25f);
        Color bg = new Color(0, 0, 0, 0);

        int border = 6;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                if (x < 2 || x >= size - 2 || y < 2 || y >= size - 2)
                    tex.SetPixel(x, y, highlight);
                else if (x < border || x >= size - border || y < border || y >= size - border)
                    tex.SetPixel(x, y, outer);
                else if (x < border + 2 || x >= size - border - 2 || y < border + 2 || y >= size - border - 2)
                    tex.SetPixel(x, y, inner);
                else
                    tex.SetPixel(x, y, bg);
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100,
            0, SpriteMeshType.FullRect, new Vector4(border + 2, border + 2, border + 2, border + 2));
    }

    /// <summary>
    /// Nền panel dưới — tối, viền nhẹ, 9-slice
    /// </summary>
    public static Sprite PanelBG()
    {
        int size = 48;
        var tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Point;

        Color edge = new Color(0.2f, 0.15f, 0.25f);
        Color fill = new Color(0.1f, 0.08f, 0.12f);
        Color line = new Color(0.3f, 0.22f, 0.35f);

        int border = 4;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                if (x < 1 || x >= size - 1 || y < 1 || y >= size - 1)
                    tex.SetPixel(x, y, line);
                else if (x < border || x >= size - border || y < border || y >= size - border)
                    tex.SetPixel(x, y, edge);
                else
                    tex.SetPixel(x, y, fill);
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100,
            0, SpriteMeshType.FullRect, new Vector4(border, border, border, border));
    }

    /// <summary>
    /// Viền avatar — vàng pixel art, 9-slice
    /// </summary>
    public static Sprite AvatarBorder()
    {
        int size = 48;
        var tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Point;

        Color gold = new Color(0.85f, 0.7f, 0.2f);
        Color darkGold = new Color(0.55f, 0.42f, 0.12f);
        Color bg = new Color(0.15f, 0.12f, 0.18f);

        int border = 4;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                if (x < 1 || x >= size - 1 || y < 1 || y >= size - 1)
                    tex.SetPixel(x, y, gold);
                else if (x < border || x >= size - border || y < border || y >= size - border)
                    tex.SetPixel(x, y, darkGold);
                else
                    tex.SetPixel(x, y, bg);
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100,
            0, SpriteMeshType.FullRect, new Vector4(border, border, border, border));
    }

    /// <summary>
    /// Bar BG — nền tối cho HP/Mana bar, 9-slice
    /// </summary>
    public static Sprite BarBG()
    {
        int w = 64, h = 16;
        var tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Point;

        Color edge = new Color(0.2f, 0.18f, 0.22f);
        Color fill = new Color(0.08f, 0.07f, 0.09f);

        int border = 3;

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                if (x < 1 || x >= w - 1 || y < 1 || y >= h - 1)
                    tex.SetPixel(x, y, edge);
                else if (x < border || x >= w - border || y < border || y >= h - border)
                    tex.SetPixel(x, y, new Color(0.12f, 0.1f, 0.13f));
                else
                    tex.SetPixel(x, y, fill);
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100,
            0, SpriteMeshType.FullRect, new Vector4(border, border, border, border));
    }

    /// <summary>
    /// Bar Fill — thanh trắng đơn giản, Image.Filled sẽ tô màu
    /// </summary>
    public static Sprite BarFill()
    {
        int w = 64, h = 16;
        var tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Point;

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                // gradient nhẹ trên dưới
                float t = (float)y / h;
                float brightness = Mathf.Lerp(0.85f, 1f, t);
                tex.SetPixel(x, y, new Color(brightness, brightness, brightness));
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100);
    }

    /// <summary>
    /// Glow — soft radial gradient trắng → trong suốt
    /// </summary>
    public static Sprite Glow()
    {
        int size = 64;
        var tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Bilinear;

        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                float t = Mathf.Clamp01(dist / radius);
                float alpha = Mathf.Lerp(0.8f, 0f, t * t);
                tex.SetPixel(x, y, new Color(1, 0.95f, 0.7f, alpha));
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100);
    }

    /// <summary>
    /// Top decoration — boss mắt đỏ placeholder (pixel art đơn giản)
    /// </summary>
    public static Sprite TopDecoration()
    {
        int w = 128, h = 48;
        var tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Point;

        Color bg = new Color(0.05f, 0.03f, 0.08f);
        Color stone = new Color(0.25f, 0.2f, 0.3f);
        Color eye = new Color(0.9f, 0.15f, 0.1f);
        Color eyeGlow = new Color(1f, 0.3f, 0.15f);

        // fill BG
        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                tex.SetPixel(x, y, bg);

        // stone arch
        for (int x = 20; x < w - 20; x++)
        {
            for (int y = 0; y < 10; y++)
                tex.SetPixel(x, y, stone);
            for (int y = h - 10; y < h; y++)
                tex.SetPixel(x, y, stone);
        }

        // 2 eyes
        DrawCircle(tex, w / 2 - 16, h / 2, 5, eye);
        DrawCircle(tex, w / 2 + 16, h / 2, 5, eye);

        // eye glow (1px center)
        DrawCircle(tex, w / 2 - 16, h / 2, 2, eyeGlow);
        DrawCircle(tex, w / 2 + 16, h / 2, 2, eyeGlow);

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100);
    }

    /// <summary>
    /// Avatar placeholder — mặt đơn giản
    /// </summary>
    public static Sprite AvatarPlaceholder(Color skinColor, Color eyeColor)
    {
        int size = 48;
        var tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Point;

        Color bg = new Color(0.15f, 0.12f, 0.18f);

        // fill BG
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                tex.SetPixel(x, y, bg);

        // head circle
        DrawCircle(tex, size / 2, size / 2 + 2, 16, skinColor);

        // eyes
        DrawCircle(tex, size / 2 - 6, size / 2 + 4, 3, Color.white);
        DrawCircle(tex, size / 2 + 6, size / 2 + 4, 3, Color.white);
        DrawCircle(tex, size / 2 - 6, size / 2 + 4, 1, eyeColor);
        DrawCircle(tex, size / 2 + 6, size / 2 + 4, 1, eyeColor);

        // mouth
        for (int x = size / 2 - 4; x <= size / 2 + 4; x++)
            tex.SetPixel(x, size / 2 - 5, new Color(0.3f, 0.15f, 0.1f));

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100);
    }

    /// <summary>
    /// Player avatar — da sáng, mắt xanh
    /// </summary>
    public static Sprite PlayerAvatar()
    {
        return AvatarPlaceholder(
            new Color(0.85f, 0.72f, 0.6f),     // da
            new Color(0.2f, 0.5f, 0.9f)         // mắt xanh
        );
    }

    /// <summary>
    /// Enemy avatar — da tím, mắt đỏ
    /// </summary>
    public static Sprite EnemyAvatar()
    {
        return AvatarPlaceholder(
            new Color(0.5f, 0.35f, 0.55f),      // da tím
            new Color(0.9f, 0.15f, 0.1f)        // mắt đỏ
        );
    }

    /// <summary>
    /// Button — 9-slice, rounded rectangle
    /// </summary>
    public static Sprite ButtonSprite()
    {
        int w = 48, h = 24;
        var tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Point;

        Color face = new Color(0.55f, 0.45f, 0.28f);
        Color highlight = new Color(0.7f, 0.58f, 0.35f);
        Color shadow = new Color(0.35f, 0.28f, 0.16f);
        Color edge = new Color(0.25f, 0.2f, 0.12f);

        int border = 3;

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                if (x < 1 || x >= w - 1 || y < 1 || y >= h - 1)
                    tex.SetPixel(x, y, edge);
                else if (y >= h - border)
                    tex.SetPixel(x, y, highlight);
                else if (y < border)
                    tex.SetPixel(x, y, shadow);
                else if (x < border || x >= w - border)
                    tex.SetPixel(x, y, face);
                else
                    tex.SetPixel(x, y, face);
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100,
            0, SpriteMeshType.FullRect, new Vector4(border, border, border, border));
    }

    /// <summary>
    /// Skill button BG — tối, viền tím
    /// </summary>
    public static Sprite SkillButton()
    {
        int size = 48;
        var tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Point;

        Color fill = new Color(0.18f, 0.14f, 0.25f);
        Color edge = new Color(0.4f, 0.3f, 0.55f);
        Color corner = new Color(0.5f, 0.38f, 0.65f);

        int border = 3;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                if (x < 1 || x >= size - 1 || y < 1 || y >= size - 1)
                    tex.SetPixel(x, y, corner);
                else if (x < border || x >= size - border || y < border || y >= size - border)
                    tex.SetPixel(x, y, edge);
                else
                    tex.SetPixel(x, y, fill);
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100,
            0, SpriteMeshType.FullRect, new Vector4(border, border, border, border));
    }

    // =====================================================
    //                  INTERNAL
    // =====================================================

    static void DrawCircle(Texture2D tex, int cx, int cy, int radius, Color color)
    {
        int r2 = radius * radius;
        for (int x = cx - radius; x <= cx + radius; x++)
        {
            for (int y = cy - radius; y <= cy + radius; y++)
            {
                if (x < 0 || x >= tex.width || y < 0 || y >= tex.height) continue;
                int dx = x - cx, dy = y - cy;
                if (dx * dx + dy * dy <= r2)
                    tex.SetPixel(x, y, color);
            }
        }
    }
}
