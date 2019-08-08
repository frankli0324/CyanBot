# EssentialBot

本项目依赖[cqhttp.Cyan](/frank-bots/cqhttp.Cyan)

## Purpose

本项目试图在cqhttp.Cyan的基础上建立指令框架与带权限的管理功能。  
类似于[nonebot](/richardchien/nonebot)之于[python-aiocqhttp](/richardchien/python-aiocqhttp)之间的关系

## Configuration/Deploy

本项目默认使用http方式链接cqhttp插件。如需其它连接方式，可以根据cqhttp.Cyan的使用说明自行修改  
由于本项目与cqhttp.Cyan保持同步更新，所以此处对cqhttp.Cyan的依赖需要clone两份代码。。  
```sh
git clone https://github.com/frank-bots/cqhttp.Cyan
git clone https://github.com/frank-bots/EssentialBot
cd EssentialBot
cp config.json.sample config.json
# 按照直觉配置就好啦（XD
dotnet restore

# then...
dotnet run
# or
dotnet build
# or
dotnet publish -c Release
```
