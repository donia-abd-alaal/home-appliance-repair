using Xunit;

// Static counters are shared by all appliances, so tests must not run in parallel.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
