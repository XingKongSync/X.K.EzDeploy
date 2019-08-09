# X.K.EzDeploy
提供图形界面，将任意控制台程序部署为 Windows 服务

开发人员可以将自己编写的一个或多个控制台应用程序通过本工具组织起来，然后连同本工具一起打包，发送给运维人员，方便快速部署

## 概述 / Introduction
基于微软的工具 srvany.exe 和 instsrv.exe，提供了图形界面，可以方便地将任意控制台应用添加到系统服务中，或者从系统服务中删除，也可以控制服务的运行和停止

受到 https://wangye.org/blog/archives/42/ 的启发，并且与该作者发布的服务管理工具 SrvanyUI 兼容

## 为什么要使用此工具 / Advantage
srvany.exe 固然好用，但是其不提供对进程存活与否的检测，举个例子：假设通过 srvany 启动了一个进程 a.exe，运行一段时间后，a.exe 由于崩溃而退出了，此时在系统服务中观察到该服务的状态仍为“正在运行”

本工具除了提供服务的安装功能外，还提供一个守护进程，可以负责将错误的服务运行状态纠正过来，也支持对停止运行的服务进行重启

本工具提供的守护进程也是通过服务来实现的，其作为一个特殊的服务，放置在程序的 Service 文件夹下

## 用法 / Usage
现将工程编译，在编译的输出文件夹中，可以看到跟 EasyDeploy.exe 同级的有一个 Service 文件夹，请将您在 Service 下新建一个文件夹，并将您的服务放置在那个文件夹下，参考 DemoService
然后，在您的服务文件夹下新建一个名为 ServiceConfig.json 的文本文件


ServiceConfig.json 的格式如下
```
{
	"ServiceName": "DemoService", //服务唯一标识，建议全英文
	"ExcutableFilePath": "DemoService.exe", //您的控制台程序的可执行文件名
	"Args": "", //您的控制台程序启动时的所需参数
	"DisplayName": "测试服务", //对部署人员友好的服务名称
	"Description": "这个服务会启动一个HTTP服务器，若访问 http://127.0.0.1:8123 成功，则说明服务成功启动" //服务的功能描述
}
```

若您有多个服务，可以重复上述步骤，然后将整个 EasyDeploy 程序文件夹保持结构完整，发送给运维人员


需要注意的是，建议您在其他服务都安装完成后，再安装 WatchDogService，否则 WatchDogService 无法察觉您所做的更改。

若您先安装了 WatchDogService，您也可以随时卸载 WatchDogService，当您再次安装 WatchDogService时，WatchDogService会自动应用当前已安装服务的变更。
