\ ch04.fth -- Decisions, Decisions -- T.Brumley

marker ch04

\ Chapter 4 covers decisions and branching. Here we start to
\ see "programming".
\
\ Some key points covered in the text:
\
\ No GOTO statement.
\
\ When checking a value, the pattern is v dup test if ..... then 
\ 
\ Truth values were 0 and 1, with the C convention that anything
\ that is not false is true. Modern Forth uses -1 for true but the
\ not false is true behavior is still available.
\
\ The usual explanations of boolean operators instead of addition
\ or subtraction.
\ 

\ As of the early 1980s the ounces per dozen eggs table for
\ sizing was:
\
\ Extra Large         27-30
\ Large               24-27
\ Medium              21-24
\ Small               18-21

\ Problems

\ 3. bar carding function.

: card ( n -- , is a person aged n allowed to purchase alcohol? )
    21 <
    if ." under age"
    else ." drink away"
    then ;

\ 4. Print number sign.

: sign.test ( nn -- , prints positive, negative, or zero )
    dup if
        0< if ." negative" else ." positive" then
    else ." zero" drop then
    ;

\ 5. Guard our original stars from chapter 1 so that it
\ will not print anything for zero or negative n.

\ prior definition of stars ... second shadows first.

: stars ( n -- , print n stars )
    0 do 42 emit loop ; 

: stars ( n -- , protect prior stars from 0 )
    dup if abs stars else drop then ;

\ 6. Write within (n low high -- flag , n in [low..high)
\ 
\ within and within? are already taken, so we'll use
\ ?range which has the same semantics as within
\ low <= n < high

: ?range ( n low high -- flag , is n in low <= n < high )
    rot dup rot ( low n n high )
    <           ( low n flag-h )
    -rot        ( flag-h low n )
    <=          ( flag-h flag-n )
    and ;       ( passed? )
    
\ 7. Wirte a guess the number game (yawn).

: guess ( n g -- )
    2dup = if
        ." correct!"
        2drop
    else
        over > if              \ n g n -- n t/f
            ." you are high"
        else
            ." you are low"
        then
    then ;

\ 8. Using nested tests with if/else/then write a definition
\ speller that spells out one through four, prefixing with
\ negative when appropriate.

: speller ( n -- )
    dup
    -4 5 ?range if
        dup abs swap
        0 < if ." negative " then
        dup 4 = if ." four"   else
        dup 3 = if ." three"  else
        dup 2 = if ." two"    else
        dup 1 = if ." one"    else
                   ." zero"
        then then then then drop
    else
        ." out of range dude" drop
    then ;

\ 9. Trap could be used as part of binary search algorithm,
\ print between, not between, or match (only possible when
\ low = high). Use ?range and remember that it is a right
\ open interval.

: trap ( n low high -- n until matched )
    3dup 1+ ?range
    if
        = if
            ." you got it" drop
        else
            ." between"
        then
    else
       ." not between" 2drop
    then ;      

\ End ch04.fth.
