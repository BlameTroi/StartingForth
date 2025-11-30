( Forth <-> Ascii Conversion)              ( © 1985 MacTutor by J. Langowski 
)

\ Found at https://preserve.mactech.com/articles/mactech/Vol.01/01.10/ForthBlocks/index.html
 
: tx-blks ;

variable ifile#  -1 ifile# ! 
variable ofile# -1  ofile# !    
: ifile ifile# @ ;  : ofile ofile# @ ;

hex 4D414341 constant "maca decimal
create ="blks      "blks ,
create ="textdata  "text , "data ,

18 field +fcb.name    22 field +fcb.vrefnum
32 field +fcb.type    36 field +fcb.creator

( standard file reply record fields )
00 field +good    : @good  +good w@ ;
02 field +ftype   : @ftype +ftype @ ;
06 field +vrefnum : @vrefnum +vrefnum <w@ ;
10 field +fname

create ireply 80 allot    
ireply +fname constant iname
create oreply 80 allot    
oreply +fname constant oname
variable screen#        variable cur.line#
create text.buf 100 allot 
99 constant text.read.limit
5 constant ch.menu

: moverefnum  ( file#\reply -- )
    @vrefnum  swap >fcb +fcb.vrefnum w! ;

: get.input.name 
      >r >r >r
      100 100 xy>point  0   r>  r>  r@
      (get.file) page r> @good 
      0= if abort then  ;

: get.save.name
      page >r >r >r  
      100 100 xy>point  r>  r>  r@
      (put.file) r> @good 
      0= if abort then ;

: text.open  
    next.fcb ifile# ! 
    page ." Text file to convert:"
    ="textdata 2 ireply get.input.name
    iname ifile assign 
    ifile ireply moverefnum
    ifile open   ?file.error
    ifile rewind ?file.error ;

: block.open
    next.fcb ifile# ! 
    page ." Blocks file to convert:"
    ="blks 1 ireply get.input.name
    iname ifile assign 
    ifile ireply moverefnum
    ifile open  ?file.error  ifile select ;

: block.create 
   next.fcb ofile# !
   " Mac Tutor Blocks" 
   oname over c@ 1+ cmove
   " Save as:" oname oreply get.save.name
   oname ofile assign
   ofile oreply moverefnum ofile delete
   ofile create.blocks.file ?file.error
   ofile open ?file.error  ofile select
   2 ofile append.blocks  
   1 screen# !   0 cur.line# !
   0 block b/buf bl fill update flush ;

:  text.create 
   next.fcb ofile# !
   " Mac Tutor Text" oname over c@ 1+ cmove
   " Save as:" oname oreply get.save.name
   oname ofile assign    
   ofile oreply moverefnum ofile delete
   ofile create.file ?file.error
   ofile open ?file.error  ofile rewind
   ofile get.file.info
   "text ofile >fcb +fcb.type !
   "maca ofile >fcb +fcb.creator !
   ofile set.file.info  ;

: >line.start ( block\line -- addr of line)
  64 * swap block + ;

: write.line 
    >line.start 64 -trailing
    over over + 13 swap c!  ( add CR )
    1+  ofile write.text  ;

: write.screen
    dup 16 0 do dup i ( screen\line )
    over over . 2 spaces . cr ( debug )
    write.line  loop drop ( n ) ;

: copy.block  ifile get.eof b/buf /
    0 do  i . cr i write.screen
      do.events drop loop ;

: read.line  ifile current.position
   text.buf 1+  text.read.limit  
   ifile  read.text
   ifile current.position swap -  
   text.buf  c!  ;

: copy.line 
  read.line  ?eof not
  if cur.line# @ dup .
     64 * screen# @ dup . cr 
     block +  text.buf 1+  swap
     text.buf c@ 1- cmove  
     1 cur.line# +!  cur.line# @ 10 >
     if 0 cur.line# !   update flush
        1 screen# +!  1 ofile append.blocks
     then  do.events drop true
  else update flush false then ;

: copy.text  begin copy.line not until ;

: >text block.open text.create copy.block ;

: >block text.open block.create copy.text ;

: change.menu
    0 " Change" ch.menu new.menu
    " Forth->Ascii;Ascii->Forth;"  
    ch.menu append.items draw.menu.bar
    ch.menu menu.selection:
    1 activate.event scale -1 xor events !
    0 hilite.menu
     case  1 of  >text      endof
           2 OF  >block     endof endcase
    events on do.events abort ;

change.menu

Listing 2: Modula 2 FP 'accuracy test'
