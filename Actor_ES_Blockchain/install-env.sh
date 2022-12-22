docker run -d -p 5672:5672 -p 15672:15672 --hostname rabbit --name rabbit --restart=always -e RABBITMQ_DEFAULT_USER=admin -e RABBITMQ_DEFAULT_PASS=admin  rabbitmq:3-management
docker run -d -p 27017:27017 --name mongo --restart=always -td mongo 
docker run -d -p 5432:5432 --name postgres -e POSTGRES_PASSWORD=123456 --restart=always postgres 
docker run -d --name elasticsearch -p 9200:9200 -p 9300:9300 -e "discovery.type=single-node" --restart=always elasticsearch:7.2.0