# Modules

### math.bas
ABS(x)

SIN(x), COS(x), TAN(x)

SQRT(x), POW(x, y)

RND(), RND(max), RND(min, max)

FLOOR(x), CEIL(x)

PI, E — как константы

можно реализовать как нативные вызовы (сопоставление в VM → System.Math)

### str.bas
UPPER(s), LOWER(s)

TRIM(s)

LEN(s)

LEFT(s, n), RIGHT(s, n)

REPLACE(s, a, b)

CONTAINS(s, sub)

SPLIT(s, sep) → array

### fs.bas
обёртки над FSREAD, FSWRITE, FSEXISTS:
WRITEFILE, APPENDFILE, DELETEFILE, READLINES(path)

### time.bas
NOW() → string (ISO-8601)

UNIX() → int (unix time)

SLEEP(ms) → блокировка (system call)

FORMAT_TIME(ticks) → "HH:MM:SS"

### util.bas
SWAP(a, b)

MAX(a, b), MIN(a, b)

CLAMP(x, min, max)

ISNUM(x), ISSTR(x) — alias к опкоду

