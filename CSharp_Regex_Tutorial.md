# C# .NET Framework 4.0 正则表达式完全指南

> 面向初学者的详细教程

---

## 目录

1. [正则表达式简介](#1-正则表达式简介)
2. [C#中使用正则表达式的准备工作](#2-c中使用正则表达式的准备工作)
3. [正则表达式基础语法](#3-正则表达式基础语法)
4. [Regex类详解](#4-regex类详解)
5. [字符串匹配（IsMatch）](#5-字符串匹配ismatch)
6. [获取匹配内容（Match和Matches）](#6-获取匹配内容match和matches)
7. [字符串替换（Replace）](#7-字符串替换replace)
8. [字符串分割（Split）](#8-字符串分割split)
9. [分组与捕获](#9-分组与捕获)
10. [常用正则表达式模式](#10-常用正则表达式模式)
11. [性能优化建议](#11-性能优化建议)
12. [实战案例](#12-实战案例)
13. [常见问题与解决方案](#13-常见问题与解决方案)

---

## 1. 正则表达式简介

### 1.1 什么是正则表达式？

正则表达式（Regular Expression，简称 Regex 或 RegExp）是一种用于描述字符串模式的强大工具。它使用特定的语法规则来定义搜索模式，可以用于：

- **验证**：检查字符串是否符合特定格式（如邮箱、手机号）
- **搜索**：在文本中查找符合模式的内容
- **提取**：从字符串中提取特定部分
- **替换**：将匹配的内容替换为其他内容
- **分割**：按照模式分割字符串

### 1.2 为什么要学习正则表达式？

```
场景示例：
假设你需要从一段文本中提取所有的电话号码、邮箱地址或URL链接。
如果不使用正则表达式，你可能需要编写大量的字符串处理代码。
而使用正则表达式，只需要一行代码就能完成！
```

---

## 2. C#中使用正则表达式的准备工作

### 2.1 引入命名空间

在C#中使用正则表达式，首先需要引入 `System.Text.RegularExpressions` 命名空间：

```csharp
using System.Text.RegularExpressions;
```

### 2.2 核心类介绍

| 类名 | 说明 |
|------|------|
| `Regex` | 正则表达式的主类，提供所有正则操作方法 |
| `Match` | 表示单个正则表达式匹配的结果 |
| `MatchCollection` | 表示所有匹配结果的集合 |
| `Group` | 表示单个捕获组的结果 |
| `GroupCollection` | 表示所有捕获组的集合 |
| `Capture` | 表示单个子表达式捕获的结果 |

### 2.3 创建Regex对象的两种方式

```csharp
// 方式一：实例化方式
Regex regex = new Regex(@"\d+");
bool isMatch = regex.IsMatch("abc123");

// 方式二：静态方法方式
bool isMatch2 = Regex.IsMatch("abc123", @"\d+");
```

**建议**：如果同一个正则表达式需要多次使用，推荐使用实例化方式，可以提高性能。

---

## 3. 正则表达式基础语法

### 3.1 普通字符

普通字符就是字面意义上的字符，它们匹配自身。

```csharp
// 匹配字符串中的 "hello"
Regex regex = new Regex("hello");
bool result = regex.IsMatch("hello world");  // true
```

### 3.2 元字符（特殊字符）

元字符是具有特殊含义的字符：

| 元字符 | 说明 | 示例 |
|--------|------|------|
| `.` | 匹配除换行符外的任意单个字符 | `a.c` 匹配 "abc"、"adc" |
| `^` | 匹配字符串的开始位置 | `^hello` 匹配以 "hello" 开头 |
| `$` | 匹配字符串的结束位置 | `world$` 匹配以 "world" 结尾 |
| `*` | 匹配前面的元素零次或多次 | `ab*c` 匹配 "ac"、"abc"、"abbc" |
| `+` | 匹配前面的元素一次或多次 | `ab+c` 匹配 "abc"、"abbc"，不匹配 "ac" |
| `?` | 匹配前面的元素零次或一次 | `ab?c` 匹配 "ac"、"abc" |
| `\` | 转义字符 | `\.` 匹配实际的点号 |
| `|` | 或运算符 | `cat|dog` 匹配 "cat" 或 "dog" |
| `()` | 分组 | `(ab)+` 匹配 "ab"、"abab" |
| `[]` | 字符类 | `[abc]` 匹配 "a"、"b" 或 "c" |
| `{}` | 量词 | `a{2,4}` 匹配 "aa"、"aaa"、"aaaa" |

### 3.3 字符类

字符类用方括号 `[]` 表示，匹配其中任意一个字符：

```csharp
// [abc]    - 匹配 a、b 或 c
// [a-z]    - 匹配任意小写字母
// [A-Z]    - 匹配任意大写字母
// [0-9]    - 匹配任意数字
// [a-zA-Z] - 匹配任意字母
// [^abc]   - 匹配除了 a、b、c 之外的任意字符（取反）

Regex regex1 = new Regex("[aeiou]");  // 匹配元音字母
Regex regex2 = new Regex("[^0-9]");   // 匹配非数字字符
```

### 3.4 预定义字符类

C# 正则表达式提供了一些预定义的字符类：

| 预定义类 | 等价写法 | 说明 |
|----------|----------|------|
| `\d` | `[0-9]` | 匹配任意数字 |
| `\D` | `[^0-9]` | 匹配任意非数字 |
| `\w` | `[a-zA-Z0-9_]` | 匹配单词字符（字母、数字、下划线） |
| `\W` | `[^a-zA-Z0-9_]` | 匹配非单词字符 |
| `\s` | `[\t\n\r\f\v ]` | 匹配空白字符（空格、制表符、换行符等） |
| `\S` | `[^\t\n\r\f\v ]` | 匹配非空白字符 |

```csharp
// 示例：匹配数字
Regex digitRegex = new Regex(@"\d+");
Console.WriteLine(digitRegex.IsMatch("abc123"));  // true

// 示例：匹配单词
Regex wordRegex = new Regex(@"\w+");
Match match = wordRegex.Match("Hello World");
Console.WriteLine(match.Value);  // "Hello"
```

### 3.5 量词

量词用于指定前面的元素出现的次数：

| 量词 | 说明 | 示例 |
|------|------|------|
| `*` | 0次或多次 | `a*` 匹配 ""、"a"、"aa"... |
| `+` | 1次或多次 | `a+` 匹配 "a"、"aa"、"aaa"... |
| `?` | 0次或1次 | `a?` 匹配 "" 或 "a" |
| `{n}` | 恰好n次 | `a{3}` 匹配 "aaa" |
| `{n,}` | 至少n次 | `a{2,}` 匹配 "aa"、"aaa"... |
| `{n,m}` | n到m次 | `a{2,4}` 匹配 "aa"、"aaa"、"aaaa" |

```csharp
// 匹配2-4位数字
Regex regex = new Regex(@"\d{2,4}");

Console.WriteLine(regex.IsMatch("1"));      // false（只有1位）
Console.WriteLine(regex.IsMatch("12"));     // true
Console.WriteLine(regex.IsMatch("123"));    // true
Console.WriteLine(regex.IsMatch("1234"));   // true
Console.WriteLine(regex.IsMatch("12345"));  // true（包含2-4位数字的子串）
```

### 3.6 贪婪与非贪婪模式

默认情况下，量词是"贪婪"的，会尽可能多地匹配字符。在量词后加 `?` 可以变为"非贪婪"模式：

```csharp
string html = "<div>内容1</div><div>内容2</div>";

// 贪婪模式：匹配尽可能多的字符
Regex greedyRegex = new Regex(@"<div>.*</div>");
Match greedyMatch = greedyRegex.Match(html);
Console.WriteLine(greedyMatch.Value);
// 输出: <div>内容1</div><div>内容2</div>

// 非贪婪模式：匹配尽可能少的字符
Regex lazyRegex = new Regex(@"<div>.*?</div>");
Match lazyMatch = lazyRegex.Match(html);
Console.WriteLine(lazyMatch.Value);
// 输出: <div>内容1</div>
```

### 3.7 锚点（边界匹配）

| 锚点 | 说明 |
|------|------|
| `^` | 字符串开始（或行开始，在多行模式下） |
| `$` | 字符串结束（或行结束，在多行模式下） |
| `\b` | 单词边界 |
| `\B` | 非单词边界 |

```csharp
// 单词边界示例
string text = "cat catalog scattered";

// 只匹配独立的单词 "cat"
Regex regex = new Regex(@"\bcat\b");
MatchCollection matches = regex.Matches(text);

foreach (Match m in matches)
{
    Console.WriteLine($"找到: '{m.Value}' 位置: {m.Index}");
}
// 输出: 找到: 'cat' 位置: 0
// （catalog 和 scattered 中的 cat 不会被匹配）
```

---

## 4. Regex类详解

### 4.1 构造函数

```csharp
// 基本构造函数
Regex regex1 = new Regex(@"\d+");

// 带选项的构造函数
Regex regex2 = new Regex(@"\d+", RegexOptions.IgnoreCase);

// 带选项和超时的构造函数（防止正则表达式DOS攻击）
Regex regex3 = new Regex(@"\d+", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
```

### 4.2 RegexOptions 枚举

| 选项 | 说明 |
|------|------|
| `None` | 无特殊选项 |
| `IgnoreCase` | 忽略大小写 |
| `Multiline` | 多行模式（^ 和 $ 匹配每行的开始和结束） |
| `Singleline` | 单行模式（. 可以匹配换行符） |
| `ExplicitCapture` | 只捕获显式命名的组 |
| `Compiled` | 编译正则表达式以提高性能 |
| `IgnorePatternWhitespace` | 忽略模式中的空白和注释 |
| `RightToLeft` | 从右向左匹配 |
| `ECMAScript` | 启用ECMAScript兼容模式 |
| `CultureInvariant` | 忽略区域性差异 |

```csharp
// 组合多个选项
Regex regex = new Regex(@"hello", 
    RegexOptions.IgnoreCase | RegexOptions.Multiline);
```

### 4.3 主要方法概览

| 方法 | 说明 | 返回类型 |
|------|------|----------|
| `IsMatch()` | 检查是否存在匹配 | `bool` |
| `Match()` | 获取第一个匹配 | `Match` |
| `Matches()` | 获取所有匹配 | `MatchCollection` |
| `Replace()` | 替换匹配的内容 | `string` |
| `Split()` | 按匹配分割字符串 | `string[]` |

---

## 5. 字符串匹配（IsMatch）

`IsMatch` 方法用于检查字符串是否包含符合正则表达式模式的内容。

### 5.1 基本用法

```csharp
using System;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string input = "我的电话是13812345678";
        
        // 方式一：使用静态方法
        bool hasPhone = Regex.IsMatch(input, @"1[3-9]\d{9}");
        Console.WriteLine($"包含手机号: {hasPhone}");  // true
        
        // 方式二：使用实例方法
        Regex phoneRegex = new Regex(@"1[3-9]\d{9}");
        bool hasPhone2 = phoneRegex.IsMatch(input);
        Console.WriteLine($"包含手机号: {hasPhone2}");  // true
    }
}
```

### 5.2 指定起始位置

```csharp
string input = "abc123def456";
Regex regex = new Regex(@"\d+");

// 从索引0开始检查
bool result1 = regex.IsMatch(input, 0);
Console.WriteLine(result1);  // true

// 从索引7开始检查
bool result2 = regex.IsMatch(input, 7);
Console.WriteLine(result2);  // true（匹配456）

// 从索引10开始检查
bool result3 = regex.IsMatch(input, 10);
Console.WriteLine(result3);  // true（匹配56）
```

### 5.3 完整匹配验证

如果要验证整个字符串是否完全符合模式（而不是包含符合模式的子串），需要使用 `^` 和 `$`：

```csharp
// 验证是否为纯数字字符串
string input1 = "12345";
string input2 = "123abc";

// 错误方式：只检查是否包含数字
Regex wrongRegex = new Regex(@"\d+");
Console.WriteLine(wrongRegex.IsMatch(input1));  // true
Console.WriteLine(wrongRegex.IsMatch(input2));  // true（包含数字123）

// 正确方式：使用^和$确保完整匹配
Regex correctRegex = new Regex(@"^\d+$");
Console.WriteLine(correctRegex.IsMatch(input1));  // true
Console.WriteLine(correctRegex.IsMatch(input2));  // false
```

### 5.4 常用验证示例

```csharp
public class ValidationHelper
{
    /// <summary>
    /// 验证邮箱格式
    /// </summary>
    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
            return false;
            
        string pattern = @"^[\w\.-]+@[\w\.-]+\.\w+$";
        return Regex.IsMatch(email, pattern);
    }
    
    /// <summary>
    /// 验证中国大陆手机号
    /// </summary>
    public static bool IsValidPhoneNumber(string phone)
    {
        if (string.IsNullOrEmpty(phone))
            return false;
            
        string pattern = @"^1[3-9]\d{9}$";
        return Regex.IsMatch(phone, pattern);
    }
    
    /// <summary>
    /// 验证身份证号（18位）
    /// </summary>
    public static bool IsValidIdCard(string idCard)
    {
        if (string.IsNullOrEmpty(idCard))
            return false;
            
        string pattern = @"^\d{17}[\dXx]$";
        return Regex.IsMatch(idCard, pattern);
    }
    
    /// <summary>
    /// 验证IP地址
    /// </summary>
    public static bool IsValidIPAddress(string ip)
    {
        if (string.IsNullOrEmpty(ip))
            return false;
            
        string pattern = @"^((25[0-5]|2[0-4]\d|[01]?\d\d?)\.){3}(25[0-5]|2[0-4]\d|[01]?\d\d?)$";
        return Regex.IsMatch(ip, pattern);
    }
    
    /// <summary>
    /// 验证URL
    /// </summary>
    public static bool IsValidUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
            return false;
            
        string pattern = @"^https?://[\w\-]+(\.[\w\-]+)+([\w\-.,@?^=%&:/~+#]*[\w\-@?^=%&/~+#])?$";
        return Regex.IsMatch(url, pattern, RegexOptions.IgnoreCase);
    }
}

// 使用示例
class Program
{
    static void Main()
    {
        Console.WriteLine(ValidationHelper.IsValidEmail("test@example.com"));  // true
        Console.WriteLine(ValidationHelper.IsValidEmail("invalid-email"));      // false
        
        Console.WriteLine(ValidationHelper.IsValidPhoneNumber("13812345678")); // true
        Console.WriteLine(ValidationHelper.IsValidPhoneNumber("12345678901")); // false
        
        Console.WriteLine(ValidationHelper.IsValidIPAddress("192.168.1.1"));   // true
        Console.WriteLine(ValidationHelper.IsValidIPAddress("256.1.1.1"));     // false
    }
}
```

---

## 6. 获取匹配内容（Match和Matches）

### 6.1 Match 方法 - 获取第一个匹配

```csharp
using System;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string input = "订单号：A001，金额：￥199.50，日期：2024-01-15";
        
        // 提取订单号
        Regex orderRegex = new Regex(@"[A-Z]\d{3}");
        Match orderMatch = orderRegex.Match(input);
        
        if (orderMatch.Success)
        {
            Console.WriteLine($"订单号: {orderMatch.Value}");     // A001
            Console.WriteLine($"起始位置: {orderMatch.Index}");    // 4
            Console.WriteLine($"匹配长度: {orderMatch.Length}");   // 4
        }
        
        // 提取金额
        Regex priceRegex = new Regex(@"￥(\d+\.?\d*)");
        Match priceMatch = priceRegex.Match(input);
        
        if (priceMatch.Success)
        {
            Console.WriteLine($"完整匹配: {priceMatch.Value}");           // ￥199.50
            Console.WriteLine($"金额数值: {priceMatch.Groups[1].Value}"); // 199.50
        }
    }
}
```

### 6.2 Match 对象的属性

| 属性 | 说明 |
|------|------|
| `Success` | 是否匹配成功 |
| `Value` | 匹配到的字符串 |
| `Index` | 匹配在原字符串中的起始位置 |
| `Length` | 匹配字符串的长度 |
| `Groups` | 捕获组集合 |
| `Captures` | 捕获集合 |

### 6.3 Matches 方法 - 获取所有匹配

```csharp
using System;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string input = "联系方式：13812345678、13987654321、15011112222";
        
        // 提取所有手机号
        Regex phoneRegex = new Regex(@"1[3-9]\d{9}");
        MatchCollection matches = phoneRegex.Matches(input);
        
        Console.WriteLine($"找到 {matches.Count} 个手机号：");
        
        foreach (Match match in matches)
        {
            Console.WriteLine($"  手机号: {match.Value}, 位置: {match.Index}");
        }
        
        // 输出:
        // 找到 3 个手机号：
        //   手机号: 13812345678, 位置: 5
        //   手机号: 13987654321, 位置: 17
        //   手机号: 15011112222, 位置: 29
    }
}
```

### 6.4 使用 NextMatch() 逐个获取

```csharp
string input = "a1b2c3d4e5";
Regex regex = new Regex(@"\d");

Match match = regex.Match(input);
while (match.Success)
{
    Console.WriteLine($"找到数字: {match.Value} 位置: {match.Index}");
    match = match.NextMatch();  // 获取下一个匹配
}

// 输出:
// 找到数字: 1 位置: 1
// 找到数字: 2 位置: 3
// 找到数字: 3 位置: 5
// 找到数字: 4 位置: 7
// 找到数字: 5 位置: 9
```

### 6.5 提取HTML/XML中的内容

```csharp
using System;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string html = @"
            <a href='http://www.example1.com'>链接1</a>
            <a href='http://www.example2.com'>链接2</a>
            <a href='http://www.example3.com'>链接3</a>
        ";
        
        // 提取所有链接的URL和文本
        Regex linkRegex = new Regex(@"<a\s+href='([^']+)'>([^<]+)</a>");
        MatchCollection matches = linkRegex.Matches(html);
        
        foreach (Match match in matches)
        {
            string url = match.Groups[1].Value;
            string text = match.Groups[2].Value;
            Console.WriteLine($"文本: {text}, URL: {url}");
        }
        
        // 输出:
        // 文本: 链接1, URL: http://www.example1.com
        // 文本: 链接2, URL: http://www.example2.com
        // 文本: 链接3, URL: http://www.example3.com
    }
}
```

### 6.6 指定起始位置匹配

```csharp
string input = "123abc456def789";
Regex regex = new Regex(@"\d+");

// 从位置6开始匹配
Match match = regex.Match(input, 6);
Console.WriteLine(match.Value);  // 456

// 指定起始位置和长度
Match match2 = regex.Match(input, 6, 5);  // 在索引6-10范围内匹配
Console.WriteLine(match2.Value);  // 456
```

---

## 7. 字符串替换（Replace）

### 7.1 基本替换

```csharp
using System;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string input = "我的电话是13812345678，备用电话是13987654321";
        
        // 将手机号替换为星号（静态方法）
        string result1 = Regex.Replace(input, @"1[3-9]\d{9}", "***********");
        Console.WriteLine(result1);
        // 输出: 我的电话是***********，备用电话是***********
        
        // 使用实例方法
        Regex regex = new Regex(@"1[3-9]\d{9}");
        string result2 = regex.Replace(input, "[手机号已隐藏]");
        Console.WriteLine(result2);
        // 输出: 我的电话是[手机号已隐藏]，备用电话是[手机号已隐藏]
    }
}
```

### 7.2 使用替换引用（$1, $2...）

在替换字符串中可以使用 `$n` 引用捕获组的内容：

```csharp
using System;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        // 示例1：调整日期格式
        string date = "2024-01-15";
        // 将 YYYY-MM-DD 转换为 DD/MM/YYYY
        string newDate = Regex.Replace(date, @"(\d{4})-(\d{2})-(\d{2})", "$3/$2/$1");
        Console.WriteLine(newDate);  // 15/01/2024
        
        // 示例2：手机号部分隐藏
        string phone = "13812345678";
        string maskedPhone = Regex.Replace(phone, @"(\d{3})\d{4}(\d{4})", "$1****$2");
        Console.WriteLine(maskedPhone);  // 138****5678
        
        // 示例3：给数字添加千分位
        string number = "1234567890";
        string formatted = Regex.Replace(number, @"(\d)(?=(\d{3})+$)", "$1,");
        Console.WriteLine(formatted);  // 1,234,567,890
    }
}
```

### 7.3 替换引用说明

| 引用 | 说明 |
|------|------|
| `$1, $2...` | 引用第n个捕获组 |
| `$0` 或 `$&` | 引用整个匹配 |
| `$'` | 匹配之前的文本 |
| `$'` | 匹配之后的文本 |
| `${name}` | 引用命名捕获组 |
| `$$` | 转义，表示字面量的 $ 符号 |

### 7.4 使用命名捕获组

```csharp
string input = "张三的邮箱是zhangsan@example.com";

// 使用命名捕获组
string pattern = @"(?<user>[\w\.]+)@(?<domain>[\w\.]+)";
string replacement = "用户名:${user}, 域名:${domain}";

string result = Regex.Replace(input, pattern, replacement);
Console.WriteLine(result);
// 输出: 张三的邮箱是用户名:zhangsan, 域名:example.com
```

### 7.5 使用 MatchEvaluator 委托进行复杂替换

`MatchEvaluator` 是一个委托，允许你使用自定义函数来决定替换内容：

```csharp
using System;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string input = "商品A价格100元，商品B价格200元，商品C价格300元";
        
        // 使用委托：所有价格打8折
        Regex priceRegex = new Regex(@"\d+(?=元)");
        string result = priceRegex.Replace(input, m =>
        {
            int originalPrice = int.Parse(m.Value);
            int discountedPrice = (int)(originalPrice * 0.8);
            return discountedPrice.ToString();
        });
        
        Console.WriteLine(result);
        // 输出: 商品A价格80元，商品B价格160元，商品C价格240元
    }
}
```

### 7.6 更多 MatchEvaluator 示例

```csharp
using System;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        // 示例1：将英文单词首字母大写
        string text = "hello world, this is a test";
        Regex wordRegex = new Regex(@"\b\w");
        string titleCase = wordRegex.Replace(text, m => m.Value.ToUpper());
        Console.WriteLine(titleCase);
        // 输出: Hello World, This Is A Test
        
        // 示例2：敏感词过滤（替换为等长的*号）
        string content = "这是一段包含敏感词的文本";
        string[] sensitiveWords = { "敏感词" };
        
        foreach (var word in sensitiveWords)
        {
            Regex sensitiveRegex = new Regex(word);
            content = sensitiveRegex.Replace(content, m => new string('*', m.Length));
        }
        Console.WriteLine(content);
        // 输出: 这是一段包含***的文本
        
        // 示例3：HTML实体编码
        string htmlContent = "<script>alert('XSS')</script>";
        Regex htmlRegex = new Regex(@"[<>&""]");
        string encoded = htmlRegex.Replace(htmlContent, m =>
        {
            switch (m.Value)
            {
                case "<": return "&lt;";
                case ">": return "&gt;";
                case "&": return "&amp;";
                case "\"": return "&quot;";
                default: return m.Value;
            }
        });
        Console.WriteLine(encoded);
        // 输出: &lt;script&gt;alert('XSS')&lt;/script&gt;
    }
}
```

### 7.7 限制替换次数

```csharp
string input = "a1b2c3d4e5";
Regex regex = new Regex(@"\d");

// 只替换前2个匹配
string result = regex.Replace(input, "*", 2);
Console.WriteLine(result);  // a*b*c3d4e5

// 从指定位置开始，替换前2个匹配
string result2 = regex.Replace(input, "*", 2, 4);  // 从索引4开始
Console.WriteLine(result2);  // a1b2c*d*e5
```

---

## 8. 字符串分割（Split）

### 8.1 基本分割

```csharp
using System;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string input = "苹果,香蕉;橘子 西瓜\t葡萄";
        
        // 按多种分隔符分割（逗号、分号、空格、制表符）
        string[] fruits = Regex.Split(input, @"[,;\s]+");
        
        foreach (var fruit in fruits)
        {
            Console.WriteLine(fruit);
        }
        // 输出:
        // 苹果
        // 香蕉
        // 橘子
        // 西瓜
        // 葡萄
    }
}
```

### 8.2 按数字分割

```csharp
string input = "abc123def456ghi789jkl";

// 按数字分割
string[] parts = Regex.Split(input, @"\d+");

Console.WriteLine(string.Join(", ", parts));
// 输出: abc, def, ghi, jkl
```

### 8.3 保留分隔符

如果想在分割结果中保留分隔符，需要使用捕获组：

```csharp
string input = "one1two2three3four";

// 不保留分隔符
string[] parts1 = Regex.Split(input, @"\d");
Console.WriteLine("不保留: " + string.Join("|", parts1));
// 输出: 不保留: one|two|three|four

// 保留分隔符（使用捕获组）
string[] parts2 = Regex.Split(input, @"(\d)");
Console.WriteLine("保留: " + string.Join("|", parts2));
// 输出: 保留: one|1|two|2|three|3|four
```

### 8.4 限制分割数量

```csharp
string input = "a,b,c,d,e,f";
Regex regex = new Regex(",");

// 最多分割成3部分
string[] parts = regex.Split(input, 3);

Console.WriteLine(string.Join("|", parts));
// 输出: a|b|c,d,e,f
```

### 8.5 实际应用示例

```csharp
using System;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        // 解析CSV格式的数据（考虑引号内的逗号）
        string csvLine = "张三,\"上海,浦东\",25,程序员";
        
        // 简单分割（这种方式不能正确处理引号内的逗号）
        // 需要更复杂的正则表达式
        string pattern = ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)";
        string[] fields = Regex.Split(csvLine, pattern);
        
        for (int i = 0; i < fields.Length; i++)
        {
            // 移除引号
            string field = fields[i].Trim('"');
            Console.WriteLine($"字段{i + 1}: {field}");
        }
        // 输出:
        // 字段1: 张三
        // 字段2: 上海,浦东
        // 字段3: 25
        // 字段4: 程序员
        
        // 分割驼峰命名
        string camelCase = "getUserNameFromDatabase";
        string[] words = Regex.Split(camelCase, @"(?=[A-Z])");
        Console.WriteLine("\n驼峰分割: " + string.Join(" ", words).ToLower());
        // 输出: 驼峰分割: get user name from database
    }
}
```

---

## 9. 分组与捕获

### 9.1 基本分组

使用圆括号 `()` 创建分组：

```csharp
using System;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string input = "2024-01-15 订单编号:ORD-2024-001";
        
        // 提取日期的各个部分
        Regex dateRegex = new Regex(@"(\d{4})-(\d{2})-(\d{2})");
        Match dateMatch = dateRegex.Match(input);
        
        if (dateMatch.Success)
        {
            Console.WriteLine($"完整匹配: {dateMatch.Groups[0].Value}");  // 2024-01-15
            Console.WriteLine($"年: {dateMatch.Groups[1].Value}");        // 2024
            Console.WriteLine($"月: {dateMatch.Groups[2].Value}");        // 01
            Console.WriteLine($"日: {dateMatch.Groups[3].Value}");        // 15
        }
    }
}
```

### 9.2 命名分组

使用 `(?<name>pattern)` 创建命名分组：

```csharp
using System;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string input = "联系人：张三，电话：13812345678，邮箱：zhangsan@example.com";
        
        string pattern = @"联系人：(?<name>\S+)，电话：(?<phone>\d+)，邮箱：(?<email>\S+)";
        Regex regex = new Regex(pattern);
        Match match = regex.Match(input);
        
        if (match.Success)
        {
            Console.WriteLine($"姓名: {match.Groups["name"].Value}");
            Console.WriteLine($"电话: {match.Groups["phone"].Value}");
            Console.WriteLine($"邮箱: {match.Groups["email"].Value}");
        }
        
        // 输出:
        // 姓名: 张三
        // 电话: 13812345678
        // 邮箱: zhangsan@example.com
    }
}
```

### 9.3 非捕获分组

使用 `(?:pattern)` 创建不捕获的分组（只用于分组，不保存结果）：

```csharp
using System;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string input = "http://www.example.com";
        
        // 使用捕获分组
        Regex regex1 = new Regex(@"(https?)://(.+)");
        Match match1 = regex1.Match(input);
        Console.WriteLine($"捕获分组数: {match1.Groups.Count}");  // 3（包括Group[0]）
        
        // 使用非捕获分组
        Regex regex2 = new Regex(@"(?:https?)://(.+)");
        Match match2 = regex2.Match(input);
        Console.WriteLine($"非捕获分组数: {match2.Groups.Count}");  // 2
        Console.WriteLine($"域名: {match2.Groups[1].Value}");       // www.example.com
    }
}
```

### 9.4 反向引用

在正则表达式中引用前面的捕获组：

```csharp
using System;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        // 匹配重复的单词
        string text = "This is is a test test string";
        
        // \1 引用第一个捕获组
        Regex regex = new Regex(@"\b(\w+)\s+\1\b", RegexOptions.IgnoreCase);
        MatchCollection matches = regex.Matches(text);
        
        Console.WriteLine("找到重复的单词:");
        foreach (Match match in matches)
        {
            Console.WriteLine($"  '{match.Value}'");
        }
        // 输出:
        // 找到重复的单词:
        //   'is is'
        //   'test test'
        
        // 使用命名反向引用
        Regex namedRegex = new Regex(@"\b(?<word>\w+)\s+\k<word>\b", RegexOptions.IgnoreCase);
        MatchCollection namedMatches = namedRegex.Matches(text);
        
        foreach (Match match in namedMatches)
        {
            Console.WriteLine($"重复单词: {match.Groups["word"].Value}");
        }
    }
}
```

### 9.5 遍历所有组

```csharp
using System;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string logLine = "[2024-01-15 10:30:45] [ERROR] 数据库连接失败";
        
        string pattern = @"\[(?<date>\d{4}-\d{2}-\d{2})\s+(?<time>\d{2}:\d{2}:\d{2})\]\s+\[(?<level>\w+)\]\s+(?<message>.+)";
        Regex regex = new Regex(pattern);
        Match match = regex.Match(logLine);
        
        if (match.Success)
        {
            Console.WriteLine("所有捕获组:");
            for (int i = 0; i < match.Groups.Count; i++)
            {
                Group group = match.Groups[i];
                Console.WriteLine($"  组{i}: '{group.Value}'");
            }
            
            Console.WriteLine("\n命名组:");
            string[] groupNames = regex.GetGroupNames();
            foreach (string name in groupNames)
            {
                Console.WriteLine($"  {name}: '{match.Groups[name].Value}'");
            }
        }
    }
}
```

### 9.6 多次捕获（Captures集合）

当一个捕获组在量词中重复匹配时，`Captures` 集合保存所有捕获的内容：

```csharp
using System;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string input = "a1b2c3d4";
        
        // (\d) 会匹配多次
        Regex regex = new Regex(@"([a-z](\d))+");
        Match match = regex.Match(input);
        
        if (match.Success)
        {
            Console.WriteLine($"完整匹配: {match.Value}");  // a1b2c3d4
            
            // Groups[2] 只保存最后一次匹配
            Console.WriteLine($"Groups[2].Value: {match.Groups[2].Value}");  // 4
            
            // Captures 保存所有匹配
            Console.WriteLine("所有数字捕获:");
            foreach (Capture capture in match.Groups[2].Captures)
            {
                Console.WriteLine($"  '{capture.Value}' 位置: {capture.Index}");
            }
            // 输出:
            //   '1' 位置: 1
            //   '2' 位置: 3
            //   '3' 位置: 5
            //   '4' 位置: 7
        }
    }
}
```

---

## 10. 常用正则表达式模式

### 10.1 验证类模式

```csharp
public static class RegexPatterns
{
    // 邮箱
    public const string Email = @"^[\w\.-]+@[\w\.-]+\.\w+$";
    
    // 中国大陆手机号
    public const string MobilePhone = @"^1[3-9]\d{9}$";
    
    // 固定电话（带区号）
    public const string TelePhone = @"^0\d{2,3}-?\d{7,8}$";
    
    // 身份证号（18位）
    public const string IdCard = @"^\d{17}[\dXx]$";
    
    // 邮政编码
    public const string PostalCode = @"^\d{6}$";
    
    // 中文字符
    public const string Chinese = @"^[\u4e00-\u9fa5]+$";
    
    // 用户名（字母开头，允许字母数字下划线，5-16位）
    public const string Username = @"^[a-zA-Z]\w{4,15}$";
    
    // 强密码（必须包含大小写字母和数字，8-16位）
    public const string StrongPassword = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d]{8,16}$";
    
    // IPv4地址
    public const string IPv4 = @"^((25[0-5]|2[0-4]\d|[01]?\d\d?)\.){3}(25[0-5]|2[0-4]\d|[01]?\d\d?)$";
    
    // URL
    public const string Url = @"^https?://[\w\-]+(\.[\w\-]+)+([\w\-.,@?^=%&:/~+#]*[\w\-@?^=%&/~+#])?$";
    
    // 日期（YYYY-MM-DD）
    public const string DateYMD = @"^\d{4}-(0[1-9]|1[0-2])-(0[1-9]|[12]\d|3[01])$";
    
    // 时间（HH:MM:SS）
    public const string Time = @"^([01]\d|2[0-3]):[0-5]\d:[0-5]\d$";
    
    // 正整数
    public const string PositiveInteger = @"^[1-9]\d*$";
    
    // 负整数
    public const string NegativeInteger = @"^-[1-9]\d*$";
    
    // 整数
    public const string Integer = @"^-?[1-9]\d*$|^0$";
    
    // 正浮点数
    public const string PositiveFloat = @"^[1-9]\d*\.\d+$|^0\.\d+$";
    
    // 浮点数
    public const string Float = @"^-?([1-9]\d*\.\d+|0\.\d+|[1-9]\d*|0)$";
    
    // 金额（保留两位小数）
    public const string Money = @"^(0|[1-9]\d*)(\.\d{1,2})?$";
    
    // 车牌号（新能源+普通）
    public const string LicensePlate = @"^[京津沪渝冀豫云辽黑湘皖鲁新苏浙赣鄂桂甘晋蒙陕吉闽贵粤青藏川宁琼使领][A-Z][A-HJ-NP-Z0-9]{4,5}[A-HJ-NP-Z0-9挂学警港澳]$";
}
```

### 10.2 提取类模式

```csharp
public static class ExtractPatterns
{
    // 提取所有数字
    public const string Numbers = @"\d+";
    
    // 提取所有浮点数
    public const string FloatNumbers = @"-?\d+\.?\d*";
    
    // 提取中文
    public const string ChineseChars = @"[\u4e00-\u9fa5]+";
    
    // 提取邮箱
    public const string Emails = @"[\w\.-]+@[\w\.-]+\.\w+";
    
    // 提取URL
    public const string Urls = @"https?://[\w\-]+(\.[\w\-]+)+([\w\-.,@?^=%&:/~+#]*)?";
    
    // 提取手机号
    public const string MobilePhones = @"1[3-9]\d{9}";
    
    // 提取HTML标签
    public const string HtmlTags = @"<[^>]+>";
    
    // 提取HTML标签内容
    public const string HtmlTagContent = @"<(\w+)[^>]*>(.*?)</\1>";
    
    // 提取括号内容（包括括号）
    public const string Parentheses = @"\([^)]*\)";
    
    // 提取引号内容（包括引号）
    public const string Quoted = @"""[^""]*""|'[^']*'";
    
    // 提取IP地址
    public const string IpAddresses = @"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}";
}
```

### 10.3 使用示例

```csharp
using System;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        // 验证示例
        Console.WriteLine("=== 验证示例 ===");
        
        string email = "test@example.com";
        Console.WriteLine($"邮箱验证 '{email}': {Regex.IsMatch(email, RegexPatterns.Email)}");
        
        string phone = "13812345678";
        Console.WriteLine($"手机号验证 '{phone}': {Regex.IsMatch(phone, RegexPatterns.MobilePhone)}");
        
        string password = "Abc12345";
        Console.WriteLine($"强密码验证 '{password}': {Regex.IsMatch(password, RegexPatterns.StrongPassword)}");
        
        // 提取示例
        Console.WriteLine("\n=== 提取示例 ===");
        
        string text = "联系方式：13812345678，邮箱test@example.com，金额100.50元";
        
        // 提取手机号
        Match phoneMatch = Regex.Match(text, ExtractPatterns.MobilePhones);
        Console.WriteLine($"提取手机号: {phoneMatch.Value}");
        
        // 提取邮箱
        Match emailMatch = Regex.Match(text, ExtractPatterns.Emails);
        Console.WriteLine($"提取邮箱: {emailMatch.Value}");
        
        // 提取浮点数
        MatchCollection numbers = Regex.Matches(text, ExtractPatterns.FloatNumbers);
        Console.Write("提取数字: ");
        foreach (Match m in numbers)
        {
            Console.Write($"{m.Value} ");
        }
    }
}
```

---

## 11. 性能优化建议

### 11.1 使用编译选项

对于频繁使用的正则表达式，使用 `RegexOptions.Compiled` 可以提高性能：

```csharp
// 非编译方式（每次都要解析模式）
Regex regex1 = new Regex(@"\d+");

// 编译方式（首次使用较慢，但后续调用更快）
Regex regex2 = new Regex(@"\d+", RegexOptions.Compiled);

// 适用场景：
// - 会被多次使用的正则表达式
// - 处理大量数据时
// - 性能敏感的应用
```

### 11.2 重用 Regex 对象

```csharp
// 不推荐：每次调用都创建新对象
public bool ValidateEmail(string email)
{
    return Regex.IsMatch(email, @"^[\w\.-]+@[\w\.-]+\.\w+$");
}

// 推荐：重用静态 Regex 对象
private static readonly Regex EmailRegex = new Regex(
    @"^[\w\.-]+@[\w\.-]+\.\w+$", 
    RegexOptions.Compiled
);

public bool ValidateEmailOptimized(string email)
{
    return EmailRegex.IsMatch(email);
}
```

### 11.3 避免回溯陷阱

某些正则表达式模式可能导致严重的性能问题（指数级回溯）：

```csharp
// 危险模式示例（可能导致"灾难性回溯"）
// 不推荐
string badPattern = @"(a+)+b";  // 对于 "aaaaaaaaaaaaaaaaaaaaac" 这样的输入会很慢

// 推荐：使用更精确的模式
string goodPattern = @"a+b";

// 使用超时保护
try
{
    Regex regex = new Regex(
        @"(a+)+b", 
        RegexOptions.None, 
        TimeSpan.FromSeconds(1)  // 设置1秒超时
    );
    regex.IsMatch("aaaaaaaaaaaaaaaaaaaaac");
}
catch (RegexMatchTimeoutException)
{
    Console.WriteLine("正则表达式匹配超时");
}
```

### 11.4 使用正确的锚点

```csharp
string input = "这是一段很长很长的文本...";

// 不推荐：没有锚点，会扫描整个字符串
Regex regex1 = new Regex(@"^\d+");  // 实际上总是从开头匹配

// 推荐：如果只需要检查开头，使用更明确的方式
if (input.Length > 0 && char.IsDigit(input[0]))
{
    // 处理以数字开头的情况
}
```

### 11.5 预编译正则表达式（适用于极端性能需求）

```csharp
// 在程序启动时预热正则表达式
public static class RegexCache
{
    public static readonly Regex Email;
    public static readonly Regex Phone;
    public static readonly Regex Url;
    
    static RegexCache()
    {
        Email = new Regex(@"^[\w\.-]+@[\w\.-]+\.\w+$", RegexOptions.Compiled);
        Phone = new Regex(@"^1[3-9]\d{9}$", RegexOptions.Compiled);
        Url = new Regex(@"^https?://[\w\-]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        
        // 预热（可选）
        Email.IsMatch("");
        Phone.IsMatch("");
        Url.IsMatch("");
    }
}
```

---

## 12. 实战案例

### 12.1 日志解析器

```csharp
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public string Level { get; set; }
    public string Logger { get; set; }
    public string Message { get; set; }
}

public class LogParser
{
    // 日志格式: [2024-01-15 10:30:45] [INFO] [MyApp.Services.UserService] - 用户登录成功
    private static readonly Regex LogRegex = new Regex(
        @"\[(?<date>\d{4}-\d{2}-\d{2})\s+(?<time>\d{2}:\d{2}:\d{2})\]\s+\[(?<level>\w+)\]\s+\[(?<logger>[^\]]+)\]\s+-\s+(?<message>.+)",
        RegexOptions.Compiled
    );
    
    public static LogEntry Parse(string logLine)
    {
        Match match = LogRegex.Match(logLine);
        
        if (!match.Success)
            return null;
        
        return new LogEntry
        {
            Timestamp = DateTime.Parse($"{match.Groups["date"].Value} {match.Groups["time"].Value}"),
            Level = match.Groups["level"].Value,
            Logger = match.Groups["logger"].Value,
            Message = match.Groups["message"].Value
        };
    }
    
    public static List<LogEntry> ParseMultiple(string[] logLines)
    {
        var entries = new List<LogEntry>();
        
        foreach (string line in logLines)
        {
            var entry = Parse(line);
            if (entry != null)
                entries.Add(entry);
        }
        
        return entries;
    }
}

// 使用示例
class Program
{
    static void Main()
    {
        string[] logs = 
        {
            "[2024-01-15 10:30:45] [INFO] [MyApp.Services.UserService] - 用户登录成功",
            "[2024-01-15 10:30:46] [ERROR] [MyApp.Data.Repository] - 数据库连接失败",
            "[2024-01-15 10:30:47] [DEBUG] [MyApp.Controllers.HomeController] - 请求处理完成"
        };
        
        var entries = LogParser.ParseMultiple(logs);
        
        foreach (var entry in entries)
        {
            Console.WriteLine($"时间: {entry.Timestamp}");
            Console.WriteLine($"级别: {entry.Level}");
            Console.WriteLine($"来源: {entry.Logger}");
            Console.WriteLine($"消息: {entry.Message}");
            Console.WriteLine("---");
        }
    }
}
```

### 12.2 数据清洗工具

```csharp
using System;
using System.Text.RegularExpressions;

public class DataCleaner
{
    /// <summary>
    /// 清理HTML标签
    /// </summary>
    public static string StripHtml(string html)
    {
        if (string.IsNullOrEmpty(html))
            return string.Empty;
        
        // 移除所有HTML标签
        string result = Regex.Replace(html, @"<[^>]+>", "");
        
        // 解码常见HTML实体
        result = result.Replace("&nbsp;", " ")
                       .Replace("&amp;", "&")
                       .Replace("&lt;", "<")
                       .Replace("&gt;", ">")
                       .Replace("&quot;", "\"");
        
        return result.Trim();
    }
    
    /// <summary>
    /// 清理多余空白
    /// </summary>
    public static string NormalizeWhitespace(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;
        
        // 将多个连续空白替换为单个空格
        return Regex.Replace(text, @"\s+", " ").Trim();
    }
    
    /// <summary>
    /// 提取纯数字
    /// </summary>
    public static string ExtractDigits(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;
        
        return Regex.Replace(text, @"[^\d]", "");
    }
    
    /// <summary>
    /// 格式化手机号（加密中间4位）
    /// </summary>
    public static string MaskPhone(string phone)
    {
        return Regex.Replace(phone, @"(\d{3})\d{4}(\d{4})", "$1****$2");
    }
    
    /// <summary>
    /// 格式化身份证号（加密出生日期）
    /// </summary>
    public static string MaskIdCard(string idCard)
    {
        return Regex.Replace(idCard, @"(\d{6})\d{8}(\d{3}[\dXx])", "$1********$2");
    }
    
    /// <summary>
    /// 清理特殊字符，只保留中文、英文、数字
    /// </summary>
    public static string CleanSpecialChars(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;
        
        return Regex.Replace(text, @"[^\u4e00-\u9fa5a-zA-Z0-9\s]", "");
    }
    
    /// <summary>
    /// 驼峰转下划线
    /// </summary>
    public static string CamelToUnderscore(string camelCase)
    {
        return Regex.Replace(camelCase, @"([a-z])([A-Z])", "$1_$2").ToLower();
    }
    
    /// <summary>
    /// 下划线转驼峰
    /// </summary>
    public static string UnderscoreToCamel(string underscore)
    {
        return Regex.Replace(underscore, @"_(\w)", m => m.Groups[1].Value.ToUpper());
    }
}

// 使用示例
class Program
{
    static void Main()
    {
        // HTML清理
        string html = "<p>Hello <b>World</b>!</p>&nbsp;&nbsp;";
        Console.WriteLine($"HTML清理: '{DataCleaner.StripHtml(html)}'");
        // 输出: HTML清理: 'Hello World!'
        
        // 空白处理
        string messy = "  Hello    World   \n\t Test  ";
        Console.WriteLine($"空白处理: '{DataCleaner.NormalizeWhitespace(messy)}'");
        // 输出: 空白处理: 'Hello World Test'
        
        // 手机号脱敏
        string phone = "13812345678";
        Console.WriteLine($"手机脱敏: {DataCleaner.MaskPhone(phone)}");
        // 输出: 手机脱敏: 138****5678
        
        // 驼峰转换
        string camelCase = "getUserNameById";
        Console.WriteLine($"驼峰转下划线: {DataCleaner.CamelToUnderscore(camelCase)}");
        // 输出: 驼峰转下划线: get_user_name_by_id
        
        string underscore = "get_user_name_by_id";
        Console.WriteLine($"下划线转驼峰: {DataCleaner.UnderscoreToCamel(underscore)}");
        // 输出: 下划线转驼峰: getUserNameById
    }
}
```

### 12.3 配置文件解析器

```csharp
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class IniParser
{
    private Dictionary<string, Dictionary<string, string>> _sections;
    
    // 匹配节名: [SectionName]
    private static readonly Regex SectionRegex = new Regex(
        @"^\s*\[(?<name>[^\]]+)\]\s*$",
        RegexOptions.Compiled
    );
    
    // 匹配键值对: key = value 或 key=value
    private static readonly Regex KeyValueRegex = new Regex(
        @"^\s*(?<key>[^=]+?)\s*=\s*(?<value>.*)$",
        RegexOptions.Compiled
    );
    
    // 匹配注释: ; comment 或 # comment
    private static readonly Regex CommentRegex = new Regex(
        @"^\s*[;#]",
        RegexOptions.Compiled
    );
    
    public IniParser()
    {
        _sections = new Dictionary<string, Dictionary<string, string>>(
            StringComparer.OrdinalIgnoreCase
        );
    }
    
    public void Parse(string content)
    {
        string[] lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        string currentSection = "Default";
        
        foreach (string line in lines)
        {
            // 跳过空行和注释
            if (string.IsNullOrWhiteSpace(line) || CommentRegex.IsMatch(line))
                continue;
            
            // 检查是否是节名
            Match sectionMatch = SectionRegex.Match(line);
            if (sectionMatch.Success)
            {
                currentSection = sectionMatch.Groups["name"].Value;
                if (!_sections.ContainsKey(currentSection))
                {
                    _sections[currentSection] = new Dictionary<string, string>(
                        StringComparer.OrdinalIgnoreCase
                    );
                }
                continue;
            }
            
            // 检查是否是键值对
            Match kvMatch = KeyValueRegex.Match(line);
            if (kvMatch.Success)
            {
                string key = kvMatch.Groups["key"].Value.Trim();
                string value = kvMatch.Groups["value"].Value.Trim();
                
                if (!_sections.ContainsKey(currentSection))
                {
                    _sections[currentSection] = new Dictionary<string, string>(
                        StringComparer.OrdinalIgnoreCase
                    );
                }
                
                _sections[currentSection][key] = value;
            }
        }
    }
    
    public string GetValue(string section, string key, string defaultValue = null)
    {
        if (_sections.TryGetValue(section, out var sectionData))
        {
            if (sectionData.TryGetValue(key, out var value))
            {
                return value;
            }
        }
        return defaultValue;
    }
    
    public Dictionary<string, string> GetSection(string section)
    {
        return _sections.TryGetValue(section, out var sectionData) 
            ? new Dictionary<string, string>(sectionData) 
            : new Dictionary<string, string>();
    }
}

// 使用示例
class Program
{
    static void Main()
    {
        string iniContent = @"
; 这是一个配置文件示例
[Database]
Server = localhost
Port = 3306
Database = myapp
Username = root
Password = secret123

[Application]
Name = MyApp
Version = 1.0.0
Debug = true

# 日志配置
[Logging]
Level = Debug
Path = C:\Logs\myapp.log
";
        
        var parser = new IniParser();
        parser.Parse(iniContent);
        
        // 获取单个值
        string server = parser.GetValue("Database", "Server");
        Console.WriteLine($"数据库服务器: {server}");
        
        // 获取整个节
        var dbConfig = parser.GetSection("Database");
        Console.WriteLine("\n数据库配置:");
        foreach (var kvp in dbConfig)
        {
            Console.WriteLine($"  {kvp.Key} = {kvp.Value}");
        }
    }
}
```

### 12.4 URL参数解析器

```csharp
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class UrlParser
{
    public string Protocol { get; private set; }
    public string Host { get; private set; }
    public int Port { get; private set; }
    public string Path { get; private set; }
    public string Query { get; private set; }
    public string Fragment { get; private set; }
    public Dictionary<string, string> QueryParams { get; private set; }
    
    // 完整URL匹配模式
    private static readonly Regex UrlRegex = new Regex(
        @"^(?<protocol>https?):\/\/(?<host>[\w\.-]+)(:(?<port>\d+))?(?<path>\/[^\?#]*)?(\?(?<query>[^#]*))?(#(?<fragment>.*))?$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );
    
    // 查询参数匹配模式
    private static readonly Regex QueryParamRegex = new Regex(
        @"(?<key>[^&=]+)=(?<value>[^&]*)",
        RegexOptions.Compiled
    );
    
    public bool Parse(string url)
    {
        Match match = UrlRegex.Match(url);
        
        if (!match.Success)
            return false;
        
        Protocol = match.Groups["protocol"].Value;
        Host = match.Groups["host"].Value;
        Port = match.Groups["port"].Success ? int.Parse(match.Groups["port"].Value) : (Protocol == "https" ? 443 : 80);
        Path = match.Groups["path"].Success ? match.Groups["path"].Value : "/";
        Query = match.Groups["query"].Value;
        Fragment = match.Groups["fragment"].Value;
        
        // 解析查询参数
        QueryParams = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        
        if (!string.IsNullOrEmpty(Query))
        {
            MatchCollection paramMatches = QueryParamRegex.Matches(Query);
            foreach (Match paramMatch in paramMatches)
            {
                string key = Uri.UnescapeDataString(paramMatch.Groups["key"].Value);
                string value = Uri.UnescapeDataString(paramMatch.Groups["value"].Value);
                QueryParams[key] = value;
            }
        }
        
        return true;
    }
    
    public string GetParam(string key, string defaultValue = null)
    {
        return QueryParams.TryGetValue(key, out var value) ? value : defaultValue;
    }
}

// 使用示例
class Program
{
    static void Main()
    {
        var parser = new UrlParser();
        
        string url = "https://www.example.com:8080/api/users?id=123&name=%E5%BC%A0%E4%B8%89&active=true#section1";
        
        if (parser.Parse(url))
        {
            Console.WriteLine($"协议: {parser.Protocol}");
            Console.WriteLine($"主机: {parser.Host}");
            Console.WriteLine($"端口: {parser.Port}");
            Console.WriteLine($"路径: {parser.Path}");
            Console.WriteLine($"查询字符串: {parser.Query}");
            Console.WriteLine($"锚点: {parser.Fragment}");
            
            Console.WriteLine("\n查询参数:");
            foreach (var param in parser.QueryParams)
            {
                Console.WriteLine($"  {param.Key} = {param.Value}");
            }
            
            Console.WriteLine($"\n获取单个参数 name: {parser.GetParam("name")}");
        }
        
        // 输出:
        // 协议: https
        // 主机: www.example.com
        // 端口: 8080
        // 路径: /api/users
        // 查询字符串: id=123&name=%E5%BC%A0%E4%B8%89&active=true
        // 锚点: section1
        //
        // 查询参数:
        //   id = 123
        //   name = 张三
        //   active = true
        //
        // 获取单个参数 name: 张三
    }
}
```

### 12.5 模板引擎（简单实现）

```csharp
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class SimpleTemplateEngine
{
    // 匹配变量占位符: {{variableName}}
    private static readonly Regex VariableRegex = new Regex(
        @"\{\{(?<name>\w+)\}\}",
        RegexOptions.Compiled
    );
    
    // 匹配条件语句: {{#if condition}}content{{/if}}
    private static readonly Regex ConditionRegex = new Regex(
        @"\{\{#if\s+(?<condition>\w+)\}\}(?<content>.*?)\{\{/if\}\}",
        RegexOptions.Compiled | RegexOptions.Singleline
    );
    
    // 匹配循环语句: {{#each items}}content{{/each}}
    private static readonly Regex LoopRegex = new Regex(
        @"\{\{#each\s+(?<collection>\w+)\}\}(?<content>.*?)\{\{/each\}\}",
        RegexOptions.Compiled | RegexOptions.Singleline
    );
    
    private Dictionary<string, object> _data;
    
    public SimpleTemplateEngine()
    {
        _data = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
    }
    
    public void SetValue(string key, object value)
    {
        _data[key] = value;
    }
    
    public string Render(string template)
    {
        string result = template;
        
        // 处理条件语句
        result = ConditionRegex.Replace(result, m =>
        {
            string condition = m.Groups["condition"].Value;
            string content = m.Groups["content"].Value;
            
            if (_data.TryGetValue(condition, out var value))
            {
                bool isTrue = value is bool b ? b : 
                              value is string s ? !string.IsNullOrEmpty(s) :
                              value != null;
                
                return isTrue ? content : string.Empty;
            }
            return string.Empty;
        });
        
        // 处理循环语句
        result = LoopRegex.Replace(result, m =>
        {
            string collection = m.Groups["collection"].Value;
            string content = m.Groups["content"].Value;
            
            if (_data.TryGetValue(collection, out var value) && value is IEnumerable<Dictionary<string, object>> items)
            {
                var output = new System.Text.StringBuilder();
                foreach (var item in items)
                {
                    string itemContent = content;
                    foreach (var kvp in item)
                    {
                        itemContent = itemContent.Replace($"{{{{this.{kvp.Key}}}}}", kvp.Value?.ToString() ?? "");
                    }
                    output.Append(itemContent);
                }
                return output.ToString();
            }
            return string.Empty;
        });
        
        // 处理变量替换
        result = VariableRegex.Replace(result, m =>
        {
            string name = m.Groups["name"].Value;
            return _data.TryGetValue(name, out var value) ? value?.ToString() ?? "" : "";
        });
        
        return result;
    }
}

// 使用示例
class Program
{
    static void Main()
    {
        var engine = new SimpleTemplateEngine();
        
        // 设置数据
        engine.SetValue("title", "欢迎页面");
        engine.SetValue("username", "张三");
        engine.SetValue("isLoggedIn", true);
        engine.SetValue("showAdmin", false);
        
        // 设置列表数据
        var products = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "name", "苹果" }, { "price", "5.00" } },
            new Dictionary<string, object> { { "name", "香蕉" }, { "price", "3.00" } },
            new Dictionary<string, object> { { "name", "橘子" }, { "price", "4.00" } }
        };
        engine.SetValue("products", products);
        
        // 模板
        string template = @"
=== {{title}} ===

{{#if isLoggedIn}}
您好，{{username}}！
{{/if}}

{{#if showAdmin}}
[管理员面板]
{{/if}}

商品列表：
{{#each products}}
- {{this.name}}: ￥{{this.price}}
{{/each}}

感谢使用！
";
        
        string result = engine.Render(template);
        Console.WriteLine(result);
        
        // 输出:
        // === 欢迎页面 ===
        //
        // 您好，张三！
        //
        //
        // 商品列表：
        // - 苹果: ￥5.00
        // - 香蕉: ￥3.00
        // - 橘子: ￥4.00
        //
        // 感谢使用！
    }
}
```

---

## 13. 常见问题与解决方案

### 13.1 转义字符问题

**问题**：为什么我的正则表达式不工作？

```csharp
// 错误：反斜杠被C#转义了
string pattern1 = "\d+";  // 实际上是 "d+"

// 正确方式1：使用双反斜杠
string pattern2 = "\\d+";

// 正确方式2：使用@字符串（推荐）
string pattern3 = @"\d+";
```

### 13.2 匹配特殊字符

**问题**：如何匹配正则表达式的元字符？

```csharp
// 匹配这些特殊字符需要转义: . * + ? ^ $ { } [ ] ( ) | \

string text = "价格：$100.00";

// 匹配美元符号和点号
Regex regex = new Regex(@"\$\d+\.\d+");
Match match = regex.Match(text);
Console.WriteLine(match.Value);  // $100.00

// 使用 Regex.Escape() 转义用户输入
string userInput = "test.file";
string escapedInput = Regex.Escape(userInput);  // test\.file
```

### 13.3 贪婪匹配问题

**问题**：为什么匹配了整个字符串而不是最短匹配？

```csharp
string html = "<div>内容1</div><div>内容2</div>";

// 贪婪匹配（默认）- 匹配尽可能多
Regex greedyRegex = new Regex(@"<div>.*</div>");
Console.WriteLine(greedyRegex.Match(html).Value);
// 输出: <div>内容1</div><div>内容2</div>

// 非贪婪匹配 - 匹配尽可能少
Regex lazyRegex = new Regex(@"<div>.*?</div>");
Console.WriteLine(lazyRegex.Match(html).Value);
// 输出: <div>内容1</div>
```

### 13.4 多行匹配问题

**问题**：为什么 `^` 和 `$` 只匹配整个字符串的开始和结束？

```csharp
string text = "第一行\n第二行\n第三行";

// 默认模式：^ 和 $ 只匹配整个字符串
Regex regex1 = new Regex(@"^第");
Console.WriteLine(regex1.Matches(text).Count);  // 1

// 多行模式：^ 和 $ 匹配每行的开始和结束
Regex regex2 = new Regex(@"^第", RegexOptions.Multiline);
Console.WriteLine(regex2.Matches(text).Count);  // 3
```

### 13.5 点号匹配换行问题

**问题**：为什么 `.` 不能匹配换行符？

```csharp
string text = "第一行\n第二行";

// 默认模式：. 不匹配换行符
Regex regex1 = new Regex(@".+");
Match match1 = regex1.Match(text);
Console.WriteLine(match1.Value);  // 第一行

// 单行模式：. 可以匹配换行符
Regex regex2 = new Regex(@".+", RegexOptions.Singleline);
Match match2 = regex2.Match(text);
Console.WriteLine(match2.Value);  // 第一行\n第二行
```

### 13.6 性能问题

**问题**：正则表达式执行很慢或卡住

```csharp
// 问题模式：可能导致回溯爆炸
string badPattern = @"(a+)+b";

// 解决方案1：使用超时
try
{
    Regex regex = new Regex(badPattern, RegexOptions.None, TimeSpan.FromSeconds(1));
    regex.IsMatch("aaaaaaaaaaaaaaaaaaaaac");
}
catch (RegexMatchTimeoutException)
{
    Console.WriteLine("匹配超时");
}

// 解决方案2：优化正则表达式
string goodPattern = @"a+b";

// 解决方案3：使用更具体的模式
// 不推荐: .*
// 推荐: [^<]* （如果要匹配到下一个<之前的内容）
```

### 13.7 大小写敏感问题

```csharp
string text = "Hello World HELLO world";

// 默认：大小写敏感
Regex regex1 = new Regex(@"hello");
Console.WriteLine(regex1.Matches(text).Count);  // 0

// 忽略大小写
Regex regex2 = new Regex(@"hello", RegexOptions.IgnoreCase);
Console.WriteLine(regex2.Matches(text).Count);  // 2
```

### 13.8 空白字符处理

```csharp
string text = "  Hello  World  ";

// 匹配任意空白（空格、制表符、换行符等）
Regex whitespaceRegex = new Regex(@"\s+");
string result = whitespaceRegex.Replace(text, " ");
Console.WriteLine($"'{result.Trim()}'");  // 'Hello World'

// 使用 IgnorePatternWhitespace 选项编写可读的正则表达式
Regex readableRegex = new Regex(@"
    (\d{4})   # 年
    -         # 分隔符
    (\d{2})   # 月
    -         # 分隔符
    (\d{2})   # 日
", RegexOptions.IgnorePatternWhitespace);

Match match = readableRegex.Match("2024-01-15");
Console.WriteLine($"{match.Groups[1].Value}年{match.Groups[2].Value}月{match.Groups[3].Value}日");
```

---

## 附录：正则表达式速查表

### 字符类

| 模式 | 说明 |
|------|------|
| `.` | 任意字符（除换行符） |
| `\d` | 数字 [0-9] |
| `\D` | 非数字 |
| `\w` | 单词字符 [a-zA-Z0-9_] |
| `\W` | 非单词字符 |
| `\s` | 空白字符 |
| `\S` | 非空白字符 |
| `[abc]` | a、b或c |
| `[^abc]` | 非a、b、c |
| `[a-z]` | a到z |

### 量词

| 模式 | 说明 |
|------|------|
| `*` | 0次或多次 |
| `+` | 1次或多次 |
| `?` | 0次或1次 |
| `{n}` | 恰好n次 |
| `{n,}` | 至少n次 |
| `{n,m}` | n到m次 |
| `*?` | 非贪婪0次或多次 |
| `+?` | 非贪婪1次或多次 |

### 锚点

| 模式 | 说明 |
|------|------|
| `^` | 字符串/行开始 |
| `$` | 字符串/行结束 |
| `\b` | 单词边界 |
| `\B` | 非单词边界 |

### 分组

| 模式 | 说明 |
|------|------|
| `(...)` | 捕获分组 |
| `(?:...)` | 非捕获分组 |
| `(?<name>...)` | 命名分组 |
| `\1, \2` | 反向引用 |
| `\k<name>` | 命名反向引用 |

### 断言

| 模式 | 说明 |
|------|------|
| `(?=...)` | 正向先行断言 |
| `(?!...)` | 负向先行断言 |
| `(?<=...)` | 正向后行断言 |
| `(?<!...)` | 负向后行断言 |

---

## 结语

正则表达式是一个非常强大的工具，掌握它可以让你在字符串处理方面事半功倍。本教程涵盖了 C# .NET Framework 4.0 中正则表达式的所有核心内容，包括：

1. **基础语法**：元字符、字符类、量词、锚点
2. **核心操作**：匹配、获取、替换、分割
3. **高级特性**：分组、捕获、命名组、反向引用
4. **性能优化**：编译选项、避免回溯陷阱
5. **实战应用**：日志解析、数据清洗、模板引擎等

建议学习路径：
1. 先掌握基础语法和常用模式
2. 多练习 IsMatch、Match、Replace 等基本操作
3. 逐步学习分组和高级特性
4. 在实际项目中应用和积累经验

祝你学习顺利！

---

*文档版本：1.0*  
*适用版本：C# .NET Framework 4.0+*  
*最后更新：2024年*
