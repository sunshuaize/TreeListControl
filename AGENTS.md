# AGENTS.md - 项目开发指南

本文档为在此代码库中工作的 AI 代理提供开发指南。

## 1. 项目概述

- **项目类型**: .NET Framework 4.8 WinForms 桌面应用程序
- **主要功能**: 自定义 WinForms 控件库（TreeListControl、CustomTable、CustomTabControl）
- **编程语言**: C#

## 2. 构建与运行命令

### 构建项目
```bash
cd demo
dotnet build
```

### 运行测试/演示
```bash
cd demo/bin/Debug
./demo.exe
```

### 清理并重新构建
```bash
cd demo
dotnet build --no-incremental
```

### 运行单个测试（如果存在测试项目）
```bash
dotnet test --filter "FullyQualifiedName~TestClassName.TestMethodName"
```

## 3. 代码风格指南

### 3.1 命名约定

- **命名空间**: `demo.Controls`（项目名 + 功能模块）
- **类名**: PascalCase，如 `TreeListControl`、`CustomTabControl`
- **方法名**: PascalCase，如 `AddTab`、`RefreshVisibleNodes`
- **属性名**: PascalCase，如 `TabHeight`、`SelectedIndex`
- **私有字段**: 下划线前缀 + PascalCase，如 `_tabHeight`、`_selectedIndex`
- **常量**: PascalCase，如 `DefaultTabHeight`

### 3.2 代码组织

- **窗体文件规范**: 每个窗体包含三个文件：
  - `xxx.cs` - 窗体逻辑代码
  - `xxx.Designer.cs` - 设计器生成的代码
  - `xxx.resx` - 资源文件
  示例：
  ```
  Form1.cs
  Form1.Designer.cs
  Form1.resx
  ```

- **控件声明和设置**: 除非是用户动态添加控件，否则控件的声明和初始化设置都要写在 `.Designer.cs` 文件中，保持代码分离

- **使用 #region 分组代码**，顺序如下：
  ```csharp
  #region 颜色定义
  #endregion
  
  #region 属性
  #endregion
  
  #region 构造函数
  #endregion
  
  #region 公共方法
  #endregion
  
  #region 私有方法
  #endregion
  
  #region 事件处理
  #endregion
  ```

- **using 语句按字母顺序排列**
- **每个类一个文件**

### 3.3 代码格式

- **缩进**: 4 空格
- **花括号**: 新行开始
- **属性格式**:
  ```csharp
  public int TabHeight
  {
      get => _tabHeight;
      set
      {
          _tabHeight = value;
          Invalidate();
      }
  }
  ```
- **使用 expression-bodied members（箭头函数）** 当逻辑简单时

### 3.4 注释规范

- **使用中文注释**（项目使用中文）
- **公共类和方法添加文档注释**:
  ```csharp
  /// <summary>
  /// 自定义树表格控件
  /// </summary>
  public class TreeListControl : Control
  ```
- **行内注释说明复杂逻辑**

### 3.5 类型使用

- **优先使用具体类型**，如 `List<T>` 而非 `IList<T>`
- **使用 var** 当类型明显时
- **颜色定义** 使用 `ColorTranslator.FromHtml("#RRGGBB")`

### 3.6 WinForms 自定义控件

- **继承**: 根据需要继承 `Control`、`Panel` 或现有控件
- **启用双缓冲**:
  ```csharp
  SetStyle(ControlStyles.AllPaintingInWmPaint |
           ControlStyles.DoubleBuffer |
           ControlStyles.ResizeRedraw |
           ControlStyles.UserPaint, true);
  ```
- **属性变更时调用 Invalidate()** 触发重绘
- **控件大小改变时更新子控件布局**

### 3.7 错误处理

- **验证输入参数**，在方法开始处检查
- **使用异常消息**，清晰描述错误原因
- **不吞掉异常**，除非有明确理由

### 3.8 Git 提交规范

- **提交消息使用中文**
- **保持提交原子性**：一个功能一个提交
- **不要提交敏感信息**（如密码、密钥）

## 4. 项目结构

```
TreeListControl/
├── demo/
│   ├── Controls/
│   │   ├── TreeListControl.cs       # 树形表格控件
│   │   ├── CustomTable/            # 表格控件模块
│   │   └── CustomTabControl/       # 标签页控件模块
│   │       ├── CustomTabControl.cs
│   │       ├── TabPage.cs
│   │       └── TabControlDemoForm.cs
│   ├── Form1.cs                    # 主窗体
│   ├── Program.cs                  # 程序入口
│   └── demo.csproj                 # 项目文件
└── docs/
    └── plans/                      # 设计文档
```

## 5. 常用操作

### 5.1 添加新控件

1. 在 `Controls/` 下创建新文件夹
2. 创建控件类文件
3. 在 `demo.csproj` 中添加 `<Compile Include="..."/>`
4. 创建对应的演示窗体（可选）

### 5.2 修改现有控件

1. 定位相关文件（通常在 `Controls/` 下）
2. 修改代码后重新构建
3. 运行演示程序测试

### 5.3 调试

- 使用 Visual Studio 附加到进程调试
- 或在代码中添加 `MessageBox.Show()` 临时调试

## 6. 注意事项

- **始终使用中文**与用户沟通
- **在实现新功能前先了解需求**，不确定时询问用户
- **保持代码简洁**，避免过度设计
- **测试后再提交**，确保功能正常
