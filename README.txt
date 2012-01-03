SharpLua is a version of Lua for .NET.
It can run lua files, create .NET objects and call methods on them, and 
supports most lua functions.

Created by mlnlover11 Productions
Based off of the LuaInterpreter article on CodeProject.

To use in your projects, just add a reference to the SharpLua.dll file and use the 
SharpLua.LuaRuntime.Run(code[, environment]) or RunFile function.

Features Lua doesn't have:
- built-in table copying and printing
- metatable access for all Lua objects
  - able to set in Lua also
- console library
- script library (.NET access)
- WinForms library
- many more functions, such as
  - openfile         - open a lua file with an OpenFileDialog and run it
  - set              - set (table, key, value)
  - ssave            - serialize an object to a file
  - sload            - deserialize a file to an object
  - table.removeitem - removes an item from a table

Current projects:
SharpLua              The SharpLua parser and interpreter
SharpLua.Serializer   Serializes a #Lua AST parsed from a source file
SharpLua.Compiler     A simple compiler for #Lua, I used the L# compiler
SharpLua.Interpreter  SharpLua's interpreter program
SharpLuaAddIn         A SharpDevelop addin with syntax highlighting for #Lua
CryptoLib             An experimental encryption lib for #Lua


To compile CryptoLib, you will need to change the reference to IExtendFramework to wherever you have an IExtendFramework dll
You can get IExtendFramework at https://github.com/mlnlover11/IExtendFramework