SharpLua 2.0 is a combination of a port of Lua 5.1.5, various interfaces, but will be eventually replaced by new, better, code.
Why did I start with a Lua 5.1.5 C port? because... You Can't Get More Accurate Than The Original (TM), and the C api allows easier porting of C libs.


SharpLua is an implementation of Lua for .NET
It can run lua files including bytecode (compatible with native Lua), create .NET objects and call methods on them (including static methods), and 
supports all standard lua functions. It does not support C Libraries.

To use in your projects, just add a reference to the SharpLua.dll file and use the 
SharpLua.LuaInterface class or the SharpLua.LuaRuntime, SharpLua.Lua is the raw Lua API.

Features Lua 5.1.5 doesn't have:
- table.unpack (similar to _G.unpack)
- a large extension library
- clr library (allows .NET object access)
- Syntax extensions
    - ! (not)
    - >> (right shift)
    - << (left shift)
    - & (bitwise and)
    - | (bitwise or)
    - ^^ (Xor)
    - ~(bitwise not)
    - using/do statements
    - named functions in tables (e.g. { function x() end })

Current projects:
SharpLua                     The SharpLua core, along with a LASM library
SharpLua.Interactive         SharpLua REPL
SharpLua.InterfacingTests    SharpLua interfacing tests. As apposed to using the raw Lua API.
SharpLua.Compiler            SharpLua bytecode compiler

Future projects:
SharpLua.Decompiler          SharpLua bytecode decompiler
SharpLua.Web                 Web handler for #Lua
SharpLua SharpDevelop AddIn  Support SharpLua/Lua files and projects, GUI 
                                designer, and msbuild project builder
SharpLua Wpf support