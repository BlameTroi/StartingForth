\ ch09.fth -- Under the Hood -- T.Brumley

    marker ch09

\ Chapter 9 examines the compilation and execution of Forth. Some
\ of the details are likley different between FIG and ANS Forth
\ but the concepts carry forward.
\
\ These features are known as reflection or introspection in
\ modern langauges.

\ ' (TICK) returns an execution token in ANS, not a direct
\ address. EXECUTE will execute the word referenced. Unlike in
\ FIG, ' word 80 DUMP works while it segfaults. After some
\ experimentation it looks to me as if the exeuction token (xt)
\ is an offset and not an absolute address.
\
\ Here's what I'm seeing in pforth:
\
\ ' ROT           returns the xt
\ c" ROT" FIND    returns xt -1
\ c" BOGUS" FIND  returns @c" 0
\ ' ROT EXECUTE   executes ROT
\
\ In gforth ' appears to return an address and FIND is compile
\ only. EXECUTE works.
\
\ In pforth one can't do something like 110 ' someconstant !

\ There's a lot of power here.
\
\ The line between compilation vs interpretation is a bit fuzzy
\ but very important. Several of these words are marked as being
\ compile time or execution time only. Interpretation seems to
\ be part of execution.
\
\ Semantics are more important than the implementation. The
\ return value from ' is meant to be used by the compiler and
\ by EXECUTE.

\ The Dictionary and Vocabularies
\ 
\ Dictionary format and chaining are discussed. The theory
\ presented here is still valid but the details are very much
\ like what I leanred reading _Threaded Interpreted Languages_
\ which did not have vocabularies.
\
\ Optimizations that exist in modern times won't be visible at
\ the conceptual level. We still need the dictionary even if
\ some of the word definitions are JITed.
\
\ Insert my normal rant about DEP here.

\ Dictionary entries:
\
\ Everything we define ends up in the dictionary. Each entry has
\ a header and body.
\
\ For constants or variables:
\ 
\ Header
\     word name
\     link                 to next entry
\     code pointer         code for variable or constant or ...
\
\ Body
\     parameter field      <- Parameter Field Address (PFA)
\                          1 cell for variable or constant
\                          2 cells for 2variable or 2constant
\                          or longer if ALLOTted or ,
\ 
\ For a colon defintion: 
\ 
\ Header
\     word name
\     link                   to next entry
\     code pointer           code for :
\
\ Body
\     addr some word        *interpreter pointer I is
\     addr some word        *<-currend word in definition
\     addr some word
\     addr EXIT              <- normal end of word def
\
\ Code pointers are to routines to execute, retrieve contents,
\ and update contents.

\ * I think there's a bad overload of "interpreter" here. Yes, it's
\   a threaded interpreter (in vintage Forth) but here we are just
\   using function vectors. This is different from interpretting
\   input text, which can do a compilation but doesn't necessarily
\   do so.
 
\ What the text refers to as vectored execution I think of as branch
\ tables. It might not be a an exactly match but close enough.

\ Storage map:
\ 
\ HERE is the next available position in the dictionary. It
\ advances via : CONSTANT VARIABLE ALLOT , C, and variations.
\
\ PAD is a work area at some fixed distance after HERE. It is
\ used as a scratch are for output, <# # #> and such.
\
\ In pforth : PAD HERE 128 + ;
\ In gforth : PAD HERE 176 + ; 
\
\ S0 'S and S don't seem to be in ANS but they are related to
\ the parameter stack.
\
\ There is an input buffer which is offset from the stack. In
\ ANS this is somehow related to TIB.

\ The concept of vocabularies seems to be replaced by by the
\ ANS Optional Search-Order Word Set. I'm not going to need
\ vocabularies for Advent of Code, so I just skimmed this.
\
\ These provide scoping. I'm not sure how this would impact
\ performance, but reducing the size of the visible dictionary
\ provides a benefit according to Brodie. I guess this would
\ depend on the lookup mechanism during compilation, but that
\ shouldn't matter over time.

\ Chapter 9 Problems

\ 1. Create a word COUNTS that will work with the sentencing
\ words from chapter 2 problem 6 so that it is possible for a
\ judge to say something like:
\
\ CONVICTED-OF BOOKMAKING 3 COUNTS TAX-EVASION WILL-SERVE
\
\ and get the right answer of 17 years.
\
\ The FIG solution would be:
\ 
\ : COUNTS ' ROT ROT 0 DO OVER EXECUTE LOOP SWAP DROP ;
\
\ I wasn't expecting that to work in either pforth or gforth, but
\ it does. So the semantics of ' and execute are unchanged, just
\ the actual value returned.
\
\ I clearly need to revisit this. For now I'll annotate the code
\ and move on. I don't expect to use this during Advent of Code.

\ Chapter 2 problem 6 is a court sentencing application. A judge
\ enters "convicted-of crime1 crime2 crime3 will-serve" and is 
\ presented with a total sentence. Each "crime" is "+ duration"
\ and "convicted-of" just puts 0 on the stack. "will-serve" is
\ just a print of the top of the stack.
\ 
\ This is all happening at interpretation time. As words are
\ evaluated the stack effects are applied. What COUNTS does it
\ look forward to the next word of the input stream (expecting
\ it to be a "CRIMEn". It then executes that word some number
\ of times (the prefix N to COUNTS).
 
: counts         \ ? n -- , executes next word n times 
    '            \ ? n xt   xt of next word of input stream 
    rot          \ n xt ?
    rot          \ xt ? n
    0 do
        over     \ xt ? xt
        execute  \ xt ?    expected next word adds to ?
    loop
    swap         \ ? xt    nip
    drop ;       \ ?       running sentence

\ So obviously ' also advances the input pointer past the word
\ it looks up.

\ 2. What is the beginning address of your private dictionary?
\
\ I don't have a direct way to find this from what I know so far
\ but HERE returns the next available slot in the dictionary.
\ It seems to me that in pforth the XT is an offset, so if I
\ take HERE, then define a new word and get the XT of that word
\ I should be close:

cr ." I think the dictionary starts at "
HEX HERE : BOGUS . ; ' BOGUS - . DECIMAL cr

\ Gets 140014000 on my M2 Mac.
\
\ The Text suggests EMPTY . but in pforth I don't have an EMPTY
\ and a private dictionary doesn't make sense in a single user
\ system. 

\ 3. How far is PAD from the top of the dictionary in my system?
\ 128 byges. Pad is actually defined as : PAD HERE 128 + ;

\ 4. These don't appear applicable beyond noting that BASE does
\ not seem to be stored in its dictionary entry.

\ 5. An exercise in vectored execution.
\
\ Define a one-dimensional array of cells will return the nth
\ elements address when given a preceeding subscript n.
\
\ Define several words which output something to the terminal
\ and take no stack input.
\
\ Store the addresses of these words in various entries in the
\ array. Have a do nothing word and store it in some of the
\ elements.
\
\ Define a word that takes a valid index into the array and
\ executes the word referenced by that element. 
 
variable branch-table 9 cells allot
branch-table 10 cells erase

: output-one ." I'd do anything for love " ;
: output-two ." But I won't do that " ;
: output-three ." You're dancing on the edge of a grave " ;
: output-four ." Everything louder than everything else " ;
: output-five ." You're a dead ringer for love " ;

: nop ;

: initialize-branch-table ['] nop 10 0 do dup branch-table i cells + ! loop drop ;

initialize-branch-table 
' output-one branch-table 0 cells + !
' output-four branch-table 9 cells + !
' output-three branch-table 6 cells + !
' output-five branch-table 3 cells + !
' output-two branch-table 5 cells + !

: do-it ( n -- )
    dup 0 >= over 10 < and not if ." I won't do that: " . exit then
    cells branch-table + @ execute ;

: test-branch-table
    11 -1 do i dup cr . space do-it loop cr ;

\ End of ch09.fth
