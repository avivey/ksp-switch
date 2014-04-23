#!/bin/sh
export PATH=/opt/monodevelop/bin/:$PATH
cd `dirname $0`

rm -f *.dll
astyle -n -q -pUH  *.cs
mcs -t:library -r:UnityEngine,Assembly-CSharp -lib:lib *.cs
