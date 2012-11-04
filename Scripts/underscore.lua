-- Underscore.lua
-- Copyright (C) 2012 LoDC

local bit = bit or require"bit"

--- <summary>
--- The _ (underscore) module. Could have another name, as _ is a throwaway variable. <br />
--- recomended other names: u, underscore
--- </summary>
local m = {  }

local breaker = { }

local lookupIterator = function(value)
    return m.isFunction(value) and value or (function(obj) return obj[value] end)
end

local group = function(obj, value, behavior)
    local result = { }
    local iterator = lookupIterator(value)
    m.each(obj, function(value, index)
        local key = iterator(value, index, obj)
        behavior(result, key, value)
    end)
    return result
end
m.group = group

m.push = function(list, ...)  
    local values = { ... }
    m.each(values, function(v)
        table.insert(list, v)
    end)
    return list
end

m.slice = function (list, start, stop)
    local array = { }
    stop = stop or #list
    for index = start, stop, 1 do
        table.insert(array, list[index])
    end 
    return array
end

m.each = function(obj, iterator)
    if obj == nil then
        return 
    end
    for k, v in pairs(obj) do
        iterator(v, k, obj)
    end
end
m.forEach = m.each

m.map = function(obj, iterator)
    local results = { }
    if obj == nil then
        return results
    end
    m.each(obj, function(value,index,list) 
        table.insert(results, iterator(value, index, list))
    end)
    return results
end
m.collect = m.map

m.reduce = function(obj, iterator, memo)
    local initial = memo ~= nil
    if obj == nil then
        obj = { }
    end
    m.each(obj, function(value,index,list) 
        if not initial then
            memo = value
            initial = true
        else
            memo = iterator.call(context, memo, value, index, list)
        end
    end)
    if not initial then
        error('Reduce of empty array with no initial value')
    end
    return memo
end
m.foldl = m.reduce
m.inject = m.reduce

m.reduceRight = function(obj, iterator, memo)
    local initial = memo ~= nil
    if obj == nil then
        obj = { }
    end
    local length = #obj
    local keys = m.keys(obj)
    length = #keys
    m.each(obj, function(value, index, list) 
        --index = keys ? keys[--length] : --length;
        if notinitial then
            memo = obj[index]
            initial = true
        else
            memo = iterator(memo, obj[index], index, list)
        end
    end)
    if not initial then
        error('Reduce of empty array with no initial value')
    end
    return memo
end
m.foldr = m.reduceRight

m.find = function(obj, iterator)
    local result
    m.any(obj, function(value, index, list) 
        if iterator(value, index, list) then
            result = value
            return true
        end
    end)
    return result
end
m.detect = m.find

m.filter = function(obj, iterator)
    local results = { }
    if obj == nil then
        return results
    end
    m.each(obj, function(val, key, list)
        if iterator(val, key, list) then
            table.insert(results, val)
        end
    end)
    return results
end
m.select = m.filter

m.reject = function(obj, iterator)
    return m.filter(obj, function(value, index, list)
        return not iterator(value, index, list)
    end)
end

m.every = function(obj, iterator)
    iterator = iterator or m.identity
    local result = true
    if obj == nil then
        return result
    end
    m.each(obj, function(value, index, list)
        if not iterator(value, index, list) then
            result = false
        end
    end)
    return not not result
end
m.all = m.every

m.any = function(obj, iterator)
    iterator = iterator or m.identity
    local result = false
    if obj == nil then
        return result
    end
    m.each(obj, function(value, index, list)
        result = result or iterator(value, index, list)
    end)
    return not not result
end
m.some = m.any

m.contains = function(obj, target)
    local found = false
    if obj == nil then
        return found
    end
    found = m.any(obj, function(value)
        return rawequal(value, target)
    end)
    return found
end
m.include = m.contains

m.invoke = function(obj, method, ...)
    local args = { ... }
    return m.map(obj, function(value)
        return (m.isFunction(method) and method or value[method])(value, unpack(args))
    end)
end

m.pluck = function(obj, key)
    return m.map(obj, function(value) return value[key] end)
end

m.where = function(obj, attrs)
    if m.isEmpty(attrs) then
        return { }
    end
    return m.filter(obj, function(value)
        for key, value in pairs(attrs) do
            if attrs[key] ~= value[key] then
                return false
            end
        end
      return true
    end)
end

m.max = function(obj, iterator)
    iterator = iterator or m.identity
    
    if m.isEmpty(list) then
        return -math.huge
    elseif m.isFunction(func) then
        local max = { computed = -math.huge }
        m.each(list, function(value, key, object)
            local computed = iterator(value, key, object)
            if computed >= max.computed then
                max = { computed = computed, value = value }
            end
        end)
        return max.value
    else
        return math.max(unpack(list))
    end
end

m.min = function(list, iterator)
    iterator = iterator or m.identity
    
    if m.isEmpty(list) then
        return math.huge
    elseif m.isFunction(func) then
        local min = { computed = math.huge }
        m.each(list, function(value, key, object)
            local computed = iterator(value, key, object)
            if computed < min.computed then
                min = { computed = computed, value = value }
            end
        end)
        return min.value
    else
        return math.min(unpack(list)) 
    end
end

m.shuffle = function(list)
    local rand
    local index = 0
    local shuffled = { }
    
    m.each(obj, function(value)
        rand = m.random(index)
        index = index + 1
        shuffled[index - 1] = shuffled[rand]
        shuffled[rand] = value
    end)
    
    return shuffled
end

m.sortBy = function(obj, value)
    local iterator = lookupIterator(value)
    local t = m.pluck(m.map(obj, function(value, index, list)
        return {
            ["value"] = value,
            ["index"] = index,
            ["criteria"] = iterator(value, index, list)
        }
    end), value)
    
    return table.sort(t, function(left, right)
        local a = left.criteria
        local b = right.criteria
        if a ~= b then
            if (a > b or a == nil) then return true end
            if (a < b or b == nil) then return false end
        end
        return left.index < right.index
    end)
end

m.groupBy = function(obj, value, context)
    return group(obj, value, context, function(result, key, value)
        local x
        if m.has(result, key) then
            x = result[key]
        else
            result[key] = { }
            x = result[key]
        end
        m.push(x, value);
    end)
end)

m.countBy = function(obj, value, context)
    return group(obj, value, context, function(result, key, value)
        if not m.has(result, key)) then
            result[key] = 0
         end
        result[key] = result[key] + 1
    end)
end

m.sortedIndex = function(array, obj, iterator, context)
    iterator = iterator and lookupIterator(iterator) or m.identity
    local value = iterator(context, obj)
    local low, high = 0, #array
    while low < high do
        local mid = bit.rshift(low + high, 1)
        if iterator(context, array[mid]) < value then
            low = mid + 1
        else
            high = mid
        end
    end
    return low
end

m.toArray = function(obj)
    if not obj then 
        return { }
    end 
    if not m.isObject(list) then 
        list = { list, ... }
    end  
    local cloned = { }
    m.each(list, function(value) 
        table.insert(cloned, value)  
    end) 
    return cloned
end

m.size = function(obj)
    if obj == nil then
        return 0
    end
    
    if m.isArray(list) then
        return #list
    elseif m.isObject(list) then 
        local length = 0  
        m.each(list, function() length = length + 1 end) 
        return length 
    elseif m.isString(list) then  
        return string.len(list) 
    elseif not m.isEmpty(args) then  
        return m.size(args) + 1 
    end
    
    return 0
end


m.first = function(array, n, guard)
    if array == nil then
        return nil
    end
    if not n then
        n = 1
    end
    return m.slice(array, n, guard)
end
m.head = m.first
m.take = m.first

m.initial = function(array, n, guard)
    return slice.call(array, 0, array.length - (((n == nil) or guard) and 1 or n))
end

m.last = function(array, n, guard)
    if array == nil then
        return nil
    end
    if (n ~= nil and not guard) then
        return m.slice(array, math.max(#array - n, 0))
    else
        return array[#array - 1]
    end
end

m.rest = function(array, n, guard)
    return m.slice(array, (n == nil or guard) and 1 or n)
end
m.tail = m.tail
m.drop = m.rest

m.compact = function(array)
    return m.filter(array, function(value) return not not value end)
end

m.flattenInternal = function(input, shallow, output)
    m.each(input, function(value)
        if m.isArray(value) then
            if shallow then
                m.push(output, value)
            else
                flatten(value, shallow, output)
            end
        else
            m.push(output, value)
        end
    end)
    return output
end

m.flatten = function(array, shallow)
    return m.flattenInternal(array, shallow, { })
end

m.without = function(array)
    return m.difference(array, m.slice(arguments, 1))
end

m.unique = function(array, isSorted, iterator)
    local initial = iterator and m.map(array, iterator) or array
    local results = { }
    local seen = { }
    m.each(initial, function(value, index)
        if (isSorted and not index or seen[#seen - 1]) ~= value or not m.contains(seen, value) then
            m.push(seen, value);
            m.push(results, array[index])
        end
    end)
    return results
end
m.uniq = m.unique

m.union = function(...)
    return m.uniq(concat.apply(ArrayProto, ...))
end

_.intersection = function(array) {
var rest = slice.call(arguments, 1);
return _.filter(_.uniq(array), function(item) {
  return _.every(rest, function(other) {
    return _.indexOf(other, item) >= 0;
  });
});
};

-- Take the difference between one array and a number of other arrays.
-- Only the elements present in just the first array will remain.
_.difference = function(array) {
var rest = concat.apply(ArrayProto, slice.call(arguments, 1));
return _.filter(array, function(value){ return !_.contains(rest, value); });
};

-- Zip together multiple lists into a single array -- elements that share
-- an index go together.
_.zip = function() {
var args = slice.call(arguments);
var length = _.max(_.pluck(args, 'length'));
var results = new Array(length);
for (var i = 0; i < length; i++) {
  results[i] = _.pluck(args, "" + i);
}
return results;
};

-- Converts lists into objects. Pass either a single array of `[key, value]`
-- pairs, or two parallel arrays of the same length -- one of keys, and one of
-- the corresponding values.
_.object = function(list, values) {
if (list == null) return {};
var result = {};
for (var i = 0, l = list.length; i < l; i++) {
  if (values) {
    result[list[i]] = values[i];
  } else {
    result[list[i][0]] = list[i][1];
  }
}
return result;
};

-- If the browser doesn't supply us with indexOf (I'm looking at you, **MSIE**),
-- we need this function. Return the position of the first occurrence of an
-- item in an array, or -1 if the item is not included in the array.
-- Delegates to **ECMAScript 5**'s native `indexOf` if available.
-- If the array is large and already in sort order, pass `true`
-- for **isSorted** to use binary search.
_.indexOf = function(array, item, isSorted) {
if (array == null) return -1;
var i = 0, l = array.length;
if (isSorted) {
  if (typeof isSorted == 'number') {
    i = (isSorted < 0 ? Math.max(0, l + isSorted) : isSorted);
  } else {
    i = _.sortedIndex(array, item);
    return array[i] === item ? i : -1;
  }
}
if (nativeIndexOf && array.indexOf === nativeIndexOf) return array.indexOf(item, isSorted);
for (; i < l; i++) if (array[i] === item) return i;
return -1;
};

-- Delegates to **ECMAScript 5**'s native `lastIndexOf` if available.
_.lastIndexOf = function(array, item, from) {
if (array == null) return -1;
var hasIndex = from != null;
if (nativeLastIndexOf && array.lastIndexOf === nativeLastIndexOf) {
  return hasIndex ? array.lastIndexOf(item, from) : array.lastIndexOf(item);
}
var i = (hasIndex ? from : array.length);
while (i--) if (array[i] === item) return i;
return -1;
};

-- Generate an integer Array containing an arithmetic progression. A port of
-- the native Python `range()` function. See
-- [the Python documentation](http:--docs.python.org/library/functions.html#range).
_.range = function(start, stop, step) {
if (arguments.length <= 1) {
  stop = start || 0;
  start = 0;
}
step = arguments[2] || 1;

var len = Math.max(Math.ceil((stop - start) / step), 0);
var idx = 0;
var range = new Array(len);

while(idx < len) {
  range[idx++] = start;
  start += step;
}

return range;
};

-- Function (ahem) Functions
-- ------------------

-- Reusable constructor function for prototype setting.
var ctor = function(){};

-- Create a function bound to a given object (assigning `this`, and arguments,
-- optionally). Binding with arguments is also known as `curry`.
-- Delegates to **ECMAScript 5**'s native `Function.bind` if available.
-- We check for `func.bind` first, to fail fast when `func` is undefined.
_.bind = function bind(func, context) {
var bound, args;
if (func.bind === nativeBind && nativeBind) return nativeBind.apply(func, slice.call(arguments, 1));
if (!_.isFunction(func)) throw new TypeError;
args = slice.call(arguments, 2);
return bound = function() {
  if (!(this instanceof bound)) return func.apply(context, args.concat(slice.call(arguments)));
  ctor.prototype = func.prototype;
  var self = new ctor;
  var result = func.apply(self, args.concat(slice.call(arguments)));
  if (Object(result) === result) return result;
  return self;
};
};

-- Bind all of an object's methods to that object. Useful for ensuring that
-- all callbacks defined on an object belong to it.
_.bindAll = function(obj) {
var funcs = slice.call(arguments, 1);
if (funcs.length == 0) funcs = _.functions(obj);
each(funcs, function(f) { obj[f] = _.bind(obj[f], obj); });
return obj;
};

-- Memoize an expensive function by storing its results.
_.memoize = function(func, hasher) {
var memo = {};
hasher || (hasher = _.identity);
return function() {
  var key = hasher.apply(this, arguments);
  return _.has(memo, key) ? memo[key] : (memo[key] = func.apply(this, arguments));
};
};

-- Delays a function for the given number of milliseconds, and then calls
-- it with the arguments supplied.
_.delay = function(func, wait) {
var args = slice.call(arguments, 2);
return setTimeout(function(){ return func.apply(null, args); }, wait);
};

-- Defers a function, scheduling it to run after the current call stack has
-- cleared.
_.defer = function(func) {
return _.delay.apply(_, [func, 1].concat(slice.call(arguments, 1)));
};

-- Returns a function, that, when invoked, will only be triggered at most once
-- during a given window of time.
_.throttle = function(func, wait) {
var context, args, timeout, result;
var previous = 0;
var later = function() {
  previous = new Date;
  timeout = null;
  result = func.apply(context, args);
};
return function() {
  var now = new Date;
  var remaining = wait - (now - previous);
  context = this;
  args = arguments;
  if (remaining <= 0) {
    clearTimeout(timeout);
    previous = now;
    result = func.apply(context, args);
  } else if (!timeout) {
    timeout = setTimeout(later, remaining);
  }
  return result;
};
};

-- Returns a function, that, as long as it continues to be invoked, will not
-- be triggered. The function will be called after it stops being called for
-- N milliseconds. If `immediate` is passed, trigger the function on the
-- leading edge, instead of the trailing.
_.debounce = function(func, wait, immediate) {
var timeout, result;
return function() {
  var context = this, args = arguments;
  var later = function() {
    timeout = null;
    if (!immediate) result = func.apply(context, args);
  };
  var callNow = immediate && !timeout;
  clearTimeout(timeout);
  timeout = setTimeout(later, wait);
  if (callNow) result = func.apply(context, args);
  return result;
};
};

-- Returns a function that will be executed at most one time, no matter how
-- often you call it. Useful for lazy initialization.
_.once = function(func) {
var ran = false, memo;
return function() {
  if (ran) return memo;
  ran = true;
  memo = func.apply(this, arguments);
  func = null;
  return memo;
};
};

-- Returns the first function passed as an argument to the second,
-- allowing you to adjust arguments, run code before and after, and
-- conditionally execute the original function.
_.wrap = function(func, wrapper) {
return function() {
  var args = [func];
  push.apply(args, arguments);
  return wrapper.apply(this, args);
};
};

-- Returns a function that is the composition of a list of functions, each
-- consuming the return value of the function that follows.
_.compose = function() {
var funcs = arguments;
return function() {
  var args = arguments;
  for (var i = funcs.length - 1; i >= 0; i--) {
    args = [funcs[i].apply(this, args)];
  }
  return args[0];
};
};

-- Returns a function that will only be executed after being called N times.
_.after = function(times, func) {
if (times <= 0) return func();
return function() {
  if (--times < 1) {
    return func.apply(this, arguments);
  }
};
};

-- Object Functions
-- ----------------

-- Retrieve the names of an object's properties.
-- Delegates to **ECMAScript 5**'s native `Object.keys`
_.keys = nativeKeys || function(obj) {
if (obj !== Object(obj)) throw new TypeError('Invalid object');
var keys = [];
for (var key in obj) if (_.has(obj, key)) keys[keys.length] = key;
return keys;
};

-- Retrieve the values of an object's properties.
_.values = function(obj) {
var values = [];
for (var key in obj) if (_.has(obj, key)) values.push(obj[key]);
return values;
};

-- Convert an object into a list of `[key, value]` pairs.
_.pairs = function(obj) {
var pairs = [];
for (var key in obj) if (_.has(obj, key)) pairs.push([key, obj[key]]);
return pairs;
};

-- Invert the keys and values of an object. The values must be serializable.
_.invert = function(obj) {
var result = {};
for (var key in obj) if (_.has(obj, key)) result[obj[key]] = key;
return result;
};

-- Return a sorted list of the function names available on the object.
-- Aliased as `methods`
_.functions = _.methods = function(obj) {
var names = [];
for (var key in obj) {
  if (_.isFunction(obj[key])) names.push(key);
}
return names.sort();
};

-- Extend a given object with all the properties in passed-in object(s).
_.extend = function(obj) {
each(slice.call(arguments, 1), function(source) {
  for (var prop in source) {
    obj[prop] = source[prop];
  }
});
return obj;
};

-- Return a copy of the object only containing the whitelisted properties.
_.pick = function(obj) {
var copy = {};
var keys = concat.apply(ArrayProto, slice.call(arguments, 1));
each(keys, function(key) {
  if (key in obj) copy[key] = obj[key];
});
return copy;
};

-- Return a copy of the object without the blacklisted properties.
_.omit = function(obj) {
var copy = {};
var keys = concat.apply(ArrayProto, slice.call(arguments, 1));
for (var key in obj) {
  if (!_.contains(keys, key)) copy[key] = obj[key];
}
return copy;
};

-- Fill in a given object with default properties.
_.defaults = function(obj) {
each(slice.call(arguments, 1), function(source) {
  for (var prop in source) {
    if (obj[prop] == null) obj[prop] = source[prop];
  }
});
return obj;
};

-- Create a (shallow-cloned) duplicate of an object.
_.clone = function(obj) {
if (!_.isObject(obj)) return obj;
return _.isArray(obj) ? obj.slice() : _.extend({}, obj);
};

-- Invokes interceptor with the obj, and then returns obj.
-- The primary purpose of this method is to "tap into" a method chain, in
-- order to perform operations on intermediate results within the chain.
_.tap = function(obj, interceptor) {
interceptor(obj);
return obj;
};

-- Internal recursive comparison function for `isEqual`.
var eq = function(a, b, aStack, bStack) {
-- Identical objects are equal. `0 === -0`, but they aren't identical.
-- See the Harmony `egal` proposal: http:--wiki.ecmascript.org/doku.php?id=harmony:egal.
if (a === b) return a !== 0 || 1 / a == 1 / b;
-- A strict comparison is necessary because `null == undefined`.
if (a == null || b == null) return a === b;
-- Unwrap any wrapped objects.
if (a instanceof _) a = a._wrapped;
if (b instanceof _) b = b._wrapped;
-- Compare `[[Class]]` names.
var className = toString.call(a);
if (className != toString.call(b)) return false;
switch (className) {
  -- Strings, numbers, dates, and booleans are compared by value.
  case '[object String]':
    -- Primitives and their corresponding object wrappers are equivalent; thus, `"5"` is
    -- equivalent to `new String("5")`.
    return a == String(b);
  case '[object Number]':
    -- `NaN`s are equivalent, but non-reflexive. An `egal` comparison is performed for
    -- other numeric values.
    return a != +a ? b != +b : (a == 0 ? 1 / a == 1 / b : a == +b);
  case '[object Date]':
  case '[object Boolean]':
    -- Coerce dates and booleans to numeric primitive values. Dates are compared by their
    -- millisecond representations. Note that invalid dates with millisecond representations
    -- of `NaN` are not equivalent.
    return +a == +b;
  -- RegExps are compared by their source patterns and flags.
  case '[object RegExp]':
    return a.source == b.source &&
           a.global == b.global &&
           a.multiline == b.multiline &&
           a.ignoreCase == b.ignoreCase;
}
if (typeof a != 'object' || typeof b != 'object') return false;
-- Assume equality for cyclic structures. The algorithm for detecting cyclic
-- structures is adapted from ES 5.1 section 15.12.3, abstract operation `JO`.
var length = aStack.length;
while (length--) {
  -- Linear search. Performance is inversely proportional to the number of
  -- unique nested structures.
  if (aStack[length] == a) return bStack[length] == b;
}
-- Add the first object to the stack of traversed objects.
aStack.push(a);
bStack.push(b);
var size = 0, result = true;
-- Recursively compare objects and arrays.
if (className == '[object Array]') {
  -- Compare array lengths to determine if a deep comparison is necessary.
  size = a.length;
  result = size == b.length;
  if (result) {
    -- Deep compare the contents, ignoring non-numeric properties.
    while (size--) {
      if (!(result = eq(a[size], b[size], aStack, bStack))) break;
    }
  }
} else {
  -- Objects with different constructors are not equivalent, but `Object`s
  -- from different frames are.
  var aCtor = a.constructor, bCtor = b.constructor;
  if (aCtor !== bCtor && !(_.isFunction(aCtor) && (aCtor instanceof aCtor) &&
                           _.isFunction(bCtor) && (bCtor instanceof bCtor))) {
    return false;
  }
  -- Deep compare objects.
  for (var key in a) {
    if (_.has(a, key)) {
      -- Count the expected number of properties.
      size++;
      -- Deep compare each member.
      if (!(result = _.has(b, key) && eq(a[key], b[key], aStack, bStack))) break;
    }
  }
  -- Ensure that both objects contain the same number of properties.
  if (result) {
    for (key in b) {
      if (_.has(b, key) && !(size--)) break;
    }
    result = !size;
  }
}
-- Remove the first object from the stack of traversed objects.
aStack.pop();
bStack.pop();
return result;
};

-- Perform a deep comparison to check if two objects are equal.
_.isEqual = function(a, b) {
return eq(a, b, [], []);
};

-- Is a given array, string, or object empty?
-- An "empty" object has no enumerable own-properties.
_.isEmpty = function(obj) {
if (obj == null) return true;
if (_.isArray(obj) || _.isString(obj)) return obj.length === 0;
for (var key in obj) if (_.has(obj, key)) return false;
return true;
};

-- Is a given value a DOM element?
_.isElement = function(obj) {
return !!(obj && obj.nodeType === 1);
};

-- Is a given value an array?
-- Delegates to ECMA5's native Array.isArray
_.isArray = nativeIsArray || function(obj) {
return toString.call(obj) == '[object Array]';
};

-- Is a given variable an object?
_.isObject = function(obj) {
return obj === Object(obj);
};

-- Add some isType methods: isArguments, isFunction, isString, isNumber, isDate, isRegExp.
each(['Arguments', 'Function', 'String', 'Number', 'Date', 'RegExp'], function(name) {
_['is' + name] = function(obj) {
  return toString.call(obj) == '[object ' + name + ']';
};
});

-- Define a fallback version of the method in browsers (ahem, IE), where
-- there isn't any inspectable "Arguments" type.
if (!_.isArguments(arguments)) {
_.isArguments = function(obj) {
  return !!(obj && _.has(obj, 'callee'));
};
}

-- Optimize `isFunction` if appropriate.
if (typeof (/./) !== 'function') {
_.isFunction = function(obj) {
  return typeof obj === 'function';
};
}

-- Is a given object a finite number?
_.isFinite = function(obj) {
return isFinite( obj ) && !isNaN( parseFloat(obj) );
};

-- Is the given value `NaN`? (NaN is the only number which does not equal itself).
_.isNaN = function(obj) {
return _.isNumber(obj) && obj != +obj;
};

-- Is a given value a boolean?
_.isBoolean = function(obj) {
return obj === true || obj === false || toString.call(obj) == '[object Boolean]';
};

-- Is a given value equal to null?
_.isNull = function(obj) {
return obj === null;
};

-- Is a given variable undefined?
_.isUndefined = function(obj) {
return obj === void 0;
};

-- Shortcut function for checking if an object has a given property directly
-- on itself (in other words, not on a prototype).
_.has = function(obj, key) {
return hasOwnProperty.call(obj, key);
};

-- Utility Functions
-- -----------------

-- Run Underscore.js in *noConflict* mode, returning the `_` variable to its
-- previous owner. Returns a reference to the Underscore object.
_.noConflict = function() {
root._ = previousUnderscore;
return this;
};

-- Keep the identity function around for default iterators.
_.identity = function(value) {
return value;
};

-- Run a function **n** times.
_.times = function(n, iterator, context) {
for (var i = 0; i < n; i++) iterator.call(context, i);
};

-- Return a random integer between min and max (inclusive).
_.random = function(min, max) {
if (max == null) {
  max = min;
  min = 0;
}
return min + (0 | Math.random() * (max - min + 1));
};

-- List of HTML entities for escaping.
var entityMap = {
escape: {
  '&': '&amp;',
  '<': '&lt;',
  '>': '&gt;',
  '"': '&quot;',
  "'": '&#x27;',
  '/': '&#x2F;'
}
};
entityMap.unescape = _.invert(entityMap.escape);

-- Regexes containing the keys and values listed immediately above.
var entityRegexes = {
escape:   new RegExp('[' + _.keys(entityMap.escape).join('') + ']', 'g'),
unescape: new RegExp('(' + _.keys(entityMap.unescape).join('|') + ')', 'g')
};

-- Functions for escaping and unescaping strings to/from HTML interpolation.
_.each(['escape', 'unescape'], function(method) {
_[method] = function(string) {
  if (string == null) return '';
  return ('' + string).replace(entityRegexes[method], function(match) {
    return entityMap[method][match];
  });
};
});

-- If the value of the named property is a function then invoke it;
-- otherwise, return it.
_.result = function(object, property) {
if (object == null) return null;
var value = object[property];
return _.isFunction(value) ? value.call(object) : value;
};

-- Add your own custom functions to the Underscore object.
_.mixin = function(obj) {
each(_.functions(obj), function(name){
  var func = _[name] = obj[name];
  _.prototype[name] = function() {
    var args = [this._wrapped];
    push.apply(args, arguments);
    return result.call(this, func.apply(_, args));
  };
});
};

-- Generate a unique integer id (unique within the entire client session).
-- Useful for temporary DOM ids.
var idCounter = 0;
_.uniqueId = function(prefix) {
var id = idCounter++;
return prefix ? prefix + id : id;
};

-- By default, Underscore uses ERB-style template delimiters, change the
-- following template settings to use alternative delimiters.
_.templateSettings = {
evaluate    : /<%([\s\S]+?)%>/g,
interpolate : /<%=([\s\S]+?)%>/g,
escape      : /<%-([\s\S]+?)%>/g
};

-- When customizing `templateSettings`, if you don't want to define an
-- interpolation, evaluation or escaping regex, we need one that is
-- guaranteed not to match.
var noMatch = /(.)^/;

-- Certain characters need to be escaped so that they can be put into a
-- string literal.
var escapes = {
"'":      "'",
'\\':     '\\',
'\r':     'r',
'\n':     'n',
'\t':     't',
'\u2028': 'u2028',
'\u2029': 'u2029'
};

var escaper = /\\|'|\r|\n|\t|\u2028|\u2029/g;

-- JavaScript micro-templating, similar to John Resig's implementation.
-- Underscore templating handles arbitrary delimiters, preserves whitespace,
-- and correctly escapes quotes within interpolated code.
_.template = function(text, data, settings) {
settings = _.defaults({}, settings, _.templateSettings);

-- Combine delimiters into one regular expression via alternation.
var matcher = new RegExp([
  (settings.escape || noMatch).source,
  (settings.interpolate || noMatch).source,
  (settings.evaluate || noMatch).source
].join('|') + '|$', 'g');

-- Compile the template source, escaping string literals appropriately.
var index = 0;
var source = "__p+='";
text.replace(matcher, function(match, escape, interpolate, evaluate, offset) {
  source += text.slice(index, offset)
    .replace(escaper, function(match) { return '\\' + escapes[match]; });
  source +=
    escape ? "'+\n((__t=(" + escape + "))==null?'':_.escape(__t))+\n'" :
    interpolate ? "'+\n((__t=(" + interpolate + "))==null?'':__t)+\n'" :
    evaluate ? "';\n" + evaluate + "\n__p+='" : '';
  index = offset + match.length;
});
source += "';\n";

-- If a variable is not specified, place data values in local scope.
if (!settings.variable) source = 'with(obj||{}){\n' + source + '}\n';

source = "var __t,__p='',__j=Array.prototype.join," +
  "print=function(){__p+=__j.call(arguments,'');};\n" +
  source + "return __p;\n";

try {
  var render = new Function(settings.variable || 'obj', '_', source);
} catch (e) {
  e.source = source;
  throw e;
}

if (data) return render(data, _);
var template = function(data) {
  return render.call(this, data, _);
};

-- Provide the compiled function source as a convenience for precompilation.
template.source = 'function(' + (settings.variable || 'obj') + '){\n' + source + '}';

return template;
};

-- Add a "chain" function, which will delegate to the wrapper.
_.chain = function(obj) {
return _(obj).chain();
};

-- OOP
-- ---------------
-- If Underscore is called as a function, it returns a wrapped object that
-- can be used OO-style. This wrapper holds altered versions of all the
-- underscore functions. Wrapped objects may be chained.

-- Helper function to continue chaining intermediate results.
var result = function(obj) {
return this._chain ? _(obj).chain() : obj;
};

-- Add all of the Underscore functions to the wrapper object.
_.mixin(_);

-- Add all mutator Array functions to the wrapper.
each(['pop', 'push', 'reverse', 'shift', 'sort', 'splice', 'unshift'], function(name) {
var method = ArrayProto[name];
_.prototype[name] = function() {
  var obj = this._wrapped;
  method.apply(obj, arguments);
  if ((name == 'shift' || name == 'splice') && obj.length === 0) delete obj[0];
  return result.call(this, obj);
};
});

-- Add all accessor Array functions to the wrapper.
each(['concat', 'join', 'slice'], function(name) {
var method = ArrayProto[name];
_.prototype[name] = function() {
  return result.call(this, method.apply(this._wrapped, arguments));
};
});

_.extend(_.prototype, {

-- Start chaining a wrapped Underscore object.
chain: function() {
  this._chain = true;
  return this;
},

-- Extracts the result from a wrapped and chained object.
value: function() {
  return this._wrapped;
}

});

//  Underscore.string
//  (c) 2010 Esa-Matti Suuronen <esa-matti aet suuronen dot org>
//  Underscore.string is freely distributable under the terms of the MIT license.
//  Documentation: https://github.com/epeli/underscore.string
//  Some code is borrowed from MooTools and Alexandru Marasteanu.
//  Version '2.3.0'

!function(root, String){
  'use strict';

  // Defining helper functions.

  var nativeTrim = String.prototype.trim;
  var nativeTrimRight = String.prototype.trimRight;
  var nativeTrimLeft = String.prototype.trimLeft;

  var parseNumber = function(source) { return source * 1 || 0; };

  var strRepeat = function(str, qty){
    if (qty < 1) return '';
    var result = '';
    while (qty > 0) {
      if (qty & 1) result += str;
      qty >>= 1, str += str;
    }
    return result;
  };

  var slice = [].slice;

  var defaultToWhiteSpace = function(characters) {
    if (characters == null)
      return '\\s';
    else if (characters.source)
      return characters.source;
    else
      return '[' + _s.escapeRegExp(characters) + ']';
  };

  var escapeChars = {
    lt: '<',
    gt: '>',
    quot: '"',
    apos: "'",
    amp: '&'
  };

  var reversedEscapeChars = {};
  for(var key in escapeChars){ reversedEscapeChars[escapeChars[key]] = key; }

  // sprintf() for JavaScript 0.7-beta1
  // http://www.diveintojavascript.com/projects/javascript-sprintf
  //
  // Copyright (c) Alexandru Marasteanu <alexaholic [at) gmail (dot] com>
  // All rights reserved.

  var sprintf = (function() {
    function get_type(variable) {
      return Object.prototype.toString.call(variable).slice(8, -1).toLowerCase();
    }

    var str_repeat = strRepeat;

    var str_format = function() {
      if (!str_format.cache.hasOwnProperty(arguments[0])) {
        str_format.cache[arguments[0]] = str_format.parse(arguments[0]);
      }
      return str_format.format.call(null, str_format.cache[arguments[0]], arguments);
    };

    str_format.format = function(parse_tree, argv) {
      var cursor = 1, tree_length = parse_tree.length, node_type = '', arg, output = [], i, k, match, pad, pad_character, pad_length;
      for (i = 0; i < tree_length; i++) {
        node_type = get_type(parse_tree[i]);
        if (node_type === 'string') {
          output.push(parse_tree[i]);
        }
        else if (node_type === 'array') {
          match = parse_tree[i]; // convenience purposes only
          if (match[2]) { // keyword argument
            arg = argv[cursor];
            for (k = 0; k < match[2].length; k++) {
              if (!arg.hasOwnProperty(match[2][k])) {
                throw new Error(sprintf('[_.sprintf] property "%s" does not exist', match[2][k]));
              }
              arg = arg[match[2][k]];
            }
          } else if (match[1]) { // positional argument (explicit)
            arg = argv[match[1]];
          }
          else { // positional argument (implicit)
            arg = argv[cursor++];
          }

          if (/[^s]/.test(match[8]) && (get_type(arg) != 'number')) {
            throw new Error(sprintf('[_.sprintf] expecting number but found %s', get_type(arg)));
          }
          switch (match[8]) {
            case 'b': arg = arg.toString(2); break;
            case 'c': arg = String.fromCharCode(arg); break;
            case 'd': arg = parseInt(arg, 10); break;
            case 'e': arg = match[7] ? arg.toExponential(match[7]) : arg.toExponential(); break;
            case 'f': arg = match[7] ? parseFloat(arg).toFixed(match[7]) : parseFloat(arg); break;
            case 'o': arg = arg.toString(8); break;
            case 's': arg = ((arg = String(arg)) && match[7] ? arg.substring(0, match[7]) : arg); break;
            case 'u': arg = Math.abs(arg); break;
            case 'x': arg = arg.toString(16); break;
            case 'X': arg = arg.toString(16).toUpperCase(); break;
          }
          arg = (/[def]/.test(match[8]) && match[3] && arg >= 0 ? '+'+ arg : arg);
          pad_character = match[4] ? match[4] == '0' ? '0' : match[4].charAt(1) : ' ';
          pad_length = match[6] - String(arg).length;
          pad = match[6] ? str_repeat(pad_character, pad_length) : '';
          output.push(match[5] ? arg + pad : pad + arg);
        }
      }
      return output.join('');
    };

    str_format.cache = {};

    str_format.parse = function(fmt) {
      var _fmt = fmt, match = [], parse_tree = [], arg_names = 0;
      while (_fmt) {
        if ((match = /^[^\x25]+/.exec(_fmt)) !== null) {
          parse_tree.push(match[0]);
        }
        else if ((match = /^\x25{2}/.exec(_fmt)) !== null) {
          parse_tree.push('%');
        }
        else if ((match = /^\x25(?:([1-9]\d*)\$|\(([^\)]+)\))?(\+)?(0|'[^$])?(-)?(\d+)?(?:\.(\d+))?([b-fosuxX])/.exec(_fmt)) !== null) {
          if (match[2]) {
            arg_names |= 1;
            var field_list = [], replacement_field = match[2], field_match = [];
            if ((field_match = /^([a-z_][a-z_\d]*)/i.exec(replacement_field)) !== null) {
              field_list.push(field_match[1]);
              while ((replacement_field = replacement_field.substring(field_match[0].length)) !== '') {
                if ((field_match = /^\.([a-z_][a-z_\d]*)/i.exec(replacement_field)) !== null) {
                  field_list.push(field_match[1]);
                }
                else if ((field_match = /^\[(\d+)\]/.exec(replacement_field)) !== null) {
                  field_list.push(field_match[1]);
                }
                else {
                  throw new Error('[_.sprintf] huh?');
                }
              }
            }
            else {
              throw new Error('[_.sprintf] huh?');
            }
            match[2] = field_list;
          }
          else {
            arg_names |= 2;
          }
          if (arg_names === 3) {
            throw new Error('[_.sprintf] mixing positional and named placeholders is not (yet) supported');
          }
          parse_tree.push(match);
        }
        else {
          throw new Error('[_.sprintf] huh?');
        }
        _fmt = _fmt.substring(match[0].length);
      }
      return parse_tree;
    };

    return str_format;
  })();



  // Defining underscore.string

  var _s = {

    VERSION: '2.3.0',

    isBlank: function(str){
      if (str == null) str = '';
      return (/^\s*$/).test(str);
    },

    stripTags: function(str){
      if (str == null) return '';
      return String(str).replace(/<\/?[^>]+>/g, '');
    },

    capitalize : function(str){
      str = str == null ? '' : String(str);
      return str.charAt(0).toUpperCase() + str.slice(1);
    },

    chop: function(str, step){
      if (str == null) return [];
      str = String(str);
      step = ~~step;
      return step > 0 ? str.match(new RegExp('.{1,' + step + '}', 'g')) : [str];
    },

    clean: function(str){
      return _s.strip(str).replace(/\s+/g, ' ');
    },

    count: function(str, substr){
      if (str == null || substr == null) return 0;

      str = String(str);
      substr = String(substr);

      var count = 0,
        pos = 0,
        length = substr.length;

      while (true) {
        pos = str.indexOf(substr, pos);
        if (pos === -1) break;
        count++;
        pos += length;
      }

      return count;
    },

    chars: function(str) {
      if (str == null) return [];
      return String(str).split('');
    },

    swapCase: function(str) {
      if (str == null) return '';
      return String(str).replace(/\S/g, function(c){
        return c === c.toUpperCase() ? c.toLowerCase() : c.toUpperCase();
      });
    },

    escapeHTML: function(str) {
      if (str == null) return '';
      return String(str).replace(/[&<>"']/g, function(m){ return '&' + reversedEscapeChars[m] + ';'; });
    },

    unescapeHTML: function(str) {
      if (str == null) return '';
      return String(str).replace(/\&([^;]+);/g, function(entity, entityCode){
        var match;

        if (entityCode in escapeChars) {
          return escapeChars[entityCode];
        } else if (match = entityCode.match(/^#x([\da-fA-F]+)$/)) {
          return String.fromCharCode(parseInt(match[1], 16));
        } else if (match = entityCode.match(/^#(\d+)$/)) {
          return String.fromCharCode(~~match[1]);
        } else {
          return entity;
        }
      });
    },

    escapeRegExp: function(str){
      if (str == null) return '';
      return String(str).replace(/([.*+?^=!:${}()|[\]\/\\])/g, '\\$1');
    },

    splice: function(str, i, howmany, substr){
      var arr = _s.chars(str);
      arr.splice(~~i, ~~howmany, substr);
      return arr.join('');
    },

    insert: function(str, i, substr){
      return _s.splice(str, i, 0, substr);
    },

    include: function(str, needle){
      if (needle === '') return true;
      if (str == null) return false;
      return String(str).indexOf(needle) !== -1;
    },

    join: function() {
      var args = slice.call(arguments),
        separator = args.shift();

      if (separator == null) separator = '';

      return args.join(separator);
    },

    lines: function(str) {
      if (str == null) return [];
      return String(str).split("\n");
    },

    reverse: function(str){
      return _s.chars(str).reverse().join('');
    },

    startsWith: function(str, starts){
      if (starts === '') return true;
      if (str == null || starts == null) return false;
      str = String(str); starts = String(starts);
      return str.length >= starts.length && str.slice(0, starts.length) === starts;
    },

    endsWith: function(str, ends){
      if (ends === '') return true;
      if (str == null || ends == null) return false;
      str = String(str); ends = String(ends);
      return str.length >= ends.length && str.slice(str.length - ends.length) === ends;
    },

    succ: function(str){
      if (str == null) return '';
      str = String(str);
      return str.slice(0, -1) + String.fromCharCode(str.charCodeAt(str.length-1) + 1);
    },

    titleize: function(str){
      if (str == null) return '';
      return String(str).replace(/(?:^|\s)\S/g, function(c){ return c.toUpperCase(); });
    },

    camelize: function(str){
      return _s.trim(str).replace(/[-_\s]+(.)?/g, function(match, c){ return c.toUpperCase(); });
    },

    underscored: function(str){
      return _s.trim(str).replace(/([a-z\d])([A-Z]+)/g, '$1_$2').replace(/[-\s]+/g, '_').toLowerCase();
    },

    dasherize: function(str){
      return _s.trim(str).replace(/([A-Z])/g, '-$1').replace(/[-_\s]+/g, '-').toLowerCase();
    },

    classify: function(str){
      return _s.titleize(String(str).replace(/_/g, ' ')).replace(/\s/g, '');
    },

    humanize: function(str){
      return _s.capitalize(_s.underscored(str).replace(/_id$/,'').replace(/_/g, ' '));
    },

    trim: function(str, characters){
      if (str == null) return '';
      if (!characters && nativeTrim) return nativeTrim.call(str);
      characters = defaultToWhiteSpace(characters);
      return String(str).replace(new RegExp('\^' + characters + '+|' + characters + '+$', 'g'), '');
    },

    ltrim: function(str, characters){
      if (str == null) return '';
      if (!characters && nativeTrimLeft) return nativeTrimLeft.call(str);
      characters = defaultToWhiteSpace(characters);
      return String(str).replace(new RegExp('^' + characters + '+'), '');
    },

    rtrim: function(str, characters){
      if (str == null) return '';
      if (!characters && nativeTrimRight) return nativeTrimRight.call(str);
      characters = defaultToWhiteSpace(characters);
      return String(str).replace(new RegExp(characters + '+$'), '');
    },

    truncate: function(str, length, truncateStr){
      if (str == null) return '';
      str = String(str); truncateStr = truncateStr || '...';
      length = ~~length;
      return str.length > length ? str.slice(0, length) + truncateStr : str;
    },

    /**
     * _s.prune: a more elegant version of truncate
     * prune extra chars, never leaving a half-chopped word.
     * @author github.com/rwz
     */
    prune: function(str, length, pruneStr){
      if (str == null) return '';

      str = String(str); length = ~~length;
      pruneStr = pruneStr != null ? String(pruneStr) : '...';

      if (str.length <= length) return str;

      var tmpl = function(c){ return c.toUpperCase() !== c.toLowerCase() ? 'A' : ' '; },
        template = str.slice(0, length+1).replace(/.(?=\W*\w*$)/g, tmpl); // 'Hello, world' -> 'HellAA AAAAA'

      if (template.slice(template.length-2).match(/\w\w/))
        template = template.replace(/\s*\S+$/, '');
      else
        template = _s.rtrim(template.slice(0, template.length-1));

      return (template+pruneStr).length > str.length ? str : str.slice(0, template.length)+pruneStr;
    },

    words: function(str, delimiter) {
      if (_s.isBlank(str)) return [];
      return _s.trim(str, delimiter).split(delimiter || /\s+/);
    },

    pad: function(str, length, padStr, type) {
      str = str == null ? '' : String(str);
      length = ~~length;

      var padlen  = 0;

      if (!padStr)
        padStr = ' ';
      else if (padStr.length > 1)
        padStr = padStr.charAt(0);

      switch(type) {
        case 'right':
          padlen = length - str.length;
          return str + strRepeat(padStr, padlen);
        case 'both':
          padlen = length - str.length;
          return strRepeat(padStr, Math.ceil(padlen/2)) + str
                  + strRepeat(padStr, Math.floor(padlen/2));
        default: // 'left'
          padlen = length - str.length;
          return strRepeat(padStr, padlen) + str;
        }
    },

    lpad: function(str, length, padStr) {
      return _s.pad(str, length, padStr);
    },

    rpad: function(str, length, padStr) {
      return _s.pad(str, length, padStr, 'right');
    },

    lrpad: function(str, length, padStr) {
      return _s.pad(str, length, padStr, 'both');
    },

    sprintf: sprintf,

    vsprintf: function(fmt, argv){
      argv.unshift(fmt);
      return sprintf.apply(null, argv);
    },

    toNumber: function(str, decimals) {
      if (str == null || str == '') return 0;
      str = String(str);
      var num = parseNumber(parseNumber(str).toFixed(~~decimals));
      return num === 0 && !str.match(/^0+$/) ? Number.NaN : num;
    },

    numberFormat : function(number, dec, dsep, tsep) {
      if (isNaN(number) || number == null) return '';

      number = number.toFixed(~~dec);
      tsep = tsep || ',';

      var parts = number.split('.'), fnums = parts[0],
        decimals = parts[1] ? (dsep || '.') + parts[1] : '';

      return fnums.replace(/(\d)(?=(?:\d{3})+$)/g, '$1' + tsep) + decimals;
    },

    strRight: function(str, sep){
      if (str == null) return '';
      str = String(str); sep = sep != null ? String(sep) : sep;
      var pos = !sep ? -1 : str.indexOf(sep);
      return ~pos ? str.slice(pos+sep.length, str.length) : str;
    },

    strRightBack: function(str, sep){
      if (str == null) return '';
      str = String(str); sep = sep != null ? String(sep) : sep;
      var pos = !sep ? -1 : str.lastIndexOf(sep);
      return ~pos ? str.slice(pos+sep.length, str.length) : str;
    },

    strLeft: function(str, sep){
      if (str == null) return '';
      str = String(str); sep = sep != null ? String(sep) : sep;
      var pos = !sep ? -1 : str.indexOf(sep);
      return ~pos ? str.slice(0, pos) : str;
    },

    strLeftBack: function(str, sep){
      if (str == null) return '';
      str += ''; sep = sep != null ? ''+sep : sep;
      var pos = str.lastIndexOf(sep);
      return ~pos ? str.slice(0, pos) : str;
    },

    toSentence: function(array, separator, lastSeparator, serial) {
      separator = separator || ', '
      lastSeparator = lastSeparator || ' and '
      var a = array.slice(), lastMember = a.pop();

      if (array.length > 2 && serial) lastSeparator = _s.rtrim(separator) + lastSeparator;

      return a.length ? a.join(separator) + lastSeparator + lastMember : lastMember;
    },

    toSentenceSerial: function() {
      var args = slice.call(arguments);
      args[3] = true;
      return _s.toSentence.apply(_s, args);
    },

    slugify: function(str) {
      if (str == null) return '';

      var from  = "ąàáäâãåæćęèéëêìíïîłńòóöôõøùúüûñçżź",
          to    = "aaaaaaaaceeeeeiiiilnoooooouuuunczz",
          regex = new RegExp(defaultToWhiteSpace(from), 'g');

      str = String(str).toLowerCase().replace(regex, function(c){
        var index = from.indexOf(c);
        return to.charAt(index) || '-';
      });

      return _s.dasherize(str.replace(/[^\w\s-]/g, ''));
    },

    surround: function(str, wrapper) {
      return [wrapper, str, wrapper].join('');
    },

    quote: function(str) {
      return _s.surround(str, '"');
    },

    exports: function() {
      var result = {};

      for (var prop in this) {
        if (!this.hasOwnProperty(prop) || prop.match(/^(?:include|contains|reverse)$/)) continue;
        result[prop] = this[prop];
      }

      return result;
    },

    repeat: function(str, qty, separator){
      if (str == null) return '';

      qty = ~~qty;

      // using faster implementation if separator is not needed;
      if (separator == null) return strRepeat(String(str), qty);

      // this one is about 300x slower in Google Chrome
      for (var repeat = []; qty > 0; repeat[--qty] = str) {}
      return repeat.join(separator);
    },

    levenshtein: function(str1, str2) {
      if (str1 == null && str2 == null) return 0;
      if (str1 == null) return String(str2).length;
      if (str2 == null) return String(str1).length;

      str1 = String(str1); str2 = String(str2);

      var current = [], prev, value;

      for (var i = 0; i <= str2.length; i++)
        for (var j = 0; j <= str1.length; j++) {
          if (i && j)
            if (str1.charAt(j - 1) === str2.charAt(i - 1))
              value = prev;
            else
              value = Math.min(current[j], current[j - 1], prev) + 1;
          else
            value = i + j;

          prev = current[j];
          current[j] = value;
        }

      return current.pop();
    }
  };

  // Aliases

  _s.strip    = _s.trim;
  _s.lstrip   = _s.ltrim;
  _s.rstrip   = _s.rtrim;
  _s.center   = _s.lrpad;
  _s.rjust    = _s.lpad;
  _s.ljust    = _s.rpad;
  _s.contains = _s.include;
  _s.q        = _s.quote;

  // Exporting

  // CommonJS module is defined
  if (typeof exports !== 'undefined') {
    if (typeof module !== 'undefined' && module.exports)
      module.exports = _s;

    exports._s = _s;
  }

  // Register as a named module with AMD.
  if (typeof define === 'function' && define.amd)
    define('underscore.string', [], function(){ return _s; });


  // Integrate with Underscore.js if defined
  // or create our own underscore object.
  root._ = root._ || {};
  root._.string = root._.str = _s;
}(this, String);



return m
