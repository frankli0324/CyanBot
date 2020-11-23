# CyanBot

> Some personal functions built with EssentialBot

* improved module system
* command parsing

## Deployment

`dotnet publish -c Release -r linux-x64 && docker build . -t cyanbot`

example compose file:

```yml
version: "3.7"
services:
  mirai:
    image: java
    command: java -jar /opt/onebot-kotlin-0.3.3-all.jar --args -- --no-console
    working_dir: /opt
    volumes:
      - ./mirai-http:/opt
    environment:
      - TZ=Asia/Shanghai
  cyan:
    image: cyanbot
    depends_on:
      - mirai
    restart: always
    volumes:
      - ./cyan-data:/opt
    environment:
      - access_url=ws://mirai:6700/api
      - event_url=ws://mirai:6700/event
      - access_token=your_token
```

## Development

To create a new module:

under `namespace CyanBot.Modules`, create a **public** class  
export a function as a command by decorating it with `[OnCommand ("command")]`  
export a function for handling any other messages by decorating it with `[OnMessage]`  
you may decorate a `[Onmessage]` function only once per class
