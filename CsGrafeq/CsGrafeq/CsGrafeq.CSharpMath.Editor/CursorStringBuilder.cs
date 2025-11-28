namespace CsGrafeq.CSharpMath.Editor;

using System;
using System.Text;

public class CursorStringBuilder
{
    public bool ForceToBeEmpty = false;
    private StringBuilder _sb;
    private int _cursor;

    public CursorStringBuilder(string text = "")
    {
        _sb = new StringBuilder(text);
        _cursor = 0;
    }

    /// <summary>
    /// 获取当前文本内容
    /// </summary>
    public string Text => _sb.ToString();

    /// <summary>
    /// 当前游标位置
    /// </summary>
    public int Cursor
    {
        get => _cursor;
        set => _cursor = Math.Clamp(value, 0, _sb.Length);
    }

    public CursorStringBuilder MoveCursorTo(int cursor)
    {
        Cursor = cursor;
        return this;
    }

    public CursorStringBuilder MoveCursorToEnd()
    {
        Cursor = _sb.Length;
        return this;
    }

    public CursorStringBuilder MoveCursorToStart()
    {
        Cursor = 0;
        return this;
    }

    /// <summary>
    /// 在游标处插入文本
    /// </summary>
    public CursorStringBuilder Insert(string content)
    {
        _sb.Insert(_cursor, content);
        _cursor += content.Length;
        return this;
    }

    /// <summary>
    /// 移动游标
    /// </summary>
    public CursorStringBuilder MoveCursor(int offset)
    {
        Cursor = _cursor + offset;
        return this;
    }

    public override string ToString()
    {
        return ForceToBeEmpty?"":_sb.ToString();
    }
}