SharpLua 2.0 is a combination of a port of Lua 5.1.5, various interfaces, but will be eventually replaced by new, better, code.
Why did I start with a Lua 5.1.5 C port? because... You Can't Get More Accurate Than The Original (TM), and the C api allows easier porting of C libs.


SharpLua is an implementation of Lua for .NET
It can run lua files including bytecode, create .NET objects and call methods on them (including static methods), and 
supports all standard lua functions. It does not support C Libraries.

To use in your projects, just add a reference to the SharpLua.dll file and use the 
SharpLua.LuaInterface class or the SharpLua.LuaRuntime, SharpLua.Lua is the raw Lua API.

Features Lua 5.1.5 doesn't have:
- table.unpack (similar to _G.unpack)
- clr library (partially implemented, needs a ton of work)
- Syntax extensions
    - ! (not)
    - >> (right shift) (new opcode)
    - << (left shift) (new opcode)
    - & (bitwise and)
    - | (bitwise or)
    - MAYBE: xor (a xor b)

Current projects:
SharpLua                     The SharpLua core
SharpLua.Interactive         SharpLua REPL
SharpLua.InterfacingTests    SharpLua interfacing tests. As apposed to using the raw Lua API.
SharpLua.Compiler            SharpLua bytecode compiler

Future projects:
SharpLua.Decompiler          SharpLua bytecode decompiler
SharpLua.Web                 Web handler for #Lua
SharpLua SharpDevelop AddIn  Support SharpLua/Lua files and projects, GUI 
                                designer, and msbuild project builder
SharpLua Wpf support

TODO
- Documentation
- Simple library extension
- compression library (separate project), using IExtendFramework
- set "..." to command line args in _G
- Lists
- new icon
- set              - set (table, key, value)
- table.removeitem - removes an item from a table