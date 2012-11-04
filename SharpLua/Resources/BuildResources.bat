:: Build's the EmbeddedResource precompiled core scripts
:: Run this whenever the core scripts are edited
:: DebugInfo is stripped because:
:: A - it results in a smaller file
:: B - these scripts don't (...) have errors, so it doesn't matter

..\..\bin\sluac -s -o clrlib.sluac clrlib.slua
..\..\bin\sluac -s -o extlib.sluac extlib.slua
..\..\bin\sluac -s -o luanet.sluac luanet.slua