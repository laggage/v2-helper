#!/bin/bash

echo "Begin install dotnetcore3.0 runtime"

sudo rpm -Uvh https://packages.microsoft.com/config/centos/7/packages-microsoft-prod.rpm

sudo yum update

sudo yum install aspnetcore-runtime-3.0

echo "End install!"
