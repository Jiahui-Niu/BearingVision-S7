# 成品轴承外观机 BearingVision-S7

工业视觉检测软件，用于成品轴承外观自动化检测。支持 6 路相机同步检测，自动判断 OK/NG 并通知 PLC 剔料。

**开发方：** 台视科技  
**客户：** 宁波品晗智能视觉

---

## 技术栈

| 类别 | 技术 |
|------|------|
| 语言 / 框架 | C# / WPF，.NET Framework 4.6.1 |
| 架构模式 | MVVM |
| 视觉算法平台 | VisionMaster 4.4.3（海康机器人） |
| PLC 通信 | HslCommunication（Siemens S7 / Omron FINS TCP） |
| 数据库 | SQLite（System.Data.SQLite） |
| 配置持久化 | JSON（Newtonsoft.Json） |
| 日志 | log4net |

---

## 项目结构

```
BearingVision-S7/
├── BearingVision-S7.sln                 # Visual Studio 解决方案
├── WpfApp1/                             # 主程序（WPF 界面 + 核心逻辑）
│   └── XamlGeneratedStubs.ci.cs        # CI 专用：替代 XAML 编译器生成的代码
├── ICPlatformTools/                     # 内部工具类库（SQLite / 日志 / 网络等）
├── PhotometricStereo/                   # 光度立体算法库
├── VMSdkStubs/                          # CI 编译用桩（VM SDK + HslCommunication）
├── 参考资料/                            # 界面截图 / 操作手册
└── .github/workflows/                   # GitHub Actions 自动编译
```

---

## 6 路相机工位

| 工位 | 检测部位 | 拍照数 | First 地址 | Start 地址 | Result 地址 |
|------|----------|--------|-----------|-----------|------------|
| Cam1 | 内径 1   | 18 张  | DB85.40   | DB85.0    | DB85.20    |
| Cam2 | 端面 1   | 1 张   | DB85.42   | DB85.2    | DB85.22    |
| Cam3 | 端面 2   | 1 张   | DB85.44   | DB85.4    | DB85.24    |
| Cam4 | 内径 2   | 18 张  | DB85.46   | DB85.6    | DB85.26    |
| Cam5 | 外径 1   | 15 张  | DB85.48   | DB85.8    | DB85.28    |
| Cam6 | 外径 2   | 15 张  | DB85.50   | DB85.10   | DB85.30    |

Result 值：`1 = OK`，`2 = NG`（Int16）

---

## 运行环境要求

### 操作系统
- Windows 10 / 11，64 位

### 必须安装
| 软件 | 版本 | 说明 |
|------|------|------|
| VisionMaster | 4.4.3 | 提供 VM SDK DLL 及 log4net，安装后自动注册到 GAC |
| .NET Framework | 4.6.1 | Windows 10 自带，一般无需手动安装 |
| Visual Studio | 2022 | 仅开发编译时需要 |

### 需手动放置的文件
| 文件 | 放置路径 |
|------|----------|
| `HslCommunication.dll` | `WpfApp1\bin\x64\Debug\`（或 Release 对应目录） |

### 网络 / 硬件
- 工控机与 Siemens S7-1200/1500 PLC 网络互通
- 海康工业相机通过 VisionMaster 管理

---

## 编译步骤

```bash
# 1. 安装 VisionMaster 4.4.3
# 2. 将 HslCommunication.dll 放入 WpfApp1\bin\x64\Debug\
# 3. 用 Visual Studio 2022 打开解决方案
git clone git@github.com:Jiahui-Niu/BearingVision-S7.git
# 用 VS 打开 BearingVision-S7.sln → 选 Release|x64 → 生成
```

---

## 上线部署

1. VS 编译 `Release|x64`，产物在 `WpfApp1\bin\x64\Release\`
2. 将整个 Release 目录复制到工控机（如 `D:\WpfSurface\`）
3. 工控机需已安装 VisionMaster 4.4.3
4. 运行 `WpfSurface.exe`，进入设置页填写 PLC IP、VM 方案路径等参数
5. 点击「启动」，联调 PLC 信号和相机检测结果

**日志位置：** `程序目录\logs\bearing_yyyyMMdd.log`  
出现问题时将当天日志文件发送给开发人员排查。

---

## CI 自动编译

每次 push 到 `main` 分支，GitHub Actions 自动在 Windows 云服务器上编译并上传 `WpfSurface.exe`。

[![Build](https://github.com/Jiahui-Niu/BearingVision-S7/actions/workflows/build.yml/badge.svg)](https://github.com/Jiahui-Niu/BearingVision-S7/actions/workflows/build.yml)

### CI 与生产编译的差异

| 项目 | 生产（本地 VS） | CI（GitHub Actions） |
|------|----------------|----------------------|
| VM SDK | VisionMaster 4.4.3 安装目录 | `VMSdkStubs` 桩 DLL |
| HslCommunication | `bin\x64\Debug\` 手动放置 | `VMSdkStubs` 桩类型 |
| log4net | VM 安装目录 | NuGet 2.0.15 |
| XAML 编译 | mc.exe 完整编译 | 跳过，由 `XamlGeneratedStubs.ci.cs` 提供桩代码 |

CI 可验证 C# 语法和引用正确性，但无法验证 VisionMaster 运行时行为。编译产物可以生成但不能直接运行（缺少 VM SDK 和相机驱动）。
