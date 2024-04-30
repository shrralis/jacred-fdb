#!/bin/bash

readonly custom_conf_filepath='./custom_conf/init.conf'
if [ -f "${custom_conf_filepath}" ]; then
    cp -f "${custom_conf_filepath}" './init.conf'
fi

dotnet JacRed.dll
