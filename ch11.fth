\ ch11.fth -- Extending the Compiler -- T.Brumley

marker ch11

\ Chapter 11 explores the compiler, defining words, and compiling
\ words. This is Forth's "secret power" and the most important
\ difference from other languages.

\ The key is that the compiler and interpreter/executor are all
\ very friendly with each other. You can switch modes from
\ compiling to immediate mid-definition. This allows for some
\ interesting magic.
\
\ The compiler is malleable and you are expected to modify it
\ when that's the right way to go. Custom command languages and
\ smart data structures are the obvious use cases.
\
\ Words can be viewed as DEFINING--creating a new dictionary
\ entry; or COMPILING--builds the guts of said dictionary entry.
\
\ Defining words create a new dictionary entry and gives it
\ both run-time and compile-time behaviors. For example:
\
\ 7 CONSTANT SEVEN
\
\ The runtime behavior of SEVEN is to put 7 on the stack. When
\ compiled in a definition, code to put 7 on sthe stack is added
\ to the new word. A subtle distinction, but it's important.

\ The conceptual parts of a dictionary entry are:
\
\ word name
\ dictionary link/chaining stuff
\ code pointer
\ pfa points here <---
\
\ So for
\
\ CREATE example 
\
\ "example"
\ @ next entry
\ @ runtime code for CREATE
\ pfa
\ ...
\
\ the runtime code for CREATE pushes the @ pfa on the stack
\
\ And so the definition of VARIABLE is:
\
\ : VARIABLE CREATE CELL ALLOT ;
\
\ Here the pfa is the cell.
\
\ A definine word may be defined with both compile time and
\ runtime behavior.
\
\ : DEFINING-WORD
\     CREATE ... compile time code here ...
\     DOES> ... runtime code here ... ;
\
\ And so the definition of CONSTANT could be:
\
\ : CONSTANT
\     CREATE  \ build dictionary head using next word of input
\        ,    \ take the value from the stack and store it in dict
\     DOES>   \ and for runtime
\        @ ;  \ the first cell of the pfa is the data stored by ,
\
\ So for 76 CONSTANT TROMBONES:
\
\ "trombones"
\ @ next
\ @ runtime code (pfa + CELL conceptually)
\ pfa holds 76
\ runtime code here
\
\ The bulk of the chapter explains various variations of this
\ pattern. Then there is a discussion of dropping in and out of
\ compile vs runtime mode in a definition.
\
\ Some key words to understand are:
\
\ IMMEDIATE  flags the word just defind as execute it when recognized
\ [COMPILE]  compile the next word, which is likely IMMEDIATE, into
\            the definition being built.
\ LITERAL    while compiling take the current word from the stack
\            and store in the definition, at runtime push that
\            value on the stack
\ [          leave compile mode
\ ]          return to compile mode
\
\ I'm having trouble articulating things better than the text. I
\ completely get the conceots, but keeping track of modes and
\ edge cases is still difficult. Practice will help.
\
\ The problems demonstrate the these words and more.


\ It's important to remember that the input stream can be
\
\ - a string
\ - a block
\ - the keyboard
\
\ The input stream at compile time is not the input stream at
\ runtime.


\ 1. Define a defining word named LOADED-BY that will define words
\ which load a block when they are executed. Example: 6000
\ LOADED-BY CORRESPONDENCE would define the word CORRESPONDENCE.
\ When CORRESPONDENCE is executed, block 6000 would get loaded.

: loaded-by ( n -- , fetches block n at runtime )
   create ,             \ compile the block # into the def
   does> @ load ;       \ he wants to load which executes it


\ 2. Define a defining word BASED. which will create number output
\ words for specific bases. For example, 16 BASED. H. would
\ define H. to be a word which prints the top of the stack in hex
\ but does not permanently change BASE. 
\
\ DECIMAL 17 DUP H. <return> 11 17 ok

: based. ( n -- , print tos in base n without changing base )
   create
      ,                  \ again, compiles base into the def
   does>
      @                  \ desired display base    n b     
      base @ swap        \ preserve current base   n r b
      base !             \ temporarily             n r
      swap .             \ print                   r
      base ! ;           \ restore current base    --


\ 3. Define a defining word called PLURAL which will take the
\ address of a word such as CR or STAR and create its plural
\ form, such as CRS or STARS. You'll provide PLURAL with the
\ address of the singular word by using tick. For instance, the
\ phrase
\
\ ' CR PLURAL CRS
\
\ will define CRS in the same way as though you had defined it
\
\ : CRS ?DUP IF O DO CR LOOP THEN ;

: plural ( xtc -- )
   create
      ,                 \ execute token for singular form
   does>                \ n pfa
      @                 \ n xt for singular word
      swap ?dup         \ xtc n ?n  or xtc 0
      if                \ will not run if n is 0
         0 do           \ xtc n 0 to xtc
            dup execute \ xtc xtc to xtc
         loop
      then              \ xtc all paths
      drop ;


\ *** I keep forgetting that the code in DOES> receives the pfa
\ *** on the top of the stack and I almost always want to @ it.


\ 4. The French words for DO and LOOP are TOURNE and RETOURNE.
\ Using the words DO and LOOP, define TOURNE and RETOURNE as
\ French "aliases." Then test this by writing a loop using the
\ aliases.

: tourne [compile] do ; immediate
: retourne [compile] loop ; immediate


\ 5. The FORTH-79 Standard Reference Word Set contains a word
\ called ASCII that can be used to make certain definitions more
\ readable. Instead of using a numeric ASCII code within a
\ definition, such as
\
\ : STAR 42 EMIT ;
\
\ you can use
\
\ : STAR ASCII * EMIT ;
\
\ The word ASCII reads the next character in the input stream,
\ then compiles its ASCII equivalent into the definition as a
\ literal. When the definition STAR is executed, the ASCII value
\ is pushed onto the stack.
\
\ Define the word ASCII.

\ I was doing too much; I wanted a word that would work while
\ compiling that could also be used interactively. ANS uses
\ [CHAR] and CHAR depending on the mode. gforth also allows us to
\ code '*' which will put 42 on the stack. That seems hinky now
\ that I know more about the parser and compiler, but I'll leave
\ it where I've used it while trying to use CHAR or [CHAR] in
\ future code.

: ascii ( -- c )
   32 word 1+ c@  [compile] literal ; immediate


\ 6. Write a word called LOOPS which will cause the remainder of
\ the input stream, up to the carriage return, to be executed the
\ number of times specified by the value on the stack. For
\ example:
\
\ 7 LOOPS 42 EMIT SPACE<return> * * * * * * * ok

\ This requires messing with input pointers which I mistakenly
\ thought were different from FIG Forth. Some user variables are
\ missing:
\
\     'S    Returns address TOS (not all systems)
\     S0    Address of bottom of stack and start of input buffer
\     R#    Current character position in the editor
\     H     Pointer to next available BYTE in the dictionary
\
\ HERE is not quite the same as H, but it's close.
\
\ SOURCE returns the start of the input buffer, but it is not
\ required to address anything else. S0 and SOURCE are obviously
\ transient.
\
\ But the other user variables (SCR USER BASE CONTEXT CURRENT >IN
\ BLK OFFSET) are still usable.

: loops ( n -- , repeat remainder of line n times )
   >in @         \ get offset within input buffer
   swap          \ move n over offset
   0 do          \ n 0 do
      dup >in !  \ reset input pointer
      interpret  \ process the input
   loop
   drop ;        \ discard old input buffer offset


\ End of ch11.fth
