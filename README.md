# ssV3, a.k.a. ss version 3

ss is a multi-file, multi-window text editor, a Windows version of the Sam text editor of 
Plan-9 fame, that one designed and written by Rob Pike. This is a clean sheet implementation 
in C# that makes use of .NET's regular expression engine, garbage collection, etc.

Some modest enhancements were added:

Word wrap (I often write emails and such in ss)
Automatic indention
Two modes for moving by word, one for programming, one for plain writing. (this is not very well
  developed, but the idea is there.)
Choosable fonts
Control of text encoding in files

I added commands to:

Display help files
Change the case of text 
Control case sensitivity in searches
Display text in hex.
Define line endings for working with data from various platforms
Define tab width display so tabs work like they should
Fix the width of the display to a chosen number of characters

A few further notes:

At this point, Fall of 2020, I've made a 40 year career out of IT work of various kinds, system and database
administration, programming, and almost half of it in management. My programming experience at work
was always small scale, fast turn around type stuff. ss is the largest things I've ever written. I have no doubt
a real pro, used to working on large scale software design, would do a much better and cleaner job than I have
on ss. Having said that, it does work pretty well for what it does and I find I'm able to make my way through
the code reasonably well, even when it's months between sessions. The performance is not too bad, but 
again, some one really good at that I'm sure could make it better.

There are two help files, the second of which is incomplete but I finally decided to make this public since
I had no idea when I would finish the help documentation.

I use ss every day in my work and rarely run across bugs anymore. Perhaps ss will be useful to others as well.

Cheers,
Will

