#!/usr/bin/env bash

# generate html
npx md2pdf --config-file export_config.js --as-html *.md

# convert link from .md to .html
for file in *.html
do
    sed -i '' -E 's/\.md/\.html/g' $file
    sed -i '' -E 's/res\//\.\.\/res\//g' $file
    sed -i '' -E 's/href="\.\.\/toio\-sdk\-unity/href="\.\.\/\.\.\/toio\-sdk\-unity/g' $file
done

mv *.html html

# generate pdf
# md2pdf --config-file export_config.js *.md

# concat pf
# /System/Library/Automator/Combine\ PDF\ Pages.action/Contents/Resources/join.py \
#     --output output.pdf \
#     README.pdf \
#     preparation.pdf \
#     download_sdk.pdf \
#     build_ios.pdf \
#     tutorials_basic.pdf \
#     tutorials_cubehandle.pdf \
#     tutorials_navigator.pdf \
#     usage_cube.pdf \
#     usage_simulator.pdf \
#     usage_cubehandle.pdf \
#     usage_navigator.pdf \
#     sys_cube.pdf \
#     sys_ble.pdf \
#     sys_simulaltor.pdf \
#     sys_navigator.pdf
