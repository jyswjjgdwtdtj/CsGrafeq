using System.Collections.Generic;

namespace CsGrafeqApplication.Addons.GeometricPad;

public static class ImplicitFunctionExamples
{
    public static readonly IReadOnlyList<string> Examples;

    static ImplicitFunctionExamples()
    {
        Examples = @"y=(x-1) \% x;no
\sin(x)=\cos(y);no
y-x=\sin(\exp(x+y));no
x^2+y^2=1 \lor y=-\cos(x);no
(2y-x-1)(2y-x+1)(2x+y-1)(2x+y+1)((5x-2)^2+(5y-6)^2-10)((5x)^2+(5y)^2-10)((5x+2)^2+(5y+6)^2-10)=0;no
((x-2)^2+(y-2)^2-0.4)((x-2)^2+(y-1)^2-0.4)((x-2)^2+y^2-0.4)((x-2)^2+(y+1)^2-0.4)((x-2)^2+(y+2)^2-0.4)((x-1)^2+(y-2)^2-0.4)((x-1)^2+(y-1)^2-0.4)((x-1)^2+y^2-0.4)((x-1)^2+(y+1)^2-0.4)((x-1)^2+(y+2)^2-0.4)(x^2+(y-2)^2-0.4)(x^2+(y-1)^2-0.4)(x^2+y^2-0.4)(x^2+(y+1)^2-0.4)(x^2+(y+2)^2-0.4)((x+1)^2+(y-2)^2-0.4)((x+1)^2+(y-1)^2-0.4)((x+1)^2+y^2-0.4)((x+1)^2+(y+1)^2-0.4)((x+1)^2+(y+2)^2-0.4)((x+2)^2+(y-2)^2-0.4)((x+2)^2+(y-1)^2-0.4)((x+2)^2+y^2-0.4)((x+2)^2+(y+1)^2-0.4)((x+2)^2+(y+2)^2-0.4)=0;no
#y=\gcd(⌊x),1);no
|\sin(\sqrt{x^2+y^2})|=|\cos(x)|;yes
⌊x⌋^2+⌊y⌋^2=⌊\sqrt{⌊x⌋^2+⌊y⌋^2}⌋^2;no
⌊x⌋^2+⌊y⌋^2=25;no
#gcd(⌊x⌋,⌊y⌋)>a;no
⌊x+1⌋ \% ⌊y+2⌋=1;no
sin(\frac {1} {x})=y;no
{\frac{(x(x-3)} {(x-3.005))}}^2+{\frac {y(y-3)} {y-3.005}}^2=81;yes
y= \frac {sin(x)} {x};yes
(1+99*⌊⌊y⌋*2^ceil(x) \% 2⌋)(mod(x,1)-0.5)^2+(mod(y,1)-1/2)^2=0.15;no
sin(2^⌊y)*x+pi/4*(y-⌊y))-pi/2)=0|sin(2^⌊y)*x-pi/4*(y-⌊y))-pi/2)=0;no
x/cos(x)+y/cos(y)=x*y/cos(x*y)|x/cos(x)+y/cos(y)=-(x*y/cos(x*y))|x/cos(x)-y/cos(y)=x*y/cos(x*y)|x/cos(x)-y/cos(y)=-(x*y/cos(x*y));yes
x/sin(x)+y/sin(y)=x*y/sin(x*y)|x/sin(x)+y/sin(y)=-(x*y/sin(x*y))|x/sin(x)-y/sin(y)=x*y/sin(x*y)|x/sin(x)-y/sin(y)=-(x*y/sin(x*y));yes
abs(x*cos(x)-y*sin(y))=abs(x*cos(y)-y*sin(x));yes
abs(x*cos(x)+y*sin(y))=x*cos(y)-y*sin(x);yes
sin(sqrt((x+5)^2+y^2))*cos(8*arctan(y/(x+5)))*sin(sqrt((x-5)^2+(y-5)^2))*cos(8*arctan((y-5)/(x-5)))*sin(sqrt(x^2+(y+5)^2))*cos(8*arctan((y+5)/x))>0;no
sin(abs(x+y))>max(cos(x^2),sin(y^2));no
exp(sin(x)+cos(y))=sin(exp(x+y));no
sin(sin(x)+cos(y))=cos(sin(x*y)+cos(x));no
sin(x^2+y^2)=cos(x*y);yes
abs(sin(x^2-y^2))=sin(x+y)+cos(x*y);yes
abs(sin(x^2+2*x*y))=sin(x-2*y);no
tan(sin(x)+cos(y))=sin(tan(x+y));no
arctan(sin(x)+cos(y))=sin(arctan(x+y));no
0.25*(2*sin(x*sin(y)+y*sin(x)))>0;no".Replace("\r\n", "@").Replace("\n", "@").Split("@");
    }
}