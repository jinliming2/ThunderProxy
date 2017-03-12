# ThunderProxy[![Build](https://ci.appveyor.com/api/projects/status/tpdr1ykra9y9k5nb?svg=true&retina=true)](https://ci.appveyor.com/project/LimingJin/thunderproxy)
[![Developing](https://img.shields.io/badge/Thunder%20Proxy-developing-yellow.svg)](https://github.com/jinliming2/ThunderProxy)
[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](https://raw.githubusercontent.com/jinliming2/ThunderProxy/start/LICENSE)
[![GitHub stars](https://img.shields.io/github/stars/jinliming2/ThunderProxy.svg)](https://github.com/jinliming2/ThunderProxy/stargazers)
[![GitHub forks](https://img.shields.io/github/forks/jinliming2/ThunderProxy.svg)](https://github.com/jinliming2/ThunderProxy/network)
[![GitHub issues](https://img.shields.io/github/issues/jinliming2/ThunderProxy.svg)](https://github.com/jinliming2/ThunderProxy/issues)

Use Thunder to play while downloading, without XMP installed.

使用迅雷的边下边播功能，而不需要安装“迅雷看看”。

## Known Problems 已知问题
[ ] 使用VLC播放器时，第一个数据包写出后，TCP连接会被切断，导致VLC在开始播放的时候，需要等待几秒至十几秒的时间。

## 开发环境
* Visual Studio 2017
* .NET Framework 4

输出类型为Console应用程序，如果不想看到黑框，请修改为Windows应用程序后重新生成。

## 使用步骤
1. 从GitHub Release[![GitHub Release](https://img.shields.io/github/release/jinliming2/ThunderProxy.svg)](https://github.com/jinliming2/ThunderProxy/releases)下载最新的程序，或是`git clone https://github.com/jinliming2/ThunderProxy.git`，然后自行编译最新版。
2. 直接双击主程序`ThunderProxy.exe`，将会生成一个配置文件`config.xml`。
3. 编辑配置文件`config.xml`，将其中的`Command`节点的值改为您使用的播放器的路径。（路径建议使用`<![CDATA[]]>`包裹起来）
4. 打开注册表，新建注册表项`HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Thunder Network\Xmp`，并在下面新建一个“字符串值（REG_SZ）”，名称为`Path`，值为本程序的exe路径。（如果之前安装过迅雷看看，则这个注册表项可能是已经存在的，那么就直接修改就OK了）
5. （建议）重新启动迅雷。
6. 开始一个下载，（建议在下载一部分以后），点击边下边播按钮。

## 测试说明
本程序已在`迅雷极速版 1.0.34.360`配合`VLC Windows 64-bit 2.2.4`测试成功！
