# Universal Jar Scanner
This tool uses some code from [overwolf/jar-infection-scanner](https://github.com/overwolf/jar-infection-scanner)

This tool was also co-developped by [@SSUnlimited9](https://github.com/SSUnlimited9)

## Usage
Requires .NET 6 runtime. You can install it using [this link](https://dotnet.microsoft.com/en-us/download/dotnet/6.0).

Simply run the provided executable, write a scan path in the UI (or click `Browse`) then click `Scan` to start scanning.

The output will notify you of all infected files.

## Building
The tool uses the [Avalonia](https://www.nuget.org/packages/Avalonia) package obtained from NuGet. Obtain a copy of all dependancies then build the .NET project like normal.

## Why this was made
Even though a tool was already available, it does not work with Linux (even through wine). Since linux users can also be affected, this tool will help scan on there too.

The scan logic is all contained in the [Scanner.cs](Scanner.cs). Except for some extra code to work with the GUI library, this uses system dependancies only to perform the scan. This means that the tool should work on all platforms that .NET supports. Technically this tool should work for MacOS too, but I do not have a Mac to test it on. Those on MacOS can build it and try for yourself. A CLI version will also be available soon.

## License
Uses the MIT license. See [LICENSE](LICENSE) for more information.
