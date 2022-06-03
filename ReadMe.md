# DummyFile

## Purpose

DummyFile lets you easily create a number of dummy files for various testing scenarios. The files are generated from random plain text *(aka 'human readable')* characters. Per default, the file's contents is generated as one 'big blob'. However, you can specify to add line feeds (actually line feed plus carriage return) to simulate 'proper text files'.

This is a .NET Core application/assembly, so it needs to be invoked with the [```dotnet```](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet) command. It runs on both Windows and Linux platforms.

**Please note:** It expects the typical command line parameter delimiter for each platform. ```/``` on Windows, ```--``` on Linux systems. The samples below use Windows-style delimiters.

---

## Usage

```dotnet DummyFile.dll /n=<No. of files> /s=<file size> [/f=<folder to create files in>] [/lf] [/ll=<No. of characters per line>] [/fe=<file extension>]```

E.g.

```dotnet DummyFile.dll /n=10 /s=12MB```

- Create 10 files (in the current folder) with a size of 12MB each, do not add line feed(s).  

```dotnet DummyFile.dll /n=10 /s=12MB /f=c:\temp```

- Create 10 files in the folder c:\temp with a size of 12MB each, do not add line feed(s).  

```dotnet DummyFile.dll /n=10 /s=12MB /f=c:\temp /lf```

- Create 10 files in the folder c:\temp with a size of 12MB each, add line feed(s). Line length defaults to 80 characters.

```dotnet DummyFile.dll /n=10 /s=12MB /f=c:\temp /lf /ll=72```

- Create 10 files in the folder c:\temp with a size of 12MB each, add line feed(s). Line length should be 72 characters.

```dotnet DummyFile.dll /n=10 /s=12MB /f=c: \temp /lf /ll=72 /fe=txt```

- Create 10 files with the file extension 'txt' in the folder c:\temp with a size of 12MB each, add line feed(s). Line length should be 72 characters.

---

## Parameters

- /n  
  Number of files to create **(mandatory)**.

- /s  
  File size of each file **(mandatory)**.

- /f  
  Folder in which the file(s) should be created.

- /lf  
  Create files with line feeds (CrLf).

- /ll  
  Line length (number of characters). Can only be used in conjunction with /lf.

- /fe  
  File extension. Defaults to 'tmp'.

Allowed file size units for parameter /s are:  

- &lt;empty&gt; = Byte e.g. /s=100  

- kb = Kilobyte e.g. /s=100kb  

- mb = Megabyte e.g. /s=100mb  

- gb = Gigabyte e.g. /s=100gb  

Please note: 1 KB = 1024 byte, 1 MB = 1024 KB etc.
