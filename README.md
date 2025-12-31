# Working Through _Starting Forth_ by Leo Brodie

## Summary

I am (re)learning Forth for this year's _Advent of Code_. I poked at a
couple of texts and decided to work through the OG Forth introductory
text _Starting Forth_. I read this back in the early 1980s with Forth
on a 6809 system, but I never used Forth seriously and what little I
learned is lost in my faulty memory.

Gone are the days when I could skim a book and hack at a computer and
quickly learn a language. Age does that to a mind. Writing about a
topic is a good way to study. See William Zinsser's
_Writing to Learn_ for an excellent examination of the value of
 writing to learning.

Applying some structure to the writing and going through the motions
of reviewing and publishing gives me a sense of external
accountability and keeps me on task.

## Status

I worked through the first 11 chapters. I haven't worked chapter 12
here. I plan to read it and then move to _Thinking Forth_ before
seriously working on Advent of Code 2025. 

## The Text

Leo Brodie's _Starting Forth_ is _The C Programming Language_ of
the Forth world, albeit a bit more whimsical. Physical copies can
be found if you look, but the fine people at Forth, Inc. along
with Leo have made the text available online.

A lightly updated HTML browsable version can be found at
[_Starting Forth_](https://www.forth.com/starting-forth/0-starting-forth/).

## The Standard

A Forth reference generally consists of Forth words (compiled callable
code), their stack effects, and a brief explanation of the word's
intent. I started creating my own glossary as I worked through the
text but the reference glossary for modern ANS Forth is the Forth
Standards Organization website.

My glossary is incomplete but pieces of it are included in this
repository. I'd like to create a searchable index with stack effect
comments, but by the time I get to it I probably won't need it.

A glossary of Forth words broken out by feature groupings is at
[Forth Standard](https://forth-standard.org/standard/words).

If you are trying to learn Forth, bookmark that site.

## Which Forth

I started with `pforth` but switched to `gforth` once I figured out
how to build the 0.7.9 version on a Mac. Standard compliance is
pretty good in `pforth` and I only looked to `gforth` for support for
BLOCKS. Beyond more complete support for extension word sets, the
only reason I found to prefer `gforth` was that `pforth` isn't quite
as forgiving of my ignorance. I would get segfaults when I did
(admittedly) stupid things--using words in the wrong mode and the
like.

I believe I wouldn't have those same issues now that I understand more
about the Forth environment.

You can find [`pforth`](https://www.softsynth.com/pforth/) at(
[repository](https://github.com/philburk/pforth/)). It builds on an
ARM Mac and runs well.

`gforth` is [at](https://gforth.org/). The older 0.7.3 release is
available from most package managers, including Brew on the Mac. I've
posted notes on how I was able to build 0.7.9 on my Mac to Reddit and
the gforth mailing list.

## Structure

My work for each chapter (follow along experimentation, my commentary,
and worked problems) is in a file that can be loaded into a Forth
system using INCLUDE or REQUIRE.

Each file should load without error and its definitions should work.
There are redefinition warnings from several of the files. They can
be safely ignored. I usually have notes in the files highlighting the
warnings.

I've included one of my utility include files since it has a working
RANDOM which I don't find in `gforth`. This is `TxbWords`. This file
also demonstrates conditional loading using [DEFINED], [IF], and
others. It provides warnings on redefinition but as with the chapter
files, they can be safely ignored.

If Block support is needed, `gforth` must be used. See `ch10`.

## License

These are just my notes as I work through the text. I'm basically
using GitHub as an offsite backup for my notes. I don't think anyone
else will find much of value here, but if anyone does that's a
bonus.

My code and ramblings are all public domain. If you want to use any
of this and need a more explicit license, pick from either the MIT
or the UNLICENSE as in the LICENSE file.

This program is distributed in the hope that it will be useful, but
WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.

Troy Brumley\
BlameTroi@gmail.com\
So let it be written...\
...so let it be done
