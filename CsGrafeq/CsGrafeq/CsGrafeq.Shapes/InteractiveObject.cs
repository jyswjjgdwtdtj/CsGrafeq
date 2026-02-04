using CsGrafeq.I18N;
using CsGrafeq.MVVM;
using ReactiveUI;
using static CsGrafeq.Utilities.ColorHelper;

namespace CsGrafeq.Shapes;

public abstract class InteractiveObject: ObservableObject, IDisposable
{
    
    /// <summary>
    ///     是否可以交互
    /// </summary>
    protected bool CanInteract
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = true;
    
    /// <summary>
    ///     颜色 argb格式
    /// </summary>
    public uint Color
    {
        get => field;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value | 0xff000000);
            InvokeChanged();
        }
    } = GetRandomColor();

    /// <summary>
    ///     是否可见
    /// </summary>
    public bool Visible
    {
        get => field;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            InvokeChanged();
        }
    } = true;
    
    /// <summary>
    ///     当前图形名称
    /// </summary>
    public MultiLanguageData TypeName { get; init; }
    
    
    /// <summary>
    ///     简介
    /// </summary>
    public string Description
    {
        get => field;
        protected set => this.RaiseAndSetIfChanged(ref field, value);
    } = "";
    /// <summary>
    ///     用户是否可以从UI改变
    /// </summary>
    public bool IsUserEnabled
    {
        get => field;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            InvokeChanged();
        }
    } = true;

    public abstract void Dispose();
    public abstract void InvokeChanged();
    
    
    /// <summary>
    ///     是否被删除（但仍保留在撤销链上）
    /// </summary>
    public bool IsDeleted
    {
        get => field;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            InvokeChanged();
        }
    } = false;
}