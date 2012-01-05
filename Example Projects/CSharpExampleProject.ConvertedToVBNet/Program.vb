'
' * Created by SharpDevelop.
' * User: elijah
' * Date: 1/4/2012
' * Time: 3:03 PM
' * 
' * To change this template use Tools | Options | Coding | Edit Standard Headers.
' 

Imports SharpLua
Imports SharpLua.LuaTypes

Class Program
    Public Shared Sub Main(cmd_args_1938475092347027340582734 As String())
        ' random name doesnt interfere with my variables
        ' Create a global environment
        Dim t As LuaTable = LuaRuntime.CreateGlobalEnviroment()
        
        ' to add an item, dont use AddValue, it sticks it into the back
        ' instead, use SetNameValue
        t.SetNameValue("obj", New LuaString("sample object"))
        
        ' we can set the MetaTable of item "obj", but first we must get it from
        ' the global environment:
        Dim val As LuaValue = t.GetValue("obj")
        
        ' We can then print "val"
        Console.WriteLine(val.ToString())
        ' --> sample object
        ' to register methods, use the Register function (using Addresses or delegates)
        t.Register("samplefunc", Function(args As LuaValue()) New LuaNumber(100))
        
        ' To run Lua, use the Run function in LuaRuntime 
        ' we pass "t" as the specified environment, otherwise it will 
        ' create a new environment to run in.
        LuaRuntime.Run("print(""obj:"", obj, """ & vbCrLf & "samplefunc:"", samplefunc())", t)
        
        ' we can also call .NET methods using Lua-created .NET object
        ' such as:
        LuaRuntime.Run("script.reference""VBNetExampleProject""", t)
        LuaRuntime.Run("obj2 = script.create(""VBNetExampleProject.TestClass"")", t)
        LuaRuntime.Run("print(""testint:"", obj2.testint, ""TestFunc:"", obj2.TestFunc())", t)
        
        ' the reason for this is because script.create returns an advanced Userdata with
        ' metatables that check any indexing or setting and map them to the .NET object
        ' if it doesn't find it, it does nothing
        LuaRuntime.Run("obj2.ThisValueDoesntExistInDotNet = ""hey""", t)
        ' but when trying to get, it appears to be a function -- the result of the metatable indexing system.
        ' the same value was printed twice, with different functions each time, proving its not actually there:
        Console.WriteLine(LuaRuntime.Run("return ""Sample attemption at creating an object: "" .. tostring(obj2.ThisValueDoesntExistInDotNet)", t))
        ' Console.WriteLine was used to show that you can print the returned value of executed code
        LuaRuntime.Run("print(obj2.ThisValueDoesntExistInDotNet)", t)
        ' and you cant call them, though they appear to be functions:
        LuaRuntime.Run("pcall(obj2.ThisValueDoesntExistInDotNet, print(""Error calling non-existant function \""ThisValueDoesntExistInDotNet\""""))", t)
        
        ' Lua can also create "classes"
        LuaRuntime.Run("c = class()", t)
        Console.WriteLine(LuaRuntime.Run("return c", t))
        
        ' Let you see the output
        Console.Write("Press any key to continue . . . ")
        Console.ReadKey(True)
    End Sub
End Class

Public Class TestClass
    Public testint As LuaValue = New LuaNumber(100)
    Public Function TestFunc() As String
        Return "Testing..."
    End Function
End Class
