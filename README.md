# 3Dmigoto ShaderHacker Tools

This software is intended to be a suite of tools to assist shaderhackers.

## Current features

The application can convert FrameAnalysis logs to a tabular format.

There are several ways to enter the `d3dx.ini` or `log.txt` path:
- Drag it to the exe
- Run the exe with the file name as the first argument
- Drag it into the console window when prompted
- Type it into the console window when prompted

When running the application without passing a `log.txt` file argument, the following options are presented and multiple sessions can be performed without restarting.

### Import metadata from ini files and shader fixes

If running the application with a `d3dx.ini` path (or with a `log.txt` path from the original FrameAnalysis folder it was dumped into), the application will scrape these files for Shader and Asset names to associate with hashes read from the frame analysis.

### Auto Conversion (`Auto` mode)

The tool can convert logs as they are generated automatically. This requires providing the `d3dx.ini` file path for your game.

When a new FrameAnalysis folder is detected, it will generate a CSV and a conversion message log, see below for more information.

### Interactive Conversion (`Manual` mode)

It's possible to activate manual mode implicitly by providing a `log.txt` file path and optionally the desired column configuration via the command line, and in this case the application will exit when the session completes.

When a single log file is loaded, it is possible to perform multiple functions without reloading the log. The following functions can be performed:

#### Import Frame Analysis `log.txt`

The application will parse the content of the file into memory for future operations.

For each Draw Call, messages may appear for inefficient use of the API (overwriting slots in the same draw call).

Messages may also appear for API calls that are supported, but the majority of those that seemed relevant have.

Please raise an issue if something is missing that seems useful.

#### Generate simplified Frame Analysis CSV (`Log` and `Set-Columns` functions)

It is possible to customise the columns output by the program.

These can be provided via the command line, either alone or following the file path.

If they are not supplied via the command line, you will be prompted to enter them when the application starts.

If you wish to change the columns being output while operating in `Manual` mode, the function `set-columns` can be used.

The following column ids are valid:

- `All` : uses all below except shaders
- `IA` : Input Assembler hashes (shorthand for the following, which both include common Input Assembler info)
	- `VB` : Vertex buffer hashes, vertex Start and Count
	- `IB` : Index buffer hash, index Start and Count
- `?S` : Shader, replace `?` with `V/H/D/G/P/C` for associated shader, will output all 3 below
	- `?S-Hash` : Only exports Hash
	- `?S-CB` : Only exports Hash and Constant Buffer hashes
	- `?S-T` : Only exports Hash and Texture hashes
- `OM` : Output Merger hashes (shorthand for the following)
	- `RT` : Render Target hashes
	- `D` : Depth Stencil hash
- `Logic` : 3Dmigoto logic trace (for `ShaderOverride`s etc.), split into Pre and Post

The default configuration is equivalent to `All VS PS`.

Frame number will only be listed if more than one exists in the log (when using `analyse_options = hold`).

The output file will omit any numbered columns that contain no data (unused slots).

Detail regarding Input Assembler information:
- `*_` : Start Vertex/Index/Instance
- `*#` : Vertex/Index/Instance Count
- A `?` value means it is not explicitly specified for the type of `Draw` being performed

As well as making the data tabular, the following things are improved over a raw log:
- Partial re-use of the same pipeline state is extrapolated per draw-call so each can be understood in isolation
- Where Buffers are mapped (edited) or Views are cleared, an asterisk will be added to the end of the hash
- Shaders, Buffers and Textures which have been named via Overrides in an ini or in a shader fix will be annotated.
- 3Dmigoto Logic trace has been processed to be more readable and less verbose

#### Generate Asset lifecycle CSV  (`Asset` function)

By entering an asset hash (for a Buffer or Texture), a CSV file will be produced with every usage of that asset over the course of the log.

Where pipeline state re-use has been active, a distinct list of shaders used over the active period will be written to simplify the output, but be aware that the last use may be after other uses listed below it.

Input Assembler assets (IB & VB) will identify the VS hash, whereas Output Merger assets (RT & D) will identify the PS hash.

## Download and providing feedback

Currently I consider this to be pre-release, so I am only providing copies to those who ask for it, although I can't stop you from checking it out and building it yourself.

Anyone evaluating this tool, please raise issues with a sample Log.txt and details of your issue. I'm also open to suggestions, although I have a long list of ideas already.

## Requirements

This app is built on .NET Core 3.1, and will require the runtime to be installed. 

It is therefore theoretically compatible with Linux, but has only been tested on Windows 10 versions 1903 and 2004 (although any Windows 10 should be fine).

## Credits

DarkStarSword, Bo3b and Chiri for their work on 3Dmigoto without which this would be pointless.
