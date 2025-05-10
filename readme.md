### TODO:

- find out how to paralelize the comb generation
- find out how to paralelize the sharpe calculation

combs = getAllCombs


for comb in combs ..
    for w in getRandW(1000) ..
        get sharpe

do i paralelize the inner loop?
do i paralelize the outer loop?
do i paralelize both?
is the paralelization async? Or concurrency? Or multiple processes? Or multiple threads?
what is the difference between them and which one do I use and where?





### Honesty section:

- GPT was used to consult interop between F# and C# specifically to understand errors related to mismatching types, it was also used to ascertain whether the function used to generate combinations was the as good as possible.
