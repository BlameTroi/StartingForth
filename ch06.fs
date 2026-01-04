\ ch06.fs -- Throw it for a Loop -- T.Brumley

\ Chapter 6 builds on Chapter 4's decision making examples with
\ the introduction of loops. This was pretty straight forward.
\ The only real problem I had was selecting between some of the
\ conditional loops, and confusing EXIT and LEAVE.

\ Discussion starts with definite loops or counting loops. In
\ Basic and other languages these are FOR loops, but Forth
\ follows the Fortran and PL/I convention of DO.
\
\ The main thing to understand is how the parameter stack is
\ used to hold the indices. So in
\
\ : do-must-be-in-defintion 10 0 DO 42 emit LOOP cr ; 
\
\ 10 and 0 are moved to the return stack with the same
\ ordering. 0 is top of stack. The index (top of stack) is
\ incremented at each LOOP and then compared with the second
\ value on the return stack.

\ Forth uses the terminology index and limit for these values.

\ Like the Pascal FOR-DO the index is always incremented by 1.
\ Unlike Pascal looping stops when index = limit when going up
\ or is greater than the limit, where in Pascal the limit
\ iteration is taken.
\
\ When going down, the limit iteration is taken.
\
\ end if index >= limit       for increasing
\ end if index < limit        for decreasing
\
\ so:

: upness    10 0 do i .  1 +loop ; \ prints 0 1 2 ... 7 8 9 

: downess  -10 0 do i . -1 +loop ; \ prints 0 -1 -2 ... -9 -10

\ A DO loop always executes at least once. The check is
\ trailing.
\
\ The above end rules combined with "at least once" produces
\ the following:

: thinkaboutit   100 10 do i . -1 +loop ; \ prints 10

\ Since the index is conceptully incremented (moves toward the
\ limit) the limit should be greater in magnitude than the
\ starting index.

\ The use of I (and by inference I' and J) is discussed. The
\ implementation details may be wrong in modern Forths so don't
\ worry about which stack item I grabs. I just trust that it is
\ the index.
\
\ I' is not in the standard. I believe R@ does what I' did.
\
\ Leaving data on the stack across iterations is allowed and
\ often makes sense.
\
\ This compound interest example demonstrates these techniques.

: r%  ( n % -- n% )      \ as in chapter 5, 2 places and round
   swap 100 * * 500 + 10000 / ;

: compound  ( amt int -- )
   swap 21 1 do                \ int amt | 21 1
      ." year " i . 3 spaces   \ no change       <-access index
      2dup                     \ int amt int amt <-prep
      r%                       \ int amt %       <-calc
      +                        \ int new-amt     <-set up next
      dup ." balance " . cr    \ int new-amt
   loop                        \ int new-amt | 21 2 <-test/iter
   2drop ;                     \ clear accumulators

\ amt and int ore swapped to simplify stack manipulation within
\ the loop--it's easier to sum the running balance with the
\ amount on top. As written, R% is order agnostic.

\ There's a brief discussion of using the index as a condition
\ for an IF statement. The example of line breaking output is
\ given. Print 256 asterisks with a cr after every 16th.

: rectangle   256 0 do
   i 16 mod 0= if cr
   then ." *" loop ;

\ Of course, loops can be nested. The standard doesn't mention
\ any specified nesting limit.

: multiplications  cr 11 1 do dup i * 5 u.r loop drop ; 

\ There is a legacy definition of TABLE in gforth. Ignore the
\ warning.

: table  cr 11 1 do i multiplications loop ;

\ The above can also be done this way.

: table-2
   cr 11 1 do
      11 1 do
         i j * 5 u.r     \ u.r ( n width -- ) prints n in width
      loop
      cr
   loop ;

\ The index increment (it is ALWAYS an increment) doesn't have
\ to be 1. Indeed it can be negative, but that's on you.

: pentajumps   50 0 do i . 5 +loop ;

: falling   -10 0 do i . -1 +loop ;

\ Increasing index, the loop terminates when the index has
\ reached or exceeded the limit.

: upness-2 5 0 do i . 1 +loop ; \ 0 1 2 3 4

\ Decreasing index, the loop terminates when the index has
\ passed the limit.

: downness-2 -5 0 do i . -1 +loop ; \ 0 1 2 3 4 5

\ Indefinite loops -- or non counting loops -- are those that
\ end based on a true or false test, or not at all.
\
\ Forth has two of these.
\
\ BEGIN <code> <test> UNTIL -- like the repeat until in Pascal
\
\ BEGIN <code> <test> WHILE <code> REPEAT  -- like a while loop
\
\ And any of these can become an infinite loop by replacing
\ <test> with false or 0.

\ Early exit from a loop.
\
\ The word leave will "jump" to the end of the current loop.
\
\   : earlyexit
\   100 0 do
\      i 50 > if
\         ." leaving" cr leave
\      then
\      ." still here" cr
\   loop ." and out" cr ;
\
\ prints still here still here leaving and out


\ Chapter 6 problems.
\
\ The problems are finally getting more interesting.


\ 1-4 are a group
\ 1. Write a word STARS that prints n stars.
\ 2. Define BOX that prints some number of lines of STARS.
\ 3. Define \SLANT that prints some number of 10 star lines at
\    a right downard slant.
\ 4. And /SLANT, left upward slant.

: stars ( n -- ) 0 do 42 emit loop ;

: box ( w h -- ) cr 0 do dup stars cr loop drop ;

: \stars ( n -- )
   cr 0 swap 0 do dup spaces 1+ 10 stars cr loop drop ;

: /stars ( n -- )
   cr
   dup 0 do           \ n n 0 -- n
      1- dup spaces 10 stars cr
   loop
   drop ;             \ n --

\ 5. Rewrite \STARS using BEGIN UNTIL and BEGIN WHILE REPEAT.
\ In all of these, the number of spaces to achieve the slant
\ effect is line# +/- n-1.

: /stars2 ( n -- )     \ while loop, leading test
   cr
   begin
      dup 0 >
   while
      1- dup spaces
      10 stars cr
   repeat
   drop ;

: /stars3 ( n -- )     \ while loop, leading test
   cr
   begin
      1- dup spaces      \ n-1
      10 stars cr
      dup 1 <
   until
   drop ;


\ 6. Write a word DIAMONDS that prints n large diamonds made up
\ of lines of asterisks: from 1 to 19, then 19 to 1, making a
\ diamond.

: diamond ( -- ) \ factor factor factor
   20 1 do 9 i 2 / - spaces i stars cr 2 +loop
   1 19 do 9 i 2 / - spaces i stars cr -2 +loop ;

: diamonds ( n -- )
   cr 0 do diamond loop ;


\ 7. Write a word DOUBLED that conditionally leaves the loop
\ once the balance is doubled. I was too lazy to do a lot of
\ stack juggling and used a DO LOOP, which is how the book
\ solution is written.

: doubled          \ book
   6 1000
   21 0 do
      cr ." year " i 2 u.r
      2dup swap r% + dup ."  balance " .
      dup 2000 > if
         cr cr ." more than doubled in " i 1+ . ." years "
         leave
      then
   loop
   2drop ;

\ better formatting and no assumption on how long it will take
\ to double the balance.

: doubler ( amt int -- )
   over 2* rot rot swap 999 1 do
      cr ." year " i 2 u.r 3 spaces
      2dup r% + dup ." balance " .
      dup 2over drop > if
         cr cr ." more than doubled in " i . ."  years "
         leave
      then loop 2drop drop ;


\ 8. Define a word ** that computes whole number exponential
\ values. My try works but I don't like it much. The book
\ version is cleaner. Stack juggling is clouding things for
\ me.

: ** ( n p -- n^p )
   dup 2 < if          \ n ** 1 is n, treat invalid as same
      drop
   else                \ n p
      over            \ n p n
      dup             \ n p n n
      rot 1           \ n n n p 1
      do              \ n n ni       s/b n n^i
         over *
      loop
      swap drop       \ n^p
   then
   swap drop ;         \ TODO: I'm carrying an extra n?


\ Here's the book's version, much cleaner. I would probably
\ gate this for p < 1.

: **book            \ n p -- n^p
   1- ?dup if       \ either n p-1 p-1  -or- n 0
      over          \ n p-1 n
      rot rot       \ n n p-1
      0 do
         over *     \ n np*n
      loop
      swap drop     \ n
   then ;

\ End of ch06.fs.
