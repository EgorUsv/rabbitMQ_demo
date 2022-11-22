# rabbitMQ_demo

How to run:
1. Download and install latest Erlang version https://www.erlang.org/downloads (**NOT** check README, cause it 150MB)
2. Download latest RabbitMQ Server https://www.rabbitmq.com/download.html
3. Install RabbitMQ ***WITHOUT*** RabbitMQ Service checkbox
4. After installation you can run server `C:\Program Files\RabbitMQ Server\rabbitmq_server-3.11.3\sbin\rabbitmq-service.bat`, but it will be run without web UI.

To Enable web UI you shoud run this `.\rabbitmq-plugins.bat enable rabbitmq_management`. This command will add web UI as server plugin. Web UI by default runs at `http://localhost:15672`  Login and password: 
*guest*.
For localhost run you should run in private window.
