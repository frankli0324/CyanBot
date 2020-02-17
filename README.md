# CyanBot
> Some personal functions built with EssentialBot

* improved module system
* command parsing

## Deployment

`dotnet publish -c Release`, deploy the `bin/Release/netcoreapp3.1/publish` to your server  

create following files before execution:

file        | description
----------- | ------------------------------------------------------------------------
config.json | see `config.json.sample`
bots        | messages sent by users whose ID is present in this file will be ignored

fire up with `dotnet CyanBot.dll` or simply `./CyanBot`


## Development

To create a new module:

under `namespace CyanBot.Modules`, create a **public** class  
export a function as a command by decorating it with `[OnCommand ("command")]`  
export a function for handling any other messages by decorating it with `[OnMessage]`  
you may decorate a `[Onmessage]` function only once per class
