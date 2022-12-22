#!/bin/bash

imageName="appwebclient:1.0"
docker build -f "./clientDockerfile" . -t $imageName
docker tag $imageName cloudwing/$imageName
# docker push cloudwing/$imageName