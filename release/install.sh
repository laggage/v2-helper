#!/bin/bash

# install dotnetcore3.0 runtime

sudo rpm -Uvh https://packages.microsoft.com/config/centos/7/packages-microsoft-prod.rpm
sudo yum install aspnetcore-runtime-3.0

wget https://github.com/laggage/v2-helper/releases/download/1.0.0/config.json
wget https://github.com/laggage/v2-helper/releases/download/1.0.0/MyV2ray.Console.dll
wget https://github.com/laggage/v2-helper/releases/download/1.0.0/MyV2ray.Console.runtimeconfig.json
wget https://github.com/laggage/v2-helper/releases/download/1.0.0/v2-helper

cp ./v2-helper /usr/bin/v2-helper
chmod +x /usr/bin/v2-helper
echo '初始化配置文件'
cp config.json /etc/v2ray/config.json
