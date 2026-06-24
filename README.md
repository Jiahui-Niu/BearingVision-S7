# 成品轴承外观机 BearingVision-S7

工业视觉检测软件，用于成品轴承外观自动化检测。支持 6 路相机同步检测，自动判断 OK/NG 并通知 PLC 剔料。

**开发方：** 台视科技  
**客户：** 宁波品晗智能视觉

[![Build](https://github.com/Jiahui-Niu/BearingVision-S7/actions/workflows/build.yml/badge.svg)](https://github.com/Jiahui-Niu/BearingVision-S7/actions/workflows/build.yml)

---

## 目录

- [技术栈](#技术栈)
- [项目结构](#项目结构)
- [功能说明](#功能说明)
- [6 路相机工位](#6-路相机工位)
- [运行环境要求](#运行环境要求)
- [编译步骤](#编译步骤)
- [上线部署](#上线部署)
- [模拟测试（无硬件环境）](#模拟测试无硬件环境)
- [账号权限](#账号权限)
- [配置文件说明](#配置文件说明)
- [日志说明](#日志说明)
- [CI 自动编译](#ci-自动编译)

---

## 技术栈

| 类别 | 技术 |
|------|------|
| 语言 / 框架 | C# / WPF，.NET Framework 4.6.1 |
| 架构模式 | MVVM |
| 视觉算法平台 | VisionMaster 4.4.0（海康机器人） |
| PLC 通信 | HslCommunication（Siemens S7 / Omron FINS TCP） |
| 数据库 | SQLite（System.Data.SQLite） |
| 配置持久化 | JSON（Newtonsoft.Json） |
| 日志 | log4net |
| 文件传输 | FluentFTP / Renci.SshNet |
| 硬件监控 | OpenHardwareMonitorLib |

---

## 项目结构

```
BearingVision-S7/
├── BearingVision-S7.sln              # Visual Studio 解决方案
├── WpfApp1/                          # 主程序（WPF 界面 + 核心逻辑）
│   ├── Model/
│   │   └── AppConfig.cs             # 配置模型（JSON 持久化）
│   ├── ViewModel/
│   │   ├── MainViewModel.cs         # 主逻辑：PLC轮询、VM触发、模拟模式
│   │   ├── MainImageShowViewModel.cs# 每路相机的图像/统计数据
│   │   ├── CheckSetViewModel.cs     # 检测配置页 ViewModel
│   │   ├── BrightnessViewModel.cs   # 光源亮度 ViewModel
│   │   └── TCPClient.cs             # PLC 通信封装
│   ├── View/
│   │   ├── LoginWindow.xaml         # 登录窗口
│   │   └── ReViewWindow.xaml        # 历史图像回放窗口
│   ├── UserControls/
│   │   ├── MainImageShowUserControl.xaml  # 6路相机实时图像
│   │   ├── CheckSetUserControl.xaml       # 检测参数配置表
│   │   ├── BrightnessUserControl.xaml     # 光源亮度设置
│   │   ├── DataSelectUserControl.xaml     # 各相机统计数据表
│   │   └── TCPClientUserControl.xaml      # PLC连接测试
│   ├── Themes/
│   │   └── GlobalStyle.xaml         # 全局深绿主题样式
│   └── XamlGeneratedStubs.ci.cs     # CI 专用：替代 XAML 编译器生成代码
├── ICPlatformTools/                  # 内部工具类库（SQLite / 日志 / 网络等）
├── PhotometricStereo/                # 光度立体算法库
├── VMSdkStubs/                       # CI 编译用桩（VM SDK + HslCommunication）
└── .github/workflows/                # GitHub Actions 自动编译
```

---

## 功能说明

### 主页面
- 6 路相机实时图像显示，每路独立显示 OK/NG 结果
- 生产统计：总数 / 合格数 / 不合格数 / 合格率（按相机分别统计）
- 开始 / 停止检测
- 手动触发每路相机（调试用）
- 相机在线/离线开关
- 型号、批次号管理
- 磁盘剩余、CPU 占用监控

### 方案配置
- VM 方案文件路径选择（.sol 文件）
- PLC 类型、IP、端口配置
- 光源亮度分阶段配置

### 检测配置
- PLC 地址配置（总数/OK/NG/启动/清料）
- 每路相机的 First / Start / Result PLC 地址
- 拍照数量、延时、间隔设置
- 图片保存路径、保存天数、OK/NG 分别存图开关
- **模拟模式**（见下方说明）

### 历史回放
- 按相机、日期、OK/NG 筛选历史图片
- 缩略图列表 + 大图预览
- 显示检测时间、结果、缺陷信息

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
| VisionMaster | 4.4.0 | 提供 VM SDK DLL 及算法运行时 |
| .NET Framework | 4.6.1 | Windows 10 自带，一般无需手动安装 |
| Visual Studio | 2022 | 仅开发编译时需要 |

### 需手动放置的文件
| 文件 | 放置路径 | 说明 |
|------|----------|------|
| `HslCommunication.dll` | `WpfApp1\bin\x64\Debug\` | PLC 通信库，需自行获取对应版本 |

> VM SDK 的托管 DLL（VM.Core.dll 等）已在 csproj 中设置 `Private=True`，编译后自动复制到输出目录，无需手动放置。

### 网络 / 硬件（生产环境）
- 工控机与 Siemens S7-1200/1500 PLC 网络互通（默认 IP 192.168.0.1:102）
- 海康工业相机通过 VisionMaster 方案管理

---

## 编译步骤

```bash
# 1. 克隆项目
git clone git@github.com:Jiahui-Niu/BearingVision-S7.git

# 2. 安装 VisionMaster 4.4.0

# 3. 将 HslCommunication.dll 放入 WpfApp1\bin\x64\Debug\

# 4. 用 Visual Studio 2022 打开 BearingVision-S7.sln
#    选择平台 x64 → 右键解决方案 → 重新生成解决方案

# 输出文件：WpfApp1\bin\x64\Debug\WpfSurface.exe
```

---

## 上线部署

1. VS 编译 `Release|x64`，产物在 `WpfApp1\bin\x64\Release\`
2. 将整个 Release 目录复制到工控机（如 `D:\WpfSurface\`）
3. 工控机需已安装 VisionMaster 4.4.0
4. 运行 `WpfSurface.exe`
5. 在「方案配置」页填写 VM 方案路径、PLC IP 等参数，保存
6. 点击「开始」，联调 PLC 信号和相机检测结果

> PLC 连接失败时程序以**离线模式**继续运行，PLC 相关功能不可用，但界面和 VM 检测功能正常。

---

## 模拟测试（无硬件环境）

在没有相机和 PLC 的笔记本/开发机上，可以使用**模拟模式**走完完整检测流程。

### 使用步骤

1. 准备一个文件夹，放入若干测试图片（支持 `.jpg` `.png` `.bmp`）
2. 打开程序，进入「**检测配置**」标签
3. 勾选「**启用模拟模式**」
4. 点击「选择」选择测试图片文件夹
5. 设置触发间隔（默认 3000ms，即每 3 秒触发一次）
6. 点击「**保存**」
7. 回到主页面，点击「**开始**」

### 模拟模式行为

| 项目 | 说明 |
|------|------|
| PLC | 不需要连接，自动跳过 |
| VM 方案 | 不需要 .sol 文件 |
| 触发方式 | 程序自动按间隔轮流触发每路在线相机 |
| 图像来源 | 从测试图片文件夹随机取图显示 |
| 检测结果 | 固定返回 OK（用于界面流程验证） |
| 存图 | 正常执行，图片保存至配置的存图路径 |
| 统计 | 正常更新总数、合格数、合格率 |

### 可验证的内容
- 主界面图像刷新和统计更新是否正常
- 存图目录结构是否正确
- 配置保存和加载是否正常
- 回放窗口能否查看历史图片

---

## 账号权限

| 用户名 | 密码 | 权限级别 |
|--------|------|----------|
| 操作员 | 111  | 基础操作 |
| 工程师 | 222  | 参数配置 |
| 管理员 | 333  | 全部功能 |

> 也可点击「跳过登录」以操作员身份直接进入。

---

## 配置文件说明

配置自动保存在程序目录下的 `AppData.dat`（JSON 格式），首次启动时自动生成默认配置。

主要配置项：

| 字段 | 说明 | 默认值 |
|------|------|--------|
| `SolutionPath` | VM 方案文件路径 | `D:\Debug\Sol\成品.sol` |
| `PLCType` | PLC 类型（S7 / Fins） | `S7` |
| `PLCIp` | PLC IP 地址 | `192.168.0.1` |
| `PLCPort` | PLC 端口 | `102` |
| `SaveImagePath` | 图片保存根目录 | `D:\Images` |
| `SaveDays` | 图片保留天数 | `30` |
| `SaveOK` / `SaveNG` | 是否保存 OK/NG 图 | `true` |
| `SimulationMode` | 是否启用模拟模式 | `false` |
| `SimulationImageFolder` | 模拟测试图片文件夹 | 空 |
| `SimulationIntervalMs` | 模拟触发间隔（ms） | `3000` |
| `VMProcedurePrefix` | VM 流程名前缀 | `Cam` |
| `VMGlobalVarModuleName` | VM 全局变量模块名 | `全局变量` |
| `VMResultVarName` | VM 检测结果变量名 | `Result` |
| `VMRunTimeout` | VM 执行超时（ms） | `8000` |

图片保存目录结构：
```
D:\Images\
└── 20260624\
    ├── Cam1\
    │   ├── OK\
    │   └── NG\
    ├── Cam2\
    ...
```

---

## 日志说明

日志文件位于程序目录下的 `logs\` 文件夹：

```
logs\
└── bearing_20260624.log
```

日志级别：`DEBUG` / `INFO` / `WARN` / `ERROR`

关键日志标记：

| 标记 | 含义 |
|------|------|
| `========== 开始启动检测` | 检测流程启动 |
| `[Cam1] First信号上升沿` | PLC 触发信号到来 |
| `[Cam1] VM检测完成` | 本次检测结果和耗时 |
| `【启动中止】` | 启动过程中发生错误 |
| `【警告】PLC连接失败` | 离线模式运行提示 |
| `模拟模式启动` | 模拟模式已开启 |

出现问题时将当天日志文件发送给开发人员排查。

---

## CI 自动编译

每次 push 到 `main` 分支，GitHub Actions 自动在 Windows 云服务器上编译并上传 `WpfSurface.exe`。

### CI 与生产编译的差异

| 项目 | 生产（本地 VS） | CI（GitHub Actions） |
|------|----------------|----------------------|
| VM SDK | VisionMaster 4.4.0 安装目录 | `VMSdkStubs` 桩 DLL |
| HslCommunication | `bin\x64\Debug\` 手动放置 | `VMSdkStubs` 桩类型 |
| log4net | VM 安装目录 | NuGet 2.0.15 |
| XAML 编译 | mc.exe 完整编译 | 跳过，由 `XamlGeneratedStubs.ci.cs` 提供桩代码 |

CI 可验证 C# 语法和引用正确性，但无法验证 VisionMaster 运行时行为。
