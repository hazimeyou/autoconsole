説明動画：https://youtu.be/X2d1SJ6KP4w  
ビルド方法  
net.8でビルド  
Program.csは名前をautoconsoleでコンソールアプリとして（注autoconsoledllを先にビルドしアセンブリにdllを追加する必要があります）  
Class1.csは名前をautoconsoledllでクラスライブラリとして  
autoconsoleがあるところにconfig.txtを追加し操作したいコンソールアプリの場所（.exe等は無し）をフルパスで指定  
操作したいアプリの場所にare.batを追加しその中に操作したいアプリの名前を記入  
autoconsole.exeを実行  
