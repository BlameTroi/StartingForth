\ ch01.fs -- Fundamental Forth -- T.Brumley

\ Definitions from working through the chapter along with any
\ solved problems.

\ Chapter 1 is an introduction to the basic syntax of Forth.

\ Some of these words are used in later examples:

: star 42 emit ;

: stars                \ n -- , stars to print
   0 max 512 min       \ not in text, but protect typos
   0 do star loop ;

: margin
   cr 30 spaces ;

: blip
   margin
   star ;

: bar
   margin
   5 stars ;

: f                     \ composition
   bar
   blip
   bar
   blip
   blip
   cr ;

\ Problems: 

\ 1. Define `gift` to print some gift name, `giver` to print then
\ name of a person giving a gift, and `thanks` using `gift` and
\ `giver` to print a thank you message.

: gift   ( -- )  ." gaming dice" ;
: giver  ( -- )  ." Stephanie" ;
: thanks ( -- )
   cr ." Dear " giver ." ," cr
   5 spaces ." Thanks for the " gift ." ." ;

\ 2. Define ten-less that takes the number on the stack, subtracts
\ 10, and leaves the result on the. (NOTE: `-` has not been
\ introduced, so use `+` instead.)

: ten-less   ( n -- n-10 )
   -10 + ;

\ 3. Redefine `giver` to use a different name and run `thanks`
\ again. Why is the old definition of `giver` still used by
\ `thanks`.

\ Order matters. When a word is compiled, it is bound to the words
\ before it in the dictionary. The new definition shadows the
\ old. A bit like closures.

\ ASIDE: Accidental security against malicious patching by
\ redefining words.

\ end ch01.fs
