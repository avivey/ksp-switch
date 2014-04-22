#!/bin/sh
export PATH=/opt/monodevelop/bin/:$PATH
cd `dirname $0`

rm -f *.dll
astyle -n -q *.cs
mcs -t:library -r:UnityEngine,Assembly-CSharp -lib:lib *.cs
