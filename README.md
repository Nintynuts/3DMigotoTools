# 3Dmigoto ShaderHacker Tools

This software is intended to be a suite of tools to assist shaderhackers.

## Current features

The application can convert FrameAnalysis logs to a more readable, tabular format and annotate shader labels and buffer names (metadata).

There are several ways to enter the `d3dx.ini` or `log.txt` path:
- Drag it to the exe
- Run the exe with the file name as the first argument
- Drag it into the console window when prompted
- Type it into the console window when prompted

Providing a `log.txt` file path and optionally the desired column configuration via the command line will implicity enter `Manual` mode, and the application will exit when the session completes.

When running the application without passing a `log.txt` file argument, the following options are presented and multiple sessions can be performed without restarting.

### Import metadata from ini files and shader fixes

If running the application with a `d3dx.ini` path (or with a `log.txt` path from the original FrameAnalysis folder it was dumped into), the application will scrape names to associate with hashes:
- `*.ini` files for Shader and Texture Overrides (plus `[Include]` files).
- `*_replace.txt` for Shader, constant buffers and texure registers (plus `#include` files).

> **_NOTE:_** The name in the `*_replace.txt` file takes prescedence over the `*.ini` override name.

> **_NOTE:_** You can reload the metadata while in `Manual` mode with the `get-metadata` function.

### Auto Conversion (`Auto` mode)

By providing the `d3dx.ini` file path for your game, the tool can convert logs as they are generated automatically.

When a new FrameAnalysis folder is detected, it will generate a CSV and a conversion message log.

There will be a prompt for whether to export a single, split or both file(s) in the case of multiple-frame logs.

See "Frame Analysis Import" and "Frame Analysis Export" below for more details.

### Interactive Conversion (`Manual` mode)

A single log file is loaded, after which it is possible to perform multiple functions without reloading the log.

#### Frame Analysis Import (`log.txt`)

The application will parse the content of the file into memory for future operations.

For each Draw Call, messages may appear for inefficient use of the API (overwriting registers in the same draw call).

Messages may also appear for API calls that are supported, but the majority of those that seemed relevant have.

> **_NOTE:_** Please raise an issue if something is missing that seems useful.

#### Generate Asset lifecycle (`Asset` function)

By entering an asset hash (for a Buffer or Texture), a CSV file will be produced with every usage in the log.

Where pipeline state re-use has been active, a distinct list of shaders used over the active period will be written to simplify the output, but be aware that the last use may be after other uses listed below it.

> **_NOTE:_** Input Assembler assets (IB & VB) will identify the VS hash, whereas Output Merger assets (RT & D) will identify the PS hash.

#### Frame Analysis Export (`Log` function, or `Auto` mode)

As well as making the data tabular, the following things are improved over a raw log:
- Partial re-use of the same pipeline state is extrapolated per draw-call so each can be understood in isolation
- Where Buffers are mapped (edited) or Views are cleared, an asterisk will be added to the end of the hash
- Shaders, Buffers and Textures which have been named via Overrides in an ini or in a shader fix will be annotated.
- 3Dmigoto Logic trace has been processed to be more readable and less verbose

When using `analyse_options = hold` and analysing multiple frames the application will ask if whether to export a single, split or both file(s). 

> **_NOTE:_**  Frame number will only be listed in the single output if more than one exists in the log. Split frames can be useful for comparing differences.

##### Custom Column Ouput

It is possible to customise the columns output by the program.

These can be provided via the command line following the file path or entered when required.

> **_NOTE:_**  You can change the output columns in `Manual` mode with the `set-columns` function.

The following column ids are valid:

- `All` : uses all below except shaders
- `IA` : Input Assembler hashes (shorthand for the following, which both include common Input Assembler info)
	- `VB` : Vertex buffer hashes, vertex Start and Count
	- `IB` : Index buffer hash, index Start and Count
- `?S` : Shader, replace `?` with `V/H/D/G/P/C` for associated shader, will output all 3 below
	- `?S-Hash` : Only exports Hash
	- `?S-CB`* : Only exports Hash and Constant Buffer hashes
	- `?S-T`* : Only exports Hash and Texture hashes
- `OM` : Output Merger hashes (shorthand for the following)
	- `RT` : Render Target hashes
	- `D` : Depth Stencil hash
- `Logic` : 3Dmigoto logic trace (for `ShaderOverride`s etc.), split into Pre and Post

*Output specific registers by suffixing with `:` and comma separated numbers (for Example `PS-T:1,3,7`) 

If not specified, the output file will omit any numbered columns that contain no data (unused registers).

> **_NOTE:_**  The default configuration is equivalent to `All VS PS`.

Detail regarding Input Assembler information:
- `*_` : Start Vertex/Index/Instance
- `*#` : Vertex/Index/Instance Count
- A `?` value means it is not explicitly specified for the type of `Draw` being performed

## Download and providing feedback

Currently I consider this to be pre-release, so I am only providing copies to those who ask for it, although I can't stop you from checking it out and building it yourself.

Anyone evaluating this tool, please raise issues with a sample Log.txt and details of your issue. I'm also open to suggestions, although I have a long list of ideas already.

## Requirements

This app is built on .NET 6, and will require the runtime to be installed. 

It is therefore theoretically compatible with Linux, but has only been tested on Windows 10 version 20H2 (although any Windows 10 should be fine).

## Credits

DarkStarSword, Bo3b and Chiri for their work on 3Dmigoto without which this would be pointless.
