( ch10.fth -- I/O and You -- T.Brumley )

   marker ch10

(
   Chapter 10 deals with Forth input/output operations in
   more depth, including disk access. Block buffers for disk
   access in pforth are not implemented. I believe they can
   be loaded as an extension word set for gforth. As I'm
   planning on using pforth I won't worry too much about
   blocks here.

   Strings and text and key input are intermixed here with
   blocks. I've read the chapter through and several of the
   definitions don't exist in pforth. They aren't in gforth
   without some optional block support.

   I'll do the glossary and work the problems without trying
   to create block support. Hopefully this won't be too much
   of a mess.

   Update: I don't like the approach outlined above. The
   more I look at blocks the more I like them. I guess I'm
   a sick bastage. I'm going to try to implement the block
   word set.
)

\ A variable number generator (16-bit) from Starting Forth.
\
\ The RANDOM and CHOOSE in pforth use this algorithm. Copied
\ here and shadows the pforth implementation.
\
\ The version from Starting Forth doesn't include the "mask
\ and". The pforth version uses "65535 and" to clip the
\ range to 16-bits. "16777215 and" would clip the range to
\ 24-bits.

variable random-seed   here rnd !

65535 constant random-mask   ( 24 bit )

: random ( -- n )
   random-seed @
   31421 *
   6927 +
   random-mask and
   dup   rnd !   ;

: choose ( u1 -- u2 )
   random um*  swap  drop   ;

\ Infrastructure.

\ Write something to create blockish files -- mapping lines
\ into 16x64 without newlines. Be able to read this into
\ memory to access as if it were a real block.

\ Chapter 10 problems.

\ 1. Enter some text into a block and then define a word
\ CHANGE that takes two ASCII values and and changes all
\ the occurrences of the first two the second.

\ 2. Define a word called FORTUNE whic will print a preiction
\ at your terminal. The prediction should be chosen from a
\ block of 16 lines of 64 characters, remove trailing blanks.

\ 3. Buddha brings you the twelve years of animals.
\
\ The cycle order is:
\
\ Rat Ox Tiger Rabbit Dragon Snake
\ Horse Ram Monkey Cock Dog Boar
\
\ Define .ANIMAL that expects 0-11 on the stack, and prints
\ the corresponding animal name.
\
\ Define (JUNEESHEE) which takes a year and prints the name
\ of the animal of the year. 1900 is the year of the Rat,
\ 1901 is the year of the Ox
\
\ Define JUNEESHEE which prompts the user for their birth
\ year and prints the name name of the year's animal. Do
\ this so that the user does not have to press return.

\ 4. Rewrite the definition of LETTER in this chapter so
\ that it uses names and descriptions that have been
\ entered into a block instead of character arrays. Define
\ LETTERS so that it prints one letter for every person
\ in your file.

\ 5. Write a virutal array (disk backed 'virtual' storage
\ accessed via @ and !). ... This gets vary into block
\ I/O so I'm not sure I feel a need to do it.





\ End of ch10.fth
