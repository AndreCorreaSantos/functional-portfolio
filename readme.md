## Speedup Comparison

A simple Python version was implemented to get a solid grasp of the logic, and that implementation was later optimized with NumPy vectorized operations and Numba batch operators.

Following this implementation, an F# sequential implementation was devised and another F# parallel version (which parallelized both the inner and outer loop) was also developed.

All of the aforementioned implementations were evaluated 5 times and their times to compute all of the 142 thousand Sharpes were logged, and the comparison can be seen below:

![Execution Time Chart](images/comparison.png)

| Version         | Average Time (minutes) | Speedup vs Previous |
|----------------|------------------------|----------------------|
| F# Sequential   | 42.73                  | -                    |
| F# Parallel     | 7.80                   | 5.48×                |
| Python Numba    | 2.19                   | 3.56×                |

Although the Numba version remains the fastest, it is remarkable that even without access to highly efficient vectorized operations, the F# parallel implementation was able to achieve similar performance.

### Honesty section:

- GPT was used to consult interop between F# and C#, specifically to understand errors related to mismatching types. It was also used to ascertain whether the function used to generate combinations was as good as possible.
- It was also used to verify the get Sharpe calculations, and to check if any optimizations could be implemented.
