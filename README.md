# Zelda Object Manager
A Command-Line application written in C# to manage and manipulate Zelda 64 object files. Do note that this is a Work in Progress and none of the following features are ready for deployment.

## Due Credits
`gfxdis.f3dex2` originally by [glankk](https://github.com/glankk/n64/tree/master/src/gfxdis) and modified by me for use by this application.  
`gfxasm.f3dex2` originally by [z64me](https://github.com/z64me/gfxasm) and modified by me for use by this application.  
[DenOfLions](https://github.com/denoflionsx) for preliminary stress-testing.
***
# Let's review the arguments!

This program features two main functions. This includes `export` and `reskin`.

## export
*\* Currently Unsupported*  
`export` has three sub-functions, or rather, three formats that you can export. This includes `obj`\*, `objex`\*, `c`\* and`zobj`.
Here's an example for each that we can break down and review the arguments:  

```
ZeldaObjectManager.exe export obj
```

```
ZeldaObjectManager.exe export objex
```

```
ZeldaObjectManager.exe export c
```

```
ZeldaObjectManager.exe export zobj -m -e="file.cfg" -o="output.bin" -s06="input.zobj" 0x06021F78 -s06="input_2.zobj" 0x06013F38
```
`-m` is an optinal argument that enables the output of a "map" of the final output file. This will be a text file named the same as your output file.  
`-e` is an optional argument where you can provide a configuration file (with a different specification) that will embed any sort of given data into the final output binary.  
`-o[XX]` is a required argument where you specify the directory and/or filename of your final output file. There is an optional part so that if you specify a segment number with your output file, the output file will be allocated to that segment. By default it will write to segment `0x06`.  
`-sXX` is a required argument where you specify and load a file into one of sixteen zero-indexed different segments. Note that the segments are in hexadecimal format, so segment `-s10` is  unsupported, but `-s0A` is perfectly valid. Do note that if a display list references data that is not in a currently loaded segment, it will be ignored.  
`0xXXXXXXXX` every display list offset that occurs after loading a segment is pulled from the currently loaded segment. This means that if you load a different segment after a display list  
entry, it will pull from the next one instead.  


### Embed Configuration
Embeddable files are simple. There are few symbols to remember. Here is an example of a valid configuration that covers the supported features.

```
# This is a comment followed by the embed configuration specification.

# You can also define variables similar to CSS. Anything wrapped in quotation marks will be treated as text
# and anything else will be treated as raw data.
# Every line except for comments should be terminated with a semicolon.
var(DATANAME)="This is a string.";
var(BYTESTRING)=0xDF00000000000000;

# The START keyword is given a defined offset. If the offset is greater than 0x0, the file will be padded to that offset.
START=0x0;
header="This is a header to be embedded. This will go at the offset defined by the START keyword before any output data is written.";
# You can write byte arrays wrapped in curely brackets, or you can write byte strings without a terminator. You can also invoke a variable by writing its symbol name prefixed with a $ character.
# If you want to write multiple things at a location, separate them with a | character.
footer=$DATANAME|{0xDE, 0xAD, 0xBE, 0xEF}|$BYTESTRING|0xDEADBEEF;
END;
# Nothing after the END keyword will be parsed by the configuration parser.
```

## reskin
This feature is currently unsupported.
TODO:
* Write Reskin Specification
