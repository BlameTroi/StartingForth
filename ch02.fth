\ ch02.fth -- How to get results -- T.Brumley

\ definitions from working through the chapter along with
\ any solved problems.

marker ch02            \ forget         

\ Chapter 2 is long explanation of using the stack and the
\ words commonly used for stack juggling. It's a slog if
\ one isn't familiar with postfix or Reverse Polish Notation.
\
\ Doubles are introduced but the examples are from the
\ 16-bit era. The words work the same (cell pairs instead
\ of cell) but the Forth I use doesn't push a number such
\ as `123.45` onto the stack as a double 12345. gforth does
\ but I prefer pforth.
\
\ On my Apple Silicon Macbook the top word in a pair is the
\ most significant.
\
\ 12.34 .s <2> 1234 0 in gforth
 
\ Calculator-style practice problems
\ 
\ Converting infix to postfix exercises/quizzes are worth
\ doing for practice (wax-on, wax-off).
\
\ Brodie refers to this as calculator style.
\ 
\ 1. c(a + b)        a b + c *
\
\ 2. 3a - b
\    ------  + c     3 a * b - 4 / c +
\      4
\
\ 3. 0.5 ab         ab
\    ------       -----         a b * 200 /        
\      100          200 
\
\ 4. n + 1
\    -----        n dup 1 + swap /    -- text n 1 + n /
\      n
\
\ 5. x(7x + 5)    x 7 over * 5 + *    -- text 7 x * 5 + x *
\
\ And postfix to infix:
\
\ 6. a b - b a + /         a - b
\    a-b  b+a /            -----
\                          b + a
\
\ 7. a b 10 * /           a
\                       -----
\                        10b

\ Definition-style practice problems
\ 
\ From calculator to definition style, demonstrated
\ using unit conversions.

\ conversions based on inches

: yards>in   36 * ;
: ft>in      12 * ;

\ normalize all measurements

: yards      36 * ;
: feet       12 * ;
: inches          ;          \ a no-op for readability

\ and synonyms for readability as well

: yard       yards ;
: foot       feet  ;
: inch             ;

\ so 10 yards 2 feet + 9 inches + should be 393 inches.
\    1 yard 2 feet + 1 inch +     should be 61
\    2 yards 1 foot               should be 84

\ Convert infix to postfix, put in definitions, and
\ add stack effect comments. Brodie allows us to jiggle
\ the definition to force a convenient stack order but
\ that's no fun.

\ 1.     ab + c      \ or if c b a then * + 

: 2b1    ( a b c -- n )
     rot * + ;
 
\ 2.     a - 4b           
\        ------   +   c 
\           6

: b2b2 4 * - 6 / + ; ( c a b -- n )

: 2b2             ( a b c -- n )
    rot           ( c b a -- )
    swap 4 *      ( c a 4b --)
    - 6 /  +  ;   ( -- n )

\ 3.      a
\       ----
\        8 b

: b2b3 8 * / ; ( a b -- n )

: 2b3             ( a b -- n )
    8 * / ;

\ 4.   0.5 ab
\      ------
\        100

: b2b4 * 200 / ; ( a b -- n )

: 2b4             ( a b -- n )
    * 200 / ;

\ 5.   a(2a + 3)

: b2b5 2 * 3 + * ; ( a a -- n )

: 2b5             ( a -- n )
    dup 2* 3 + * ;

\ 6.    a - b
\       -----
\         c

: b2b6 - swap / ; ( c a b -- n )

: 2b6            ( a b c -- n )
    -rot - swap / ;


\ Make more use of stack swizzler words.


\ quiz c

\ 1. Write a phrase to flip three items on the
\ stack. So a b c becomes c b a.

: flip3  ( a b c -- c b a )
    rot rot swap ;

\ 2. Write an over that does not use over.

: revo1  ( a b -- a b a )
    swap dup      ( b a a -- )
    rot swap ;    ( -- a b a )

: revo2  ( a b -- a b a )
    2dup drop ;   ( -- a b a )

\ 3. Write <rot (these days known as -rot) so that
\ a b c becomes c a b.

: <rot   ( a b c -- c a b )
    rot rot ;

\ Write definitions for these equations with
\ the indicated stack effects.

\ 4.   n + 1
\      -----       ( n -- result )
\        n

: 2c4    ( n -- result )
    dup 1+ swap / ;

\ 5.  x(7x + 5)     ( x -- result )

: 2c5     ( x -- result )
    dup 7 * 5 + * ;

\ 6. 9a^2 - ba    ( a b -- result )

: 2c6      ( a b -- result )
    over *
    swap dup * 9 * swap - ;

: b2c6     ( a b -- result )
    over 9 * swap - * ;


\ Chapter problems
 
\ 1. Difference dup dup vs 2dup?
\    a b dup dup -- a b b b
\    a b 2dup    -- a b a b

\ 2. Write a phrase that reverses the top four items
\ on the stack.  ( 1 2 3 4 -- 4 3 2 1 )

: rev4  ( a b c d -- d c b a )
    swap 2swap swap ;

\ 3. Write 3dup ( a b c -- a b c a b c )

: 3dup        ( a b c -- a b c a b c )
    dup 2over rot ;

\ Write expressions for the equations with the
\ specified stack effects.

\ 4.  a^2 + ab + c        ( c a b -- result )

: 2p4            ( c a b -- result )
    over         ( c a b a -- )
    *            ( c a ab -- )
    swap         ( c ab a -- )
    dup *        ( c ab a^2 -- )
    + + ;        ( -- result )

\ 5.   a - b
\      -----     ( a b -- result )
\      a + b

: 2p5            ( a b -- result )
    2dup         ( a b a b -- )
    -            ( a b a-b -- )
    rot rot +    ( a-b a+b -- )
    / ;          ( -- resulst )

\ 6. Write a set of words to calculate a prison
\ sentence.
\ 
\ Spec:
\
\ Prison sentence calculator. Convicted of <crime> gets <years> to
\ serve.
\
\ convicted-of arson homicide tax-evasion
\ will-serve 35 years
\
\ homicide 20 years
\ arson 10 years
\ bookmaking 2 years
\ tax-evasion 1 year

\ We're limited to basic stack and arithmetic here.

\ Crimes:
: homicide 20 + ;
: arson 10 + ;
: bookmaking 2 + ;
: tax-evasion 1 + ;

\ Prelude:
: convicted-of ( -- years , )
    0 ;

\ Report:
: will-serve ( years -- , )
    . ." years" ;

\ 7. Write egg.cartons that takes a number of eggs and
\ returns the number of cartons (dozens) and how many
\ are left over.

: egg.cartons    ( eggs -- left-over filled-cartons )
    /mod . ;

\ end of ch02.fth
