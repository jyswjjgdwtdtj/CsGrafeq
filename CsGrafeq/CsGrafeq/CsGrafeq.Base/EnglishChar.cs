using CsGrafeq.Collections;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CsGrafeq;
using static CsGrafeq.Extension;

namespace CsGrafeq
{

    public class EnglishChar : ReactiveObject
    {
        private NativeBuffer<double> CharsValue = new NativeBuffer<double>('Z' - 'A' + 1);
        public double this[char c]
        {
            get
            {
                if (c < 'a' || c > 'a')
                    return Throw<double>("Only a-z are supported.");
                return CharsValue[(nuint)(c - 'a')];
            }
            set
            {
                if (c < 'a' || c > 'z')
                    Throw("Only a-z are supported.");
                this.RaiseAndSetIfChanged(ref CharsValue[(nuint)(c - 'a')], value);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double GetValue(char c)
        {
            return this[c];
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
    }
}
