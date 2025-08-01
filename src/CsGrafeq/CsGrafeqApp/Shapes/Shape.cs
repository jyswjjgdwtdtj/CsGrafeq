using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Reactive;
using Avalonia.ReactiveUI;
using Avalonia.Rendering;
using CsGrafeqApp.Classes;
using CsGrafeqApp.Shapes.ShapeGetter;

namespace CsGrafeqApp.Shapes
{
    public delegate void ShapeChangedHandler();
    public delegate void ShapeChangedHandler<T>(Shape shape, T args);
    public delegate void ShapeChangedHandler<T1, T2>(T1 shape, T2 args) where T1:Shape;
    public abstract class Shape:ReactiveObject
    {
        public Shape() { 
            
        }
        public abstract void RefreshValues();
        public virtual void InvokeEvent()
        {
            ShapeChanged?.Invoke();
        }
        public event ShapeChangedHandler? ShapeChanged;
        public event ShapeChangedHandler<bool>? SelectedChanged;
        public List<Shape> SubShapes = new List<Shape>();
        protected bool CanSelected=true;
        protected bool CanPointOver = true;
        public bool PointerOver
        {
            get => field;
            set
            {
                if (!CanPointOver)
                    return;
                this.RaiseAndSetIfChanged(ref field, value);
                InvokeEvent();
            }
        }
        public bool Selected
        {
            get => field;
            set
            {
                if (!CanSelected)
                    return;
                if (field != value)
                    SelectedChanged?.Invoke(this, value);
                this.RaiseAndSetIfChanged(ref field, value);
                InvokeEvent();
            }
        } = false;
        public bool Visible
        {
            get => field;
            set
            {
                this.RaiseAndSetIfChanged(ref field, value);
                InvokeEvent();
            }
        } = true;
        public string Name
        {
            get => field;
            set
            {
                this.RaiseAndSetIfChanged(ref field, value);
            }
        } = "";
        protected abstract string TypeName { get; }
        public string Type => TypeName + ":";
        public abstract string Description { get; }
        public abstract Vec HitTest(Vec vec);
        public abstract Getter Getter { get; }
    }
}
