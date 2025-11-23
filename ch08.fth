\ ch08.fth -- Variables, Constants, and Arrays -- T.Brumley

    marker ch08

\ Chapter 8 finally introduces variables. The stack can only deal
\ with so many items. And the programmer can typically deal with
\ even fewer stack items.
\
\ It's all really pointers and the expression of them seems very
\ natural to me. The assmebly language experience is handy.

\ Everything is cell sized (we can access at a byte level, but
\ it all starts with cells).

\ Brodie provides a rule of thumb for when to use variables:
\
\    In Forth, variables are appropriate for any value that
\    is used inside a definition which may need to change at
\    any time after the definition has already been compiled.

\ Variables

\ A variable name in a definition compiles to placing the address
\ of the variable's contents on the stack. The two main words
\ dealing with variables are store and fetch.
\
\ !       ( n addr -- , stores n at addr )
\
\ @       ( addr -- n , fetches n from addr )
\
\ ?       ( addr -- , fetches and print : ? @ . ; )

\ How do variables work behind the scenes?
\
\ VARIABLE is a compiling word, just like :.
\
\ It adds an entry for the name following to the dictionary.
\
\     VARIABLE VNAME 
\     VNAME | get and set code | space for the value of the variable 
\
\ An example of how to use a variable as a persistent counter
\ would be:
\
\     variable eggs
\     : egg.reset  0 eggs ! ;
\     : egg        1 eggs +! ;
\ 
\     egg.reset egg egg egg eggs ? should print 3.
\
\ There is only +!, not a minus bang (makes sense when you think
\ of stack ordering).

\ Constants

\ So seeing how a variable is defined in code, it makes sense that a
\ constant is defined thusly:
\
\     value CONSTANT CNAME
\
\     CNAME | get code | space holds the value from top of stack
\
\ Where a variable places an address on the stack, the constant places
\ the value on the stack, enforcing constantness.
\
\ There is some discussion of optimizing constants when programming
\ an eprom of defining frequently used numbers as constants.
\
\     0 constant 0
\     1 constant 1
\
\ I expect modern Forths for production environments to deal with
\ optimization. 

\ Double length forms are available via 2VARIABLE 2@ 2! (no 2+? or
\ 2+!) and 2CONSTANT.

\ To be consistent between pforth and gforth, I'll probably use
\ phrasing such as this to define double constants.
\
\     160 s>d constant cname
\
\ The same could be accomplished with 160 0 ... but S>D is more
\ intention revealing.

\ As noted elsewhere, doubles aren't likely to be something I use
\ often. I just don't see a need for them on a 64-bit system. 

\ Brodie describes on trick that I like. Remember those rational
\ approximations from a prior chapter? They can be put in a double
\ constant and applied via */.

355 113 2constant pi
: circle.area ( r -- a ) dup * pi */ ;
 
\ Arrays

\ Unlike in Pascal, Basic, Fortran, etc., the bones of the array
\ implementation are visible. Depending on how it's coded, a C
\ array is somewhere between Assembly/Forth and Pascal/Basic/Fortran
\ in visibility.

\ Arrays are an extension of variables. When added to the dictionary
\ a "next possible entry" address is tracked by the compiler. Using
\ ALLOT bumps this by the requested number of *bytes*. So:
\
\     VARIABLE limits 8 ALLOT      \ extends by another cell (badly)
\
\ Gives us a two element array.

\ Aside: I know the English meaning of allot and this is just one of
\ the bits of Forth that predate the use of allocate and alloc. It's
\ a homophone of "a lot". Insert the "is two a lot ..." meme here.
\
\ It bugged me in the 1980s and still bugs me. It's a minor nit, but
\ I'll kvetch anyway.

\ There's discussion of zero indexing, which is unavoidable at this
\ level of coding. I'm a one based guy myself but as in assembly or
\ C, this is not the field to fight that battle on.
\
\ Forth provides FILL and ERASE to initialize or clear any block
\ of storage, but this should surely be restricted to arrays (and
\ later structures).

\ The text is very locked into 16-bit architectures here, and is
\ comfortable allocating bytes (2 per cell is very easy to track).
\ I'll go through the egg example but add the use of CELL and CELLS.
\
\ Looking through the text, the words CELL and CELLS are not mentioned.

\ These are predefined in pforth and gforth.
: CELL   8 ;      ( -- 8, size of a cell, would be obtained from runtime )
: CELLS  CELL * ; ( n -- bytes to hold that many cells )

\ So instead of:

variable somearray 32 8 * allot

\ We would do:

variable otherarray 32 cells allot

\ Since the first cell is index 0, 32 is the highest usuable index.
\ The array is 33 elements long. I suspect I'll trip over this a
\ few times.

\ And so on, using CELLS for indexing: array[1] = @array + 1 cells,
\ [0] = @ + 0 cells, and so on. Note there is no range or bounds
\ checks.
\
\ This is just like and 0 indexed addressing scheme in other
\ languages. If the syntax feels a little cluttered, a wrapper
\ word such as : idx ( addr n -- ) cells * + ; might help, but
\ I suspect that with practice you would automatically filter
\ what you see the way some people do with parentheses in lisps.

\ The egg example counts cartons of eggs by egg size, as with
\ the weights in a prior example.

\ There are four categories of eggs, along with an error and
\ reject category. Six slots, 0-5.

\ State and constants.
18 constant weight.reject
21 constant weight.small
24 constant weight.medium
27 constant weight.large
30 constant weight.extra.large
0 constant egg.reject       \ 18 < oz
1 constant egg.small        \ 21 <
2 constant egg.medium       \ 24 <
3 constant egg.large        \ 27 <
4 constant egg.extra.large  \ 30 <
5 constant egg.error        \ 30 >=
variable carton.counts 5 cells allot

\ Given a carton weight, return its egg.<size> category.
: category ( n -- n )
    dup weight.reject      < if egg.reject      else
    dup weight.small       < if egg.small       else
    dup weight.medium      < if egg.medium      else
    dup weight.large       < if egg.large       else
    dup weight.extra.large < if egg.extra.large else
                                egg.error
    then then then then then swap drop ;

\ Given an egg.<size> print the size label.
: label ( n -- )
    dup egg.reject      = if ." reject "      else
    dup egg.small       = if ." small "       else
    dup egg.medium      = if ." medium "      else
    dup egg.large       = if ." large "       else
    dup egg.extra.large = if ." extra large " else
                             ." ERROR "
    then then then then then drop ;

\ Initialize.
: reset.counts ( -- ) carton.counts 6 cells 0 fill ;

\ Make sure they start at zeros.
reset.counts

\ Map egg.<size> category into carton.counts index.
: counter ( n -- ) cells carton.counts + ;

\ Increment carton count for this egg.<size>.
: tally ( n -- ) counter 1 swap +! ;

\ Compose something useful. eggsize:
: eggsize  ( n -- , count and print size label for weight n )
    category dup label tally ;

\ And report counts.
: report   ( page ) cr cr ." Quantity    Size " cr cr
    6 0 do
        i counter @ 5 u.r
        7 spaces i label cr
    loop cr ;

\ I spent some time polishing the code from the text. I added
\ more constants and changed some names for the sake of clarity.
\ This and my own typos gave me some debugging time which is
\ always a good learning tool.

\ Factoring code

\ Brodie launches into a discussion of proper Forthish code
\ factoring, starting with this quote from Moore:
\
\    A good Forth vocabulary contains a large number of small
\    words. It is not enough to break a problem into small
\    pieces. The object is to isolate words that can be reused.
\
\ Or the modern phrasing is DRY--Don't Repeat Yourself.
\
\ This generates some bogus complaints about optimization from
\ people--the weight of word chaining exceeding the weight of
\ word code--but I think that's bogus. It's not all indirect
\ threaded code anymore. In any event, benchmark it don't just
\ assume that the overhead matters.
\
\ In memory restricted environments the chaining overhead was
\ considered a good trade for a smaller memory footprint. That
\ is not as much of a concern in this era.

\ A Forth convention is that any word should consume its own
\ parameters. If parameters need repeating, that should be
\ done by the caller. So, looking at eggsize where both label
\ and tally need the category code, do:
\
\     category dup label tally
\
\ instead of:
\
\     category label tally     \ where label preserves its parameters
\
\ Structure programming would call this loose coupling.
\
\ There are best practices that will become clear over time.

\ Byte Arrays

\ Just address by byte (or double byte, etc) instead of cells. Useful
\ if memory is an issue or dealing with character data. I'm not sure what
\ other string support exists yet.

\ Array Initialization

\ CREATE is like VARIABLE but does not allot any storage other than
\ the dictionary header.
\
\       create limits
\
\       limits | code for create | unallocated
\
\ , (COMMA) takes a number off the stack and stores it at a cell in the
\ current location in the dictionary (in this case in limits).
\
\       220 ,
\
\       limits | code | 220 | unallocated
\
\ For a byte array, use C, (C-COMMA) to store a single byte.
\ 
\ So it looks to me as if CONSTANT, VARIABLE, and CREATE place a new
\ dictionary header down, appropriate code to access the contents,
\ and adjusts the dictionary pointer to the next free spot. ALLOT,
\ COMMA and C-COMMA may or may not store information in the entry,
\ but they always adjust the dictionary pointer.

\ I believe my understanding is correct but my terminology isn't
\ consistent with either Brodie or modern Forth. I'll learn the
\ new terminology in time.

\ Target compilation / tethered development

\ A VARIABLE is established in RAM while a CONSTANT or CREATE is in
\ ROM. Like our old literal pools in assembly.

\ Chapter 8 problems.

\ 1. a&b. A simple pie bakery where pies can be baked, eaten, or
\ frozen. Maintain counters. Baking a pie increments the inentory,
\ eating decrements, and freezing will freeze the enitre inventory.
\ If no pies are available when attempt to eat a pie, return an
\ error message.

variable pies
variable frozen
variable eaten

: pie-reset ( -- )
    0 pies ! 0 frozen ! 0 eaten ! ;

: bake-pie ( -- )
    1 pies +! ; 

: eat-pie ( -- )
    pies @ 1 < if ." no pie for you! " 
               else
                    ." yummy "
                   -1 pies +!
                    1 eaten +!
               then ; 

: freeze-pies ( -- )
    pies @ dup 0 > if 
        dup
        ." freezing " . ."  pie(s) "
        frozen +!
        0 pies !
    else
        drop
        ." no pies to freeze "
    then ;
 
\ 2. Write a word .base that prints the current base in decimal.

: .base ( -- )
    base @ dup decimal . base ! ;

\ 3. Write a number formatting word M. that prints a double length
\ word with a floating decimal point controlled by a variable
\ PLACES.

variable places
0 places !

: m. ( d -- , print with PLACES places )
    places @ 1 <       \ lsc hsc p
    if
        d.             \ --
    else
        tuck dabs      \ stash sign
        <#
        places @
        0 do           \ lsc hsc p 0
            #
        loop
        '.' hold
        #s
        rot sign
        #>
        type space
    then ;

4. \ A colored pencil inventory.

0 constant plain
1 constant red
2 constant blue
3 constant green
4 constant orange

variable #pencils 4 cells allot

: pencils-reset #pencils 4 cells erase ;

: pencils ( n -- addr , of color slot )
    dup 0 < if ." error " exit then
    dup 4 > if ." error " exit then
    cells #pencils + ;

: pencils-init
    23 red pencils !
    15 blue pencils !
    12 green pencils !
    0 orange pencils ! ;

\ 5. Create an array of 10 cells with random values from 0 to
\ 70. Then print a histogram (horizontally) with one asterisk
\ per unit. Exploration code following found that pforth's
\ random returns a value 0-65535, chopped 70 ways that 936 per.

variable dataset 9 cells allot

: dataset-reset dataset 9 cells erase ;

: dataset-init \ seed with random values 0-70
    10 0 do
        random 936 /      \ 64K scaled 0-70
        dataset i cells + !
        cr i . dataset i cells + ?
    loop cr ;

: dataset-histogram ( -- ) \ assumes you've seeded the dataset
    10 0 do
        cr
        i 1+ 2 .r
        dataset i cells + @ dup 4 .r space
        0 do
            42 emit
        loop
    loop
    cr ;
    
\ Exploration -- what is the range of random in
\ pforth? 0->65535 via the code below. So to scale
\ that to 0-70 we divide by 936. Not worried about
\ fractionsal numbers for the histogram exercise.

variable minrandom
variable maxrandom

: rangefinder ( n -- )
     999999999 minrandom !
    -999999999 maxrandom !

    0 do
        random
        dup minrandom @ < if dup minrandom ! then
        dup maxrandom @ > if dup maxrandom ! then
        drop
    loop
    cr ." min " minrandom ?
    cr ." max " maxrandom ?
    cr ;
    
\ 6. A tic tac toe UI, enter moves as a number 1-9 and a mark.
\ Squares are number from upper left to lower right. Use a byte
\ array with -1 for O and 1 for X.
\
\ Using -1 as a marker in byte was a trick question. c@ does not
\ sign extend. 

variable ttboard 1 cells allot     \ 1 character would work
ttboard 2 cells erase              \ but this is better imo.

\ In writing these I original had nested ifs, many dups, and
\ such in the X and Y words. It seems better to factor each
\ test out (legal? -- is this a valid slots, empty? -- is the
\ slot filled).
\
\ I'm guessing this is closer to idiomatic Forth.
\
\ There is still some repetition, but it is possible to be
\ too terse. 

: legal? ( n -- f )
     dup 0 > swap 10 < and ;
 
: empty? ( n -- f )
    ttboard + c@ 0= ;

: place-or-error ( b n -- , place byte on board if legal and open )
    dup legal?                   \ b n f
    over empty?                  \ b n f f -- b n f
    and if ttboard + c!          \ b n
    else ." bad move " 2drop     \
    then ;

: x ( n -- )
    1 swap place-or-error ;
            
: o ( n -- )
    -1 swap place-or-error ;

\ It's been suggest to avoid exit, but I'm having trouble
\ coming up with a nesting format I find readable. Using
\ exits as in guardian ifs seems safe.

: glyph ( n -- )
    ttboard + c@ dup          \ n -- b b
      1 = if ." X" drop exit then
    255 = if ." O" exit then
    space ;

: horizontal.bar 10 0 do '-' emit loop ;

: print-row ( n -- , n is row # 1 2 or 3 )
    1- 3 *         \ map row to first glyph
    4 1 do
        space
        dup i + glyph        
        i 3 < if space '|' emit then
    loop
    drop ;
 
: print-board
    1 print-row cr
    horizontal.bar cr
    2 print-row cr
    horizontal.bar cr
    3 print-row cr ;
    
\ End of ch08.fth
