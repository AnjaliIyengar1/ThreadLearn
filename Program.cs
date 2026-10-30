using ThreadLearn;
using ThreadLearn.Stages;

string[] files = TestFilesManagement.EnsureFiles("testdata");
Console.WriteLine($"Files: {files.Length}, Total: {TestFilesManagement.TotalMb(files):F0} MB, CPU cores: {Environment.ProcessorCount}");

// Comment out any stage you don't want to run.
// Keep Stage 1 on when comparing: it is the baseline the other stages are measured against.

// Stage 1: all work on one thread (baseline)
//Measure.Run("Single-threaded", () => SingleThread.Run(files));

// Stage 2: manual threads, files split up front between threads
/*Measure.Run("Manual threads (4)", () => ManualThreads.Run(files, threadCount: 4));
Measure.Run($"Manual threads ({Environment.ProcessorCount})", () => ManualThreads.Run(files, Environment.ProcessorCount));*/

// Lock demo: runs No lock, lock and Interlocked, and prints each result
//CounterRace.Run(Environment.ProcessorCount);

// Run 1 vs run 2: the same thread IDs appear again, because the threads are reused.
// Parallel.ForEach: [Thread 1] appears, because the main thread is helping.
/*Measure.Run("Task.Run, run 1", () => ThreadPoolTasks.RunTasks(files));
Measure.Run("Task.Run, run 2", () => ThreadPoolTasks.RunTasks(files));
Measure.Run("Parallel.ForEach", () => ThreadPoolTasks.RunParallelForEach(files));*/

// Cancellation: ask work to stop after a time limit
Measure.Run("Loop, cancel after 100 ms", () => Cancellation.RunLoop(files, 100));
Measure.Run("Parallel.ForEach, cancel after 30 ms (1)", () => Cancellation.RunParallel(files, 30));
Measure.Run("Parallel.ForEach, cancel after 30 ms (2)", () => Cancellation.RunParallel(files, 30));