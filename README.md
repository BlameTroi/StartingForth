# Working Through _Starting Forth_ by Leo Brodie

## Summary

I'm (re)learning Forth for this year's _Advent of Code_. I poked
at a couple of texts and decided to work through the OG Forth
introductory text _Starting Forth_. I read this back in the early
1980s with Forth on a 6809 system, but I never used Forth
seriously and what little I learned is lost in my faulty memory.

Gone are the days when I could skim a book and hack at a computer
and quickly learn a language. Age does that to a mind. Writing
about a topic is a good way to study. See William Zinsser's
_Writing to Learn_ for an excellent examination of the value of
writing to learning.

Applying some structure to the writing and going through the
motions of reviewing and publishing gives me a sense of external
accountability and keeps me on task.

## The Text

Leo Brodie's _Starting Forth_ is _The C Programming Language_ of
the Forth world, albeit a bit more whimsical. Physical copies can
be found if you look, but the fine people at Forth, Inc. along
with Leo have made the text available online.

A lightly updated HTML browsable version can be found at
[_Starting Forth_](https://www.forth.com/starting-forth/0-starting-forth/).

## The Standard

A Forth reference generally consists of Forth words (compiled
callable code), their stack effects, and a brief explanation of
the word's intent. I created a glossary as I worked through the
text but the reference glossary for modern ANS Forth is the Forth
Standards Organization website.

A glossary of Forth words broken out by feature groupings is at
[Forth Standard](https://forth-standard.org/standard/words).

If you are trying to learn Forth, bookmark that site.

## Which Forth

I have already settled on using `pforth` for this exercise. It is
mostly ANS standard with minor differences that won't matter to
me. Cells are now eight bytes wide instead of two, and ANS Forth
does differ from the FIG FORTH dialect described in the text.

You can find [`pforth`](https://www.softsynth.com/pforth/) at
([repository](https://github.com/philburk/pforth/)). It builds on
an ARM Mac and runs well.

## Structure

My work for each chapter (follow along experimentation, my
commentary, and worked problems) is in a file that can be loaded
into a Forth system. `pforth` uses the INCLUDE statement.

Each file should load without error and its definitions should
work.

I created my own glossary which is just a copy of the various
Forth words and their stack effects in a plaintext file.

## License

These are just my notes as I work through the text. I'm basically
using GitHub as an offsite backup for my notes. I don't think
anyone else will find value here, but if anyone does that's a
bonus.

My code and ramblings are all public domain, as are my ramblings.
If you want to use any of this and need a more explicit license,
pick from either the MIT or the UNLICENSE as in the LICENSE file.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.

Troy Brumley\
BlameTroi@gmail.com\
So let it be written...\
...so let it be done
