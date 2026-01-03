\ blocks.fth -- An attempt at the optional Block word set for pforth -- T.Brumley.

   marker block-writer

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

   Blocks are stored in the data space (dictionary).

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
)

\ BLK returns the address of a cell containing zero or the
\ number of the block being interpretted. 

variable BLK        ( -- a-addr )
0 BLK !

\ BLOCK returns the address of the first character of the block
\ buffer assigned to u.

: BLOCK ( u -- a-addr )
   true abort" not implemented!" ;

\ BUFFER returns the address of the first character of
\ the block buffer assigned to u.

: BUFFER ( u -- a-addr )
   true abort" not implemented!" ;

\ EVALUATE extends the base EVALUATE to include storing
\ 0 in BLK.

: EVALUATE ( ? -- ? )
   true abort" not implemented!" ;
   
\ FLUSH invoke SAVE-BUFFERS and then unassign all block
\ buffers.

: FLUSH ( -- )
   save-buffers       \ persist each updated buffer
   true abort" not complete!" ; \ unassign all buffers

\ LOAD saves the current input source and then redirects
\ it to a block, process, and then restore. This may not
\ be needed for my exercises.

: LOAD ( i * x u -- j * x )
   true abort" not complete!" ;

\ SAVE-BUFFERS persists each updated block buffer and
\ marks those buffers as not updated.

: SAVE-BUFFERS ( -- )
   true abort" not implemented!" ;

\ UPDATE makes the current block buffer as modified. It does
\ not trigger an I/O.

: UPDATE ( -- )
   true abort" not implemented!" ;

\ EMPTY-BUFFERS unassigns all block buffers discarding any
\ pending updates.

: EMPTY-BUFFERS ( -- )
   true abort" not implemented!" ;

\ LIST displays block u in an implementation defined format.
\ Stores u in SCR.

: LIST ( u -- )
   true abort" not implemented!" ;

\ REFILL extends the base REFILL to: when the input source is
\ a block make the next block the input source and current
\ input buffer by adding one to the value of BLK and seeting
\ >IN to zero. Returns true if the new value of BLK is a
\ valid block, false otherwise.

: REFILL ( -- flag )
   true abort" not implemented!" ;

\ SCR returns the address of the cell containing the block
\ number of the block most recently LISTed.

: SCR ( -- a-addr )
   true abort" not implemented!" ;

\ THRU loads blocks u1 through u2, any stack effects are the
\ result of the words loaded.

: THRU ( i * x u1 u2 -- j * x )
   true abort" not implemented!" ;

\ '\' extends the execution semantics of \ so that if BLK is
\ contains zero, parse and discard the remainder of the parse
\ area, otherwise parse and discard the portion of the parse
\ area corresponding to the remainder of the current line.
\ Blcoks are 16 by 64, so typically the next multiple of 64. 

\ I don't think I'll do this if I can avoid it.

\ End of blocks.fth
