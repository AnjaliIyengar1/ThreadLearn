# ThreadLearn

## Objective
Learn how threads, CPU cores and shared memory behave in .NET by running the same workload in different ways and measuring the results.

**Workload:** compute the SHA-256 hash of 50 test files (520 MB total, random bytes, fixed seed so files are identical every run).
**Machine:** macOS, 10 CPU cores. All runs with `dotnet run -c Release`.

## How to run
Requires the .NET 10 SDK.
```
cd ThreadLearn
dotnet run -c Release
```
The first run generates the test files in `testdata/` (git-ignored). Choose which stage runs by commenting/uncommenting lines in `Program.cs`. Each stage lives in its own file under `Stages/`.

## Status
Done: Stages 1–4 below.
Next: async/await (I/O-bound vs CPU-bound, in progress), thread-pool starvation, false sharing, producer/consumer with `Channel<T>`, memory and GC (`ArrayPool`), BenchmarkDotNet, multiple processes, live thread UI.

## Key terms
- **Wall time:** real elapsed time, start to finish. Lower = faster.
- **CPU time:** total computing done, adding up every core's busy time (like total person-hours).
- **Avg CPU %:** CPU time / wall time x 100. 100% = one core busy; max = cores x 100% = 1000%.
- **Race condition:** threads change shared data at the same time and overwrite each other, giving wrong results.
- **Atomic:** an operation done as one unbreakable step that no other thread can interrupt.

---

## Stage 1: Single-threaded

All files hashed one after another on the main thread.

```
Files: 50, Total: 520 MB, CPU cores: 10
[Thread 1] file_049.bin  265EB5A35085EDEA...
[Thread 1] file_048.bin  627FC81D01AF8A52...
[Thread 1] file_004.bin  A13C84C41DFE9EFC...
...
Single-threaded: 266 ms
CPU time: 278 ms, Avg CPU: 104% (max possible: 1000%)
```

- Only one thread was used: every line shows `[Thread 1]`.
- ~100% CPU = one core busy, the other 9 idle.
- A later run gave 439 ms at 78% CPU: the thread was waiting for disk (files no longer cached in RAM). Lower CPU % with higher wall time is worse, not better.

---

## Stage 2: Manual threads

Creates N threads with `new Thread`; files split up front (thread 0 gets 0,4,8...; thread 1 gets 1,5,9...).

```
[Thread 5] file_049.bin  265EB5A35085EDEA...
  -> Thread 5 done: 13 files, 128 MB, 80 ms
[Thread 7] file_047.bin  3E59B33D17F5CDEE...
  -> Thread 7 done: 12 files, 124 MB, 81 ms
[Thread 6] file_046.bin  5F4BBC47BDA3357A...
  -> Thread 6 done: 12 files, 147 MB, 90 ms
Manual threads (4): 90 ms, CPU time: 325 ms, Avg CPU: 358% (max possible: 1000%)
```

- Many threads were used: output shows different thread IDs (4, 5, 6, 7).

| Mode | Wall time | CPU time | Avg CPU |
|---|---|---|---|
| Single-threaded | 266 ms | 278 ms | 104% |
| 4 threads | 90 ms | 325 ms | 358% |
| 10 threads | 74 ms | 575 ms | 776% |

- 4 threads: same CPU time as single, spread across ~3.6 cores at once, so ~3x faster.
- 10 threads: only slightly faster than 4, and CPU time grew. More threads give diminishing returns.
- The thread with the most MB (147 MB) finished last and set the total time: load imbalance.

---

## Stage 2b: Race condition and locks

10 threads each add 1 to one shared counter 1,000,000 times. Expected total: 10,000,000.

```
Expected total: 10,000,000
No lock     : 2,162,486 in 5 ms
lock        : 10,000,000 in 262 ms
Interlocked : 10,000,000 in 207 ms
```

- **No lock is fast but wrong:** 75-86% of increments lost, different every run. `count++` is 3 steps (read, add, write), so threads overwrite each other.
- **`lock` and `Interlocked` are both correct.** `lock` lets one thread at a time into the code; `Interlocked` uses an atomic CPU instruction.

---

## Stage 3: Thread pool (`Task.Run`, `Parallel.ForEach`)

.NET keeps a set of threads (the thread pool) and reuses them; we just hand it work.

```
=== Task.Run, run 1 ===
[Thread 9] file_004.bin  A13C84C41DFE9EFC...
[Thread 6] file_001.bin  29FDFE7C4FBFF59E...
[Thread 13] file_008.bin  20C29F1E6E1A48E2...
...
Task.Run, run 1: 171 ms, CPU time: 422 ms, Avg CPU: 246% (max possible: 1000%)

=== Task.Run, run 2 ===
[Thread 9] file_001.bin  29FDFE7C4FBFF59E...
[Thread 14] file_004.bin  A13C84C41DFE9EFC...
[Thread 11] file_008.bin  20C29F1E6E1A48E2...
...
Task.Run, run 2: 70 ms, CPU time: 547 ms, Avg CPU: 776% (max possible: 1000%)

=== Parallel.ForEach ===
[Thread 14] file_001.bin  29FDFE7C4FBFF59E...
[Thread 1] file_000.bin  77994B05DC2AD168...
[Thread 12] file_010.bin  8B0B148A34E9E0A6...
...
Parallel.ForEach: 76 ms, CPU time: 574 ms, Avg CPU: 746% (max possible: 1000%)
```

- **Run 1 vs run 2:** the same thread IDs appear again, because the threads are reused.
- **Parallel.ForEach:** `[Thread 1]` appears, because the main thread is helping.

---

## Stage 4: Cancellation (`CancellationToken`)

A thread can't be force-stopped. You ask it to stop with a `CancellationTokenSource` (`Cancel()` / `CancelAfter(ms)`), pass the `CancellationToken` to the worker, and the worker checks it at safe points. The file in progress always finishes; no new file starts.

```
=== Loop, cancel after 100 ms ===
  Stopped after 21 of 50 files
Loop, cancel after 100 ms: 105 ms, CPU time: 97 ms, Avg CPU: 91% (max possible: 1000%)

=== Parallel.ForEach, cancel after 30 ms (1) ===
  Cancelled after 44 of 50 files
Parallel.ForEach, cancel after 30 ms (1): 69 ms, CPU time: 516 ms, Avg CPU: 743% (max possible: 1000%)

=== Parallel.ForEach, cancel after 30 ms (2) ===
  Cancelled after 43 of 50 files
Parallel.ForEach, cancel after 30 ms (2): 66 ms, CPU time: 538 ms, Avg CPU: 813% (max possible: 1000%)
```

- **Loop:** checks the token itself before each file; stopped at 105 ms, just after the 100 ms request.
- **Parallel.ForEach:** checks the token for you and throws `OperationCanceledException`. It stopped at ~67 ms instead of 30 ms, almost a full run.
- **Why late:** `CancelAfter` uses a timer whose callback runs on a pool thread. All pool threads were busy hashing, so the stop signal most likely waited for a free thread. A cancel is only a request; how fast work stops depends on when the signal is delivered and checked.