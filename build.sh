#!/bin/sh
export PATH=/opt/monodevelop/bin/:$PATH
cd `dirname $0`

rm -f *.dll
astyle -n -q -pUH  *.cs
mcs -t:library -r:UnityEngine,UnityEngine.UI,Assembly-CSharp,Toolbar,KSPUtil -lib:lib -out:SwitchVessel.dll  *.cs
