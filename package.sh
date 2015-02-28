#!/bin/sh -e
cd `dirname $0`
rm -rf archive
TARGET=SwitchVessel

./build.sh

git diff --quiet HEAD || echo WORKSAPCE IS NOT CLEAN!

mkdir -p archive/$TARGET
cp SwitchVessel.png SwitchVessel.dll archive/$TARGET/

cd archive/
zip -r $TARGET.zip $TARGET/
