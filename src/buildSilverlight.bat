
cd FSharp.PowerPack
msbuild /p:TargetFramework=Silverlight %1 %2 %3 %4 %5 %6 %7 %8 %9
cd ..

cd FSharp.PowerPack.Compatibility
msbuild /p:TargetFramework=Silverlight %1 %2 %3 %4 %5 %6 %7 %8 %9
cd ..

cd FSharp.PowerPack.Linq
msbuild /p:TargetFramework=Silverlight %1 %2 %3 %4 %5 %6 %7 %8 %9
cd ..

cd FSharp.PowerPack.Unittests
msbuild /p:TargetFramework=Silverlight %1 %2 %3 %4 %5 %6 %7 %8 %9
cd ..
