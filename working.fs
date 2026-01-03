\ blocks.fs -- An attempt at the optional Block word set for pforth -- T.Brumley.

(
   Starting Forth and some other vintage texts assume the
   block file system and not more modern Posix files. I
   actually like the block interface for scratch work so I've
   decided to try to implement the optional Block word set as
   described in the Forth standard.

   https://forth-standard.org/standard/block

   This will use Posix files for persistence.

   No immediate support for editing or using the blocks
   aside from scracth storage is planned at this time.
)

(
   A block is a 1024 character chunk of data on disk,
   identified by a block number.

   A block buffer is a block-sized region of data where a
   block is made temporarily available for use. The current
   block buffer is the one more recently accessed by BLOCK,
   BUFFER, LOAD, LIST, or THRU.

   Blocks are stored in the data space aka dictionary.

   Block buffer addresses are transient.
)

(
   The base block words are:

   BLK
   BLOCK
   BUFFER
   EVALUTE
   FLUSH
   LOAD
   SAVE-BUFFERS
   UPDATE

   The block extension words are:

   EMPTY-BUFFERS
   LIST
   REFILL
   SCR
   THRU
   \           <- yes this is the backslash comment marker

   Merging code from Tom Zimmer's block support found on
   GitHub and re-ordering the base definitions. The words
   that are part of the Blocks word set are in all capitals
   when declared. 

)

\ Some words from Win32Forth that I like:
\ TODO: Move these to TxbWords.

: cells- ( a1 n1 -- a1-n1*cell ) \ multiply n1 by the cell size and subtract
   cell * - ;

: cell+  ( a1 -- a1+cell )       \ add a cell to a1
   cell + ;

: cell-  ( a1 -- a1-cell )       \ subtract a cell from a1
   cell - ;

: +cells ( n1 a1 -- n1*cell+a1 ) \ multiply n1 by the cell size and add
   swap cell * + ;

: -cells ( n1 a1 -- a1-n1*cell ) \ multiply n1 by the cell size and
   swap cell * - ;

\ From an old Google Groups post:
\ 
\ SCAN and SKIP are useful words for character searching
\ within a string, and generally useful for parsing strings
\ into tokens separated by a specific delimiter character.
\ SCAN and SKIP are not standard words, but are available in
\ many Forth systems.
\
\ LSCAN and LSKIP are cell based
\ WSCAN and WSKIP are word based, but I'm not sure what
\ the definition of (machine) word is meant to be.
\
\ I found a few definitions, I'll try these as a starting
\ point.

\ /STRING over min rot over + -rot - ;
\ : /STRING  DUP >R - SWAP R> CHARS + SWAP ;
\  T{ s1  5 /STRING -> s1 SWAP 5 + SWAP 5 - }T
\ T{ s1 10 /STRING -4 /STRING -> s1 6 /STRING }T
\ T{ s1  0 /STRING -> s1 }T 
\ create foo 10 chars allot
\ foo 2 3 /string cr . foo - .
: scan ( a u c -- a1 u1 )
   over 0=
   if
      drop exit
   then
   >r over
   c@
   r@ =
   if
      r>
      drop
   else
      1
      /string
      r>
      recurse
   then ;

: skip ( a u c -- a1 u1 )
   over 0=
   if
      drop exit
   then
   >r over
   c@
   r@ =
   if
      1
      /string
      r>
      recurse
   else
      r>
      drop
   then ; 

: LSCAN ( adr len long -- adr' len' )
\ Scan for char through addr for len, returning addr' and len' of char.
        >R 2DUP CELLS R> -ROT   \ adr len long adr len
        OVER + SWAP       \ adr len long adr+len adr
        ?DO DUP I @ =
                IF LEAVE
                ELSE >R 1- >R CELL+ R> R>
                THEN CELL
       +LOOP DROP ;

\ : SKIP  ( adr len char -- adr' len' ) \ skip leading chars "char" in string
\ : SCAN  ( adr len char -- adr' len' ) \ search first occurence of char "char" in string
\ : WSKIP ( adr len word -- adr' len' ) \ skip leading words "word" in string
\ : WSCAN ( adr len word -- adr' len' ) \ search first occurence of word "word" in string
\ : LSKIP ( adr len long -- adr' len' ) \ skip leading cells "long" in string
\ : LSCAN ( adr len long -- adr' len' ) \ search first occurence of cell "long" in string

\ these exist in pforth:

\ : CHARS  ( n1 -- n1*char )       \ multiply n1 by the character size (1 byte)
\ : CHAR+  ( a1 -- a1+char )       \ add the characters size in bytes to a1

\ Constants

1024 constant b/buf
  64 constant c/l                 \ characters per line
   8 constant #buffers            \ buffer pool
  -1 value blockhandle         \ current block file handle

\ BLK returns the address of a cell containing zero or the
\ number of the block being interpretted. 

variable BLK        ( -- a-addr )
0 BLK !

\ SCR returns the address of the cell containing the block
\ number of the block most recently LISTed.

variable SCR        ( -- a-addr )
0 SCR !

\ cur_buffer# is the buffer number of the current block.
 
variable cur_buffer#
cur_buffer# off

#buffers cells constant buflen

variable rec_array b/buf #buffers * allot  \ array of blocks
variable rec#s     buflen           allot  \ block # array
variable rec#updt  buflen           allot  \ update flags
variable rec#use   buflen           allot  \ block bubbleup stack
variable rec#fil   buflen           allot  \ hcb for each block

: buf#>bufaddr ( n -- addr )
   b/buf * rec_array + ;

\ Yes, these shadow the variables.
 
: >rec#s ( n -- addr )  \ return buffer n's record addr
   rec#s +cells ;

: >rec#updt ( n -- addr ) \ return buffer n's update addr
   rec#updt +cells ;

: >rec#fil ( n -- addr ) \ return buffer n's file addr
   rec#fil +cells ;

\ note this doesn't consume its input

: chkfil ( n -- n f ) \ is buffer n current?
   dup dup 8 =        \ magic number?
   if
      drop false exit
   else
      >rec#fil @ blockhandle =
   then ;

\ This scans by cell not bytes or words
: bubbleup ( n -- )   \ move buffer n to end of list
   >r rec#use #buffers r@ lscan dup 0=
   abort" Buffer# number not in buffer list"
   1- cells >r dup cell+ swap r> move \ move list down except first
   r> rec#use buflen + cell - ! ;     \ stuff first at end of list.

\ n1 = block we are looking for
\ n2 = buffer #
\ f1 = do we have it? true if we do
\ therefore, if false then n2 isn't there

\ This scans by cell not bytes or words
: ?gotrec ( n1 -- <n2> f1 )    
   rec#s #buffers rot lscan nip
   #buffers swap - ( tos is buffer # with matching block # )
   chkfil
   if
      true
   else
      drop false
   then ;

\ n1 = block to position to
\ Set file pointer to block pos n1

: pos_block ( n1 -- )
   0max b/buf * 0 blockhandle reposition-file drop ;

\ a1 = destination address
\ n1 = block number to read
\ read block n1 to address

: read_block ( a1 n1 -- )
   pos_block
   b/buf blockhandle read-file swap b/buf <> or
   abort" Error reading block" ;

\ n1 = buffer number
\ n2 = block number to write
\ write n1 to disk

: write_block ( n1 n2 -- )
   pos_block
   dup buf#>bufaddr
   b/buf rot >rec#fil @ write-file
   abort" Error writing block, check disk space." ;

\ The above are marked internal.
\ And the next are marked external.

\ n1 = block# a2 = bufaddr
\ Save all updated buffers to disk.

\ SAVE-BUFFERS persists each updated block buffer and
\ marks those buffers as not updated.

: SAVE-BUFFERS ( -- )           \ save all updated buffers to disk
   #buffers 0                   \ through all the buffers
   do rec#use @ >r              \ find a buffer
      r@ bubbleup               \ bump to highest priority
      r@ cur_buffer# !          \ set current buffer var
      r@ >rec#updt dup @        \ check update flag
      if
         off                    \ clear update flag
         r@ dup >rec#s @        \ get block #
         write_block            \ write it
      else
         drop                   \ discard, already cleared
      then
      r>drop
   loop ;

\ BUFFER returns the address of the first character of
\ the block buffer assigned to u.

: BUFFER ( n1 -- a1 )         \ Assign least used buffer to rec n1
   dup ?gotrec                \ check if already present
   if
      >r drop                 \ buffer already assigned, save it
   else
      rec#use @ >r            \ assign LRU buffer
      r@ >rec#updt dup @      \ check update flag
      if
         off                  \ clear update flag
         r@ dup >rec#s @      \ get block #
         write_block          \ write it
      else
         drop                 \ discard, already cleared
      then
      r@ >rec#s   !           \ set block #
      blockhandle r@ >rec#fil !    \ set the file hcb
   then
   r@ bubbleup                \ bump to highest priority
   r@ cur_buffer# !           \ set current buffer var
   r> buf#>bufaddr ;          \ calc buffer addr

\ EMPTY-BUFFERS unassigns all block buffers discarding any
\ pending updates.

: EMPTY-BUFFERS ( -- )                 \ clean out the virtual buffers
   rec_array b/buf #buffers * erase
   rec#s    buflen -1 fill
   rec#updt buflen erase
   rec#fil  buflen erase
   rec#use  #buffers 0
   do
      i over ! cell+     \ initialize the bubbleup stack
   loop
   drop ;

\ FLUSH invoke SAVE-BUFFERS and then unassign all block
\ buffers.

: FLUSH ( -- )                 \ Write any updated buffers to disk
        SAVE-BUFFERS
        EMPTY-BUFFERS ;

\ UPDATE makes the current block buffer as modified. It does
\ not trigger an I/O.

: UPDATE ( -- )                 \ mark the current block as updated
        cur_buffer# @ >rec#updt on ;

                                       \ n1 = block # to get
                                       \ a1 is address of block # n1
\ BLOCK returns the address of the first character of the block
\ buffer assigned to u.

: BLOCK ( n1 -- a1 )           \ Get block n1 into memory
   dup ?gotrec
   if
      nip dup >r buf#>bufaddr
      r@ cur_buffer# ! r> bubbleup
   else
      blockhandle 0< abort" No file open"
      dup buffer dup rot read_block
   then    ;

\ LIST displays block u in an implementation defined format.
\ Stores u in SCR.
\ 
: LIST ( n1 -- )       \ display block n1 on the console
   dup SCR !
   BLOCK b/buf bounds
   do
      cr i c/l type
   c/l +loop    ;

: wipe ( n1 -- )       \ erase the specified block to blanks
   BUFFER b/buf blank update ;

: set-blockfile ( fileid -- )
   to blockhandle ;

\ warning off

\ EVALUATE extends the base EVALUATE to include storing
\ 0 in BLK.

: EVALUATE ( a1 n1 -- )
   blk off evaluate ;

: save-input ( -- xxx 8 )
   save-input
   blk @ swap 1+ ;

\ Is (source) source?
\ SOURCE simplifies the process of directly accessing the
\ input buffer by hiding the differences between its location
\ for different input sources. This also gives implementors
\ more flexibility in their implementation of buffering
\ mechanisms for different input sources. 
\
\ Probably not based on this explanation from std 6.1.2216
\ ( -- c-addr u )
\ c-addr is the address of, and u is the number of characters in, the input buffer. 
 
: restore-input ( xxx 8 -- f1 )
   swap blk ! 1-
   restore-input >r
   blk @ 0>
   if
      blk @ block b/buf (source) 2! \ force back to block
   then
   r> ;

\ REFILL extends the base REFILL to: when the input source is
\ a block make the next block the input source and current
\ input buffer by adding one to the value of BLK and seeting
\ >IN to zero. Returns true if the new value of BLK is a
\ valid block, false otherwise.

: REFILL ( -- f1 )
   blk @ 0=
   if
      refill
   else
      >in off
      ?loading on
      BLK @ 1+ b/buf BLOCK (source) 2!
      true
   then    ;

\ There is no linefeed in blocks so we need to track length
\ to find the end of the line.
\
\ immediate marks the most recently defined word as an
\ immediate word (once recognized, doth it)

: \ ( -- )
BLK @ 0=
if
   postpone \
else
   >in @ c/l / 1+ c/l * >in !
then    ; immediate

\ warning on

: blkmessage    ( n1 -- )
   blk @ 0>
   if
      base @ >r
      cr ." Error: " pocket count type space
      dup -2 =
      if
         drop msg @ count type
      else
         ." Error # " .
      then
      cr ." Block: " BLK @ .
      ." at Line: " >in @ c/l / .
      cr BLK @ BLOCK >in @ c/l / c/l * + c/l type
      BLK off   \ reset BLK cause noone else does!!!
      r> base !
   else
      _message
   then    ;

' blkmessage is message

\ LOAD saves the current input source and then redirects
\ it to a block, process, and then restore. This may not
\ be needed for my exercises.

: LOAD \ ???   whay curly? { loadblk \ incntr outcntr -- }
   SAVE-INPUT dup 1+ dup to incntr
                        to outcntr
   begin  >r -1 +to incntr  incntr  0= until
   loadblk BLK !
   >in off
   ?loading on
   BLK @ BLOCK b/buf (source) 2!
   interpret
   begin
      r> -1 +to outcntr
      outcntr 0=
   until
   RESTORE-INPUT drop ;

\ THRU loads blocks u1 through u2, any stack effects are the
\ result of the words loaded.
 
: THRU ( n1 n2 -- )
   1+ swap
   ?do
      i LOAD
   loop    ;

\ Actual OS file access, will need review.

: CLOSE-BLOCKFILE ( -- )
   blockhandle -1 <>
   if
      flush
      blockhandle      \ Roderick Mcban - February 11th, 2002
      close-file drop
   then
   -1 to blockhandle ;

: OPEN-BLOCKFILE ( -<filename>- )
   CLOSE-BLOCKFILE
   /parse-word count r/w open-file abort" Failed to open Block File"
   set-blockfile
   EMPTY-BUFFERS ;

: CREATE-BLOCKFILE ( u1 -<filename>- )  \ create a blank file of u1 block long
   CLOSE-BLOCKFILE
   /parse-word count r/w create-file
   abort" Failed to create Block File"
   SET-BLOCKFILE
   dup b/buf m* blockhandle resize-file
   abort" Unable to create a file of that size"
   EMPTY-BUFFERS
   0
   do
      i wipe
   loop
   FLUSH ;

: #blocks ( -- n1 )       \ return the number of block in the current file
   blockhandle file-size drop b/buf um/mod nip ;

\ +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
\ initialization of the block system
\ +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

empty-buffers           \ Initialize the virtual memory arrays interpretively

\ INTERNAL        \ another internal definitions

: virtual-init  ( --- ) \ and during the system startup initialization
   -1 to blockhandle
   empty-buffers ;

\ ******** initialization-chain chain-add virtual-init

\ MODULE          \ end of the module
\ environment definitions
\ : BLOCK         TRUE ;
\ : BLOCK-EXT     TRUE ;

\ only forth also definitions
\ End of blocks.fs
