a = nil
b, c = false, true
d = 123.5
e = "text"
f = function (x) return x^2 end
g = { [f(3)] = b; "x", "y"; x = 1, f(d), [30] = 23; 45 }
g[30] = g.x
g[1] = nil
g.x = nil
g.y = { a, b, c, #g}
g.y[1] = - d * (d + d) / d;
h = { 0x3f4C, 7e2, 3.12E4, [[text]], [==[
very long text!
]==]}

print "Hello, Lua!"

PI = 3.1415926

function circle_length(radius)
    return 2*PI*radius
end

print(string.format("length of cirecle is {0}", circle_length(10)))

Rectangle = {}
Rectangle.__index = Rectangle;
function Rectangle.new(w, h)
    rect = { width = w, height = h }
    setmetatable(rect, Rectangle)
    return rect
end
function Rectangle:area()
    return self.width * self.height
end

rect = Rectangle.new(20, 30)
print(rect:area())

function factorial(n)
    if n <= 0 then return 1
    else return n*factorial(n-1) end
end
for n=1,5 do
    print(n.."! = "..factorial(n))
end

function Matrix(r,c)
  local m = {cols=c,rows=r}
  for y=1,r do
    m[y] = {}
    for x=1,c do
      m[y][x]=0
    end
  end
  return m
end

function printMatrix(m)
  for y=1,m.rows do
    io.write '[ '
    for x=1,m.cols do
      if m[y][x] == '.' then break end
      io.write(m[y][x])
      io.write(' ')
    end
    io.write ']'
    io.write '\r\n'
  end
end

m = Matrix(3, 5)
m[2][3] = '.'
printMatrix(m)

function fillNumbers()
    local t = {}
    for n = 1,75 do
        local m3 = n%3 == 0
        local m5 = n%5 == 0
        if m3 and m5 then
            t[n] = 2
        elseif m3 or m5 then
            t[n] = 1
        else
            t[n] = '_'
        end
    end
    return t
end
print(table.concat(fillNumbers()))

function f(v)
    t = type(v)
    if t == 'nil' then
    elseif t == 'boolean' then
    elseif t == 'number' then
    elseif t == 'string' then
    elseif t == 'function' then
    elseif t == 'table' then
    else
    end
end

X = 5
do
    local x = X
    X = 7
end
while X<9 do X=X+1 end
print(X, math.sin(X)^2+math.cos(X)^2)

repeat X=X-1 until X<6
print(X, math.sin(X)^2+math.cos(X)^2)

function testvararg(...)
   print(...)
end
testvararg(a, b, X)

print '----'
for i,v in ipairs(g) do
   print(i,v)
end
print '----'
table.sort(g)
for k,v in pairs(g) do
   print(k,v)
end
print '----'