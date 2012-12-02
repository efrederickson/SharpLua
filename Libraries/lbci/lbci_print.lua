require"lbci"

local write=io.write

local function S(t,k,x,c)
	local n=t[k]
	if n==1 then k=string.sub(k,1,-2) end
	write(n,x or ""," ",k,c or ", ")
end

local function constant(f,i)
	if i~=nil and i<0 then
		write(inspector.getconstant(f,-i)," ")
	end
end

local function inspect(f,all)
 local F=inspector.getheader(f)
 local k

 if F.line==0 then k="main" else k="function" end
 write("\n",k," <",F.source,":",F.line,",",F.lastline,"> (")
 S(F,"instructions",nil,")\n")
 if F.isvararg then k="+" else k="" end
 S(F,"params",k)
 S(F,"slots")
 S(F,"upvalues")
 S(F,"locals")
 S(F,"constants")
 S(F,"functions",nil,"\n")

 for i=1,F.instructions do
  local a,b,c,d,e=inspector.getinstruction(f,i)
  b=string.sub(b.."          ",1,9)
  write("\t",i,"\t[",a,"]\t",b,"\t",c," ",d," ",e or "","\t; ")
  constant(f,c)
  constant(f,d)
  constant(f,e)
  write("\n")
 end

 if all then
  for i=1,F.functions do
   inspect(inspector.getfunction(f,i),all)
  end
 end

end

local f=assert(loadfile"print.lua")
inspect(f)
inspect(function () end)
