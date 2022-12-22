$imageName="appwebclient:latest"
# 改配置文件
docker build -f "./Dockerfile" . -t $imageName
docker tag $imageName cloudwing/$imageName
docker push cloudwing/$imageName