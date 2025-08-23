The old version is based on .NET Framework, which means it's out of date and with poor performance , but can run in most of computers which is of Windows System. 

The implicit function plotting part of the sketchpad can draw implicit functions with 2 variables \
It's an open source alternative to Grafeq. \
It is based on Reliable Two-Dimensional Graphing Methods for Mathematical Formulae with Two Free Variables by Jeff
Tupper of University of Toronto, SIGGRAPH 2001. \
It implements the Jeff Tupper's algorithms before Branch Cut Tracking or Algorithm 3.2.\
[See more](Example.md)\
[Thesis](https://www.dgp.toronto.edu/~mooncake/msc.html)\

The program use dynamic compilation to accelarate the calculation of implicit functions. If you are concerned about the performance of dynamic compilataion, you can try my another relative project.\
.NET Core JIT:\
![.NET Core JIT](./Images/Core_JIT.png)\
.NET Core AOT\
![.NET Core AOT](./Images/Core_AOT.png)\
.NET Framework\
![.NET Framework](./Images/Framework.png)\