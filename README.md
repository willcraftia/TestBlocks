TestBlocks
==========
Windows と Xbox 対応で作成しているが、Xbox に関してはビルド確認を行なっているのみであり、Xbox 360 への配置を伴ったデバッグは行なっていない。

また、.NET Compact Framework for Xbox 360 で対応していないクラスも用いており、そのようなクラスを実装する場合にはプロジェクトを分け、Xbox ビルドでは代替実装あるいは Mock 実装として非サポート機能としている。

なお、XNA Game Studio 4.0 における .NET Compact Framework for Xbox 360 のサポート状況については [.NET Compact Framework for Xbox 360 の名前空間、型、およびメンバー](http://msdn.microsoft.com/ja-jp/library/bb203915\(XNAGameStudio.40\).aspx) を参照のこと。

# 依存ライブラリ
ここに含めている各プロジェクトは、幾つかのオープンソース ライブラリに依存している。このため、各プロジェクトをビルドして実行するには、それらライブラリを自身でダウンロードし、各プロジェクトの [参照設定] でライブラリを関連付ける必要がある。

## DotNetZip
[DotNetZip](http://dotnetzip.codeplex.com/)。バージョン v1.9.1.8 、CompactFramework 用 DLL。  

## Json.NET
[Json.NET](http://json.codeplex.com/)。バージョン 4.5 Release 11、NET40 用 DLL。

デフォルトの Xbox プロジェクトが構成するライブラリでは Json.NET を利用できない。このため、Xbox ビルドでは Json.NET を含めず、これに依存するメソッドは Mock 実装としている。

# アセンブリ概要
ここで開発している実際のアセンブリ名および名前空間では、接頭語として Willcraftia.Xna を付けているが、ここではこれを除いた簡略表記で記載する。

## All
全プロジェクトを一つにまとめたソリューション。リファクタリングのために用いている。

## Framework
基礎となるクラスをまとめたライブラリ。  

## Framework.IO.Compression
データ圧縮に関するクラスをまとめたライブラリ。  

### 依存
+ Framework
+ DotNetZip

## Framework.Noise
ノイズ生成に関するクラスをまとめたライブラリ。

### 依存
+ Framework

## Framework.Plugins

プラグインに関するクラスをまとめたライブラリ。Xbox では不要と思われる。

### 依存
+ Framework

## Framework.Serialization.Json
Framework に含まれる ISerializer インタフェースの JSON 実装を含むライブラリ。
Xbox ビルドでは Mock 実装。

### 依存
+ Framework
+ Json.NET

## Framework.Serialization.Xml
Framework に含まれる ISerializer インタフェースの XML 実装を含むライブラリ。Xbox ビルドではメソッドを Mock 実装。

System.Xml.Serialization に含まれるクラスのうち、デフォルトの Xbox プロジェクトが構成するライブラリでは利用できないクラス (XmlSerializer など) に依存している。

### 依存
+ Framework
+ Xbox デフォルト参照にない System.Xml.Serialization 内クラス。

## Blocks
ブロック世界を表現するための基礎となるクラスをまとめたライブラリ。

### 依存
+ Framework
+ Framework.IO.Compression
+ Framework.Serialization.Json
+ Framework.Serialization.Xml
+ DotNetZip
+ Json.NET
