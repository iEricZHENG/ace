#!/bin/bash

imageName="apphost:1.0"
docker build -f "./hostDockerfile" . -t $imageName
docker tag $imageName cloudwing/$imageName
# docker push cloudwing/$imageName