\ ch07.fs -- A Number of Kinds of Numbers -- T.Brumley

\ Chapter 7 discusses double precision numbers, unsigned versions
\ of both single and double precision, and more words to operate
\ on them.

\ For me this is all stuff I learned in the 1970s working in
\ assembly language. There are some Forth words to learn in place
\ of SRA/SRL/... for arithmetic shifts Forth uses 2* and 2/.

\ The use of BASE is discussed but unless I missed it Brodie
\ didn't mention that in its own base, a base is always '10'. I
\ think that's a useful realization for newbies.
\
\ <any number> base ! base @ . prints 10
\
\ Doubles are discussed but not all of the conversions described
\ work these days. Both gforth and pforth support the various D
\ words. Input of double precision using the old convention of
\ "if it starts with a digit and isn't a known word, discard
\  punctuation and store the result as a double" doesn't work
\  anymore. Gforth handles 12.34 while pforth does not complain
\  but doesn't put anything on the stack. Patterns such as
\  1:23:45 that worked in old forth error as word not found in
\  both gforth and pforth.
\
\ In gforth these are all stored as "1234 0" on the stack: 1.234,
\ 12.34, 123.4, and 1234.; the decimal point isn't really one.

\ NOTE: According to Forth Programmer's Handbook, the only
\ required punctuation is a period/decimal point. Various Forths
\ may have more.

\ Number formatting using <# # #S and #> is explained. It works
\ backwards, digits are laid down right to left, which can be
\ confusing at first. But this is the way. n BASE @ /MOD gives
\ the current digit. The number to display
\ must be a double and should be unsigned.
\
\ An example is printing a time in hours/minutes/seconds format.

: sextal 6 base ! ;
: :00    # sextal # decimal 58 hold ;
: sec    <# :00 :00 #s #> type space ;

\ 4500. sec  1:15:00  ok

\ I would probably name :00 as :## and sec as h:mm:ss. I need to
\ take a deeper look at the calculations to deal with the 60
\ boundaries.

\ Techniques for dealing with signed and single cell numbers are
\ covered next. Some of the ordering of high and low cells can be
\ different based on architecture, but on the M2 I have high word
\ on top.
\
\ 123.4 is stored 1234 0 and -12.34 is stored as -1234 -1. Knowing
\ this, the word D. can be explained.

: study-d.         ( d -- , or lsc hsc -- )
    swap           \ hsc lsc
    over           \ hsc lsc hsc
    dabs           \ hsc |lsc hsc|
    <# #s          \ hsc |...consumed but space still used|
    \ *** sign behavior changed in standard so we need to ***
    rot            \ get signed hsc on top for sign
    sign           \ if tos neg store '-'
    #>             \ ends conversion
    type space ;   \ display string

\ In 1970s Forth, the word SIGN checks the sign of the third word
\ on the stack and places a '-' in the output buffer if needed.
\
\ Modern gforth and pforth check the top of stack, so when using
\ any old picture formatting, be sure to ROT before SIGN.
\
\ To reinforce that the buffer is laid down right to left:
\
\ <# sign #s #>       1234-
\ <# #s sign #>       -1234

\ Knowing this a simple print as currency word can be defined:

: .$  swap over dabs
    <# # # '.' hold
    #s rot sign
    '$' hold
    #> type space ;

\ By setting up the stack as described above, a single can be
\ extended to be a double by just plopping a zero or -1 on the
\ top of the stack.
\
\ preserving sign and setting up the sign indicator for <#:

: s>sd ( s --  SIGN s 0/-1 ) dup dup dup abs swap dup 0< if -1 else 0 then ;  

: u>ud ( u -- SIGN u 0 ) 0 swap 0 ;

\ And some other variations.

\ Next mixed mode operations are discussed, bringing in the M
\ series of words (M/ M+ M* ...). Not all of these exist in
\ current standards, but there is a good explanation of why and
\ how they should be used.
\
\ These are generally not needed anymore unless one is on an
\ narrow word length system (typically embedded). 

\ The subject of numbers in definitions is examined. The main
\ point is that the base at word compile time determins the
\ value, so in HEX a 14 is a DECIMAL 20.

\ The chapter ends with a review of all the words related to
\ doubles. Once you understand how a double is stored, they
\ are all pretty easy to understand.

\ Chapter 7 problems.

\ 1. Write a word that determines the largest positive number that
\ a cell may hold. The text suggested a BEGIN UNTIL but I find
\ BEGIN WHILE REPEAT more natural.

\ Basically starting from 1 shift left (2*) until the result
\ is negative. Subtracting one from that (viewed as unsigned)
\ brings us the largest positive value. Along the way I count
\ shifts and display the bit width of a cell. Stack juggling
\ practice. 

: n-max  ( -- max-positive-n bits )
    \ basic idea is to shift left/multiply by 2
    \ until negative
    1 1                   \ seed bits prior
    begin
        dup 2* dup        \ bits prior new new for test
        0>                \
    while
        nip               \ bits prior new -- new
        swap 1+ swap      \ -- bits new
    repeat
    drop                  \ bits prior
    cr ." maximum positive n " dup 1- + .      \ 2**(bits+1)-1
    cr ." for a cell width of "
    1+                    \ we stopped shifting before sign bit
    dup . ."  bits or " 2/ 2/ 2/ . ." bytes " cr ;


\ 2. A word problem to explain why we use OR instead of + when
\ selecting between options. Bit masking.


\ 3. Write a BEEP word that sounds the terminal bell (7) three
\ times. Printing "BEEP" after each bell and pausing long enough
\ between bells to count the rings.

\ I don't do bells, but it would be something like the following
\ given what we have seen in the book. However, even 100000
\ iterations was not perceptible in the delay on my Mac.
\
\ The book's delay is 20000 0 DO LOOP ;
\
\ These days a timer would be used. The standard provides the word
\ MS to delay for at least some number of milliseconds. This is in
\ the Facility extensions.

: longish-pause ( -- ) \ 50000 0 do i drop loop ;
   500 ms ;

: beep ( -- )
    3 0 do
        7 emit
        ." BEEP "
        longish-pause
    loop ;


\ Problems 4 and 5 are practice in double length math.


\ 4. a. Rewrite the termperature conversions from chapter 5
\ assuming input and results are double-length signed integers
\ scalled by 10. (105 degrees would be 1050, 10.5 would be
\ 105, etc.)

\ I was already scaling the calculations so this should just
\ work once I double literals and operators.
\
\ Or not: Several words aren't available. 
\
\ I'll fake out the M words (mixed).
\
\ While the specification is for double pecision input and
\ output, I only expand the result so it can be fed into
\ the .deg formatter.

\ These overrides are for the a couple of problems.
\ The redefinition warnings can be ignored.

: m+ + ;
: m*/ */ ;
: m* * ;

: f>c   ( df -- dc )
   -320 M+ 10 18 M*/ s>d ;
: c>f   ( dc -- df )
   18 10 M*/ 320 M+ s>d ;
: c>k   ( dc -- dk )
   2732 M+ s>d ;
: k>c   ( dk -- dc )
   -2732 M+ s>d ;  
: f>k   ( df -- dk )
   f>c d>s c>k ;
: k>f   ( dk -- df )
   k>c d>s c>f ;


\ 4. b. Write a formatted output word named .DEG which will
\ display a double length signed integer scaled by 10 as a
\ string of digits, a decimal point, and one fractional
\ digit.

: .deg  ( d -- )
    tuck               \ hc lc hc --
    dabs               \ hc ulc uhc --
    <#
    # '.' hold         \ 9.
    #s                 \ 999
    rot sign           \ (-)
    #> type ;


\ 4. c. Test the following conversions.
\
\    0.0 F>C
\  212.0 F>C
\   20.5 F>C
\   16.0 C>F
\  -40.0 C>F
\  100.0 K>C
\  100.0 K>F
\  233.0 K>C
\  233.0 K>F
\ 
\ The above conversions work and properly feed the .deg word.


\ 5. a. Write a routine to evaluate the quadratic equation:
\
\ y = 7x^2 + 20x + 5
\
\ Given x, and returns a double length result.
\
\ x = -b +/- sqrt(b^2-4ac)
\     -------------------
\            2a

\ I wrote both a general solver and specific to this equation
\ solver with a, b, and c hard coded:

: solver ( a b c x -- d )
    swap       \ a b x c 
    >r         \ a b x     | c
    swap       \ a x b     | c
    over       \ a x b x   | c
    *          \ a x bx    | c
    r>         \ a x bx c  |
    + >r       \ a x       | bx+c
    dup * *    \ ax^2      | bx+c     
    r> +       \ ax^2+bx+c | bx+c     
    ; 

: solver2 ( x -- d )
    dup 7 *            \ x 7x
    20 +               \ x 7x+20
    *                  \ 7x^2+20x
    5 +                \ 7x^2+20x+5
    ;

\ The hard coded version is closes to the text's answer. The way
\ the problem was worded, this is actually what was requested.

\ Here is the text's solution. My M*/ doesn't really match the
\ semantics of the FIG version so the ROt 1 breaks things.
\ 
\ : dpoly dup 7 m* 20 m+ rot 1 m*/ 5 m+ ;
\
\ Here's a modified version:

: dpoly
    dup           \ x x
    7 m*          \ x 7x
    20 m+         \ x 7x+20
    *             \ 7x^2+20x
    5 +           \ 7x^2+20x+5
    ;

\ The hardcoded coefficients make it easy to follow the
\ calculation. I could probably redo my variable version to
\ optimize but meh. The key thing to remember here is:
\
\ Keep factoring to simplify terms.


\ 5. b. How large an x will not overflow a 32-bit signed
\ integer?

: ?dmax  0 begin 1+ dup dpoly 0 ( d< ) < until 1- . ;

: findmax
    0                    \ starting x
    begin
        1+ dup solver2   \ x y
        32767 >          \ check for needing more than 16-bits
    until
    1- . ;

\ 67 hits 32768, so 66. If we're unsigned, 95 is the max. Very
\ nice of Brodie to line that up perfectly.


\ 6. Write a word which prints the numbers 0 through 16 in
\ decimal, hexadecimal, and binary form in three justified
\ columns.

: binary decimal 2 base ! ;

: tabler
    cr
    17 0 do
        i 4 .r
        i 4 hex .r decimal
        i 8 binary .r decimal
        cr
    loop ;


\ 7. What does it tell you when the you enter .. and don't get an
\ error?

\ In the old days it meant the double precision wordset had been
\ loaded. It would put 0 0 on the stack.


\ 8. Write a phone number picture output.

\ The text only required 7 digits with an optional #s for any
\ remaining digits (an area code or such). These days phone
\ numbers are always at least 10 digits.
\
\ I did it a bit differently but decided steal and idea from the
\ text's solution to to see if there are any digits left to
\ print.

: .ph# ( d -- )
    <# # # # # '-' hold # # #          \ the old 7 digit number
    over if                            \ anything left
        bl hold
        ')' hold # # # '(' hold        \ us area code
    then                               \ any dangling
    over if                            \ and again?
        bl hold #s                     \ whatever's left
    then
    #> type space ;

\ Note the visual confusion working from right to left can cause
\ if you aren't paying attention.

\ End of ch07.fs
