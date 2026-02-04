using Avalonia.Controls;
using CsGrafeq.Keyboard;

namespace CsGrafeqApplication.Core.Utils;

public static class InputHelper
{
    public static bool TryFindTextBox(this TopLevel top, out TextBox textBox)
    {
        var f = top.FocusManager?.GetFocusedElement();

        if (f is TextBox tb)
        {
            textBox = tb;
            return true;
        }

        textBox = null;
        return false;
    }

    public static bool Input(this TopLevel top, KeyboardInput input)
    {
        var f = top.FocusManager?.GetFocusedElement();
        /*if (f is RichExpressionBox mb)
        {
            mb.PressKey(input);
            return true;
        }*/

        if (f is TextBox tb)
            switch (input)
            {
                case KeyboardInput.Backspace:
                    tb.Backspace();
                    break;
                case KeyboardInput.Delete:
                    tb.Delete();
                    break;
                case KeyboardInput.Left:
                    tb.CursorLeft();
                    break;
                case KeyboardInput.Right:
                    tb.CursorRight();
                    break;
                case KeyboardInput.Absolute:
                    tb.InsertTextAtCursor("abs()");
                    tb.CursorLeft();
                    break;
                default:
                    tb.InsertTextAtCursor(((char)(int)input).ToString());
                    break;
            }

        return false;
    }

    public static bool Input(this TopLevel top, string str)
    {
        var f = top.FocusManager?.GetFocusedElement();
        /*if (f is RichExpressionBox mb)
        {
            mb.PressKey(input);
            return true;
        }*/

        if (f is TextBox tb)
            tb.InsertTextAtCursor(str);

        return false;
    }

    /// <summary>
    ///     在 TextBox 的当前光标位置插入文本
    /// </summary>
    /// <param name="textBox">目标 TextBox 控件</param>
    /// <param name="textToInsert">要插入的文本</param>
    /// <param name="moveCursorToEnd">是否将光标移动到插入文本的末尾，默认为 true</param>
    public static void InsertTextAtCursor(this TextBox textBox, string textToInsert, bool moveCursorToEnd = true)
    {
        if (textBox == null)
            throw new ArgumentNullException(nameof(textBox));

        if (string.IsNullOrEmpty(textToInsert))
            return;

        // 获取当前光标位置
        var cursorPosition = textBox.CaretIndex;

        // 获取当前文本内容
        var currentText = textBox.Text ?? string.Empty;

        // 处理选中文本的情况（替换选中内容）
        var selectionStart = textBox.SelectionStart;
        var selectionEnd = textBox.SelectionEnd;

        if (selectionStart != selectionEnd)
        {
            // 有选中文本，删除选中部分并插入新文本
            currentText = currentText.Remove(selectionStart, selectionEnd - selectionStart);
            currentText = currentText.Insert(selectionStart, textToInsert);
            textBox.Text = currentText;

            // 设置新的光标位置
            if (moveCursorToEnd)
                textBox.CaretIndex = selectionStart + textToInsert.Length;
            else
                textBox.CaretIndex = selectionStart;

            // 清除选择
            textBox.SelectionStart = textBox.CaretIndex;
            textBox.SelectionEnd = textBox.CaretIndex;
        }
        else
        {
            // 没有选中文本，在光标位置插入
            currentText = currentText.Insert(cursorPosition, textToInsert);
            textBox.Text = currentText;

            // 设置新的光标位置
            if (moveCursorToEnd)
                textBox.CaretIndex = cursorPosition + textToInsert.Length;
            else
                textBox.CaretIndex = cursorPosition;
        }

        // 确保 TextBox 获得焦点
        textBox.Focus();
    }

    /// <summary>
    ///     在 TextBox 中执行退格（Backspace）操作：
    ///     - 如果有选中文本则删除选中内容；
    ///     - 否则删除光标前的一个字符（若存在）。
    ///     操作后更新光标位置并清除选择，确保控件获得焦点。
    /// </summary>
    /// <param name="textBox">目标 TextBox</param>
    public static void Backspace(this TextBox textBox)
    {
        if (textBox == null)
            throw new ArgumentNullException(nameof(textBox));

        var currentText = textBox.Text ?? string.Empty;
        var selectionStart = textBox.SelectionStart;
        var selectionEnd = textBox.SelectionEnd;

        if (selectionStart != selectionEnd)
        {
            // 删除选中内容
            currentText = currentText.Remove(selectionStart, selectionEnd - selectionStart);
            textBox.Text = currentText;
            textBox.CaretIndex = selectionStart;
        }
        else
        {
            var caret = textBox.CaretIndex;
            if (caret > 0 && currentText.Length > 0)
            {
                // 删除光标前的一个字符
                currentText = currentText.Remove(caret - 1, 1);
                textBox.Text = currentText;
                textBox.CaretIndex = caret - 1;
            }
            else
            {
                // 光标已在最前，无操作
                textBox.CaretIndex = caret;
            }
        }

        // 清除选择并聚焦
        textBox.SelectionStart = textBox.CaretIndex;
        textBox.SelectionEnd = textBox.CaretIndex;
        textBox.Focus();
    }

    /// <summary>
    ///     在 TextBox 中执行删除（Delete）操作：
    ///     - 如果有选中文本则删除选中内容；
    ///     - 否则删除光标后的一个字符（若存在）。
    ///     操作后更新光标位置并清除选择，确保控件获得焦点。
    /// </summary>
    /// <param name="textBox">目标 TextBox</param>
    public static void Delete(this TextBox textBox)
    {
        if (textBox == null)
            throw new ArgumentNullException(nameof(textBox));

        var currentText = textBox.Text ?? string.Empty;
        var selectionStart = textBox.SelectionStart;
        var selectionEnd = textBox.SelectionEnd;

        if (selectionStart != selectionEnd)
        {
            // 删除选中内容
            currentText = currentText.Remove(selectionStart, selectionEnd - selectionStart);
            textBox.Text = currentText;
            textBox.CaretIndex = selectionStart;
        }
        else
        {
            var caret = textBox.CaretIndex;
            if (caret < currentText.Length)
            {
                // 删除光标后的一个字符
                currentText = currentText.Remove(caret, 1);
                textBox.Text = currentText;
                textBox.CaretIndex = caret;
            }
            else
            {
                // 光标在末尾，无操作
                textBox.CaretIndex = caret;
            }
        }

        // 清除选择并聚焦
        textBox.SelectionStart = textBox.CaretIndex;
        textBox.SelectionEnd = textBox.CaretIndex;
        textBox.Focus();
    }

    public static void CursorLeft(this TextBox textBox)
    {
        if (textBox == null) throw new ArgumentNullException(nameof(textBox));

        var currentText = textBox.Text ?? string.Empty;
        var selectionStart = textBox.SelectionStart;
        var selectionEnd = textBox.SelectionEnd;

        if (selectionStart != selectionEnd)
        {
            // 有选中文本，移动到选区起点
            textBox.CaretIndex = selectionStart;
        }
        else
        {
            var caret = textBox.CaretIndex;
            if (caret > 0)
                textBox.CaretIndex = caret - 1;
            else
                textBox.CaretIndex = caret;
        }

        // 清除选择并聚焦
        textBox.SelectionStart = textBox.CaretIndex;
        textBox.SelectionEnd = textBox.CaretIndex;
        textBox.Focus();
    }

    /// <summary>
    ///     将光标向右移动：
    ///     - 若有选中文本，则将光标移动到选区终点并清除选区；
    ///     - 否则将光标右移一个字符（若存在）。
    ///     操作后更新选区并聚焦控件。
    /// </summary>
    public static void CursorRight(this TextBox textBox)
    {
        if (textBox == null) throw new ArgumentNullException(nameof(textBox));

        var currentText = textBox.Text ?? string.Empty;
        var selectionStart = textBox.SelectionStart;
        var selectionEnd = textBox.SelectionEnd;

        if (selectionStart != selectionEnd)
        {
            // 有选中文本，移动到选区终点
            textBox.CaretIndex = selectionEnd;
        }
        else
        {
            var caret = textBox.CaretIndex;
            if (caret < currentText.Length)
                textBox.CaretIndex = caret + 1;
            else
                textBox.CaretIndex = caret;
        }

        // 清除选择并聚焦
        textBox.SelectionStart = textBox.CaretIndex;
        textBox.SelectionEnd = textBox.CaretIndex;
        textBox.Focus();
    }
}