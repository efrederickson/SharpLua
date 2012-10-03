LParser - Lua Parsing and refactorization engine written in pure Lua.

Originally a fork of github.com/stravant/luaminify, it will become much more than a lua minifier.
It will also be using in SharpLua as a parsing and reconstruction module with .NET bindings

Note: don't do something like "require'LParser.beautify'", because that would cause the parser to not get loaded.
Make sure you "require'LParser'"

Features:
    - Lua scanner/parser, which generates a full AST
    - Lua reconstructor
        - minimal
        - full reconstruction (TODO: options)

Todo:
    - Preserve comments in AST
    - use table.concat instead of appends in the reconstructors
    - bytecode generator (then this will be a lua compiler)

Copyright (C) 2012 LoDC