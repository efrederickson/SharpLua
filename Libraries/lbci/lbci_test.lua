require"lbci"

local function inspect(f,all)
 local F=inspector.getheader(f)
 print("header",f)
 for k,v in next,F do
  print("",k,v)
 end

 print("constants",F.constants)
 for i=1,F.constants do
  local a=inspector.getconstant(f,i)
  print("",i,a)
 end

 print("locals",F.locals)
 for i=1,F.locals do
  local a,b,c=inspector.getlocal(f,i)
  print("",i,a,b,c)
 end

 print("upvalues",F.upvalues)
 for i=1,F.upvalues do
  local a=inspector.getupvalue(f,i)
  print("",i,a)
 end

 print("functions",F.functions)
 for i=1,F.functions do
  local a=inspector.getfunction(f,i)
  print("",i,a)
 end

 print("instructions",F.instructions)
 for i=1,F.instructions do
  local a,b,c,d,e=inspector.getinstruction(f,i)
  print("",i,a,b,c,d,e)
  if b=="GETGLOBAL" then print(">>> ",inspector.getconstant(f,-d)) end
  if b=="SETGLOBAL" then print(">>>*",inspector.getconstant(f,-d)) end
 end

 print()
 if all then
  for i=1,F.functions do
   inspect(inspector.getfunction(f,i),all)
  end
 end

end

f=function (x,y)
	print(x,"=",y,23)
	local z=x+2*y
	local last
	X=245
	return inspect
end

inspect(debug.getinfo(1).func,true)
do return end

inspect(f)
inspect(inspect)
f(1,2)
inspector.setconstant(f,3,98)
f(1,2)
inspector.setconstant(f,3,123,987)
f(1,2)
inspector.setconstant(f,3)
f(1,2)
f(1,2)

