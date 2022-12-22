#!/bin/bash

# 运行时执行命令
docker run --name apphost\
           -d \
           --restart always \
           -p 11111:11111/tcp \
           -p 30000:30000/tcp \
           cloudwing/apphost:latest