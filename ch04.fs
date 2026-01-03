\ ch04.fs -- Decisions, Decisions -- T.Brumley

\ Chapter 4 covers decisions and branching. Here we start to
\ see "programming".
\
\ Some key points covered in the text:
\
\ There is no GOTO statement.
\
\ When checking a value, the pattern is (assuming the value
\ is on the top of the stack):
\
\   DUP test IF ..... ELSE ...... THEN
\
\ If that test is for check is for 0, it is possible to skip
\ the ELSE path if it is a no-op:
\
\   ?DUP IF ..... THEN 
\ 
\ Truth values were 0 and 1, with the C convention that anything
\ that is not false is true. Modern Forth uses -1 for true but the
\ "if it isn't false it must be true" behavior is still available
\ with caveats.
\
\ NOT is a poorly defined word in the Forth standard. It can either
\ mean INVERT (just flip the bits) or =0 which will return a proper
\ -1 for true and 0 for false. I think of NOT as =0, but the main
\ lesson is "don't use NOT".
\
\ The usual explanations of boolean operators instead of addition
\ or subtraction is offered in the text.


\ Problems


\ 3. bar carding function.

: card ( n -- , is a person aged n allowed to purchase alcohol? )
    21 <
    if ." under age"
    else ." drink away"
    then ;


\ 4. Print number sign.

: sign-test ( nn -- , prints positive, negative, or zero )
    dup if
        0< if ." negative" else ." positive" then
    else ." zero" drop then
    ;


\ 5. Guard our original stars from chapter 1 so that it will not
\ print anything for zero or negative n.

\ Prior definition of stars ... second shadows first.
\ This sequence will get redefinition warnings. This ok.

: stars ( n -- , print n stars )
    0 do 42 emit loop ; 

: stars ( n -- , protect prior stars from 0 )
    dup if abs stars else drop then ;


\ 6. Write WITHIN (n low high -- flag , n in [low..high)
\ 
\ WITHIN is now in the ANS standard and some Forths include
\ WITHIN? which in n in [low..high]. I'll use ?RANGE for this.

: ?range ( n low high -- flag , is n in low <= n < high )
    rot dup rot ( low n n high )
    <           ( low n flag-h )
    -rot        ( flag-h low n )
    <=          ( flag-h flag-n )
    and ;       ( passed? )

    
\ 7. Write a guess the number game (yawn).

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


\ 8. Using nested tests with IF/ELSE/THEN write a definition
\ SPELLER that spells out one through four, prefixing with
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


\ 9. TRAP could be used as part of binary search algorithm, print
\ between, not between, or match (only possible when low = high).
\ Use ?RANGE and remember that it is a right open interval.

\ 3dup is from chapter 2 problems.
: 3dup        ( a b c -- a b c a b c )
    dup 2over rot ;

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

\ End ch04.fs.
