# VKArchive
## VK Export Parsing Utility

VKArchive is a software that allows you to parse HTML files acquired from a [VK profile export feature](https://vk.com/data_protection?section=rules&scroll_to_archive=1) into JSON.

## Features

- Converts all messages to a single format.
- Supports attachments (photos, videos etc).
- A single timezone (UTC+0) for all converted messages.
- Supports dialogue titles.

## Installation and usage

You will need [.NET 6.0 Runtime](https://dotnet.microsoft.com/en-us/download) installed for this software to work. Download a binary file from [Releases](https://github.com/kolya5544/VKArchive/releases) or compile the source code using `dotnet build`.

To run the binary file, on Windows you should use your terminal (for example, `cmd.exe`) and open `VKMessagesParser.exe`.

For Linux, to run the program, you will have to use `dotnet VKMessagesParser.dll`

## Contributions

We don't accept any major contributions.

## Converted data format

All HTML messages data in an exported VK archive is converted to a single JSON file of this structure:
```json
{
	"dialogues":[
		{
			"id":127172472,
			"title":"Николай Кушнаренко",
			"messages":[
				{
					"sender":0,
					"messageId":1234,
					"timestamp":123456,
					"body":"Привет, Николай",
					"attachment":[
						{
							"name":"Фотография",
							"url":"https://.../"
						}
					]
				}
			]
		}
	],
	"timestamp":12345678
}
```

`sender` will always be `0` if the message was sent by the exported archive owner. Attachment contents can also include service information sometimes. `timestamp` of root object is the time at which the archive was processed, NOT when the archive was initially made.