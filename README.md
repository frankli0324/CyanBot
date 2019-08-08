# EssentialBot

本项目依赖[cqhttp.Cyan](/frank-bots/cqhttp.Cyan)

## Purpose

本项目试图在cqhttp.Cyan的基础上建立指令框架与带权限的管理功能。  
类似于[nonebot](/richardchien/nonebot)之于[python-aiocqhttp](/richardchien/python-aiocqhttp)

## Configuration/Deploy

本项目默认使用http方式链接cqhttp插件。如需其它连接方式，可以根据cqhttp.Cyan的使用说明自行修改  
由于本项目与cqhttp.Cyan保持同步更新，所以此处对cqhttp.Cyan的依赖需要clone两份代码。。  
```sh
git clone https://github.com/frank-bots/cqhttp.Cyan
git clone https://github.com/frank-bots/EssentialBot

cd cqhttp.Cyan
dotnet restore;dotnet build
cd ../EssentialBot
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

## Module System

如果需要添加新的Module，可以在Functions文件夹中新建一个类  
类名请起得好听一点，因为之后会用来load和unload(  
EssentialBot会在启动时通过Reflection来加载CyanBot.Functions命名空间下所有含有签名为`public static void LoadModule();`与`public static void UnloadModule();`的函数的类作为Module。  
配置文件中的super_user项所指定的QQ号码可以通过私聊bot来控制Module的启用与否
```
/get_loaded
/load_module
/unload_module
```
