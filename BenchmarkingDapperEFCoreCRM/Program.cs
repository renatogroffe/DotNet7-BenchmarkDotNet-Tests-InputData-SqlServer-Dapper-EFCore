using BenchmarkDotNet.Running;
using BenchmarkingDapperEFCoreCRM.Tests;

new BenchmarkSwitcher(new [] { typeof(CRMTests) }).Run(args);