using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BaseText : BaseGadget
{
    [SerializeField]
    string m_TextID = "";

    [SerializeField]
    bool m_AdjustSizeToText = false;

    [HideInInspector]
    public TextMeshProUGUI m_Text;

    protected new void Awake()
    {
        base.Awake();

        SetTextFromID(m_TextID);
    }

    public void CheckText()
    {
        if (m_Text == null)
        {
            m_Text = GetComponent<TextMeshProUGUI>();
        }
    }

    public void SetTextFromID(string NewText, bool CheckValid = true)
    {
        SetText(NewText);
    }

    public void SetText(string NewText)
    {
        CheckText();
        m_Text.text = NewText;

        if (m_AdjustSizeToText)
        {
            SetSize(GetPreferredWidth(), GetPreferredHeight());
        }
    }

    public string GetText()
    {
        CheckText();
        return m_Text.text;
    }

    public float GetPreferredWidth()
    {
        CheckText();
        return m_Text.preferredWidth;
    }

    public float GetPreferredHeight()
    {
        CheckText();
        return m_Text.preferredHeight;
    }

    public void SetColour(Color NewColour)
    {
        CheckText();
        m_Text.color = NewColour;
    }

    public Color GetColour()
    {
        CheckText();
        return m_Text.color;
    }

    public void EnableFontStyle(FontStyles NewStyle, bool StyleEnabled)
    {
        if (StyleEnabled)
            m_Text.fontStyle = m_Text.fontStyle | NewStyle;
        else
            m_Text.fontStyle = m_Text.fontStyle & ~NewStyle;
    }

    public void SetStrikeThrough(bool StrikeThrough)
    {
        EnableFontStyle(FontStyles.Strikethrough, StrikeThrough);
    }
}
