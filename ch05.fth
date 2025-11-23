\ ch05.fth -- The Philosophy of Fixed Point -- T.Brumley

marker ch05

\ Chapter 5 explains the virtues of fixed point and scaling and
\ introduces additional operators. This is the first time the
\ cell size has caused noticeable issues. Double precision was
\ mandatory in the era of 16 bit computers. The words are basically
\ the same but the boundary/size are different.

\ The return stack is introduced as a convenient place to stash
\ some values with the requirement that the stack is restored
\ before anything that uses return values is done.
\
\ Words are introduced for parameter stack access. The definition
\ of i, i' and j are not current. These days i and j are used to
\ access loop indices. I believe I should be r@.
\
\ A word to calculate quadriatic roots using the old definitions
\ is given. quadriatic-p is the text version, while quadriatic-g
\ uses what I believe is the correct words.
\ 
\ a b c x          pforth      gforth
\ 2 7 9 3    -p      30         48
\ 2 7 9 3    -g      48         48
\ 
\ In gforth either version works, but I suspect that's an accident.
\ I and J are undefined outside a loop per the standard.
 
: quadratic-p ( a b c x -- n) >r swap rot i * + r> * + ;
: quadratic-g ( a b c x -- n) >r swap rot r@ * + r> * + ;

\ While digging into this I learned that not is non-standard.
\ pforth implements it as 0= while gforth does not implement
\ it.
\
\ pforth implements not as 0=, which is what it was meant to
\ be a synonym of older forths (readability). invert is -1 xor
\ in both.
\
\ defining not in gforth as : not 0= ; does what I want, but is
\ it really the right thing to do?

\ Floating point is introduced but discouraged for most of
\ the applications Forth is used for. Instead use fixed point
\ and scale to align decimal points and allow for fractional
\ values.

\ On page 116 /* (star-slash) and scalars are introduced. The
\
\ n1 n2 n3 /*       is (n1 * n2)/n3, but the calculation n1*n2
\ is stored in an 'intermediate result' that is a double and
\ not a single cell.
\
\ so in another language dbl(n1) * dbl(n3) = d
\ then d/n3 to get the result.
\
\ There are rules for signs but I don't worry about them right
\ now.

\ And here's where cell size starts to show up. The word % defined
\ as 100 */ properly calculates some values that doing each
\ operation separately won't.
\
\ 225 32 % is 72 as is 225 32 * 100 /, but on a 16-bit system
\ 2000 34 % is 680 but 2000 34 * 100 / is 68000 100 /, which
\ won't work right as 68000 is won't fit in 16 bits (65534).
\
\ Then there's a discussion of using scaling to handle fractional
\ results. The example of taking 32% of to two places illustrates.
\
\ 225 = 72.00
\ 226 = 72.32
\ 227 = 72.64
\
\ A scaled solution is R%, defined as 10 */ 5 + 10 / ; which will
\ round up to a whole number.
\
\ And even better r% that scales to two places. This won't work
\ right on 16-bit systems.
\ 
\ take a percentage scaled by 100, rounded.
\ IE, 50% is 50 not .50

: r% ( n % -- n% ) swap 100 * * 500 + 10000 / ;
\
\ More factoring and using rational numbers instead of irrational
\ are discussed.
\
\ Defer and minimize division.
\
\ This leads into using rational approximations for commonly
\ used constants such as pi or e. There's a good table on
\ page 122, copied here. I've never needed the arcsec/degree
\ stuff. The 12th root of 2 relates to music theory. Log and
\ ln of 2 are useful when computing logs for bases other than
\ 10 or e.
\
\ Note that 16384 = 32768 / 2
\ log = log base 10
\ ln = log base e
\ degrees and arcsecs for conversions
\ and of course c is the speed of light in a vacuum in meters/second.
\ The value should be 2.99E8, so that's 299.79248 million meters
\ per second.
\
\ Constant          approx   16 bit rational   error
\ 
\ pi                 3.141       355 /   113   8.5E-8
\ sqrt(2)            1.414     19601 / 13860   1.5E-9
\ sqrt(3)            1.732     18817 / 10864   1.1E-9
\ e                  2.718     28667 / 10546   5.5E-9
\ sqrt(10)           3.162     22936 /  7253   5.7E-9
\ 12th root of 2     1.059     26797 / 25293   1.0E-9
\ log(2)/1.6384      0.183     20040 / 11103   1.1E-8
\ ln(2)/16.384       0.042       485 / 11464   1.0E-7
\ .001 degree/rev*   0.858     18199 / 21109   1.4E-9
\ arcsec/rev*        0.309      9118 / 29509   1.0E-8
\ c              2.9979248     24559 /  8192   1.6E-8
\
\ * 22-bit 
\
\ While researching some of the values I didn't know (12th root of
\ 2) I found a table with a few more constants and some additional
\ rational approximations, including a couple of 64 bit or 32 bit
\ unsigned approximations. I've copied that with attribution to my
\ bit notes folder.

\ Chapter 5 problems

\ 1. Write a definition to calculate - ab/c.
 
: ab/c  ( a b c -- n )  */ negate ;  

\ 2. Find the maximum of four numbers on the stack.

: 4max ( a b c d -- max )
    max          ( a b max --  )
    max          ( a max --  )
    max ;        ( -- max )

\ 3. and 4. combined -- first we write the forth expressions
\ for the basic temperature conversions, and then definitions
\ for them. 
\ 
\ "calculator style" three formula
\ c = (f - 32) / 1.8
\ f = (c * 1.8) + 32
\ k = c + 273
\
\ a 0f = -17c
\ b 212f = 100c
\ c -32f = -35c
\ d 16c = 61f
\ e 233k = -40
\
\ c = f - 32 * 5 / 9

\ The answers in the original text used the M+ and M*/ words,
\ which don't exist in pforth and shouldn't be needed since
\ everything is double precision. I can define these words
\ since M* is available, but I don't see a need.
\
\ The words are introduced in Chapter 7 A Number of Kinds of
\ Numbers.
\
\ Rounding errors show up quickly, so I did the intermediate
\ calculations scaled by 10. I'm remembering Mrs. Fuhrman's
\ reminders that too many decimal places are just as wrong
\ as too few.
 
: f>c   ( f -- c )
   10 * -320 + 10 18 */ 5 + 10 / ;
: c>f   ( c -- f )
   10 * 18 10 */ 325 + 10 / ;
: c>k   ( c -- k )
   10 * 2731 + 10 / ;
: k>c   ( k -- c )
   10 * 2731 - 10 /  ;
: f>k   ( f -- k )
   f>c c>k ;
: k>f   ( k -- f )
   k>c c>f ;
    
\ End of ch05.fth
