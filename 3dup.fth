\ 3dup.fth -- create a copy of the top 3 items on the stack -- T.Brumley.

: 3dup ( n1 n2 n3 -- n1 n2 n3 n1 n2 n3 )
    dup >r -rot
    dup >r -rot
    dup >r -rot
    r>
    r>
    r> ;
