\ ch10.fs -- I/O and You -- T.Brumley

include TxbWords.fs

\ The various TH words get redefined warnings in gforth, these
\ and others are safe to ignore.
\
\ This really uses the Block word set so this won't work in
\ pforth.

\ When loaded some instream tests fire off.

\ Chapter 10 deals with Forth input/output operations in more
\ depth, including disk access. Block buffers for disk access
\ are not implemented in pforth. They are available in gforth.

\ When I started this chapter I didn't see myself as ever
\ needing to use the block editor so I dummied up some loads
\ into temporary blocks.

\ As I look ahead to _Thinking Forth_ I believe I may want to
\ at least try to use blocks for source to get a better feel of
\ how and why things were done back in the day.


\ Strings, text and key input are intermixed here with blocks.
\ I've read the chapter through and several of the definitions
\ don't exist in pforth. Some are in gforth and I worked some
\ of the problems there.

\ Deblocked buzzphrase generator v1

use temporary-blocks.fb  \ can safely be deleted

: >block-line ( addr u u u -- )
   swap block swap 64 * + \ addr u blkaddr
   dup 64 blank           \ clear the line
   swap move ;            \ and move

\ Rather than mess with the block editor I'll build the blocks
\ manually. This is still in a block, and gforth has a default
\ backing file "blocks.fb" but I'm not concerned with its
\ contents.

132 block 1024 '*' fill update
133 block 1024 blank    update
flush

\  0                   20                  40
s" INTEGRATED          MANAGEMENT          CRITERIA      " 132 0 >block-line
s" TOTAL               ORGANIZATION        FLEXIBILITY   " 132 1 >block-line
s" SYSTEMATIZED        MONITORED           CAPABILITY    " 132 2 >block-line
s" PARALLEL            RECIPROCAL          MOBILITY      " 132 3 >block-line
s" FUNCTIONAL          DIGITAL             PROGRAMMING   " 132 4 >block-line
s" RESPONSIVE          LOGISTICAL          CONCEPTS      " 132 5 >block-line
s" OPTIMAL             TRANSITIONAL        TIME PHASING  " 132 6 >block-line
s" SYNCHRONIZED        INCREMENTAL         PROJECTIONS   " 132 7 >block-line
s" COMPATIBLE          THIRD GENERATION    HARDWARE      " 132 8 >block-line
s" QUALIFIED           POLICY              THROUGH-PUT   " 132 9 >block-line 
s" PARTIAL             DECISION            ENGINEERING   " 132 10 >block-line

update flush

\ Select a buzzword. N is the column offset, which is 0 for
\ the first column (adj), 20 for the second (adj), and 40 for
\ the third column (noun).

: buzz ( n -- )
   132 block +            ( first char on line 0 )
   11 choose 64 *         ( select line 0-10 )
   +                      ( word on line )
   20 -trailing type ;    ( readjust length for print )

: 1adj 0 buzz ;
: 2adj 20 buzz ;
: noun 40 buzz ;

\ build a buzzphrase

: phrase 1adj space 2adj space noun ;

\ Replace Gartner Group with "AI".

: paragraph
   cr ." By using " phrase space ." coordinated with "
   cr phrase space ." it is possible for even the most "
   cr phrase space ." to function as "
   cr phrase space ." within the constraints of "
   cr phrase ." . " ;

paragraph

\ Chapter 10 problems.

\ 1. Enter some text into a block and then define a word CHANGE
\ that takes two ASCII values and and changes all the first to
\ to the second.

\ Copy the buzzword table to another block.

132 block 133 block 1024 move update save-buffers

: change ( blk fr to -- )
   rot block dup 1024 + swap  ( fr to end-block start-block )
   do
      over                    ( from to from )
      i c@ = if               ( check cur )
         dup i c!             ( change if needed )
      then
   loop                       ( leaving from to on stack )
   2drop ;

133 'O' 'o' change update
133 list


\ 2. Define a word called FORTUNE which will print a preiction
\ at your terminal. The prediction should be chosen from a
\ block of 16 lines of 64 characters, remove trailing blanks.

134 block 1024 blank update flush

s" When everyone in the world sees beauty, then ugly exists."
   134 0 >block-line
s" What is and what is not create each other." 134 1 >block-line
s" High and low rest on each other." 134 2 >block-line
s" First and last follow each other." 134 3 >block-line
s" What’s the difference between yes and no?" 134 4 >block-line
s" What’s the difference between beautiful and ugly?"
   134 5 >block-line
s" Heavy is the root of light." 134 6 >block-line
s" What should be shrunken must first be stretched."
   134 7 >block-line
s" What should be weakened must first be strengthened."
   134 8 >block-line
s" What should be abolished must first be cherished."
   134 9 >block-line
s" What should be deprived must first be enriched."
   134 10 >block-line
s" What has no substance can penetrate what has no opening."
   134 11 >block-line
s" If princes and kings were not exalted they might be overthrown."
   134 12 >block-line
s" Ruling a great country is like cooking a small fish."
   134 13 >block-line
s" In lightness the root is lost. In haste the ruler is lost."
   134 14 >block-line
s" The Way is eternal. Until your last day you are free from peril."
   134 15 >block-line
update flush

\ Select a line from the Taoist sayings block.

: fortune ( -- )
  16 choose 64 * 134 block + 64 type cr ;


\ 3. Buddha brings you the twelve years of animals.
\
\ The cycle order is:
\
\ Rat Ox Tiger Rabbit Dragon Snake
\ Horse Ram Monkey Cock Dog Boar
\
\ Define .ANIMAL that expects 0-11 on the stack, and prints the
\ corresponding animal name.
\
\ I decided to go with a simple case instead of creating a
\ separate array.

: .animal ( n -- )
  abs 12 mod          ( make sure it is in range 0-11 )
  case
     0 of ." Rat" endof
     1 of ." Ox" endof
     2 of ." Tiger" endof
     3 of ." Rabbit" endof
     4 of ." Dragon" endof
     5 of ." Snake" endof
     6 of ." Horse" endof
     7 of ." Ram" endof
     8 of ." Monkey" endof
     9 of ." Cock" endof
     10 of ." Dog" endof
     11 of ." Boar" endof
     ." Default is impossible"
  endcase ;

\ Define (JUNEESHEE) which takes a year and prints the name of
\ the animal of the year. 1900 is the year of the Rat, 1901 is
\ the year of the Ox, and so on.

: (juneeshee) ( n -- , given year >= 1900 print animal )
   1900 - dup
   0< if
      ." BAD YEAR, NEED >= 1900" drop
   else
      12 mod .animal
   then ;

\ Define JUNEESHEE which prompts the user for their birth year
\ and prints the name name of the year's animal. Do this so
\ that the user does not have to press return.

: juneeshee ( -- , accept year >= 1900 and print animal )
   ." Please enter your birth year: "
   pad 16 blank        ( scrub )
   4 pad c!            ( maximum length )
   pad 1+ 4 expect     ( read it )
   pad number d>s      ( seems to always be a double )
   dup 1900 < if
      abort" invlid year, must be 1900 or greater."
   then
   cr ." You were born in the Year of the "
   (juneeshee) cr ;


\ 4. Rewrite the definition of LETTER in this chapter so that
\ it uses names and descriptions that have been entered into a
\ block instead of character arrays. Define LETTERS so that it
\ prints one letter for every person in your file.

( As in the text ... rewrite this )

( FORM LOVE LETTER )
( First -- fix the string vriable declarations )

\ NAME is defined in gforth but is not in the standard. It's
\ part of the compiler/interpreter support so this shadowing
\ shouldn't matter for these exercises. It's actually listed
\ as an alias of PARSE-NAME if I'm reading this right.

\ I ended up not finishing this. I think I have the concepts
\ down and this is basicaly a repeat of earlier problems.
\ Upcoming problem 5 covers the new areas in this problem and
\ is more interesting to me.

create name 14 allot
create eyes 12 allot
create me   14 allot

44 constant ccomma
1 constant cbreak

\ Gather and parse comma delimited attributes for the form love
\ letter.

: vitals
   name 14 blank
   eyes 12 blank
   me 12 blank
   ccomma text pad name 14 move
   ccomma text pad eyes 12 move
   cbreak text pad me   14 move ;

\ Assuming that vitals have been entered, generate a rather
\ insipid love letter.

: letter
   page
   ." Dear " name 14 -trailing type ." ,"
   cr ." I go to heaven whenever I see your deep "
      eyes 12 -trailing type ."  eyes. Can"
   cr ." you go to the movies this Friday?"
   cr 30 spaces ." Love, "
   cr 30 spaces me 14 -trailing type
   cr ." PS: Wear something " eyes 12 -trailing type
   ."  to show off those eyes!" 
   cr ;


\ 5. Write a virtual array (disk backed 'virtual' storage
\ accessed via @ and !).
\
\ First select an unused block in your range of assigned
\ blocks. There can be no text on this block; binary data will
\ be stored in it. Put this block number in a variable. Then
\ define an access word which accepts a cell subscript from
\ the stack, then computes the block number corresponding to
\ this subscript, calls BLOCK and returns the memory address
\ of the subscripted cell. This access word should also call
\ UPDATE. Test your work so far.

139 constant my-block-no
variable my-block
my-block-no my-block !

: reset-my-block
   my-block @ block 1024 erase update flush ;

reset-my-block

1024 cell / 1- constant cells-per-block

\ I borrowed the idea of th from _Thinking Forth_ but made it
\ too specific. After I found out that gforth already has th
\ and friends I decided to wrap them to limit them to my
\ block. The built in versions are better but I had already
\ started down this path.

: th ( n -- addr )         \ find cell n in my-block
   my-block @ block        \ block in memory
   swap th                 \ position for "real" th
   update ;                \ might be dirtied

: th@ ( n -- addr ) th @ ; \ fetch nth item in my-block
   th @ ;

: th! ( n1 n2 -- ) th ! ;  \ store n1 into n2th item in my-block
   th ! ;

\ Next use the first cell as a count of how many data items are
\ stored in the array. Define a word PUT which will store a
\ value into the next available cell of the array. Define a
\ display routine which will print the stored elements in the
\ array.

: put ( n -- , store n in the next available slot )
   0 th@                  \ how many aleady, watch overflow
   dup cells-per-block >= if
      abort" block array overflow!"
   then
   1+ dup 0 th!           \ increment
   th! ;                  \ then use index to address and store

\ Note: width hardcoded, no checking for bounds or valid index.

: prt ( n -- )            \ print nth entry 8 chars wide
   th@ 8 .r ;

\ Now use this virtual array facility to define a word ENTER
\ which will accept pairs of numbers and store them in the
\ array. Bug: does not support negative nubers. Bug: does not
\ allow for extra spaces. "ENTER 1,2" works, but not "  1,2" or
\ "1, 2", these three are entered as 1 2 0 2 1 0.
\
\ I see solutions for these bugs but they don't add value to
\ the problem.

: enter ( -- , "enter num,num" )
   0 0                     \ double word for >number
   ccomma word count       \ ud c-addr u1
   >number                 \ ud c-addr+ u2
   2drop drop              \ n
   put                     \ bug, leading blanks break >number
   0 0 cbreak word count >number 2drop drop put ;

\ Finally, define TABLE to print the data stored above with
\ eight numbers per line

: tabler                      \ TABLE is in base for Search
   0 th@                      \ how many 
   dup 0> if                  \ guard against empty
      1+ 1 do                 \ loop is from first live entry
         i 8 mod 1 = if cr then  \ newline every 8
         i prt space 
      loop 
   then ;

\ End of ch10.fs
