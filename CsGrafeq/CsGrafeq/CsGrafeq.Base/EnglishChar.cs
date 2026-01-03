using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using CsGrafeq.Collections;
using CsGrafeq.MVVM;
using ReactiveUI;
using static CsGrafeq.Utilities.ThrowHelper;
using static System.Math;

namespace CsGrafeq;

public class EnglishChar : ObservableObject
{
    protected EnglishChar()
    {
        PropertyChanged += (s, e) =>
        {
            if (e.PropertyName.Length == 1)
            {
                var c = e.PropertyName[0];
                if ('A' <= c && c <= 'Z') CharValueChanged?.Invoke((EnglishCharEnum)Pow(2, c - 'A'));
            }
        };
        A = 10;
    }

    public static EnglishChar Instance { get; } = new();
    public NativeBuffer<double> CharsValue { get; } = new('Z' - 'A' + 1, true);
    public ObservableCollection<uint> CharsReferenceCounter { get; } = new(new uint['Z' - 'A' + 1]);

    public double this[char c]
    {
        get
        {
            if (c < 'a' || c > 'z')
                return Throw<double>("Only a-z are supported.");
            return CharsValue[(nuint)(c - 'a')];
        }
        set
        {
            if (c < 'a' || c > 'z')
                Throw("Only a-z are supported.");
            this.RaiseAndSetIfChanged(ref CharsValue[(nuint)(c - 'a')], value, (c + "").ToUpper());
        }
    }

    public double A
    {
        get => CharsValue[0];
        set => this.RaiseAndSetIfChanged(ref CharsValue[0], value);
    }

    public double B
    {
        get => CharsValue[1];
        set => this.RaiseAndSetIfChanged(ref CharsValue[1], value);
    }

    public double C
    {
        get => CharsValue[2];
        set => this.RaiseAndSetIfChanged(ref CharsValue[2], value);
    }

    public double D
    {
        get => CharsValue[3];
        set => this.RaiseAndSetIfChanged(ref CharsValue[3], value);
    }

    public double E
    {
        get => CharsValue[4];
        set => this.RaiseAndSetIfChanged(ref CharsValue[4], value);
    }

    public double F
    {
        get => CharsValue[5];
        set => this.RaiseAndSetIfChanged(ref CharsValue[5], value);
    }

    public double G
    {
        get => CharsValue[6];
        set => this.RaiseAndSetIfChanged(ref CharsValue[6], value);
    }

    public double H
    {
        get => CharsValue[7];
        set => this.RaiseAndSetIfChanged(ref CharsValue[7], value);
    }

    public double I
    {
        get => CharsValue[8];
        set => this.RaiseAndSetIfChanged(ref CharsValue[8], value);
    }

    public double J
    {
        get => CharsValue[9];
        set => this.RaiseAndSetIfChanged(ref CharsValue[9], value);
    }

    public double K
    {
        get => CharsValue[10];
        set => this.RaiseAndSetIfChanged(ref CharsValue[10], value);
    }

    public double L
    {
        get => CharsValue[11];
        set => this.RaiseAndSetIfChanged(ref CharsValue[11], value);
    }

    public double M
    {
        get => CharsValue[12];
        set => this.RaiseAndSetIfChanged(ref CharsValue[12], value);
    }

    public double N
    {
        get => CharsValue[13];
        set => this.RaiseAndSetIfChanged(ref CharsValue[13], value);
    }

    public double O
    {
        get => CharsValue[14];
        set => this.RaiseAndSetIfChanged(ref CharsValue[14], value);
    }

    public double P
    {
        get => CharsValue[15];
        set => this.RaiseAndSetIfChanged(ref CharsValue[15], value);
    }

    public double Q
    {
        get => CharsValue[16];
        set => this.RaiseAndSetIfChanged(ref CharsValue[16], value);
    }

    public double R
    {
        get => CharsValue[17];
        set => this.RaiseAndSetIfChanged(ref CharsValue[17], value);
    }

    public double S
    {
        get => CharsValue[18];
        set => this.RaiseAndSetIfChanged(ref CharsValue[18], value);
    }

    public double T
    {
        get => CharsValue[19];
        set => this.RaiseAndSetIfChanged(ref CharsValue[19], value);
    }

    public double U
    {
        get => CharsValue[20];
        set => this.RaiseAndSetIfChanged(ref CharsValue[20], value);
    }

    public double V
    {
        get => CharsValue[21];
        set => this.RaiseAndSetIfChanged(ref CharsValue[21], value);
    }

    public double W
    {
        get => CharsValue[22];
        set => this.RaiseAndSetIfChanged(ref CharsValue[22], value);
    }

    public double X
    {
        get => CharsValue[23];
        set => this.RaiseAndSetIfChanged(ref CharsValue[23], value);
    }

    public double Y
    {
        get => CharsValue[24];
        set => this.RaiseAndSetIfChanged(ref CharsValue[24], value);
    }

    public double Z
    {
        get => CharsValue[25];
        set => this.RaiseAndSetIfChanged(ref CharsValue[25], value);
    }

    public event Action<EnglishCharEnum> CharValueChanged;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double GetValue(char c)
    {
        return this[c];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double StaticGetValue(char c)
    {
        return Instance[c];
    }

    public void AddReference(EnglishCharEnum c)
    {
        var uc = (uint)c;
        for (var i = 0; i < 26; i++)
            if (((uc >> i) & 0x1) == 0x1)
                CharsReferenceCounter[i]++;
        this.RaisePropertyChanged(nameof(CharsReferenceCounter));
    }

    public void RemoveReference(EnglishCharEnum c)
    {
        var uc = (uint)c;
        for (var i = 0; i < 26; i++)
            if (((uc >> i) & 0x1) == 0x1)
            {
                if (CharsReferenceCounter[i] == 0)
                    Throw(new Exception("The reference counter is already zero."));
                CharsReferenceCounter[i]--;
            }

        this.RaisePropertyChanged(nameof(CharsReferenceCounter));
    }
}

[Flags]
public enum EnglishCharEnum : long
{
    None = 0,
    A = 1L << 0,
    B = 1L << 1,
    C = 1L << 2,
    D = 1L << 3,
    E = 1L << 4,
    F = 1L << 5,
    G = 1L << 6,
    H = 1L << 7,
    I = 1L << 8,
    J = 1L << 9,
    K = 1L << 10,
    L = 1L << 11,
    M = 1L << 12,
    N = 1L << 13,
    O = 1L << 14,
    P = 1L << 15,
    Q = 1L << 16,
    R = 1L << 17,
    S = 1L << 18,
    T = 1L << 19,
    U = 1L << 20,
    V = 1L << 21,
    W = 1L << 22,
    X = 1L << 23,
    Y = 1L << 24,
    Z = 1L << 25
}