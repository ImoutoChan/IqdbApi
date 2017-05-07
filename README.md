# Iqdb Api Library

[![NuGet](https://img.shields.io/nuget/v/IqdbApi.svg?style=flat-square)](https://www.nuget.org/packages/IqdbApi/)
[![license](https://img.shields.io/github/license/ImoutoChan/IqdbApi.svg?style=flat-square)](https://github.com/ImoutoChan/IqdbApi)

C# library for searching images on [iqdb.org](https://iqdb.org)

## Usage

```C#
static async void Run()
{
    IIqdbApi api = new IqdbApi();
    using (var fs = new FileStream("imageToSearch.jpg", FileMode.Open))
    {
        var searchResults = await api.SearchFile(fs);
    }
    
    var results = await api.SearchUrl("https://cs541604.userapi.com/c836722/v836722677/342ba/JKnecCszdCM.jpg");
}
```

## Installation

Install as [NuGet package](https://www.nuget.org/packages/IqdbApi/):

```powershell
Install-Package IqdbApi
```

## TODO:

* search more
* ignore colors
* preventing bans
