# CsGrafeq
## 简介
一个支持隐函数绘制的Winform控件

CsGrafeq 是一个强大的控件以绘制带有两个参数（x与y）的隐函数，是Grafeq的开源替代品

CsGrafeq 部分基于由Jeff Tupper of University of Toronto, SIGGRAPH 2001的Reliable Two-Dimensional Graphing Methods for Mathematical Formulae with Two Free Variables，同时在判断子像素时使用了Marching Cubes算法的思路

出于精简的考量，CsGrafeq 使用 Microsoft.WindowsAPICodePack.DirectX 来绘制函数，然而这可以轻易地改写，使用Shapdx

## 例子

y=x y=x^3 y=e^x y=sin(x)

![image](https://github.com/jyswjjgdwtdtj/CsGrafeq/blob/main/ExampleImage/1.bmp)

x/sin(x)+y/sin(y)=x*y/sin(x*y)

![image](https://github.com/jyswjjgdwtdtj/CsGrafeq/blob/main/ExampleImage/2.bmp)

## 特点
1. 用鼠标控制以调整坐标轴大小与显示的区域
2. 对函数进行编译以加快运行速度
3. 唯一依赖项是 Microsoft.WindowsAPICodePack.DirectX.dll
