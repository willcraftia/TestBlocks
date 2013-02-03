TestBlocks
==========

# 注意

幾つかのプロジェクトは、.NET Compact Framework for Xbox 360 で対応していないクラスを使用している。

XNA Game Studio 4.0 における .NET Compact Framework for Xbox 360 のサポート状況については [.NET Compact Framework for Xbox 360 の名前空間、型、およびメンバー](http://msdn.microsoft.com/ja-jp/library/bb203915\(XNAGameStudio.40\).aspx) を参照のこと。

# 依存ライブラリ
幾つかのプロジェクトは、オープンソース ライブラリに依存している。このため、それらプロジェクトのビルドでは、依存するオープン ソース ライブラリを自身でダウンロードし、各プロジェクトの [参照設定] で関連付ける必要がある。

## DotNetZip
[DotNetZip](http://dotnetzip.codeplex.com/)。バージョン v1.9.1.8 、CompactFramework 用 DLL。  

## Json.NET
[Json.NET](http://json.codeplex.com/)。バージョン 4.5 Release 11、NET40 用 DLL。

# プロジェクト分割方針

様々なアプリケーションで用いることができると考えられるクラスは、Framework プロジェクトへまとめる。
ただし、他のオープンソース ライブラリ、あるいは、Windows でのみ利用できるクラスを用いるようなクラスがある場合、それらを専用のプロジェクトへまとめ、Framework アセンブリとは分離させる。

また、ブロック世界の表現に用いるクラスは Blocks プロジェクトへまとめる。
ただし、Framework プロジェクトと同様の方針により、必要に応じて専用のプロジェクトへまとめる。

# プロジェクト概要
実際のアセンブリ名および名前空間では接頭語を Willcraftia.Xna としているが、ここではこれを除いた簡略表記で記載する。

## All
全プロジェクトを一つにまとめたソリューション。

## Framework
基礎となるクラスをまとめたライブラリ。  

## Framework.IO.Compression
データ圧縮に関するクラスをまとめたライブラリ。  

### 依存
+ Framework
+ DotNetZip

## Framework.Serialization.Json
Framework に含まれる ISerializer インタフェースの JSON 実装を含むライブラリ。
Xbox ビルドでは Mock 実装。

### 依存
+ Framework
+ Json.NET

## Blocks
ブロック世界を表現するための基礎となるクラスをまとめたライブラリ。

### 依存
+ Framework
+ Framework.IO.Compression
+ Framework.Serialization.Json
+ DotNetZip
+ Json.NET
